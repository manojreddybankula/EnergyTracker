using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnergyTracker.Domain.Entities;

namespace EnergyTracker.Application.Interfaces
{
    /// <summary>
    /// Repository abstraction for energy readings.
    /// </summary>
    public interface IEnergyReadingRepository
    {
        Task AddReadingsAsync(IEnumerable<EnergyReading> readings);
        Task<bool> ExistsAsync(string userId, string product, DateTime timestamp);
        Task<List<EnergyReading>> GetReadingsAsync(string userId, string? product = null);
    }
}
