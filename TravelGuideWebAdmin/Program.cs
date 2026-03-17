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
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured. Set it in appsettings or environment variables.");
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]   ?? "TravelGuideWebAdmin";
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
    // EnsureCreated tạo tất cả bảng từ DbSet (User_POI_Logs, POI_Images, POI_Medias, v.v.)
    // Không dùng raw SQL để tạo bảng nữa — EF Core quản lý schema
    db.Database.EnsureCreated();
    EnsureAdminUser(db, app.Configuration);

    // Migrate các cột mới thêm vào (backward compatibility)
    EnsureColumn(db, "POIs",  "DescriptionEn", "TEXT");
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
    // Whitelist để tránh SQL Injection nếu tham số được truyền từ nguồn động
    var allowedTables  = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "POIs", "Tours", "Users", "Tour_POIs", "User_POI_Logs", "POI_Images", "POI_Medias" };
    var allowedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "DescriptionEn", "Points", "Tier", "PhoneNumber" };
    var allowedTypes   = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TEXT", "INTEGER", "REAL", "BLOB" };

    if (!allowedTables.Contains(table))
        throw new ArgumentException($"Table '{table}' is not in the allowed list.", nameof(table));
    if (!allowedColumns.Contains(column))
        throw new ArgumentException($"Column '{column}' is not in the allowed list.", nameof(column));
    if (!allowedTypes.Contains(type))
        throw new ArgumentException($"Type '{type}' is not in the allowed list.", nameof(type));

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
