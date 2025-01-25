namespace AggregatorAPI.Models;

public class AggregatedResult
{
    public WeatherInfo Weather { get; set; }
    public List<Article> News { get; set; }
    //add third api !!!
}
