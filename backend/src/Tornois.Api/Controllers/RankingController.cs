using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tornois.Application.Contracts;

namespace Tornois.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("public")]
public sealed class RankingController(ISportsPlatformService platformService) : ControllerBase
{
    [HttpGet("competitions/{competitionId:int}/standings")]
    public ActionResult<IReadOnlyList<StandingDto>> GetStandings(int competitionId)
        => Ok(platformService.GetStandings(competitionId));

    [HttpGet("players")]
    public ActionResult<IReadOnlyList<PlayerRankingDto>> GetPlayerRankings([FromQuery] int competitionId, [FromQuery] string category = "")
        => Ok(platformService.GetPlayerRankings(competitionId, category));
}