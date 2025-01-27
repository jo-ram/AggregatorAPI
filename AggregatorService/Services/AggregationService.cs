using AggregatorAPI.Interfaces;
using AggregatorAPI.Models;
using AggregatorAPI.Helpers;
using Aggregator.Service.Models;

namespace AggregatorAPI.Services;

public class AggregationService : IAggregationService
{
    private readonly INewsService _newsService;
    private readonly IGithubService _githubService;
    private readonly IWeatherService _weatherService;
    public AggregationService(
        INewsService newsService, 
        IGithubService githubService, 
        IWeatherService weatherService)
    {
        _newsService = newsService;
        _githubService = githubService;
        _weatherService = weatherService;
    }

    public async Task<Result<AggregatedResult>> GetAggregatedDataAsync(string city, string newsQuery, string shortBy, string filter, string githubOrgRepo, string repoFilter)
    {
        try
        {
            var weatherTask = _weatherService.GetCurrentWeatherAsync(city);
            var newsTask = _newsService.GetNewsAsync(newsQuery);
            var githubTask = _githubService.GetGithubReposAsync(githubOrgRepo);

            await Task.WhenAll(weatherTask, newsTask, githubTask);

            var weatherResult = await weatherTask;
            var newsResult = await newsTask;
            var githubResult = await githubTask;

            var articles = newsResult.Success ? newsResult.Data.Articles.ToList() : new List<Article>();
            var githubRepos = githubResult.Success ? githubResult.Data.ToList() : new List<GithubRepoInfo>();
            var weatherInfo = weatherResult.Success ? weatherResult.Data : new WeatherInfo { WeatherDescription = "Unavailable" };


            var (filteredArticles, filteredRepos) = await ApplyFilteringAndShorting(articles, githubRepos, shortBy, filter, repoFilter);

            var aggregatedResult = new AggregatedResult
            {
                News = filteredArticles,
                Weather = weatherInfo,
                GithubRepos = filteredRepos,
                Errors = new List<string>
            {
                weatherResult.HasException ? "WeatherService failed." : null,
                newsResult.HasException ? "NewsService failed." : null,
                githubResult.HasException ? "GithubService failed." : null
            }.Where(e => e != null).ToList()};

            return Result<AggregatedResult>.ActionSuccessful(aggregatedResult, 200);
        }
        catch (Exception ex)
        {
            return Result<AggregatedResult>.Exception(500, ex);
        }
    }

    private async Task<(List<Article> articles, List<GithubRepoInfo> repos)> ApplyFilteringAndShorting(List<Article> articles, List<GithubRepoInfo> githubRepos, string shortBy, string filterArticles, string repoFilter)
    {
        if (!string.IsNullOrEmpty(filterArticles) && articles.Count > 0)
        {
            var filterArticleFunc = FilterHelper.ParseFilter<Article>(filterArticles);
            if (filterArticleFunc != null) articles = articles.Where(filterArticleFunc).ToList();
        }

        if (!string.IsNullOrEmpty(repoFilter) && githubRepos.Count > 0)
        {
            var filterRepoFunc = FilterHelper.ParseFilter<GithubRepoInfo>(repoFilter);
            if (filterRepoFunc != null) githubRepos = githubRepos.Where(filterRepoFunc).ToList();
        }

        if (!string.IsNullOrEmpty(shortBy))
        {
            articles = shortBy.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? articles.OrderBy(a => DateTime.Parse(a.PublishedAt)).ToList()
                : articles.OrderByDescending(a => DateTime.Parse(a.PublishedAt)).ToList();

            githubRepos = shortBy.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? githubRepos.OrderBy(r => DateTime.Parse(r.CreatedOn)).ToList()
                : githubRepos.OrderByDescending(r => DateTime.Parse(r.CreatedOn)).ToList();
        }


        return (articles, githubRepos);
    }
}


