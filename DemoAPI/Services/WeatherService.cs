using AggregatorAPI.Models;
using AggregatorAPI.Models.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace AggregatorAPI.Services;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiSettings _settings;
    public WeatherService(HttpClient httpClient, IOptions<WeatherApiSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<WeatherInfo> GetCurrentWeatherAsync(string city)
    {
        try
        {
            //Set default value in case no input from user 
            if (string.IsNullOrEmpty(city)) city = "Greece";
            var requestUrl = $"{_settings.BaseUrl}?q={city}&appid={_settings.ApiKey}&units=metric";
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var weatherData = JObject.Parse(content);

            return weatherData != null ? new WeatherInfo
            {
                City = weatherData["name"]?.ToString(),
                Temperature = (double?)weatherData["main"]?["temp"] ?? 0.0,
                WeatherDescription = weatherData["weather"]?[0]?["description"]?.ToString()
            }
            : new WeatherInfo();
        }
        catch (Exception ex) 
        {
            // add exception !!
            return null;
        }
    }
}
