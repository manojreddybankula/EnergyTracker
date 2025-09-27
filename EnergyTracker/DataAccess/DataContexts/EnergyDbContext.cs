using Microsoft.EntityFrameworkCore;
using EnergyTracker.DataAccess.Entities;

namespace EnergyTracker.DataAccess.DataContexts
{
    public class EnergyDbContext : DbContext
    {
        public EnergyDbContext(DbContextOptions<EnergyDbContext> options) : base(options) { }

        public DbSet<EnergyReading> Readings => Set<EnergyReading>();
        public DbSet<ProductPrice> ProductPrices => Set<ProductPrice>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EnergyReading>()
                .HasIndex(r => new { r.UserId, r.Product, r.Timestamp })
                .IsUnique();

            modelBuilder.Entity<ProductPrice>().HasKey(p => p.Product);

            modelBuilder.Entity<ProductPrice>().HasData(
                new ProductPrice { Product = "electricity", PricePerKWh = 0.30 },
                new ProductPrice { Product = "gas", PricePerKWh = 0.20 }
            );
        }
    }
}
