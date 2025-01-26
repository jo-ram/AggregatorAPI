using AggregatorAPI.Interfaces;
using AggregatorAPI.Models;
using AggregatorAPI.Models.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AggregatorAPI.Services;

public class GithubService : IGithubService
{
    private readonly HttpClient _httpClient;
    private readonly GithubApiSettings _githubApiSettings;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IMemoryCacheService _memoryCacheService;
    public GithubService(
        HttpClient httpClient, 
        IOptions<GithubApiSettings> options, 
        IMemoryCacheService memoryCacheService,
        IRetryPolicy retryPolicy)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Test app");
        _githubApiSettings = options.Value;
        _memoryCacheService = memoryCacheService;
        _retryPolicy = retryPolicy;
    }

    public async Task<List<GithubRepoInfo>> GetGithubReposAsync(string githubOrg)
    {
        try
        {
            var cacheKey = !string.IsNullOrEmpty(githubOrg) ? $"GitHubRepos_{githubOrg}" : $"GitHubRepos_dotnet";
            var cachedRepos = _memoryCacheService.Retrieve<List<GithubRepoInfo>>(cacheKey);
            if (cachedRepos != null) return cachedRepos;

            var requestUrl = !string.IsNullOrEmpty(githubOrg)
            ? _githubApiSettings.BaseUrl.Replace("{org}", githubOrg) : _githubApiSettings.BaseUrl.Replace("{org}", "dotnet");

            HttpResponseMessage result = await _retryPolicy.RetryHttpRequestStandardAsync(requestUrl, async () => await _httpClient.GetAsync(requestUrl));
            //var response = await _httpClient.GetAsync(requestUrl);
            result.EnsureSuccessStatusCode();

            var content = await result.Content.ReadAsStringAsync();
            dynamic repoData = JsonConvert.DeserializeObject(content);

            var repos = new List<GithubRepoInfo>();
            foreach (var item in repoData)
            {
                repos.Add(new GithubRepoInfo
                {
                    Name = item.name,
                    Description = item.description,
                    LastUpdatedAt = item.updated_at,
                    CreatedOn = item.created_at,
                    Language = item.language
                });
            }

            _memoryCacheService.Add(cacheKey, repos);
            return repos;
        }
        catch (Exception ex)
        {
            return new List<GithubRepoInfo>();
        }
    }
}
