using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tornois.Application.Contracts;

namespace Tornois.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("public")]
public sealed class TeamsController(ISportsPlatformService platformService) : ControllerBase
{
    [HttpGet]
    public ActionResult<PagedResult<TeamDto>> GetTeams([FromQuery] int? sportId, [FromQuery] PageRequest request)
        => Ok(platformService.GetTeams(sportId, request));

    [HttpGet("{teamId:int}/roster")]
    public ActionResult<TeamRosterDto> GetRoster(int teamId)
    {
        var roster = platformService.GetTeamRoster(teamId);
        return roster is null ? NotFound() : Ok(roster);
    }
}