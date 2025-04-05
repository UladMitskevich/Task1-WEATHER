using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WEATHER.API.Services.Contracts;
using WEATHER.API.Services.Implementation;

namespace WEATHER.API.Functions.V1
{
    public class GetWeatherRequestFunction
    {
        private readonly ILogger<GetWeatherRequestFunction> _logger;
        private readonly IWeatherService _weatherService;

        public GetWeatherRequestFunction(ILogger<GetWeatherRequestFunction> logger, IWeatherService weatherService)
        {
            _logger = logger;
            _weatherService = weatherService;
        }

        [Function("GetWeatherRequestFunction")]

        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "GET",Route = "v1/WeatherRequests")] HttpRequest httpRequest)
        {
            _logger.LogInformation("C# HTTP trigger function processed a req.");

            var fromParam = httpRequest.Query["from"];
            var toParam = httpRequest.Query["to"];

            //should be done with validators. FluentValidator etc.
            //could be more optimized. Coz now validation mixed with parsing
            DateTime? from = null;
            DateTime? to = null;

            if (!string.IsNullOrEmpty(fromParam))
            {
                if (!DateTime. TryParse(fromParam, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime fromDate))
                {
                    return new BadRequestObjectResult("Invalid 'from' date. Please provide a date in a valid format (e.g., '2025-04-05T14:00:00Z').");
                }
                from = fromDate;
            }

            if (!string.IsNullOrEmpty(toParam))
            {
                if (!DateTime.TryParse(toParam, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime toDate))
                {
                    return new BadRequestObjectResult("Invalid 'to' date. Please provide a date in a valid format (e.g., '2025-04-05T14:00:00Z').");
                }
                to = toDate;
            }

            if (from.HasValue && to.HasValue && from > to)
            {
                return new BadRequestObjectResult("The 'from' date must be earlier than the 'to' date.");
            }

            var weatherData = await _weatherService.GetAsync(from, to);
            //todo:mapping to dto

            return new OkObjectResult(weatherData);

        }
    }
}
