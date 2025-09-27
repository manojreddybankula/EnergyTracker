namespace EnergyTracker.Domains
{
    public class AggregatedReportDto
    {
        public string Period { get; set; } = null!;
        public string Product { get; set; } = null!;
        public double TotalKWh { get; set; }
        public double TotalCost { get; set; }
    }
}
