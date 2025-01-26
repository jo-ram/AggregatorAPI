using AggregatorAPI.Interfaces;
using AggregatorAPI.Models;
using AggregatorAPI.Models.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace AggregatorAPI.Services;

public class NewsService : INewsService
{
    private readonly HttpClient _httpClient;
    private readonly NewsApiSettings _settings;
    private readonly IRetryPolicy _retryPolicy; 
    private readonly IMemoryCacheService _memoryCacheService;
    private readonly IStatisticsService _statisticsService;

    public NewsService(
        HttpClient httpClient, 
        IOptions<NewsApiSettings> options,
        IMemoryCacheService memoryCacheService,
        IRetryPolicy retryPolicy,
        IStatisticsService statisticsService)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Test app");
        _settings = options.Value;
        _memoryCacheService = memoryCacheService;
        _retryPolicy = retryPolicy;
        _statisticsService = statisticsService;
    }

    public async Task<NewsInfo> GetNewsAsync(string query, string category = null, string language = "en")
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            if (string.IsNullOrEmpty(query)) query = "Election";
            var cacheKey = $"News_{query}";

            var cachedNews = _memoryCacheService.Retrieve<NewsInfo>(cacheKey);
            if (cachedNews != null) return cachedNews;

            var url = $"{_settings.BaseUrl}?q={query}&language={language}&apiKey={_settings.ApiKey}";
            if (!string.IsNullOrEmpty(category)) url += $"&category={category}";

            HttpResponseMessage result = await _retryPolicy.RetryHttpRequestStandardAsync(url, async () => await _httpClient.GetAsync(url));
            //var response = await _httpClient.GetAsync(url);
            result.EnsureSuccessStatusCode();

            var content = await result.Content.ReadAsStringAsync();
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
        finally
        {
            stopwatch.Stop();
            _statisticsService.LogRequest("NewsService", stopwatch.ElapsedMilliseconds);
        }
    }
}
