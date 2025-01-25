using AggregatorAPI.Models;

namespace AggregatorAPI.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherInfo> GetCurrentWeatherAsync(string city);
    }
}