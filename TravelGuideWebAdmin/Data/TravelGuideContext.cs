using Microsoft.EntityFrameworkCore;
using TravelGuideWebAdmin.Models;

namespace TravelGuideWebAdmin.Data
{
    public class TravelGuideContext : DbContext
    {
        public TravelGuideContext(DbContextOptions<TravelGuideContext> options) : base(options)
        {
        }

        public DbSet<POI> POIs { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Tour_POI> Tour_POIs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<User_POI_Log> User_POI_Logs { get; set; }
        public DbSet<POI_Image> POI_Images { get; set; }
        public DbSet<POI_Media> POI_Medias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();
        }
    }
}
