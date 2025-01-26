using AggregatorAPI.Models.Settings;

namespace AggregatorAPI.Configuration;

public static class SettingsOptionsExtension
{
    public static void AddOptions(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<WeatherApiSettings>(
                builder.Configuration.GetSection(nameof(WeatherApiSettings)));
        builder.Services.Configure<GithubApiSettings>(
               builder.Configuration.GetSection(nameof(GithubApiSettings)));
        builder.Services.Configure<NewsApiSettings>(
               builder.Configuration.GetSection(nameof(NewsApiSettings)));
    }
}
