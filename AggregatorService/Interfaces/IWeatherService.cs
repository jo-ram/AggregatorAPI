using Aggregator.Service.Models;
using AggregatorAPI.Models;

namespace AggregatorAPI.Interfaces;

public interface IWeatherService
{
    Task<Result<WeatherInfo>> GetCurrentWeatherAsync(string city);
}