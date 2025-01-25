using AggregatorAPI.Interfaces;
using AggregatorAPI.Models;
using AggregatorAPI.Models.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace AggregatorAPI.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiSettings _settings;
    private readonly IMemoryCacheService _memoryCacheService;
    public WeatherService(HttpClient httpClient, 
        IOptions<WeatherApiSettings> options, 
        IMemoryCacheService memoryCacheService)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _memoryCacheService = memoryCacheService;
    }

    public async Task<WeatherInfo> GetCurrentWeatherAsync(string city)
    {
        try
        {
            //Set default value in case no input from user 
            if (string.IsNullOrEmpty(city)) city = "Greece";

            var cacheKey = $"Weather_{city}";

            //Retrive from cache if key exists 
            var cachedWeather = _memoryCacheService.Retrieve<WeatherInfo>(cacheKey);
            if (cachedWeather != null)
                return cachedWeather;

            var requestUrl = $"{_settings.BaseUrl}?q={city}&appid={_settings.ApiKey}&units=metric";
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var weatherData = JObject.Parse(content);

            _memoryCacheService.Add(cacheKey, weatherData);

            return weatherData is null ? new WeatherInfo() : new WeatherInfo
            {
                City = weatherData["name"]?.ToString(),
                Temperature = (double?)weatherData["main"]?["temp"] ?? 0.0,
                WeatherDescription = weatherData["weather"]?[0]?["description"]?.ToString()
            };
        }
        catch (Exception ex)
        {
            // add exception !!
            return null;
        }
    }
}
