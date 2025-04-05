using Azure.Data.Tables;
using WEATHER.API.Models;

namespace WEATHER.API.Services.Contracts
{
    /// <summary>
    /// Service for WeatherRequest management in Azure Table Storage.
    /// </summary>
    public interface IWeatherService
    {
        Task CreateAsync(string partitionName, string rowKey, string status, string payloadReference);
        //Could be search Model object in params
        Task<IEnumerable<TableEntity>> GetAsync(DateTime? from, DateTime? to);
        Task<WeatherRequest> GetAsync(string partitionKey, string rowKey);
    }
}