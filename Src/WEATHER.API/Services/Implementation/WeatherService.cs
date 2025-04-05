using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using WEATHER.API.Models;
using WEATHER.API.Services.Contracts;

namespace WEATHER.API.Services.Implementation
{
    public class WeatherService : IWeatherService
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly TableClient _tableClient;

        public WeatherService(ILogger<WeatherService> logger, TableClient tableClient)
        {
            _logger = logger;
            _tableClient = tableClient;
        }

        public async Task CreateAsync(string partitionName, string rowKey, string status, string payloadReference)
        {
            //could be arguments ckecks here
            var weatherEntity = new TableEntity(partitionName, rowKey)
            {
                { "Status", status },
                { "PayloadReference", payloadReference },/*blobClient.Uri.AbsolutePath*/
                { "Timestamp", DateTime.UtcNow }
            };

            await _tableClient.AddEntityAsync(weatherEntity);
        }

        public async Task<IEnumerable<TableEntity>> GetAsync(DateTime? from, DateTime? to)
        {
            // Dynamically build the query filter based on 'from' and 'to'
            // Of course we should implement BUILDER here but for simplicity we use just string concatenation
            var filters = new List<string>();

            if (from.HasValue)
            {
                filters.Add($"RowKey ge '{from.Value:yyyy-MM-ddTHH:mm:ssZ}'");
            }

            if (to.HasValue)
            {
                filters.Add($"RowKey le '{to.Value:yyyy-MM-ddTHH:mm:ssZ}'");
            }

            string filter = filters.Count > 0 ? string.Join(" and ", filters) : null;

            var query = _tableClient.QueryAsync<TableEntity>(filter);

            var results = new List<TableEntity>();
            await foreach (var entity in query)
            {
                results.Add(entity);
            }
            //tableEntity should be mapper to concrete contract model. Now omitted
            //here we return business models
            //should be paginated for more complex cases
            return results;

        }

        public async Task<WeatherRequest> GetAsync(string partitionKey, string rowKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey)) throw new ArgumentException("PartitionKey cannot be null or empty.", nameof(partitionKey));
            if (string.IsNullOrWhiteSpace(rowKey)) throw new ArgumentException("RowKey cannot be null or empty.", nameof(rowKey));
            try
            {
                Response<TableEntity> response = await _tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);

                TableEntity entity = response.Value;

                var model = new WeatherRequest
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    Status = entity.TryGetValue("Status", out var status)
                        ? status?.ToString()
                        : null,
                    PayloadReference = entity.TryGetValue("PayloadReference", out var payloadReference)
                        ? payloadReference?.ToString()
                        : null
                };
                return model;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogError("Entity not found.");
                return null;
            }
        }
    }
}
