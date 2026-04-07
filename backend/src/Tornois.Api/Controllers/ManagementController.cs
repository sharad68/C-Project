using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tornois.Application.Contracts;

namespace Tornois.Api.Controllers;

[ApiController]
[Authorize(Roles = "superadmin,editor")]
[Route("api/admin/management")]
public sealed class ManagementController(ITournamentManagementService managementService) : ControllerBase
{
    [HttpPost("sports")]
    public ActionResult<SportDto> CreateSport([FromBody] SportUpsertRequest request)
        => CreatedResult(() => managementService.UpsertSport(null, request, GetActorName()), "sports");

    [HttpPut("sports/{sportId:int}")]
    public ActionResult<SportDto> UpdateSport(int sportId, [FromBody] SportUpsertRequest request)
        => OkResult(() => managementService.UpsertSport(sportId, request, GetActorName()));

    [HttpDelete("sports/{sportId:int}")]
    public IActionResult DeleteSport(int sportId)
        => NoContentResult(() => managementService.DeleteSport(sportId, GetActorName()));

    [HttpPost("competitions")]
    public ActionResult<CompetitionDto> CreateCompetition([FromBody] CompetitionUpsertRequest request)
        => CreatedResult(() => managementService.UpsertCompetition(null, request, GetActorName()), "competitions");

    [HttpPut("competitions/{competitionId:int}")]
    public ActionResult<CompetitionDto> UpdateCompetition(int competitionId, [FromBody] CompetitionUpsertRequest request)
        => OkResult(() => managementService.UpsertCompetition(competitionId, request, GetActorName()));

    [HttpDelete("competitions/{competitionId:int}")]
    public IActionResult DeleteCompetition(int competitionId)
        => NoContentResult(() => managementService.DeleteCompetition(competitionId, GetActorName()));

    [HttpPost("teams")]
    public ActionResult<TeamDto> CreateTeam([FromBody] TeamUpsertRequest request)
        => CreatedResult(() => managementService.UpsertTeam(null, request, GetActorName()), "teams");

    [HttpPut("teams/{teamId:int}")]
    public ActionResult<TeamDto> UpdateTeam(int teamId, [FromBody] TeamUpsertRequest request)
        => OkResult(() => managementService.UpsertTeam(teamId, request, GetActorName()));

    [HttpDelete("teams/{teamId:int}")]
    public IActionResult DeleteTeam(int teamId)
        => NoContentResult(() => managementService.DeleteTeam(teamId, GetActorName()));

    [HttpPost("people")]
    public ActionResult<PersonDto> CreatePerson([FromBody] PersonUpsertRequest request)
        => CreatedResult(() => managementService.UpsertPerson(null, request, GetActorName()), "people");

    [HttpPut("people/{personId:int}")]
    public ActionResult<PersonDto> UpdatePerson(int personId, [FromBody] PersonUpsertRequest request)
        => OkResult(() => managementService.UpsertPerson(personId, request, GetActorName()));

    [HttpDelete("people/{personId:int}")]
    public IActionResult DeletePerson(int personId)
        => NoContentResult(() => managementService.DeletePerson(personId, GetActorName()));

    [HttpPost("matches")]
    public ActionResult<MatchDto> CreateMatch([FromBody] MatchUpsertRequest request)
        => CreatedResult(() => managementService.UpsertMatch(null, request, GetActorName()), "matches");

    [HttpPut("matches/{matchId:int}")]
    public ActionResult<MatchDto> UpdateMatch(int matchId, [FromBody] MatchUpsertRequest request)
        => OkResult(() => managementService.UpsertMatch(matchId, request, GetActorName()));

    [HttpDelete("matches/{matchId:int}")]
    public IActionResult DeleteMatch(int matchId)
        => NoContentResult(() => managementService.DeleteMatch(matchId, GetActorName()));

    private ActionResult<T> CreatedResult<T>(Func<T> action, string resourceSegment) where T : class
    {
        try
        {
            var result = action();
            return Created($"/api/admin/management/{resourceSegment}", result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ToProblem(exception.Message, StatusCodes.Status404NotFound));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ToProblem(exception.Message, StatusCodes.Status400BadRequest));
        }
    }

    private ActionResult<T> OkResult<T>(Func<T> action) where T : class
    {
        try
        {
            return Ok(action());
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ToProblem(exception.Message, StatusCodes.Status404NotFound));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ToProblem(exception.Message, StatusCodes.Status400BadRequest));
        }
    }

    private IActionResult NoContentResult(Action action)
    {
        try
        {
            action();
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ToProblem(exception.Message, StatusCodes.Status404NotFound));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(ToProblem(exception.Message, StatusCodes.Status400BadRequest));
        }
    }

    private string GetActorName()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.Identity?.Name
            ?? "unknown";

    private static ProblemDetails ToProblem(string detail, int statusCode)
        => new()
        {
            Title = statusCode == StatusCodes.Status404NotFound ? "Resource not found" : "Unable to process request",
            Detail = detail,
            Status = statusCode
        };
}
