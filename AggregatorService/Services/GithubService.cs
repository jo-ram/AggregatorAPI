using Aggregator.Service.Models;
using AggregatorAPI.Interfaces;
using AggregatorAPI.Models;
using AggregatorAPI.Models.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;

namespace AggregatorAPI.Services;

public class GithubService : IGithubService
{
    private readonly HttpClient _httpClient;
    private readonly GithubApiSettings _githubApiSettings;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IMemoryCacheService _memoryCacheService;
    private readonly IStatisticsService _statisticsService;

    public GithubService(
        HttpClient httpClient, 
        IOptions<GithubApiSettings> options, 
        IMemoryCacheService memoryCacheService,
        IRetryPolicy retryPolicy,
        IStatisticsService statisticsService)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Test app");
        _githubApiSettings = options.Value;
        _memoryCacheService = memoryCacheService;
        _retryPolicy = retryPolicy;
        _statisticsService = statisticsService;
    }

    public async Task<Result<List<GithubRepoInfo>>> GetGithubReposAsync(string githubOrg)
    {
        try
        {
            var cacheKey = !string.IsNullOrEmpty(githubOrg) ? $"GitHubRepos_{githubOrg}" : $"GitHubRepos_dotnet";
            var cachedRepos = _memoryCacheService.Retrieve<List<GithubRepoInfo>>(cacheKey);
            if (cachedRepos != null) return Result<List<GithubRepoInfo>>.ActionSuccessful(cachedRepos, 200); //return cachedRepos;

            var requestUrl = !string.IsNullOrEmpty(githubOrg)
            ? _githubApiSettings.BaseUrl.Replace("{org}", githubOrg) : _githubApiSettings.BaseUrl.Replace("{org}", "dotnet");

            var stopwatch = Stopwatch.StartNew();
            HttpResponseMessage result = await _retryPolicy.RetryHttpRequestStandardAsync(requestUrl, async () => await _httpClient.GetAsync(requestUrl));
            stopwatch.Stop();
            _statisticsService.LogRequest("GithubService", stopwatch.ElapsedMilliseconds);
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
            return Result<List<GithubRepoInfo>>.ActionSuccessful(repos, 200);
        }
        catch (Exception ex)
        {
            return Result<List<GithubRepoInfo>>.Exception(500, ex);
        }
    }
}
