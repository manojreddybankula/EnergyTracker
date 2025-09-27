using System;

namespace EnergyTracker.Domain.Entities
{
    /// <summary>
    /// Represents a single energy reading for a user and product at a specific timestamp.
    /// </summary>
    public class EnergyReading
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = null!;
        public string Product { get; set; } = null!; // "electricity" or "gas"
        public double KWh { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
