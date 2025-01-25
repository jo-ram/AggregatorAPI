using AggregatorAPI.Models;

namespace AggregatorAPI.Interfaces
{
    public interface IAggregationService
    {
        Task<AggregatedResult> GetAggregatedDataAsync(string city, string newsQuery, string shortBy, string filter);
    }
}