using AggregatorAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AggregatorAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AggregationController : ControllerBase
{
    private IAggregationService aggregationService;  
    public AggregationController(IAggregationService aggregationService)
    {
        this.aggregationService = aggregationService;
    }

    [HttpGet("aggregated-data")]
    public async Task<IActionResult> GetAggregatedData(
        [FromQuery] string searchQueryParam, 
        [FromQuery] string city, 
        [FromQuery] string githubOrgRepo, 
        [FromQuery] string shortBy = null, 
        [FromQuery] string filter = null)
    {
        try
        {
            var aggregatedData = await aggregationService.GetAggregatedDataAsync(searchQueryParam, city, shortBy, filter, githubOrgRepo);
            return Ok(aggregatedData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
