namespace EnergyTracker.DataAccess.Entities
{
    public class ProductPrice
    {
        public string Product { get; set; } = null!;
        public double PricePerKWh { get; set; }
    }
}
