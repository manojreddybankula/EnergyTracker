using EnergyTracker.DataAccess.Entities;
using EnergyTracker.DataAccess.Interfaces;
using EnergyTracker.DataAccess.DataContexts;
using Microsoft.EntityFrameworkCore;

namespace EnergyTracker.DataAccess.Repositories
{
    public class ProductPriceRepository : IProductPriceRepository
    {
        private readonly EnergyDbContext _db;

        public ProductPriceRepository(EnergyDbContext db)
        {
            _db = db;
        }

        public async Task<ProductPrice?> GetPriceAsync(string product)
        {
            return await _db.ProductPrices.FindAsync(product);
        }

        public async Task<List<ProductPrice>> GetAllPricesAsync()
        {
            return await _db.ProductPrices.ToListAsync();
        }
    }
}
