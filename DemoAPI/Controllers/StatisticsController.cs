using AggregatorAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AggregatorAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }
    [HttpGet("request-statistics")]
    public IActionResult GetRequestStatistics()
    {
        var stats = _statisticsService.GetStatistics();
        return Ok(stats);
    }
}
