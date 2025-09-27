using System.Threading.Tasks;
using EnergyTracker.Domain.Entities;
using System.Collections.Generic;

namespace EnergyTracker.Application.Interfaces
{
    /// <summary>
    /// Repository abstraction for product pricing.
    /// </summary>
    public interface IProductPriceRepository
    {
        Task<ProductPrice?> GetPriceAsync(string product);
        Task<List<ProductPrice>> GetAllPricesAsync();
    }
}
