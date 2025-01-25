using AggregatorAPI.Interfaces;
using AggregatorAPI.Services;

namespace AggregatorAPI.Configuration;

public static class ServicesExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {

        services.AddTransient<INewsService, NewsService>();
        services.AddTransient<RedditService>();
        services.AddTransient<IWeatherService, WeatherService>();
        services.AddTransient<IAggregationService, AggregationService>();
        services.AddTransient<IMemoryCacheService, MemoryCacheService>();
        return services;
    }
}
