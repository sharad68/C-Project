using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tornois.Api.Infrastructure;
using Tornois.Application.Contracts;

namespace Tornois.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AdminController(
    IAdminAuthService authService,
    ISportsPlatformService platformService,
    IJwtTokenService tokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public ActionResult<AdminLoginResponse> Login([FromBody] AdminLoginRequest request)
    {
        var admin = authService.ValidateCredentials(request.UserName, request.Password);
        if (admin is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid credentials",
                Detail = "The supplied username or password is incorrect.",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        return Ok(tokenService.CreateToken(admin));
    }

    [Authorize]
    [HttpGet("verify")]
    public IActionResult VerifyToken() => Ok(new { valid = true, user = BuildIdentity() });

    [Authorize]
    [HttpGet("me")]
    public ActionResult<AdminIdentityDto> Me() => Ok(BuildIdentity());

    [Authorize(Roles = "superadmin")]
    [HttpGet("users")]
    public ActionResult<IReadOnlyList<AdminIdentityDto>> GetUsers() => Ok(authService.GetAdmins());

    [Authorize(Roles = "superadmin")]
    [HttpPost("users")]
    public ActionResult<AdminIdentityDto> UpsertUser([FromBody] AdminUserUpsertRequest request)
        => Ok(authService.UpsertAdminUser(request, BuildIdentity().UserName));

    [Authorize]
    [HttpGet("changes")]
    public ActionResult<IReadOnlyList<ChangeLogDto>> GetChanges() => Ok(platformService.GetRecentChanges());

    private AdminIdentityDto BuildIdentity()
    {
        var userName = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue("unique_name")
            ?? User.Identity?.Name
            ?? "unknown";

        var displayName = User.FindFirstValue(ClaimTypes.Name) ?? userName;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "readonly";
        return new AdminIdentityDto(userName, displayName, role);
    }
}