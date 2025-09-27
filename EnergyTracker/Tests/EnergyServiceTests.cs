using Xunit;
using Moq;
using EnergyTracker.Application.Services;
using EnergyTracker.Application.Interfaces;
using EnergyTracker.Domain.Entities;
using EnergyTracker.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class EnergyServiceTests
{
    [Fact]
    public async Task UploadReadings_RejectsNegativeKWh()
    {
        var repo = new Mock<IEnergyReadingRepository>();
        var priceRepo = new Mock<IProductPriceRepository>();
        var service = new EnergyService(repo.Object, priceRepo.Object);

        var req = new UploadReadingsRequest
        {
            UserId = "u1",
            Readings = new List<ReadingDto>
            {
                new ReadingDto { Product = "electricity", KWh = -1, Timestamp = DateTime.UtcNow }
            }
        };

        var (success, errors) = await service.UploadReadingsAsync(req);
        Assert.False(success);
        Assert.Contains(errors, e => e.Contains("Negative kWh"));
    }

    [Fact]
    public async Task UploadReadings_RejectsDuplicates()
    {
        var repo = new Mock<IEnergyReadingRepository>();
        repo.Setup(r => r.ExistsAsync("u1", "electricity", It.IsAny<DateTime>())).ReturnsAsync(true);
        var priceRepo = new Mock<IProductPriceRepository>();
        var service = new EnergyService(repo.Object, priceRepo.Object);

        var req = new UploadReadingsRequest
        {
            UserId = "u1",
            Readings = new List<ReadingDto>
            {
                new ReadingDto { Product = "electricity", KWh = 1, Timestamp = DateTime.UtcNow }
            }
        };

        var (success, errors) = await service.UploadReadingsAsync(req);
        Assert.False(success);
        Assert.Contains(errors, e => e.Contains("Duplicate"));
    }

    [Fact]
    public async Task GetAggregatedReport_GroupsByMonth()
    {
        var repo = new Mock<IEnergyReadingRepository>();
        var priceRepo = new Mock<IProductPriceRepository>();
        priceRepo.Setup(p => p.GetAllPricesAsync()).ReturnsAsync(new List<ProductPrice>
        {
            new ProductPrice { Product = "electricity", PricePerKWh = 0.30 }
        });

        repo.Setup(r => r.GetReadingsAsync("u1", null)).ReturnsAsync(new List<EnergyReading>
        {
            new EnergyReading { UserId = "u1", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 1, 1) },
            new EnergyReading { UserId = "u1", Product = "electricity", KWh = 20, Timestamp = new DateTime(2025, 1, 15) }
        });

        var service = new EnergyService(repo.Object, priceRepo.Object);
        var report = await service.GetAggregatedReportAsync("u1", "month", "all");
        Assert.Single(report);
        Assert.Equal(30, report[0].TotalKWh);
        Assert.Equal(9, report[0].TotalCost);
    }

    [Fact]
    public async Task GetAnomalies_FlagsPeriodsAboveRollingAverage()
    {
        var repo = new Mock<IEnergyReadingRepository>();
        var priceRepo = new Mock<IProductPriceRepository>();
        priceRepo.Setup(p => p.GetAllPricesAsync()).ReturnsAsync(new List<ProductPrice>
        {
            new ProductPrice { Product = "electricity", PricePerKWh = 0.30 }
        });

        repo.Setup(r => r.GetReadingsAsync("u1", null)).ReturnsAsync(new List<EnergyReading>
        {
            new EnergyReading { UserId = "u1", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 1, 1) },
            new EnergyReading { UserId = "u1", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 2, 1) },
            new EnergyReading { UserId = "u1", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 3, 1) },
            new EnergyReading { UserId = "u1", Product = "electricity", KWh = 20, Timestamp = new DateTime(2025, 4, 1) }
        });

        var service = new EnergyService(repo.Object, priceRepo.Object);
        var anomalies = await service.GetAnomaliesAsync("u1", "month");
        Assert.Single(anomalies);
        Assert.Equal("2025-04", anomalies[0].Period);
        Assert.True(anomalies[0].PercentAboveAverage > 50);
    }
}
