using AggregatorAPI.Models.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AggregatorAPI;

public class NewsService
{
    private readonly HttpClient _httpClient;
    private readonly NewsApiSettings _settings;

    public NewsService(HttpClient httpClient, IOptions<NewsApiSettings> options)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Test app");
        _settings = options.Value;
    }

    public async Task<NewsResponse> GetNewsAsync(string query, string category = null, string language = "en")
    {
        try
        {
            var url = $"{_settings.BaseUrl}?q={query}&language={language}&apiKey={_settings.NewsApiKey}";
            if (!string.IsNullOrEmpty(category))
            {
                url += $"&category={category}";
            }

            var response = await _httpClient.GetAsync(url);
            //response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<NewsResponse>(content);
        }
        catch(Exception ex)
        {
            return null;
        }
    }
}
