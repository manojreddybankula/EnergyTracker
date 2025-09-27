namespace EnergyTracker.Application.DTOs
{
    /// <summary>
    /// DTO for aggregated report results.
    /// </summary>
    public class AggregatedReportDto
    {
        public string Period { get; set; } = null!;
        public string Product { get; set; } = null!;
        public double TotalKWh { get; set; }
        public double TotalCost { get; set; }
    }
}
