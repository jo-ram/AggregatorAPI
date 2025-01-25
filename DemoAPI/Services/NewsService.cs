using AggregatorAPI.Models;
using AggregatorAPI.Models.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AggregatorAPI.Services;

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

    public async Task<List<NewsInfo>> GetNewsAsync(string query, string category = null, string language = "en")
    {
        try
        {
            //Set default value in case no input from user 
            if (string.IsNullOrEmpty(query)) query = "Election";

            var url = $"{_settings.BaseUrl}?q={query}&language={language}&apiKey={_settings.ApiKey}";
            if (!string.IsNullOrEmpty(category)) url += $"&category={category}";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            if(content != null) return JsonSerializer.Deserialize<List<NewsInfo>>(content);
            return new List<NewsInfo>();
        }
        catch (Exception ex)
        {
            // add exception handling !!!
            return null;
        }
    }
}
