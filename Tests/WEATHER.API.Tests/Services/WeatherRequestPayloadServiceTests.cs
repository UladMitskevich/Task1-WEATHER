using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Moq;
using WEATHER.API.Services.Implementation;

namespace WEATHER.API.Tests.Services
{
    /// <summary>
    /// Sample tests with a lot of points to improve and refactor
    /// </summary>
    public class WeatherRequestPayloadServiceTests
    {
        private readonly Mock<ILogger<ScheduledWeatherFetcherFunction>> _loggerMock;
        private readonly Mock<BlobServiceClient> _blobServiceClientMock;
        private readonly Mock<BlobContainerClient> _blobContainerClientMock;
        private readonly Mock<BlobClient> _blobClientMock;
        private readonly WeatherRequestPayloadService _weatherRequestPayloadService;

        public WeatherRequestPayloadServiceTests()
        {
            _loggerMock = new Mock<ILogger<ScheduledWeatherFetcherFunction>>();
            _blobServiceClientMock = new Mock<BlobServiceClient>();
            _blobContainerClientMock = new Mock<BlobContainerClient>();
            _blobClientMock = new Mock<BlobClient>();

            _blobServiceClientMock.Setup(b => b.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(_blobContainerClientMock.Object);

            _blobContainerClientMock.Setup(b => b.GetBlobClient(It.IsAny<string>()))
                .Returns(_blobClientMock.Object);

            _weatherRequestPayloadService = new WeatherRequestPayloadService(_loggerMock.Object, _blobServiceClientMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldUploadBlob()
        {
            // Arrange
            var blobName = "test-blob";
            var data = "{\"key\":\"value\"}";
            var blobContentInfo = BlobsModelFactory.BlobContentInfo(new ETag("etag"), DateTimeOffset.UtcNow, null, null, null, null, 0);

            _blobContainerClientMock.Setup(b => b.CreateIfNotExistsAsync(It.IsAny<PublicAccessType>(), It.IsAny<IDictionary<string, string>>(), null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(Response.FromValue(BlobsModelFactory.BlobContainerInfo(new ETag("etag"), DateTimeOffset.UtcNow), new Mock<Response>().Object));

            _blobClientMock.Setup(b => b.UploadAsync(It.IsAny<BinaryData>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(blobContentInfo, new Mock<Response>().Object));

            // Act
            var result = await _weatherRequestPayloadService.CreateAsync(blobName, data);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(blobContentInfo, result.Value);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnBlobStream()
        {
            // Arrange
            var blobName = "test-blob";
            var expectedStream = new MemoryStream();
            var blobDownloadInfo = BlobsModelFactory.BlobDownloadInfo(content: expectedStream);

            _blobClientMock.Setup(b => b.DownloadToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<Response>().Object);

            // Act
            var result = await _weatherRequestPayloadService.GetAsync(blobName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedStream.ToArray(), ((MemoryStream)result).ToArray());
        }
    }
}