using AggregatorAPI.Interfaces;
using AggregatorAPI.Models;
using AggregatorAPI.Models.Settings;
using AggregatorAPI.Services;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Text.Json;

public class WeatherServiceTests
{
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly Mock<IOptions<WeatherApiSettings>> _optionsMock;
    private readonly Mock<IMemoryCacheService> _memoryCacheServiceMock;
    private readonly Mock<IRetryPolicy> _retryPolicyMock;
    private readonly Mock<IStatisticsService> _statisticsServiceMock;
    private readonly WeatherService _weatherService;

    public WeatherServiceTests()
    {
        _httpClientMock = new Mock<HttpClient>();
        _optionsMock = new Mock<IOptions<WeatherApiSettings>>();
        _memoryCacheServiceMock = new Mock<IMemoryCacheService>();
        _retryPolicyMock = new Mock<IRetryPolicy>();
        _statisticsServiceMock = new Mock<IStatisticsService>();

        var settings = new WeatherApiSettings
        {
            BaseUrl = "https://api.openweathermap.org/data/2.5/weather",
            ApiKey = "0b5dc4dddc65b958101f72fe70d5e0a3"
        };
        _optionsMock.Setup(o => o.Value).Returns(settings);

        var httpClient = new HttpClient();
        _weatherService = new WeatherService(httpClient, _optionsMock.Object, _memoryCacheServiceMock.Object, _retryPolicyMock.Object, _statisticsServiceMock.Object);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ShouldReturnFromCache_WhenCacheExists()
    {
        var city = "London";
        var cacheKey = $"Weather_{city.ToLower()}";
        var cachedWeather = new WeatherInfo
        {
            City = city,
            Temperature = 15.5,
            WeatherDescription = "Sunny"
        };

        _memoryCacheServiceMock.Setup(m => m.Retrieve<WeatherInfo>(cacheKey)).Returns(cachedWeather);

        var result = await _weatherService.GetCurrentWeatherAsync(city);

        // Assert
        Assert.Equal(cachedWeather.City, result.City);
        Assert.Equal(cachedWeather.Temperature, result.Temperature);
        Assert.Equal(cachedWeather.WeatherDescription, result.WeatherDescription);
        _memoryCacheServiceMock.Verify(m => m.Retrieve<WeatherInfo>(cacheKey), Times.Once);
        _statisticsServiceMock.Verify(s => s.LogRequest(It.IsAny<string>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ShouldFetchFromApi_WhenCacheDoesNotExist()
    {
        var city = "London";
        var cacheKey = $"Weather_{city.ToLower()}";
        _memoryCacheServiceMock.Setup(m => m.Retrieve<WeatherInfo>(cacheKey)).Returns((WeatherInfo)null);

        var apiResponse = new
        {
            name = city,
            main = new { temp = 15.5 },
            weather = new[] { new { description = "Sunny" } }
        };
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(apiResponse))
        };

        _retryPolicyMock.Setup(r => r.RetryHttpRequestStandardAsync(It.IsAny<string>(), It.IsAny<Func<Task<HttpResponseMessage>>>()))
            .ReturnsAsync(responseMessage);

        var result = await _weatherService.GetCurrentWeatherAsync(city);

        Assert.Equal(city, result.City);
        Assert.Equal(15.5, result.Temperature);
        Assert.Equal("Sunny", result.WeatherDescription);
        _memoryCacheServiceMock.Verify(m => m.Add(cacheKey, It.IsAny<WeatherInfo>()), Times.Once);
        _statisticsServiceMock.Verify(s => s.LogRequest("WeatherService", It.IsAny<long>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ShouldLogStatistics_WhenApiIsCalled()
    {
        var city = "Paris";
        var cacheKey = $"Weather_{city.ToLower()}";
        _memoryCacheServiceMock.Setup(m => m.Retrieve<WeatherInfo>(cacheKey)).Returns((WeatherInfo)null);

        var apiResponse = new
        {
            name = city,
            main = new { temp = 10.2 },
            weather = new[] { new { description = "Cloudy" } }
        };
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(apiResponse))
        };

        _retryPolicyMock.Setup(r => r.RetryHttpRequestStandardAsync(It.IsAny<string>(), It.IsAny<Func<Task<HttpResponseMessage>>>()))
            .ReturnsAsync(responseMessage);

        await _weatherService.GetCurrentWeatherAsync(city);

        _statisticsServiceMock.Verify(s => s.LogRequest("WeatherService", It.IsAny<long>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ShouldReturnDefault_WhenApiFails()
    {
        var city = "Berlin";
        var cacheKey = $"Weather_{city.ToLower()}";
        _memoryCacheServiceMock.Setup(m => m.Retrieve<WeatherInfo>(cacheKey)).Returns((WeatherInfo)null);

        _retryPolicyMock.Setup(r => r.RetryHttpRequestStandardAsync(It.IsAny<string>(), It.IsAny<Func<Task<HttpResponseMessage>>>()))
            .ThrowsAsync(new HttpRequestException());

        var result = await _weatherService.GetCurrentWeatherAsync(city);

        Assert.NotNull(result);
        Assert.Null(result.City);
        Assert.Equal(0.0, result.Temperature);
        Assert.Null(result.WeatherDescription);
    }
}
