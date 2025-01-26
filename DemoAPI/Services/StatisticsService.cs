using AggregatorAPI.Interfaces;
using System.Collections.Concurrent;

namespace AggregatorAPI.Services;

public class StatisticsService : IStatisticsService
{
    private readonly ConcurrentDictionary<string, List<long>> _statistics = new();
    public void LogRequest(string serviceName, long responseTimeMs)
    {
        if (!_statistics.ContainsKey(serviceName))
        {
            _statistics[serviceName] = new List<long>();
        }

        lock (_statistics[serviceName])
        {
            _statistics[serviceName].Add(responseTimeMs);
        }
    }

    public object GetStatistics()
    {
        return _statistics.ToDictionary(
            service => service.Key,
            service => new
            {
                TotalRequests = service.Value.Count,
                AverageResponseTime = service.Value.Any() ? service.Value.Average() : 0,
                PerformanceBuckets = new
                {
                    Fast = service.Value.Count(time => time < 100),
                    Average = service.Value.Count(time => time >= 100 && time <= 200),
                    Slow = service.Value.Count(time => time > 200)
                }
            });
    }
}
