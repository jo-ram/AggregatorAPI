namespace AggregatorAPI.Interfaces
{
    public interface IStatisticsService
    {
        object GetStatistics();
        void LogRequest(string serviceName, long responseTimeMs);
    }
}