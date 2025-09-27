using EnergyTracker.DataAccess.Entities;

namespace EnergyTracker.DataAccess.Interfaces
{
    public interface IEnergyReadingRepository
    {
        Task AddReadingsAsync(IEnumerable<EnergyReading> readings);
        Task<bool> ExistsAsync(string userId, string product, DateTime timestamp);
        Task<List<EnergyReading>> GetReadingsAsync(string userId, string? product = null);
    }
}
