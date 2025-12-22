using Microsoft.EntityFrameworkCore;
using HomeRadar.Models;

namespace HomeRadar.Data
{
    /// <summary>
    /// Entity Framework DbContext - Veritabanı bağlantı ve yönetim sınıfı
    /// </summary>
    public class EmlakContext : DbContext
    {
        public EmlakContext(DbContextOptions<EmlakContext> options) : base(options)
        {
        }

        // DbSets - Veritabanı tabloları
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<District> Districts { get; set; } = null!;
        public DbSet<BuildingType> BuildingTypes { get; set; } = null!;
        public DbSet<Feature> Features { get; set; } = null!;
        public DbSet<Listing> Listings { get; set; } = null!;
        public DbSet<ListingFeature> ListingFeatures { get; set; } = null!;
        public DbSet<Prediction> Predictions { get; set; } = null!;

        // ASP.NET Core'da connection string Program.cs'de Dependency Injection ile verilir
        // OnConfiguring artık gerekli değil

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Entity Konfigürasyonu
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                
                // Email unique constraint
                entity.HasIndex(e => e.Email).IsUnique();
                
                // Role check constraint (PostgreSQL'de trigger ile yapılacak)
                // entity.HasCheckConstraint("CK_User_Role", "Role IN ('Admin', 'User')");
            });

            // District Entity Konfigürasyonu
            modelBuilder.Entity<District>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name);
            });

            // BuildingType Entity Konfigürasyonu
            modelBuilder.Entity<BuildingType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Feature Entity Konfigürasyonu
            modelBuilder.Entity<Feature>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Listing Entity Konfigürasyonu
            modelBuilder.Entity<Listing>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Foreign Key Relationships
                entity.HasOne(d => d.District)
                    .WithMany(p => p.Listings)
                    .HasForeignKey(d => d.DistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.BuildingType)
                    .WithMany(p => p.Listings)
                    .HasForeignKey(d => d.BuildingTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Constraints
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.SquareMeters).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.RoomCount).IsRequired();
                entity.Property(e => e.SalonCount).IsRequired();
                entity.Property(e => e.BuildingAge).IsRequired();
                
                // Check constraints (PostgreSQL'de trigger ile yapılacak)
                // Price > 0
                // SquareMeters > 0
                // RoomCount > 0
                // BuildingAge >= 0
            });

            // ListingFeature Entity Konfigürasyonu (Many-to-Many)
            modelBuilder.Entity<ListingFeature>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(d => d.Listing)
                    .WithMany(p => p.ListingFeatures)
                    .HasForeignKey(d => d.ListingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Feature)
                    .WithMany(p => p.ListingFeatures)
                    .HasForeignKey(d => d.FeatureId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Unique constraint - Aynı listing'e aynı feature iki kez eklenemez
                entity.HasIndex(e => new { e.ListingId, e.FeatureId }).IsUnique();
            });

            // Prediction Entity Konfigürasyonu
            modelBuilder.Entity<Prediction>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Predictions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Listing)
                    .WithMany(p => p.Predictions)
                    .HasForeignKey(d => d.ListingId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.District)
                    .WithMany()
                    .HasForeignKey(d => d.DistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.BuildingType)
                    .WithMany()
                    .HasForeignKey(d => d.BuildingTypeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(e => e.PredictedPriceMin).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.PredictedPriceMax).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.PredictedPriceAvg).IsRequired().HasColumnType("decimal(18,2)");
            });
        }
    }
}

