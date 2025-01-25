using Microsoft.AspNetCore.Mvc;

namespace AggregatorAPI;

[Route("api/[controller]")]
[ApiController]
public class AggregatorController : ControllerBase
{
    private readonly NewsService newsService;
    private readonly RedditService redditService;
    public AggregatorController(NewsService newsService, RedditService redditService)
    {
        this.newsService = newsService;
        this.redditService = redditService;
    }

    [HttpGet("aggregated-data")]
    public async Task<IActionResult> GetAggregatedData([FromQuery] string query = null, [FromQuery] string city = null)
    {
        try
        {
            var newsData = await newsService.GetNewsAsync(query);
            var redditData = await redditService.GetPostsFromSubredditAsync(query);
            return Ok(redditData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
