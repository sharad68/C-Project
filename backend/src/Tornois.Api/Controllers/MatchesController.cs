using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tornois.Application.Contracts;

namespace Tornois.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("public")]
public sealed class MatchesController(ISportsPlatformService platformService) : ControllerBase
{
    [HttpGet("live")]
    public ActionResult<IReadOnlyList<MatchDto>> GetLiveMatches()
        => Ok(platformService.GetLiveMatches());

    [HttpGet("upcoming")]
    public ActionResult<IReadOnlyList<MatchDto>> GetUpcomingFixtures([FromQuery] int? sportId, [FromQuery] int? competitionId, [FromQuery] int count = 10)
        => Ok(platformService.GetUpcomingFixtures(sportId, competitionId, count));

    [HttpGet("{matchId:int}")]
    public ActionResult<MatchDetailDto> GetMatchDetails(int matchId)
    {
        var match = platformService.GetMatchDetail(matchId);
        return match is null ? NotFound() : Ok(match);
    }
}