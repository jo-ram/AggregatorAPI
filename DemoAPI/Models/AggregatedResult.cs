namespace AggregatorAPI.Models;

public class AggregatedResult
{
    public WeatherInfo Weather { get; set; }
    public List<NewsInfo> News { get; set; }
    //add third api !!!
}
