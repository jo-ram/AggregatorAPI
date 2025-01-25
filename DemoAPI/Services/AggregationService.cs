using AggregatorAPI.Models;

namespace AggregatorAPI.Services;

public class AggregationService
{
    private readonly NewsService newsService;
    private readonly RedditService redditService;
    private readonly WeatherService weatherService;
    public AggregationService(NewsService newsService, RedditService redditService, WeatherService weatherService)
    {
        this.newsService = newsService;
        this.redditService = redditService;
        this.weatherService = weatherService;
    }

    public async Task<AggregatedResult> GetAggregatedDataAsync(string city,string newsQuery, string shortBy, string filter)//,string githubOwner,string githubRepo)
    {
        var weatherTask = await weatherService.GetCurrentWeatherAsync(city);
        //var newsTask = newsService.GetNewsAsync(newsQuery);
        //var githubTask = _githubService.GetRepositoryInfoAsync(githubOwner, githubRepo);

        //var aggregatedResult = await Task.WhenAll(weatherTask);//, newsTask);//, githubTask);

        var sdjfhsd = string.Empty;
        var asd = new AggregatedResult
        {
            //Weather = await weatherTask,
            //News = await newsTask
            //GitHub = githubTask.Result
        };

        //if (aggregatedResult.News.Any() && (!string.IsNullOrEmpty(shortBy) || !string.IsNullOrEmpty(shortBy)))
        //    aggregatedResult.News = ApplyFilterAndSortNews(aggregatedResult.News, filter, shortBy);

        return asd;
    }

    private List<NewsInfo> ApplyFilterAndSortNews(List<NewsInfo> news, string filter, string sortBy)
    {
        var query = news.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            if (filter.Equals("authorName", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(n => n.Articles.Any(a => a.Author.Equals(filter, StringComparison.OrdinalIgnoreCase))).ToList();
            }
            else if (filter.Equals("recent", StringComparison.OrdinalIgnoreCase))
            {
                var cutoff = DateTime.UtcNow.AddDays(-7);

                query = query.Where(n =>
                    n.Articles != null &&
                    n.Articles.Any(a =>
                    {
                        if (DateTime.TryParse(a.PublishedAt, out var publishedDate))
                        {
                            return publishedDate >= cutoff;
                        }
                        return false;
                    })
                );
            }
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            if (sortBy.Equals("dateDesc", StringComparison.OrdinalIgnoreCase))
            {
                query = query.OrderByDescending(n => n.Articles.SelectMany(t => t.PublishedAt));
            }
            else if (sortBy.Equals("dateAsc", StringComparison.OrdinalIgnoreCase))
            {
                query = query.OrderBy(n => n.Articles.SelectMany(t => t.PublishedAt));
            }
            else if (sortBy.Equals("title", StringComparison.OrdinalIgnoreCase))
            {
                query = query.OrderBy(n => n.Articles.SelectMany(t => t.Title));
            }
        }

        return query.ToList();
    }
}

