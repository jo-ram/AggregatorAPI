namespace AggregatorAPI.Models;

public class AggregatedResult
{
    public WeatherInfo Weather { get; set; }
    public List<Article> News { get; set; }
    public List<GithubRepoInfo> GithubRepos { get; set; }
}
