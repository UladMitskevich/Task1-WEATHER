using Microsoft.Extensions.Logging;
using WEATHER.API.Services.Contracts;

namespace WEATHER.API.Services.Implementation
{
    public class WeatherFetcherService : IWeatherFetcherService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ScheduledWeatherFetcherFunction> _logger;
        private readonly IWeatherService _weatherService;
        private readonly IWeatherRequestPayloadService _weatherRequestPayloadService;

        public WeatherFetcherService(ILogger<ScheduledWeatherFetcherFunction> logger, HttpClient httpClient,
            IWeatherService weatherService,
            IWeatherRequestPayloadService weatherRequestPayloadService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _weatherService = weatherService;
            _weatherRequestPayloadService = weatherRequestPayloadService;
        }

        public async Task CollectAsync(string city, DateTime triggeredDateTime)
        {
            var endpoint = Environment.GetEnvironmentVariable("OpenWeatherMapAPIEndpointWeather");
            var apiKey = Environment.GetEnvironmentVariable("OpenWeatherMapAPIKey");
            var apiUrl = $"{endpoint}?q={city}&appid={apiKey}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                var responseStatus = response.IsSuccessStatusCode ? "Success" : "Failure";
                var responseData = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Received data: {responseData}");

                var blobName = $"{city}/{triggeredDateTime:yyyy-MM-dd}/{triggeredDateTime:HH:mm:ss}.json";

                await _weatherRequestPayloadService.CreateAsync(blobName, responseData);
                await _weatherService.CreateAsync(city, $"{triggeredDateTime:yyyy-MM-ddTHH:mm:ssZ}", responseStatus, blobName);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error making HTTP call: {ex.Message}");
            }
        }
    }
}
