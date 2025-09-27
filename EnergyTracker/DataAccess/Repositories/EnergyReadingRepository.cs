using EnergyTracker.DataAccess.DataContexts;
using EnergyTracker.DataAccess.Entities;
using EnergyTracker.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnergyTracker.DataAccess.Repositories
{
    public class EnergyReadingRepository : IEnergyReadingRepository
    {
        private readonly EnergyDbContext _db;

        public EnergyReadingRepository(EnergyDbContext db)
        {
            _db = db;
        }

        public async Task AddReadingsAsync(IEnumerable<EnergyReading> readings)
        {
            await _db.Readings.AddRangeAsync(readings);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string userId, string product, DateTime timestamp)
        {
            return await _db.Readings.AnyAsync(r =>
                r.UserId == userId && r.Product == product && r.Timestamp == timestamp);
        }

        public async Task<List<EnergyReading>> GetReadingsAsync(string userId, string? product = null)
        {
            var query = _db.Readings.AsQueryable().Where(r => r.UserId == userId);
            if (!string.IsNullOrEmpty(product))
                query = query.Where(r => r.Product == product);
            return await query.ToListAsync();
        }
    }
}
