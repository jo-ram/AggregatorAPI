namespace AggregatorAPI.Interfaces;

public interface IRetryPolicy
{
    Task<HttpResponseMessage> RetryHttpRequestStandardAsync(string requestUri, Func<Task<HttpResponseMessage>> requestFunc);
}