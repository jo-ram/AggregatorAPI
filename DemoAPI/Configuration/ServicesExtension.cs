using AggregatorAPI.Interfaces;
using AggregatorAPI.Services;
using AggregatorAPI.Helpers;

namespace AggregatorAPI.Configuration;

public static class ServicesExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {

        services.AddTransient<INewsService, NewsService>();
        services.AddTransient<IGithubService, GithubService>();
        services.AddTransient<IWeatherService, WeatherService>();
        services.AddTransient<IAggregationService, AggregationService>();
        services.AddTransient<IMemoryCacheService, MemoryCacheService>();
        services.AddTransient<IRetryPolicy, RetryPolicy>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        return services;
    }
}
