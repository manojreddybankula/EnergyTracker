using System;
using System.Collections.Generic;

namespace EnergyTracker.Application.DTOs
{
    /// <summary>
    /// DTO for a single reading in the upload batch.
    /// </summary>
    public class ReadingDto
    {
        public string Product { get; set; } = null!;
        public double KWh { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// DTO for uploading a batch of readings.
    /// </summary>
    public class UploadReadingsRequest
    {
        public string UserId { get; set; } = null!;
        public List<ReadingDto> Readings { get; set; } = new();
    }
}
