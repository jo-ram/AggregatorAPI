using AggregatorAPI.Interfaces;
using AggregatorAPI.Models;
using AggregatorAPI.Models.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AggregatorAPI.Services;

public class NewsService : INewsService
{
    private readonly HttpClient _httpClient;
    private readonly NewsApiSettings _settings;

    private readonly IMemoryCacheService _memoryCacheService;

    public NewsService(
        HttpClient httpClient, 
        IOptions<NewsApiSettings> options,
        IMemoryCacheService memoryCacheService)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Test app");
        _settings = options.Value;
        _memoryCacheService = memoryCacheService;
    }

    public async Task<NewsInfo> GetNewsAsync(string query, string category = null, string language = "en")
    {
        try
        {
            if (string.IsNullOrEmpty(query)) query = "Election";
            var cacheKey = $"News_{query}";

            var cachedNews = _memoryCacheService.Retrieve<NewsInfo>(cacheKey);
            if (cachedNews != null) return cachedNews;

            var url = $"{_settings.BaseUrl}?q={query}&language={language}&apiKey={_settings.ApiKey}";
            if (!string.IsNullOrEmpty(category)) url += $"&category={category}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            if (content != null)
            {
                var news = JsonSerializer.Deserialize<NewsInfo>(content);
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3)
                };
                _memoryCacheService.Add(cacheKey, news, cacheOptions);
                return news;
            }
            return new NewsInfo();
        }
        catch (Exception ex)
        {
            return new NewsInfo();
        }
    }
}
