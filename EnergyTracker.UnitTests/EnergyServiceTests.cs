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

        [TestMethod]
        public async Task GivenUploadReadings_WhenReadingValid_ThenAcceptReadings()
        {
            // Arrange
            var sut = new EnergyService(_mockEnergyReadingRepo.Object, _mockProductPricingRepo.Object);

            var req = new UploadReadingsRequest
            {
                UserId = "u2",
                Readings = new List<ReadingDto>
                {
                    new ReadingDto { Product = "gas", KWh = 5, Timestamp = DateTime.UtcNow }
                }
            };

            _mockEnergyReadingRepo.Setup(r => r.ExistsAsync("u2", "gas", It.IsAny<DateTime>())).ReturnsAsync(false);
            _mockEnergyReadingRepo.Setup(r => r.AddReadingsAsync(It.IsAny<IEnumerable<EnergyReading>>())).Returns(Task.CompletedTask);

            // Act
            var (success, errors) = await sut.UploadReadingsAsync(req);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public async Task GivenUploadReadings_WhenBatchSizeExceed_ThenRejectBatch()
        {
            // Arrange
            var sut = new EnergyService(_mockEnergyReadingRepo.Object, _mockProductPricingRepo.Object);

            var req = new UploadReadingsRequest
            {
                UserId = "u3",
                Readings = Enumerable.Range(0, 10001).Select(i => new ReadingDto { Product = "electricity", KWh = 1, Timestamp = DateTime.UtcNow }).ToList()
            };

            // Act
            var (success, errors) = await sut.UploadReadingsAsync(req);

            // Assert
            Assert.IsFalse(success);
            Assert.IsTrue(errors.Any(e => e.Contains("Batch size exceeds")));
        }

        [TestMethod]
        public async Task GivenUploadReadings_WhenReadingsEmpty_ThenReturnValidationMessage()
        {
            var repo = new Mock<IEnergyReadingRepository>();
            var priceRepo = new Mock<IProductPriceRepository>();
            var service = new EnergyService(repo.Object, priceRepo.Object);

            var req = new UploadReadingsRequest { UserId = "u4", Readings = new List<ReadingDto>() };
            var (success, errors) = await service.UploadReadingsAsync(req);
            Assert.IsFalse(success);
            Assert.AreEqual(1, errors.Count);
        }

        [TestMethod]
        public async Task GivenRequestAggregatedReport_WhenPeriodProvidedAsDay_ThenGroupsByDay()
        {
            var repo = new Mock<IEnergyReadingRepository>();
            var priceRepo = new Mock<IProductPriceRepository>();
            priceRepo.Setup(p => p.GetAllPricesAsync()).ReturnsAsync(new List<ProductPrice>
            {
                new ProductPrice { Product = "gas", PricePerKWh = 0.10 }
            });
            repo.Setup(r => r.GetReadingsAsync("u5", null)).ReturnsAsync(new List<EnergyReading>
            {
                new EnergyReading { UserId = "u5", Product = "gas", KWh = 2, Timestamp = new DateTime(2025, 1, 1) },
                new EnergyReading { UserId = "u5", Product = "gas", KWh = 3, Timestamp = new DateTime(2025, 1, 1) }
            });
            var service = new EnergyService(repo.Object, priceRepo.Object);
            var report = await service.GetAggregatedReportAsync("u5", "day", "all");
            Assert.AreEqual(1, report.Count);
            Assert.AreEqual(5, report[0].TotalKWh);
            Assert.AreEqual(0.5, report[0].TotalCost);
        }

        [TestMethod]
        public async Task GivenRequestAggregatedReport_WhenNoReadingsPresent_ThenReturnsEmpty()
        {
            var repo = new Mock<IEnergyReadingRepository>();
            var priceRepo = new Mock<IProductPriceRepository>();
            priceRepo.Setup(p => p.GetAllPricesAsync()).ReturnsAsync(new List<ProductPrice>());
            repo.Setup(r => r.GetReadingsAsync("u6", null)).ReturnsAsync(new List<EnergyReading>());
            var service = new EnergyService(repo.Object, priceRepo.Object);
            var report = await service.GetAggregatedReportAsync("u6", "month", "all");
            Assert.AreEqual(0, report.Count);
        }

        [TestMethod]
        public async Task GivenRequestsAnomalies_WhenNotEnoughPeriods_ThenReturnsEmpty()
        {
            var repo = new Mock<IEnergyReadingRepository>();
            var priceRepo = new Mock<IProductPriceRepository>();
            priceRepo.Setup(p => p.GetAllPricesAsync()).ReturnsAsync(new List<ProductPrice>());
            repo.Setup(r => r.GetReadingsAsync("u7", null)).ReturnsAsync(new List<EnergyReading>
            {
                new EnergyReading { UserId = "u7", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 1, 1) },
                new EnergyReading { UserId = "u7", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 2, 1) }
            });
            var service = new EnergyService(repo.Object, priceRepo.Object);
            var anomalies = await service.GetAnomaliesAsync("u7", "month");
            Assert.AreEqual(0, anomalies.Count);
        }

        [TestMethod]
        public async Task GivenRequestsAnomalies_WhenMultipleProductsPresent_ThenHandlesCorrectly()
        {
            var repo = new Mock<IEnergyReadingRepository>();
            var priceRepo = new Mock<IProductPriceRepository>();
            priceRepo.Setup(p => p.GetAllPricesAsync()).ReturnsAsync(new List<ProductPrice>());
            repo.Setup(r => r.GetReadingsAsync("u8", null)).ReturnsAsync(new List<EnergyReading>
            {
                new EnergyReading { UserId = "u8", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 1, 1) },
                new EnergyReading { UserId = "u8", Product = "gas", KWh = 10, Timestamp = new DateTime(2025, 1, 1) },
                new EnergyReading { UserId = "u8", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 2, 1) },
                new EnergyReading { UserId = "u8", Product = "gas", KWh = 10, Timestamp = new DateTime(2025, 2, 1) },
                new EnergyReading { UserId = "u8", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 3, 1) },
                new EnergyReading { UserId = "u8", Product = "gas", KWh = 10, Timestamp = new DateTime(2025, 3, 1) },
                new EnergyReading { UserId = "u8", Product = "electricity", KWh = 20, Timestamp = new DateTime(2025, 4, 1) },
                new EnergyReading { UserId = "u8", Product = "gas", KWh = 20, Timestamp = new DateTime(2025, 4, 1) }
            });
            var service = new EnergyService(repo.Object, priceRepo.Object);
            var anomalies = await service.GetAnomaliesAsync("u8", "month");
            Assert.AreEqual(2, anomalies.Count);
            Assert.IsTrue(anomalies.All(a => a.Period == "2025-04"));
        }

        [TestMethod]
        public async Task GivenRequestsAnomalies_WhenNoReadingsPresent_ThenReturnsEmpty()
        {
            var repo = new Mock<IEnergyReadingRepository>();
            var priceRepo = new Mock<IProductPriceRepository>();
            repo.Setup(r => r.GetReadingsAsync("user", null)).ReturnsAsync(new List<EnergyReading>());
            var service = new EnergyService(repo.Object, priceRepo.Object);
            var result = await service.GetAnomaliesAsync("user", "month");
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GivenRequestsAnomalies_WhenNoAnomolies_ThenReturnsEmpty()
        {
            var repo = new Mock<IEnergyReadingRepository>();
            var priceRepo = new Mock<IProductPriceRepository>();
            repo.Setup(r => r.GetReadingsAsync("user", null)).ReturnsAsync(new List<EnergyReading>
            {
                new EnergyReading { UserId = "user", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 1, 1) },
                new EnergyReading { UserId = "user", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 2, 1) },
                new EnergyReading { UserId = "user", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 3, 1) },
                new EnergyReading { UserId = "user", Product = "electricity", KWh = 12, Timestamp = new DateTime(2025, 4, 1) }
            });
            var service = new EnergyService(repo.Object, priceRepo.Object);
            var result = await service.GetAnomaliesAsync("user", "month");
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GivenRequestsAnomalies_WhenMultipleAnomolies_ThenDetectsAllAnomalies()
        {
            var repo = new Mock<IEnergyReadingRepository>();
            var priceRepo = new Mock<IProductPriceRepository>();
            repo.Setup(r => r.GetReadingsAsync("user", null)).ReturnsAsync(new List<EnergyReading>
            {
                new EnergyReading { UserId = "user", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 1, 1) },
                new EnergyReading { UserId = "user", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 2, 1) },
                new EnergyReading { UserId = "user", Product = "electricity", KWh = 10, Timestamp = new DateTime(2025, 3, 1) },
                new EnergyReading { UserId = "user", Product = "electricity", KWh = 30, Timestamp = new DateTime(2025, 4, 1) },
                new EnergyReading { UserId = "user", Product = "gas", KWh = 5, Timestamp = new DateTime(2025, 1, 1) },
                new EnergyReading { UserId = "user", Product = "gas", KWh = 5, Timestamp = new DateTime(2025, 2, 1) },
                new EnergyReading { UserId = "user", Product = "gas", KWh = 5, Timestamp = new DateTime(2025, 3, 1) },
                new EnergyReading { UserId = "user", Product = "gas", KWh = 20, Timestamp = new DateTime(2025, 4, 1) }
            });
            var service = new EnergyService(repo.Object, priceRepo.Object);
            var result = await service.GetAnomaliesAsync("user", "month");
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(a => a.Product == "electricity"));
            Assert.IsTrue(result.Any(a => a.Product == "gas"));
        }

        [TestMethod]
        public async Task GivenRequestsAnomalies_WhenRollingAverageZero_ThenDoesNotFlag()
        {
            var repo = new Mock<IEnergyReadingRepository>();
            var priceRepo = new Mock<IProductPriceRepository>();
            repo.Setup(r => r.GetReadingsAsync("user", null)).ReturnsAsync(new List<EnergyReading>
            {
                new EnergyReading { UserId = "user", Product = "electricity", KWh = 0, Timestamp = new DateTime(2025, 1, 1) },
                new EnergyReading { UserId = "user", Product = "electricity", KWh = 0, Timestamp = new DateTime(2025, 2, 1) },
                new EnergyReading { UserId = "user", Product = "electricity", KWh = 0, Timestamp = new DateTime(2025, 3, 1) },
                new EnergyReading { UserId = "user", Product = "electricity", KWh = 100, Timestamp = new DateTime(2025, 4, 1) }
            });
            var service = new EnergyService(repo.Object, priceRepo.Object);
            var result = await service.GetAnomaliesAsync("user", "month");
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GivenRequestsAnomalies_WhenPeriodIsInvalid_ThenThrowsInvalidPeriodError()
        {
            var repo = new Mock<IEnergyReadingRepository>();
            var priceRepo = new Mock<IProductPriceRepository>();
            repo.Setup(r => r.GetReadingsAsync("user", null)).ReturnsAsync(new List<EnergyReading>());
            var service = new EnergyService(repo.Object, priceRepo.Object);
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await service.GetAnomaliesAsync("user", "invalidperiod");
            });
        }
    }
}
