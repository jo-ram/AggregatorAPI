using AggregatorAPI.Interfaces;
using AggregatorAPI.Models;
using AggregatorAPI.Models.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace AggregatorAPI.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiSettings _settings;
    private readonly IMemoryCacheService _memoryCacheService;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IStatisticsService _statisticsService;

    public WeatherService(HttpClient httpClient, 
        IOptions<WeatherApiSettings> options, 
        IMemoryCacheService memoryCacheService,
        IRetryPolicy retryPolicy,
        IStatisticsService statistics)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _memoryCacheService = memoryCacheService;
        _retryPolicy = retryPolicy;
        _statisticsService = statistics;
    }

    public async Task<WeatherInfo> GetCurrentWeatherAsync(string city)
    {
        try
        {
            //Set default value in case no input from user 
            if (string.IsNullOrEmpty(city)) city = "Greece";

            var cacheKey = $"Weather_{city.ToLower()}";

            //Retrive from cache if key exists 
            var cachedWeather = _memoryCacheService.Retrieve<WeatherInfo>(cacheKey);
            if (cachedWeather != null)
                return cachedWeather;

            var requestUrl = $"{_settings.BaseUrl}?q={city}&appid={_settings.ApiKey}&units=metric";
            var stopwatch = Stopwatch.StartNew();
            HttpResponseMessage result = await _retryPolicy.RetryHttpRequestStandardAsync(requestUrl, async () => await _httpClient.GetAsync(requestUrl));
            //var response = await _httpClient.GetAsync(requestUrl);
            stopwatch.Stop();
            _statisticsService.LogRequest("WeatherService", stopwatch.ElapsedMilliseconds);

            result.EnsureSuccessStatusCode();

            var content = await result.Content.ReadAsStringAsync();
            var weatherData = JObject.Parse(content);

            if(weatherData != null)
            {
                var weatherResult = new WeatherInfo
                {
                    City = weatherData["name"]?.ToString(),
                    Temperature = (double?)weatherData["main"]?["temp"] ?? 0.0,
                    WeatherDescription = weatherData["weather"]?[0]?["description"]?.ToString()
                };
                _memoryCacheService.Add(cacheKey, weatherResult);
                return weatherResult;
            }

            return new WeatherInfo();
        }
        catch (Exception ex)
        {
            return new WeatherInfo();
        }
    }
}
