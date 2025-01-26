using AggregatorAPI.Interfaces;
using AggregatorAPI.Models;
using AggregatorAPI.Helpers;

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

    public async Task<AggregatedResult> GetAggregatedDataAsync(string city, string newsQuery, string shortBy, string filter, string githubOrgRepo)
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

            var articles = newsResult.Articles.ToList();
            //if (newsResult != null && (!string.IsNullOrEmpty(shortBy) || !string.IsNullOrEmpty(shortBy)))
            //    articles = ApplyFilterAndSortNews(articles, filter, shortBy);

            if (!string.IsNullOrEmpty(filter))
            {
                var filterFunc = FilterHelper.ParseFilter(filter);
                if (filterFunc != null)
                    articles = articles.Where(filterFunc).ToList();
            }

            if (!string.IsNullOrEmpty(shortBy))
            {
                articles = shortBy.Equals("asc", StringComparison.OrdinalIgnoreCase) ?
                    articles.OrderBy(article => DateTime.Parse(article.PublishedAt)).ToList() :
                    articles.OrderByDescending(article => DateTime.Parse(article.PublishedAt)).ToList();
            }

            return new AggregatedResult
            {
                News = articles,
                Weather = weatherResult,
                GithubRepos = githubResult
            };
        }
        catch (Exception ex) 
        {
            return new AggregatedResult();
        }
    }
}


