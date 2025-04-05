using Azure;
using Azure.Storage.Blobs.Models;

namespace WEATHER.API.Services.Contracts
{
    public interface IWeatherRequestPayloadService
    {
        Task<Response<BlobContentInfo>> CreateAsync(string blobName, string data);
        
        Task<Stream> GetAsync(string blobName);
    }
}