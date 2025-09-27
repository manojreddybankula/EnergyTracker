namespace EnergyTracker.Domain.Entities
{
    /// <summary>
    /// Represents the price per kWh for a product (electricity or gas).
    /// </summary>
    public class ProductPrice
    {
        public string Product { get; set; } = null!;
        public double PricePerKWh { get; set; }
    }
}
