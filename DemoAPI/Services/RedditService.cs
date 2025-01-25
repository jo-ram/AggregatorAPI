using AggregatorAPI.Models.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AggregatorAPI.Services;

public class RedditService
{
    private readonly HttpClient _httpClient;
    private readonly RedditApiSettings _settings;
    private string _accessToken;

    public RedditService(HttpClient httpClient, IOptions<RedditApiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    private async Task AuthenticateAsync()
    {
        //if (!string.IsNullOrEmpty(_accessToken)) return;

        var authUrl = "https://www.reddit.com/api/v1/access_token";
        var authData = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

        var authRequest = new HttpRequestMessage(HttpMethod.Post, authUrl);
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.SecretKey}"));
        authRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        authRequest.Content = authData;

        var authResponse = await _httpClient.SendAsync(authRequest);
        authResponse.EnsureSuccessStatusCode();

        var authContent = await authResponse.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<RedditAuthResponse>(authContent);
        _accessToken = authResult.AccessToken;
    }

    public async Task<RedditResponse> GetPostsFromSubredditAsync(string subreddit, int limit = 10)
    {
        await AuthenticateAsync();

        var url = $"https://oauth.reddit.com/r/{subreddit}/hot?limit={limit}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        request.Headers.Add("User-Agent", "RedditAPI/1.0");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RedditResponse>(content);
    }

    // Fetch posts based on a search term
    public async Task<RedditResponse> SearchPostsAsync(string query, int limit = 10)
    {
        await AuthenticateAsync();

        var url = $"https://oauth.reddit.com/search?q={query}&limit={limit}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        request.Headers.Add("User-Agent", "RedditAPI/1.0");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RedditResponse>(content);
    }

    public class RedditAuthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }

    public class RedditResponse
    {
        [JsonPropertyName("data")]
        public RedditData Data { get; set; }
    }

    public class RedditData
    {
        [JsonPropertyName("children")]
        public RedditPostWrapper[] Posts { get; set; }
    }

    public class RedditPostWrapper
    {
        [JsonPropertyName("data")]
        public RedditPost Post { get; set; }
    }

    public class RedditPost
    {
        public string Title { get; set; }
        public string Selftext { get; set; }
        public string Url { get; set; }
        public string Subreddit { get; set; }
        public string Author { get; set; }
        public int Ups { get; set; }
        public int NumComments { get; set; }
    }

}

