using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WEATHER.API.Services.Contracts;

namespace WEATHER.API.Functions.V1
{
    public class GetWeatherRequestPayloadFunction
    {
        private readonly ILogger<GetWeatherRequestPayloadFunction> _logger;
        private readonly IWeatherService _weatherService;
        private readonly IWeatherRequestPayloadService _weatherRequestPayloadService;


        public GetWeatherRequestPayloadFunction(ILogger<GetWeatherRequestPayloadFunction> logger, IWeatherService weatherService,
            IWeatherRequestPayloadService weatherRequestPayloadService)
        {
            _logger = logger;
            _weatherService = weatherService;
            _weatherRequestPayloadService = weatherRequestPayloadService;
        }

        [Function("GetWeatherRequestPayloadFunction")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "GET",
            Route = "v1/WeatherRequests/{logEntryID}/Payload")] HttpRequest req, string logEntryID)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            //validation should be separated 
            if (string.IsNullOrEmpty(logEntryID))
            {
                return new BadRequestObjectResult("Missing Log Entry ID.");
            }

            var keys = logEntryID.Split('_');
            if (keys.Length != 2)
            {
                return new BadRequestObjectResult("Invalid Log Entry ID format.");
            }

            string partitionKey = keys[0];
            string rowKey = keys[1];

            //TODO: move 2 calls into separate service
            var weatherRequest = await _weatherService.GetAsync(partitionKey, rowKey);
            if (weatherRequest is null)
            {
                return new NotFoundObjectResult("Weather request item not found.");
            }

            try
            {
                var blobStream = await _weatherRequestPayloadService.GetAsync(weatherRequest.PayloadReference);

                return new FileStreamResult(blobStream, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving blob content: {ex.Message}");
                return new NotFoundObjectResult("Blob not found or an error occurred.");
            }
        }
    }
}
