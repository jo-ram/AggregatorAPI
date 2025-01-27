using AggregatorAPI.Interfaces;
using AggregatorAPI.Models.Settings;
using AggregatorAPI.Models;
using AggregatorAPI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using Moq.Protected;
using Moq;
using System.Text.Json;

namespace AggregationApiTests
{
    public class NewsServiceTest
    {
        [Fact]
        public async Task GetNewsAsync_ReturnsCachedData_WhenAvailable()
        {
            var cachedNews = new NewsInfo
            {
                Status = "ok",
                TotalResults = 1,
                Articles = new List<Article> { new Article { Title = "Cached News" } }
            };

            var mockCache = new Mock<IMemoryCacheService>();
            mockCache.Setup(c => c.Retrieve<NewsInfo>("News_Election")).Returns(cachedNews);

            var mockRetryPolicy = new Mock<IRetryPolicy>();

            var service = new NewsService(
                new HttpClient(),
                Options.Create(new NewsApiSettings { BaseUrl = "http://example.com", ApiKey = "fake-key" }),
                mockCache.Object,
                mockRetryPolicy.Object,
                new StatisticsService()
            );

            var result = await service.GetNewsAsync("Election");

            Assert.NotNull(result);
            Assert.Equal("Cached News", result.Articles[0].Title);

            mockRetryPolicy.Verify(p => p.RetryHttpRequestStandardAsync(It.IsAny<string>(), It.IsAny<Func<Task<HttpResponseMessage>>>()), Times.Never);
        }

        [Fact]
        public async Task GetNewsAsync_CallsRetryPolicy_WhenNoCache()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new NewsInfo
                {
                    Status = "ok",
                    TotalResults = 1,
                    Articles = new List<Article> { new Article { Title = "API News" } }
                }))
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(fakeResponse);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockCache = new Mock<IMemoryCacheService>();
            mockCache.Setup(c => c.Retrieve<NewsInfo>(It.IsAny<string>())).Returns((NewsInfo)null);

            var mockRetryPolicy = new Mock<IRetryPolicy>();
            mockRetryPolicy
                .Setup(p => p.RetryHttpRequestStandardAsync(It.IsAny<string>(), It.IsAny<Func<Task<HttpResponseMessage>>>()))
                .Returns<string, Func<Task<HttpResponseMessage>>>((_, requestFunc) => requestFunc());

            var service = new NewsService(
                httpClient,
                Options.Create(new NewsApiSettings { BaseUrl = "http://example.com", ApiKey = "fake-key" }),
                mockCache.Object,
                mockRetryPolicy.Object,
                new StatisticsService()
            );

            var result = await service.GetNewsAsync("Election");

            Assert.NotNull(result);
            Assert.Equal("API News", result.Articles[0].Title);

            mockRetryPolicy.Verify(p => p.RetryHttpRequestStandardAsync(It.IsAny<string>(), It.IsAny<Func<Task<HttpResponseMessage>>>()), Times.Once);
        }

        [Fact]
        public async Task GetNewsAsync_ReturnsDefault_WhenApiFails()
        {
            var mockCache = new Mock<IMemoryCacheService>();
            mockCache.Setup(c => c.Retrieve<NewsInfo>(It.IsAny<string>())).Returns((NewsInfo)null);

            var mockRetryPolicy = new Mock<IRetryPolicy>();
            mockRetryPolicy
                .Setup(p => p.RetryHttpRequestStandardAsync(It.IsAny<string>(), It.IsAny<Func<Task<HttpResponseMessage>>>()))
                .ThrowsAsync(new HttpRequestException("API failure"));

            var service = new NewsService(
                new HttpClient(), 
                Options.Create(new NewsApiSettings { BaseUrl = "http://example.com", ApiKey = "fake-key" }), 
                mockCache.Object, 
                mockRetryPolicy.Object,
                new StatisticsService()
            );

            var result = await service.GetNewsAsync("Election");

            Assert.NotNull(result);
            Assert.Empty(result.Articles); 
        }
    }
}