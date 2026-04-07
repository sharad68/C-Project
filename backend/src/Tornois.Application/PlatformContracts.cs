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

public sealed class SportUpsertRequest
{
    [Required]
    [MinLength(2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MinLength(2)]
    [RegularExpression("^[a-z0-9-]+$")]
    public string Slug { get; init; } = string.Empty;

    [Required]
    [MinLength(10)]
    public string Description { get; init; } = string.Empty;

    public bool IsOlympic { get; init; }
}

public sealed class CompetitionUpsertRequest
{
    [Range(1, int.MaxValue)]
    public int SportId { get; init; }

    [Required]
    [MinLength(2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Country { get; init; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Format { get; init; } = string.Empty;

    public bool IsCup { get; init; }

    [Required]
    [MinLength(2)]
    public string SeasonName { get; init; } = string.Empty;

    [Range(2000, 2100)]
    public int YearStart { get; init; } = DateTime.UtcNow.Year;

    [Range(2000, 2100)]
    public int YearEnd { get; init; } = DateTime.UtcNow.Year;

    public bool IsCurrent { get; init; } = true;
}

public sealed class TeamUpsertRequest
{
    [Range(1, int.MaxValue)]
    public int SportId { get; init; }

    [Required]
    [MinLength(2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string ShortName { get; init; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Country { get; init; } = string.Empty;

    public string Venue { get; init; } = string.Empty;
    public int Founded { get; init; } = DateTime.UtcNow.Year;
    public string BadgeUrl { get; init; } = string.Empty;
}

public sealed class PersonUpsertRequest
{
    [Required]
    [MinLength(2)]
    public string FullName { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Nationality { get; init; } = string.Empty;

    public DateOnly BirthDate { get; init; } = new(2000, 1, 1);

    [Required]
    [MinLength(2)]
    public string PrimaryRole { get; init; } = string.Empty;

    public string Bio { get; init; } = string.Empty;
    public string PhotoUrl { get; init; } = string.Empty;
    public int? TeamId { get; init; }
    public int? ShirtNumber { get; init; }
    public string SquadRole { get; init; } = "Starter";
}

public sealed class MatchUpsertRequest
{
    [Range(1, int.MaxValue)]
    public int CompetitionId { get; init; }

    public int? SeasonId { get; init; }

    [Range(1, int.MaxValue)]
    public int HomeTeamId { get; init; }

    [Range(1, int.MaxValue)]
    public int AwayTeamId { get; init; }

    public DateTimeOffset KickoffUtc { get; init; } = DateTimeOffset.UtcNow.AddDays(1);

    [Required]
    [MinLength(2)]
    public string Status { get; init; } = "Scheduled";

    public int HomeScore { get; init; }
    public int AwayScore { get; init; }
    public string Venue { get; init; } = string.Empty;
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

public interface ITournamentManagementService
{
    SportDto UpsertSport(int? sportId, SportUpsertRequest request, string performedBy);
    void DeleteSport(int sportId, string performedBy);
    CompetitionDto UpsertCompetition(int? competitionId, CompetitionUpsertRequest request, string performedBy);
    void DeleteCompetition(int competitionId, string performedBy);
    TeamDto UpsertTeam(int? teamId, TeamUpsertRequest request, string performedBy);
    void DeleteTeam(int teamId, string performedBy);
    PersonDto UpsertPerson(int? personId, PersonUpsertRequest request, string performedBy);
    void DeletePerson(int personId, string performedBy);
    MatchDto UpsertMatch(int? matchId, MatchUpsertRequest request, string performedBy);
    void DeleteMatch(int matchId, string performedBy);
}