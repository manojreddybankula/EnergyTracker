using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyTracker.Application.Interfaces;
using EnergyTracker.Domain.Entities;
using EnergyTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyTracker.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core repository for product pricing.
    /// </summary>
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
