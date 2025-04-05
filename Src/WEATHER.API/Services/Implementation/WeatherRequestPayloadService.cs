using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using WEATHER.API.Services.Contracts;

namespace WEATHER.API.Services.Implementation
{
    public class WeatherRequestPayloadService : IWeatherRequestPayloadService
    {
        private readonly ILogger<ScheduledWeatherFetcherFunction> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string BlobContainerName;

        public WeatherRequestPayloadService(ILogger<ScheduledWeatherFetcherFunction> logger,
            BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            _blobServiceClient = blobServiceClient;
            BlobContainerName = Environment.GetEnvironmentVariable("WeatherBlobContainerName");
        }

        public async Task<Response<BlobContentInfo>> CreateAsync(string blobName, string data)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(BlobContainerName);
            await containerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            BlobUploadOptions blobUploadOptions = new()
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "application/json"
                }
            };
            return await blobClient.UploadAsync(new BinaryData(data), blobUploadOptions);
        }

        public async Task<Stream> GetAsync(string blobName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(BlobContainerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
