using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tornois.Application.Contracts;

namespace Tornois.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("public")]
public sealed class SportsController(ISportsPlatformService platformService) : ControllerBase
{
    [HttpGet]
    public ActionResult<PagedResult<SportDto>> GetSports([FromQuery] PageRequest request)
        => Ok(platformService.GetSports(request));

    [HttpGet("{sportId:int}/competitions")]
    public ActionResult<PagedResult<CompetitionDto>> GetCompetitionsBySport(int sportId, [FromQuery] PageRequest request)
        => Ok(platformService.GetCompetitions(sportId, request));
}