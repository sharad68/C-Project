using System.ComponentModel.DataAnnotations;

namespace Tornois.Application.Contracts;

public sealed class PageRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasMore);

public sealed record SportDto(
    int Id,
    string Name,
    string Slug,
    string Description,
    bool IsOlympic);

public sealed record CompetitionDto(
    int Id,
    int SportId,
    string Sport,
    string Name,
    string Country,
    string Format,
    bool IsCup);

public sealed record SeasonDto(
    int Id,
    int CompetitionId,
    string Name,
    int YearStart,
    int YearEnd,
    bool IsCurrent);

public sealed record MatchEventDto(
    int Minute,
    string Type,
    string Subject);

public sealed record MatchDto(
    int Id,
    int CompetitionId,
    int SeasonId,
    int SportId,
    string Competition,
    string HomeTeam,
    string AwayTeam,
    DateTimeOffset KickoffUtc,
    string Status,
    int HomeScore,
    int AwayScore,
    string Venue);

public sealed record MatchDetailDto(
    MatchDto Match,
    IReadOnlyList<MatchEventDto> Events);

public sealed record TeamDto(
    int Id,
    int SportId,
    string Name,
    string ShortName,
    string Country,
    string Venue,
    int Founded,
    string BadgeUrl);

public sealed record PersonDto(
    int Id,
    int TeamId,
    string FullName,
    string Role,
    string Nationality,
    DateOnly BirthDate,
    int? ShirtNumber,
    string PhotoUrl);

public sealed record PersonProfileDto(
    PersonDto Person,
    string Bio,
    string TeamName);

public sealed record TeamRosterDto(
    TeamDto Team,
    IReadOnlyList<PersonDto> Members);

public sealed record StandingDto(
    int Position,
    string Team,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int Points);

public sealed record PlayerRankingDto(
    int Position,
    string Person,
    string Team,
    string Category,
    int Value);

public sealed record SeasonDetailDto(
    CompetitionDto Competition,
    SeasonDto Season,
    IReadOnlyList<StandingDto> Standings);

public sealed record ChangeLogDto(
    int Id,
    string Action,
    string EntityName,
    string AdminUserName,
    DateTimeOffset Timestamp,
    string Summary);

public sealed class AdminLoginRequest
{
    [Required]
    [MinLength(3)]
    public string UserName { get; init; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; init; } = string.Empty;
}

public sealed class AdminUserUpsertRequest
{
    [Required]
    [MinLength(3)]
    public string UserName { get; init; } = string.Empty;

    [Required]
    [MinLength(3)]
    public string DisplayName { get; init; } = string.Empty;

    [Required]
    [RegularExpression("^(superadmin|editor|readonly)$")]
    public string Role { get; init; } = "readonly";

    [Required]
    [MinLength(6)]
    public string Password { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}

public sealed record AdminIdentityDto(
    string UserName,
    string DisplayName,
    string Role);

public sealed record AdminLoginResponse(
    string Token,
    DateTimeOffset ExpiresAtUtc,
    AdminIdentityDto User);

public interface ISportsPlatformService
{
    PagedResult<SportDto> GetSports(PageRequest request);
    PagedResult<CompetitionDto> GetCompetitions(int? sportId, PageRequest request);
    PagedResult<SeasonDto> GetSeasons(int competitionId, PageRequest request);
    SeasonDetailDto? GetSeasonDetail(int seasonId);
    IReadOnlyList<MatchDto> GetLiveMatches();
    IReadOnlyList<MatchDto> GetUpcomingFixtures(int? sportId, int? competitionId, int count = 10);
    MatchDetailDto? GetMatchDetail(int matchId);
    PagedResult<TeamDto> GetTeams(int? sportId, PageRequest request);
    TeamRosterDto? GetTeamRoster(int teamId);
    PersonProfileDto? GetPerson(int personId);
    PagedResult<PersonDto> SearchPeople(string? query, PageRequest request);
    IReadOnlyList<StandingDto> GetStandings(int competitionId);
    IReadOnlyList<PlayerRankingDto> GetPlayerRankings(int competitionId, string category);
    IReadOnlyList<ChangeLogDto> GetRecentChanges();
}

public interface IAdminAuthService
{
    AdminIdentityDto? ValidateCredentials(string userName, string password);
    IReadOnlyList<AdminIdentityDto> GetAdmins();
    AdminIdentityDto UpsertAdminUser(AdminUserUpsertRequest request, string performedBy);
}