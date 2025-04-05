using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WEATHER.API.Services.Contracts;

namespace WEATHER.API
{
    public class ScheduledWeatherFetcherFunction
    {
        //Could be named without undescore. Depends on code style guidelines in concrete project/team.
        private readonly ILogger<ScheduledWeatherFetcherFunction> _logger;
        private readonly IWeatherFetcherService weatherFetcherService;

        public ScheduledWeatherFetcherFunction(ILogger<ScheduledWeatherFetcherFunction> logger,
            IWeatherFetcherService weatherFetcherService)
        {
            _logger = logger;   //this.logger = logger;
            this.weatherFetcherService = weatherFetcherService;
        }

        [Function("ScheduledWeatherFetcherFunction")]
        public async Task Run([TimerTrigger("%WeatherFunctionCronTimer%")] TimerInfo timerDto)
        {
            var currentDateTime = DateTime.UtcNow;
            _logger.LogInformation($"C# Timer trigger function executed at: {currentDateTime}");

            var city = Environment.GetEnvironmentVariable("OpenWeatherMapAPIEndpointQParameter");
            if (string.IsNullOrEmpty(city))
            {
                //could be in constants
                //jsut an option how to handle this
                string errorMessage = "The 'OpenWeatherMapAPIEndpointQParameter' environment variable is not set or is empty.";
                _logger.LogError(errorMessage);
                throw new ArgumentNullException("OpenWeatherMapAPIEndpointQParameter", errorMessage);
            }

            await weatherFetcherService.CollectAsync(city, currentDateTime);
        }
    }
}
