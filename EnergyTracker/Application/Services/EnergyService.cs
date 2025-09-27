using EnergyTracker.Application.DTOs;
using EnergyTracker.Application.Interfaces;
using EnergyTracker.Domain.Entities;
using System.Globalization;

namespace EnergyTracker.Application.Services
{
    public class EnergyService
    {
        private readonly IEnergyReadingRepository _readingRepo;
        private readonly IProductPriceRepository _priceRepo;
        private const int MaxBatchSize = 10000;


        public EnergyService(IEnergyReadingRepository readingRepo, IProductPriceRepository priceRepo)
        {
            _readingRepo = readingRepo;
            _priceRepo = priceRepo;
        }

        public async Task<(bool Success, List<string> Errors)> UploadReadingsAsync(UploadReadingsRequest request)
        {
            var errors = new List<string>();
            if (request.Readings.Count > MaxBatchSize)
            {
                errors.Add($"Batch size exceeds {MaxBatchSize}.");
                return (false, errors);
            }

            var validReadings = new List<EnergyReading>();
            foreach (var r in request.Readings)
            {
                if (r.KWh < 0)
                {
                    errors.Add($"Negative kWh for {r.Product} at {r.Timestamp}.");
                    continue;
                }
                if (await _readingRepo.ExistsAsync(request.UserId, r.Product, r.Timestamp))
                {
                    errors.Add($"Duplicate reading for {r.Product} at {r.Timestamp}.");
                    continue;
                }
                validReadings.Add(new EnergyReading
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Product = r.Product.ToLowerInvariant(),
                    KWh = r.KWh,
                    Timestamp = r.Timestamp
                });
            }


            if (validReadings.Count > 0)
                await _readingRepo.AddReadingsAsync(validReadings);

            return (errors.Count == 0, errors);
        }

        public async Task<List<AggregatedReportDto>> GetAggregatedReportAsync(string userId, string groupBy, string product)
        {
            var readings = await _readingRepo.GetReadingsAsync(userId, product == "all" ? null : product);
            var prices = await _priceRepo.GetAllPricesAsync();
            var result = new List<AggregatedReportDto>();

            IEnumerable<IGrouping<string, EnergyReading>> groupReadings = groupBy switch
            {
                "day" => readings.GroupBy(r => r.Timestamp.ToString("yyyy-MM-dd")),
                "week" => readings.GroupBy(r => ISOWeek.GetYear(r.Timestamp) + "-W" + ISOWeek.GetWeekOfYear(r.Timestamp).ToString("D2")),
                "month" => readings.GroupBy(r => r.Timestamp.ToString("yyyy-MM")),
                _ => throw new ArgumentException("Invalid groupBy value.")
            };

            result = groupReadings.SelectMany(reading => reading.GroupBy(x => x.Product),
                                    (reading, prod) => new AggregatedReportDto
                                    {
                                        Period = reading.Key,
                                        Product = prod.Key,
                                        TotalKWh = prod.Sum(x => x.KWh),
                                        TotalCost = Math.Round(prod.Sum(x => x.KWh) *
                                            (prices.FirstOrDefault(p => p.Product == prod.Key)?.PricePerKWh ?? 0), 2)
                                    })
                                    .ToList();

            return result.OrderBy(r => r.Period).ToList();
        }

        public async Task<List<AnomalyDto>> GetAnomaliesAsync(string userId, string period)
        {
            var readings = await _readingRepo.GetReadingsAsync(userId);
            var prices = await _priceRepo.GetAllPricesAsync();
            var anomalies = new List<AnomalyDto>();

            Func<EnergyReading, string> periodKey = period switch
            {
                "month" => r => r.Timestamp.ToString("yyyy-MM"),
                "week" => r => ISOWeek.GetYear(r.Timestamp) + "-W" + ISOWeek.GetWeekOfYear(r.Timestamp).ToString("D2"),
                "day" => r => r.Timestamp.ToString("yyyy-MM-dd"),
                _ => throw new ArgumentException("Invalid period value.")
            };

            var grouped = readings.GroupBy(periodKey)
                .Select(g => new
                {
                    Period = g.Key,
                    Products = g.GroupBy(x => x.Product)
                })
                .OrderBy(x => x.Period)
                .ToList();

            foreach (var group in grouped)
            {
                foreach (var prodGroup in group.Products)
                {
                    var idx = grouped.FindIndex(x => x.Period == group.Period);
                    if (idx < 3) continue;

                    var rollingAvg = grouped.Skip(idx - 3).Take(3)
                        .SelectMany(x => x.Products.Where(p => p.Key == prodGroup.Key).SelectMany(p => p))
                        .Sum(x => x.KWh) / 3.0;

                    var consumption = prodGroup.Sum(x => x.KWh);
                    if (rollingAvg > 0 && consumption > rollingAvg * 1.5)
                    {
                        anomalies.Add(new AnomalyDto
                        {
                            Period = group.Period,
                            Product = prodGroup.Key,
                            Consumption = consumption,
                            RollingAverage = rollingAvg,
                            PercentAboveAverage = Math.Round((consumption - rollingAvg) / rollingAvg * 100, 2)
                        });
                    }
                }
            }
            return anomalies;
        }
    }
}
