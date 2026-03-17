using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var jwtKey = builder.Configuration["Jwt:Key"] ?? "travelguide-secret-key-change";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TravelGuideWebAdmin";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TravelGuideMobileApp";

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "TravelGuideAdmin.Auth";
    })
    .AddJwtBearer("ApiBearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddDbContext<TravelGuideWebAdmin.Data.TravelGuideContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=TravelGuide.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TravelGuideWebAdmin.Data.TravelGuideContext>();
    db.Database.EnsureCreated();
    EnsureAdminUser(db, app.Configuration);
    db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS User_POI_Logs (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Username TEXT,
        PoiId INTEGER NOT NULL,
        StartTime TEXT NOT NULL,
        EndTime TEXT NOT NULL,
        TriggerType TEXT
    );");
    db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS POI_Images (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        PoiId INTEGER NOT NULL,
        ImageUrl TEXT,
        Caption TEXT
    );");
    db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS POI_Medias (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        PoiId INTEGER NOT NULL,
        Type TEXT,
        AudioUrl TEXT,
        TtsScript TEXT,
        Language TEXT
    );");

    EnsureColumn(db, "POIs", "DescriptionEn", "TEXT");
    EnsureColumn(db, "Tours", "DescriptionEn", "TEXT");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static void EnsureColumn(TravelGuideWebAdmin.Data.TravelGuideContext db, string table, string column, string type)
{
    using var conn = db.Database.GetDbConnection();
    conn.Open();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = $"PRAGMA table_info({table});";
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        if (string.Equals(reader["name"]?.ToString(), column, StringComparison.OrdinalIgnoreCase))
            return;
    }
    reader.Close();

    using var alterCmd = conn.CreateCommand();
    alterCmd.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {type};";
    alterCmd.ExecuteNonQuery();
}

static void EnsureAdminUser(TravelGuideWebAdmin.Data.TravelGuideContext db, IConfiguration config)
{
    var adminUser = config["Admin:Username"] ?? "admin";
    var adminPass = config["Admin:Password"] ?? "Admin@123";

    var exists = db.Users.Any(u => u.Username == adminUser);
    if (exists)
        return;

    db.Users.Add(new TravelGuideWebAdmin.Models.User
    {
        Username = adminUser,
        PasswordHash = TravelGuideWebAdmin.Services.PasswordHasher.Hash(adminPass),
        FullName = "System Admin",
        Email = "admin@travelguide.local",
        PhoneNumber = "0000000000",
        Role = "Admin",
        Tier = "VIP"
    });
    db.SaveChanges();
}
