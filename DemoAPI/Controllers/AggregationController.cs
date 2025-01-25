﻿using AggregatorAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AggregatorAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AggregationController : ControllerBase
{
    private AggregationService aggregationService;  
    public AggregationController(AggregationService aggregationService)
    {
        this.aggregationService = aggregationService;
    }

    [HttpGet("aggregated-data")]
    public async Task<IActionResult> GetAggregatedData(
        [FromQuery] string searchQueryParam, 
        [FromQuery] string city, 
        [FromQuery] string shortBy = null, 
        [FromQuery] string filter = null)
    {
        try
        {
            var aggregatedData = aggregationService.GetAggregatedDataAsync(searchQueryParam, city, shortBy, filter);
            return Ok(aggregatedData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
