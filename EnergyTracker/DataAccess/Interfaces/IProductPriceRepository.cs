using EnergyTracker.DataAccess.Entities;

namespace EnergyTracker.DataAccess.Interfaces
{
    public interface IProductPriceRepository
    {
        Task<ProductPrice?> GetPriceAsync(string product);
        Task<List<ProductPrice>> GetAllPricesAsync();
    }
}
