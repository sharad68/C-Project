using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tornois.Application.Contracts;

namespace Tornois.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("public")]
public sealed class PeopleController(ISportsPlatformService platformService) : ControllerBase
{
    [HttpGet("{personId:int}")]
    public ActionResult<PersonProfileDto> GetPerson(int personId)
    {
        var person = platformService.GetPerson(personId);
        return person is null ? NotFound() : Ok(person);
    }

    [HttpGet("search")]
    public ActionResult<PagedResult<PersonDto>> Search([FromQuery] string? query, [FromQuery] PageRequest request)
        => Ok(platformService.SearchPeople(query, request));
}