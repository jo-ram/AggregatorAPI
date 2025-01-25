using AggregatorAPI.Models.Settings;

namespace AggregatorAPI.Configuration;

public static class SettingsOptions
{
    public static void AddOptions(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<WeatherApiSettings>(
                builder.Configuration.GetSection(nameof(WeatherApiSettings)));
        builder.Services.Configure<RedditApiSettings>(
               builder.Configuration.GetSection(nameof(RedditApiSettings)));
        builder.Services.Configure<NewsApiSettings>(
               builder.Configuration.GetSection(nameof(NewsApiSettings)));
    }
}
