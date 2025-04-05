namespace WEATHER.API.Models
{
    public class WeatherRequest
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Status { get; set; }
        public string PayloadReference { get; set; }
        public string Timestamp { get; set; }

    }
}
