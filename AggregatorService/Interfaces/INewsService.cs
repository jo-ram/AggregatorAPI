using AggregatorAPI.Models;

namespace AggregatorAPI.Interfaces;

public interface INewsService
{
    Task<NewsInfo> GetNewsAsync(string query, string category = null, string language = "en");
}
