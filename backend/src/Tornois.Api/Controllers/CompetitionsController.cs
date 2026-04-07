using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tornois.Application.Contracts;

namespace Tornois.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("public")]
public sealed class CompetitionsController(ISportsPlatformService platformService) : ControllerBase
{
    [HttpGet]
    public ActionResult<PagedResult<CompetitionDto>> GetCompetitions([FromQuery] int? sportId, [FromQuery] PageRequest request)
        => Ok(platformService.GetCompetitions(sportId, request));

    [HttpGet("{competitionId:int}/seasons")]
    public ActionResult<PagedResult<SeasonDto>> GetSeasons(int competitionId, [FromQuery] PageRequest request)
        => Ok(platformService.GetSeasons(competitionId, request));

    [HttpGet("seasons/{seasonId:int}")]
    public ActionResult<SeasonDetailDto> GetSeasonDetails(int seasonId)
    {
        var season = platformService.GetSeasonDetail(seasonId);
        return season is null ? NotFound() : Ok(season);
    }
}