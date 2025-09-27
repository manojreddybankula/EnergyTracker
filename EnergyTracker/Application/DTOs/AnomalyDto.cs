namespace EnergyTracker.Application.DTOs
{
    /// <summary>
    /// DTO for anomaly detection results.
    /// </summary>
    public class AnomalyDto
    {
        public string Period { get; set; } = null!;
        public string Product { get; set; } = null!;
        public double Consumption { get; set; }
        public double RollingAverage { get; set; }
        public double PercentAboveAverage { get; set; }
    }
}
