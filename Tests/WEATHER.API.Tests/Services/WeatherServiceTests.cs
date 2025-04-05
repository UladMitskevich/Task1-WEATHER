using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Moq;
using WEATHER.API.Services.Implementation;

namespace WEATHER.API.Tests.Services
{
    /// <summary>
    /// Sample tests with a lot of points to improve and refactor
    /// </summary>
    public class WeatherServiceTests
    {
        private readonly Mock<ILogger<WeatherService>> _loggerMock;
        private readonly Mock<TableClient> _tableClientMock;
        private readonly WeatherService _weatherService;

        public WeatherServiceTests()
        {
            _loggerMock = new Mock<ILogger<WeatherService>>();
            _tableClientMock = new Mock<TableClient>();
            _weatherService = new WeatherService(_loggerMock.Object, _tableClientMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddEntity()
        {
            // Arrange
            var partitionName = "partition";
            var rowKey = "row";
            var status = "status";
            var payloadReference = "payload";

            // Act
            await _weatherService.CreateAsync(partitionName, rowKey, status, payloadReference);
            // Assert
            //COuld be tested better
            _tableClientMock.Verify(tc => tc.AddEntityAsync(It.Is<TableEntity>(te =>
                te.PartitionKey == partitionName &&
                te.RowKey == rowKey &&
                te["Status"].ToString() == status &&
                te["PayloadReference"].ToString() == payloadReference &&
                te["Timestamp"] != null
            ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnEntitiesWithinDateRange()
        {
            // Arrange
            var from = DateTime.UtcNow.AddDays(-1);
            var to = DateTime.UtcNow;
            var entities = new List<TableEntity>
            {
                new TableEntity("partition", "row1"),
                new TableEntity("partition", "row2")
            };
            var page = Page<TableEntity>.FromValues(entities, continuationToken: null, new Mock<Response>().Object);
            var pages = AsyncPageable<TableEntity>.FromPages(new[] { page });

            _tableClientMock.Setup(tc => tc.QueryAsync<TableEntity>(It.IsAny<string>(), null, null, default))
                .Returns(pages);
            
            // Act
            var result = await _weatherService.GetAsync(from, to);

            // Assert
            Assert.Equal(entities, result);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnWeatherRequest()
        {
            // Arrange
            var partitionKey = "partition";
            var rowKey = "row";
            var entity = new TableEntity(partitionKey, rowKey)
            {
                { "Status", "status" },
                { "PayloadReference", "payload" }
            };

            _tableClientMock.Setup(tc => tc.GetEntityAsync<TableEntity>(partitionKey, rowKey, It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(entity, new Mock<Response>().Object));

            // Act
            var result = await _weatherService.GetAsync(partitionKey, rowKey);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(partitionKey, result.PartitionKey);
            Assert.Equal(rowKey, result.RowKey);
            Assert.Equal("status", result.Status);
            Assert.Equal("payload", result.PayloadReference);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnNullIfEntityNotFound()
        {
            // Arrange
            var partitionKey = "partition";
            var rowKey = "row";
            _tableClientMock
                .Setup(tc => tc.GetEntityAsync<TableEntity>(partitionKey, rowKey,
                    It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(404, "Not Found"));

            // Act
            var result = await _weatherService.GetAsync(partitionKey, rowKey);

            // Assert
            Assert.Null(result);

        }
    }

}
