namespace WEATHER.API.Services.Contracts
{
    /// <summary>
    /// Service for Scheduled function to collect weather
    /// </summary>
    public interface IWeatherFetcherService
    {
        Task CollectAsync(string city, DateTime triggeredDateTime);
    }
}