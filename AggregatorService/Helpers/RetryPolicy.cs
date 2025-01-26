using AggregatorAPI.Interfaces;
using AggregatorAPI.Models.Settings;
using Microsoft.Extensions.Options;
using Polly;

namespace AggregatorAPI.Helpers;

public class RetryPolicy : IRetryPolicy
{
    private readonly RetrySettings _retrySettings;
    public RetryPolicy(IOptions<RetrySettings> options)
    {
        _retrySettings = options.Value;
    }

    public async Task<HttpResponseMessage> RetryHttpRequestStandardAsync(string requestUri, Func<Task<HttpResponseMessage>> requestFunc)
    {
        if (!int.TryParse(_retrySettings.Attempts, out int retryCount)) retryCount = 3;
        if (!int.TryParse(_retrySettings.Interval, out int retryTimespan)) retryTimespan = 3;

        var policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode && r.StatusCode != System.Net.HttpStatusCode.BadRequest)
            .Or<HttpRequestException>().WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(retryTimespan));

        //result could be logged for more info on the possible issue

        return await policy.ExecuteAsync(requestFunc);
    }
}
