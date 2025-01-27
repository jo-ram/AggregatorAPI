using Aggregator.Service.Models;
using AggregatorAPI.Models;
namespace AggregatorAPI.Interfaces;

public interface IAggregationService
{
    Task<Result<AggregatedResult>> GetAggregatedDataAsync(string city, string newsQuery, string shortBy, string filter, string githubOrgRepo, string repoFilter);
}