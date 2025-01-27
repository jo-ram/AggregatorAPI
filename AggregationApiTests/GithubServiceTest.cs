using AggregatorAPI.Interfaces;
using AggregatorAPI.Models;
using AggregatorAPI.Models.Settings;
using AggregatorAPI.Services;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Text;

public class GithubServiceTests
{
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly Mock<IMemoryCacheService> _memoryCacheMock;
    private readonly Mock<IRetryPolicy> _retryPolicyMock;
    private readonly Mock<IStatisticsService> _statisticsServiceMock;
    private readonly GithubService _githubService;

    public GithubServiceTests()
    {
        _httpClientMock = new Mock<HttpClient>();
        _memoryCacheMock = new Mock<IMemoryCacheService>();
        _retryPolicyMock = new Mock<IRetryPolicy>();
        _statisticsServiceMock = new Mock<IStatisticsService>();

        var githubApiSettings = Options.Create(new GithubApiSettings
        {
            BaseUrl = "https://api.github.com/orgs/{org}/repos"
        });

        _githubService = new GithubService(
            new HttpClient(),
            githubApiSettings,
            _memoryCacheMock.Object,
            _retryPolicyMock.Object,
            _statisticsServiceMock.Object
        );
    }

    [Fact]
    public async Task GetGithubReposAsync_ShouldReturnCachedData_WhenCacheExists()
    {
        var githubOrg = "testOrg";
        var cacheKey = $"GitHubRepos_{githubOrg}";
        var cachedRepos = new List<GithubRepoInfo>
        {
            new GithubRepoInfo { Name = "TestRepo", Description = "Test Description" }
        };
        _memoryCacheMock.Setup(x => x.Retrieve<List<GithubRepoInfo>>(cacheKey)).Returns(cachedRepos);

        var result = await _githubService.GetGithubReposAsync(githubOrg);

        Assert.NotNull(result.Data);
        Assert.Equal(cachedRepos, result.Data);
        _memoryCacheMock.Verify(x => x.Retrieve<List<GithubRepoInfo>>(cacheKey), Times.Once);
        _httpClientMock.VerifyNoOtherCalls(); 
    }

    [Fact]
    public async Task GetGithubReposAsync_ShouldCallApiAndCacheResult_WhenCacheIsEmpty()
    {
        var githubOrg = "testOrg";
        var cacheKey = $"GitHubRepos_{githubOrg}";
        _memoryCacheMock.Setup(x => x.Retrieve<List<GithubRepoInfo>>(cacheKey)).Returns((List<GithubRepoInfo>)null);

        var repoData = new[]
        {
            new { name = "Repo1", description = "Description1", updated_at = "2022-01-01", created_at = "2021-01-01", language = "C#" }
        };
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(repoData), Encoding.UTF8, "application/json")
        };
        _retryPolicyMock.Setup(x => x.RetryHttpRequestStandardAsync(It.IsAny<string>(), It.IsAny<Func<Task<HttpResponseMessage>>>()))
            .ReturnsAsync(httpResponse);

        var result = await _githubService.GetGithubReposAsync(githubOrg);

        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("Repo1", result.Data[0].Name);
        _memoryCacheMock.Verify(x => x.Add(cacheKey, It.IsAny<List<GithubRepoInfo>>()), Times.Once);
        _statisticsServiceMock.Verify(x => x.LogRequest("GithubService", It.IsAny<long>()), Times.Once);
    }
}