using System.Text.Json.Serialization;

namespace AggregatorAPI.Models;

public class WeatherInfo
{
    [JsonPropertyName("name")]
    public string City { get; set; }

    [JsonPropertyName("temp")]
    public double Temperature { get; set; }

    [JsonPropertyName("description")]
    public string WeatherDescription { get; set; }
}
