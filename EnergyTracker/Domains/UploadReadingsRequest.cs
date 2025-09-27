namespace EnergyTracker.Domains
{
    public class ReadingDto
    {
        public string Product { get; set; } = null!;
        public double KWh { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class UploadReadingsRequest
    {
        public string UserId { get; set; } = null!;
        public List<ReadingDto> Readings { get; set; } = new();
    }
}
