using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace garage3.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
        public DbSet<ParkingSpot> ParkingSpots => Set<ParkingSpot>();
        public DbSet<Parking> Parkings => Set<Parking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------
            // ApplicationUser
            // -------------------------
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(u => u.PersonalNumber).IsUnique();
                entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.PersonalNumber).HasMaxLength(20).IsRequired();
                entity.Property(u => u.MembershipType).HasMaxLength(20);
            });

            // -------------------------
            // VehicleType
            // -------------------------
            modelBuilder.Entity<VehicleType>(entity =>
            {
                entity.Property(vt => vt.Name).HasMaxLength(40).IsRequired();
                entity.HasIndex(vt => vt.Name).IsUnique();
                entity.HasIndex(vt => vt.Size).IsUnique();
            });

            // -------------------------
            // Vehicle
            // -------------------------
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.Property(v => v.RegistrationNumber).HasMaxLength(12).IsRequired();
                entity.HasIndex(v => v.RegistrationNumber).IsUnique();

                entity.HasOne(v => v.Owner)
                      .WithMany(u => u.Vehicles)
                      .HasForeignKey(v => v.OwnerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.VehicleType)
                      .WithMany(vt => vt.Vehicles)
                      .HasForeignKey(v => v.VehicleTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // -------------------------
            // ParkingSpot
            // -------------------------
            modelBuilder.Entity<ParkingSpot>(entity =>
            {
                entity.HasIndex(ps => ps.SpotNumber).IsUnique();
            });

            // -------------------------
            // Parking
            // -------------------------
            modelBuilder.Entity<Parking>(entity =>
            {
                entity.HasOne(p => p.Vehicle)
                      .WithMany(v => v.Parkings)
                      .HasForeignKey(p => p.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.ParkingSpot)
                      .WithMany(ps => ps.Parkings)
                      .HasForeignKey(p => p.ParkingSpotId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ SQL Server (LocalDB): only one activ parking per spot/vehicle
                entity.HasIndex(p => p.ParkingSpotId)
                      .IsUnique()
                      .HasFilter("[CheckOutTime] IS NULL");

                entity.HasIndex(p => p.VehicleId)
                      .IsUnique()
                      .HasFilter("[CheckOutTime] IS NULL");
            });
        }
    }
}
