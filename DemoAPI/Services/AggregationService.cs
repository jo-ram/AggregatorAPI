using AggregatorAPI.Interfaces;
using AggregatorAPI.Models;
using AggregatorAPI.Utils;

namespace AggregatorAPI.Services;

public class AggregationService : IAggregationService
{
    private readonly INewsService newsService;
    private readonly RedditService redditService;
    private readonly IWeatherService weatherService;
    public AggregationService(INewsService newsService, /*RedditService redditService,*/ IWeatherService weatherService)
    {
        this.newsService = newsService;
        //this.redditService = redditService;
        this.weatherService = weatherService;
    }

    public async Task<AggregatedResult> GetAggregatedDataAsync(string city, string newsQuery, string shortBy, string filter)//,string githubOwner,string githubRepo)
    {
        var weatherTask = weatherService.GetCurrentWeatherAsync(city);
        var newsTask = newsService.GetNewsAsync(newsQuery);
        //var githubTask = _githubService.GetRepositoryInfoAsync(githubOwner, githubRepo);

        await Task.WhenAll(weatherTask, newsTask);//, githubTask);

        var weatherResult = await weatherTask;
        var newsResult = await newsTask;

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
            Weather = weatherResult
        };
    }



    //private List<Article> ApplyFilterAndSortNews(List<Article> news, string filter, string sortBy)
    //{
    //    var articles = news.AsEnumerable();

    //    if (!string.IsNullOrWhiteSpace(filter))
    //    {
    //        if (filter.Equals("authorName", StringComparison.OrdinalIgnoreCase))
    //        {
    //            articles = articles.Where(a => !string.IsNullOrEmpty(a.Author) && a.Author.Equals(filter, StringComparison.OrdinalIgnoreCase));
    //        }
    //        else if (filter.Equals("recent", StringComparison.OrdinalIgnoreCase))
    //        {
    //            var cutoff = DateTime.UtcNow.AddDays(-7);

    //            articles = articles.Where(a =>
    //            {
    //                if (DateTime.TryParse(a.PublishedAt, out var publishedDate))
    //                {
    //                    return publishedDate >= cutoff;
    //                }
    //                return false;
    //            });
    //        }
    //    }

    //    if (!string.IsNullOrWhiteSpace(sortBy))
    //    {
    //        if (sortBy.Equals("dateDesc", StringComparison.OrdinalIgnoreCase))
    //        {
    //            articles = articles.OrderByDescending(a =>
    //            {
    //                if (DateTime.TryParse(a.PublishedAt, out var publishedDate))
    //                {
    //                    return publishedDate;
    //                }
    //                return DateTime.MinValue;
    //            });
    //        }
    //        else if (sortBy.Equals("dateAsc", StringComparison.OrdinalIgnoreCase))
    //        {
    //            articles = articles.OrderBy(a =>
    //            {
    //                if (DateTime.TryParse(a.PublishedAt, out var publishedDate))
    //                {
    //                    return publishedDate;
    //                }
    //                return DateTime.MinValue;
    //            });
    //        }
    //        else if (sortBy.Equals("title", StringComparison.OrdinalIgnoreCase))
    //        {
    //            articles = articles.OrderBy(a => a.Title);
    //        }
    //    }

    //    return articles.ToList();
    //}
}


