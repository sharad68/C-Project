namespace Tornois.Domain.Models;

public enum AdminRole
{
    Superadmin,
    Editor,
    Readonly
}

public sealed record Sport(
    int Id,
    string Name,
    string Slug,
    string Description,
    bool IsOlympic);

public sealed record Competition(
    int Id,
    int SportId,
    string Name,
    string Country,
    string Format,
    bool IsCup);

public sealed record Season(
    int Id,
    int CompetitionId,
    string Name,
    int YearStart,
    int YearEnd,
    bool IsCurrent);

public sealed record Team(
    int Id,
    int SportId,
    string Name,
    string ShortName,
    string Country,
    string Venue,
    int Founded,
    string BadgeUrl);

public sealed record Person(
    int Id,
    int TeamId,
    string FullName,
    string Role,
    string Nationality,
    DateOnly BirthDate,
    int? ShirtNumber,
    string Bio,
    string PhotoUrl);

public sealed record MatchEvent(
    int Minute,
    string Type,
    string Subject);

public sealed record Match(
    int Id,
    int SportId,
    int CompetitionId,
    int SeasonId,
    int HomeTeamId,
    int AwayTeamId,
    DateTimeOffset KickoffUtc,
    string Status,
    int HomeScore,
    int AwayScore,
    string Venue,
    IReadOnlyList<MatchEvent> Events);

public sealed record Standing(
    int CompetitionId,
    int TeamId,
    int Position,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int GoalsFor,
    int GoalsAgainst,
    int Points);

public sealed record PlayerRanking(
    int CompetitionId,
    int PersonId,
    int Position,
    string Category,
    int Value);

public sealed record AdminUser(
    int Id,
    string UserName,
    string DisplayName,
    AdminRole Role,
    string PasswordHash,
    bool IsActive);

public sealed record ChangeLogEntry(
    int Id,
    string Action,
    string EntityName,
    string AdminUserName,
    DateTimeOffset Timestamp,
    string Summary);