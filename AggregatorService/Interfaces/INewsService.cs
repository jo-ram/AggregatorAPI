using Aggregator.Service.Models;
using AggregatorAPI.Models;

namespace AggregatorAPI.Interfaces;

public interface INewsService
{
    Task<Result<NewsInfo>> GetNewsAsync(string query, string category = null, string language = "en");
}
