using Moq;
using EnergyTracker.Services;
using EnergyTracker.DataAccess.Entities;
using EnergyTracker.DataAccess.Interfaces;
using EnergyTracker.Domains;

namespace EnergyTracker.UnitTests
{
    [TestClass]
    public class EnergyServiceTests
    {
        private readonly Mock<IEnergyReadingRepository> _mockEnergyReadingRepo;
        private readonly Mock<IProductPriceRepository> _mockProductPricingRepo;

        private readonly EnergyService _sut;

        public EnergyServiceTests()
        {
            _mockEnergyReadingRepo = new Mock<IEnergyReadingRepository>();
            _mockProductPricingRepo = new Mock<IProductPriceRepository>();

            _sut = new EnergyService(_mockEnergyReadingRepo.Object, _mockProductPricingRepo.Object);
        }

        [TestMethod]
        public async Task GivenUploadReadings_WhenNegativeReadingProvided_ThenRejects()
        {
            // Arrange
            var req = new UploadReadingsRequest
            {
                UserId = "u1",
                Readings = new List<ReadingDto>
                {
                    new ReadingDto { Product = "electricity", KWh = -1, Timestamp = DateTime.UtcNow }
                }
            };

            // Act
            var (success, errors) = await _sut.UploadReadingsAsync(req);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(errors.Any(e => e.Contains("Negative kWh")));
        }

        [TestMethod]
        public async Task GivenUploadReadings_WhenDuplicateReadingProvided_ThenRejects()
        {
            // Arrange
            _mockEnergyReadingRepo.Setup(r => r.ExistsAsync("u1", "electricity", It.IsAny<DateTime>())).ReturnsAsync(true);
            var sut = new EnergyService(_mockEnergyReadingRepo.Object, _mockProductPricingRepo.Object);

            var req = new UploadReadingsRequest
            {
                UserId = "u1",
                Readings = new List<ReadingDto>
                {
                    new ReadingDto { Product = "electricity", KWh = 1, Timestamp = DateTime.UtcNow }
                }
            };

            // Act
            var (success, errors) = await sut.UploadReadingsAsync(req);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(errors.Any(e => e.Contains("Duplicate")));
        }

        [TestMethod]
        public async Task GivenRequestAggregatedReport_WhenPeriodProvidesAsMonth_ThenGroupByReport()
        {
            // Arrange
            _mockProductPricingRepo.Setup(p => p.GetAllPricesAsync()).ReturnsAsync(new List<ProductPrice>
            {
                new ProductPrice { Product = "electricity", PricePerKWh = 0.30 }
            });
            _mockEnergyReadingRepo.Setup(r => r.GetReadingsAsync("u1", null)).ReturnsAsync(new List<EnergyReading>
            {
                new EnergyReading { UserId = "u1", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 1, 1) },
                new EnergyReading { UserId = "u1", Product = "electricity", KWh = 20, Timestamp = new DateTime(2025, 1, 15) }
            });
            var sut = new EnergyService(_mockEnergyReadingRepo.Object, _mockProductPricingRepo.Object);

            // Act
            var report = await sut.GetAggregatedReportAsync("u1", "month", "all");

            // Assert
            Assert.AreEqual(1, report.Count);
            Assert.AreEqual(30, report[0].TotalKWh);
            Assert.AreEqual(9, report[0].TotalCost);
        }

        [TestMethod]
        public async Task GivenRequestsAnomalies_WhenPeriodIsProvided_ThenFlagPeriodsAboveRollingAverage()
        {
            // Arrange
            _mockProductPricingRepo.Setup(p => p.GetAllPricesAsync()).ReturnsAsync(new List<ProductPrice>
            {
                new ProductPrice { Product = "electricity", PricePerKWh = 0.30 }
            });
            _mockEnergyReadingRepo.Setup(r => r.GetReadingsAsync("u1", null)).ReturnsAsync(new List<EnergyReading>
            {
                new EnergyReading { UserId = "u1", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 1, 1) },
                new EnergyReading { UserId = "u1", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 2, 1) },
                new EnergyReading { UserId = "u1", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 3, 1) },
                new EnergyReading { UserId = "u1", Product = "electricity", KWh = 20, Timestamp = new DateTime(2025, 4, 1) }
            });
            var sut = new EnergyService(_mockEnergyReadingRepo.Object, _mockProductPricingRepo.Object);

            // Act
            var anomalies = await sut.GetAnomaliesAsync("u1", "month");

            // Assert
            Assert.AreEqual(1, anomalies.Count);
            Assert.AreEqual("2025-04", anomalies[0].Period);
            Assert.IsTrue(anomalies[0].PercentAboveAverage > 50);
        }
    }
}
