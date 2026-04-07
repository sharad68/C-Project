using Microsoft.Extensions.DependencyInjection;
using Tornois.Application.Contracts;
using Tornois.Domain.Models;

namespace Tornois.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTornoisPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<SeededSportsPlatformService>();
        services.AddScoped<DatabaseBackedSportsPlatformService>();
        services.AddScoped<ISportsPlatformService>(provider => provider.GetRequiredService<DatabaseBackedSportsPlatformService>());
        services.AddScoped<IAdminAuthService>(provider => provider.GetRequiredService<DatabaseBackedSportsPlatformService>());
        return services;
    }
}

internal sealed class SeededSportsPlatformService : ISportsPlatformService, IAdminAuthService
{
    private readonly IReadOnlyList<Sport> _sports =
    [
        new(1, "Football", "football", "Live football coverage across leagues and cups.", true),
        new(2, "Basketball", "basketball", "Top leagues, standings, and player leaderboards.", true),
        new(3, "Cricket", "cricket", "International and franchise cricket schedules.", false)
    ];

    private readonly IReadOnlyList<Competition> _competitions =
    [
        new(101, 1, "Premier League", "England", "League", false),
        new(102, 1, "UEFA Champions League", "Europe", "Cup", true),
        new(201, 2, "NBA", "United States", "League", false),
        new(301, 3, "Indian Premier League", "India", "League", false)
    ];

    private readonly IReadOnlyList<Season> _seasons =
    [
        new(1001, 101, "2025/26", 2025, 2026, true),
        new(1002, 102, "2025/26", 2025, 2026, true),
        new(2001, 201, "2025/26", 2025, 2026, true),
        new(3001, 301, "2026", 2026, 2026, true)
    ];

    private readonly IReadOnlyList<Team> _teams =
    [
        new(1, 1, "Arsenal", "ARS", "England", "Emirates Stadium", 1886, "https://images.unsplash.com/photo-1540747913346-19e32dc3e97e?w=200&q=80"),
        new(2, 1, "Liverpool", "LIV", "England", "Anfield", 1892, "https://images.unsplash.com/photo-1518604666860-9ed391f76460?w=200&q=80"),
        new(3, 1, "Real Madrid", "RMA", "Spain", "Santiago Bernabéu", 1902, "https://images.unsplash.com/photo-1574629810360-7efbbe195018?w=200&q=80"),
        new(4, 2, "Boston Celtics", "BOS", "USA", "TD Garden", 1946, "https://images.unsplash.com/photo-1546519638-68e109498ffc?w=200&q=80"),
        new(5, 2, "Los Angeles Lakers", "LAL", "USA", "Crypto.com Arena", 1947, "https://images.unsplash.com/photo-1519861531473-9200262188bf?w=200&q=80"),
        new(6, 3, "Mumbai Indians", "MI", "India", "Wankhede Stadium", 2008, "https://images.unsplash.com/photo-1531415074968-036ba1b575da?w=200&q=80")
    ];

    private readonly IReadOnlyList<Person> _people =
    [
        new(1, 1, "Bukayo Saka", "Forward", "England", new DateOnly(2001, 9, 5), 7, "Creative winger with elite 1v1 ability and strong final-third output.", "https://images.unsplash.com/photo-1522778119026-d647f0596c20?w=200&q=80"),
        new(2, 1, "Martin Ødegaard", "Midfielder", "Norway", new DateOnly(1998, 12, 17), 8, "Ball-progressing playmaker and team captain.", "https://images.unsplash.com/photo-1508098682722-e99c643e7485?w=200&q=80"),
        new(3, 2, "Virgil van Dijk", "Defender", "Netherlands", new DateOnly(1991, 7, 8), 4, "Dominant centre-back known for aerial control and leadership.", "https://images.unsplash.com/photo-1517466787929-bc90951d0974?w=200&q=80"),
        new(4, 4, "Jayson Tatum", "Forward", "USA", new DateOnly(1998, 3, 3), 0, "Primary scoring option with two-way impact.", "https://images.unsplash.com/photo-1519766304817-4f37bda74a26?w=200&q=80"),
        new(5, 5, "LeBron James", "Forward", "USA", new DateOnly(1984, 12, 30), 23, "Veteran playmaker and all-around leader.", "https://images.unsplash.com/photo-1517649763962-0c623066013b?w=200&q=80"),
        new(6, 6, "Jasprit Bumrah", "Bowler", "India", new DateOnly(1993, 12, 6), null, "Elite fast bowler with deceptive release and death-over accuracy.", "https://images.unsplash.com/photo-1543357480-c60d40007a3f?w=200&q=80")
    ];

    private readonly IReadOnlyList<Match> _matches =
    [
        new(5001, 1, 101, 1001, 1, 2, DateTimeOffset.UtcNow.AddMinutes(-18), "Live", 2, 1, "Emirates Stadium",
        [
            new MatchEvent(11, "goal", "Bukayo Saka"),
            new MatchEvent(19, "goal", "Luis Díaz"),
            new MatchEvent(31, "goal", "Martin Ødegaard")
        ]),
        new(5002, 1, 102, 1002, 3, 1, DateTimeOffset.UtcNow.AddDays(1), "Scheduled", 0, 0, "Santiago Bernabéu",
        [
            new MatchEvent(0, "preview", "Knockout clash loaded for tomorrow night")
        ]),
        new(5003, 2, 201, 2001, 4, 5, DateTimeOffset.UtcNow.AddHours(6), "Scheduled", 0, 0, "TD Garden",
        [
            new MatchEvent(0, "preview", "Eastern conference heavyweight meeting")
        ]),
        new(5004, 3, 301, 3001, 6, 6, DateTimeOffset.UtcNow.AddDays(2), "Scheduled", 0, 0, "Wankhede Stadium",
        [
            new MatchEvent(0, "preview", "Local derby-style franchise showcase")
        ])
    ];

    private readonly IReadOnlyList<Standing> _standings =
    [
        new(101, 1, 1, 29, 20, 5, 4, 61, 28, 65),
        new(101, 2, 2, 29, 19, 6, 4, 58, 30, 63),
        new(102, 3, 1, 8, 6, 1, 1, 18, 7, 19),
        new(201, 4, 1, 62, 47, 0, 15, 7421, 7014, 94),
        new(201, 5, 2, 62, 43, 0, 19, 7354, 7090, 86),
        new(301, 6, 1, 12, 8, 0, 4, 2100, 1950, 16)
    ];

    private readonly IReadOnlyList<PlayerRanking> _playerRankings =
    [
        new(101, 1, 1, "goals", 16),
        new(101, 2, 2, "assists", 11),
        new(201, 4, 1, "points", 28),
        new(201, 5, 2, "assists", 9),
        new(301, 6, 1, "wickets", 18)
    ];

    private readonly IReadOnlyList<ChangeLogEntry> _changeLog =
    [
        new(1, "sync", "matches.match", "system", DateTimeOffset.UtcNow.AddMinutes(-12), "Live score sync completed for 4 fixtures."),
        new(2, "update", "teams.team", "editor", DateTimeOffset.UtcNow.AddHours(-3), "Updated venue metadata for Arsenal."),
        new(3, "verify", "people.person", "superadmin", DateTimeOffset.UtcNow.AddDays(-1), "Confirmed profile enrichment for Jasprit Bumrah.")
    ];

    private readonly IReadOnlyList<AdminUser> _admins =
    [
        new(1, "superadmin", "Platform Superadmin", AdminRole.Superadmin, BCrypt.Net.BCrypt.HashPassword("Pass@123"), true),
        new(2, "editor", "Content Editor", AdminRole.Editor, BCrypt.Net.BCrypt.HashPassword("Editor@123"), true),
        new(3, "readonly", "Read Only Analyst", AdminRole.Readonly, BCrypt.Net.BCrypt.HashPassword("Viewer@123"), true)
    ];

    public PagedResult<SportDto> GetSports(PageRequest request)
        => Paginate(_sports.Select(s => new SportDto(s.Id, s.Name, s.Slug, s.Description, s.IsOlympic)), request);

    public PagedResult<CompetitionDto> GetCompetitions(int? sportId, PageRequest request)
    {
        var items = _competitions
            .Where(c => !sportId.HasValue || c.SportId == sportId.Value)
            .Select(c => new CompetitionDto(c.Id, c.SportId, GetSportName(c.SportId), c.Name, c.Country, c.Format, c.IsCup));

        return Paginate(items, request);
    }

    public PagedResult<SeasonDto> GetSeasons(int competitionId, PageRequest request)
        => Paginate(
            _seasons
                .Where(s => s.CompetitionId == competitionId)
                .Select(s => new SeasonDto(s.Id, s.CompetitionId, s.Name, s.YearStart, s.YearEnd, s.IsCurrent)),
            request);

    public SeasonDetailDto? GetSeasonDetail(int seasonId)
    {
        var season = _seasons.FirstOrDefault(s => s.Id == seasonId);
        if (season is null)
        {
            return null;
        }

        var competition = _competitions.First(c => c.Id == season.CompetitionId);
        return new SeasonDetailDto(
            new CompetitionDto(competition.Id, competition.SportId, GetSportName(competition.SportId), competition.Name, competition.Country, competition.Format, competition.IsCup),
            new SeasonDto(season.Id, season.CompetitionId, season.Name, season.YearStart, season.YearEnd, season.IsCurrent),
            GetStandings(competition.Id));
    }

    public IReadOnlyList<MatchDto> GetLiveMatches()
        => _matches.Where(m => string.Equals(m.Status, "Live", StringComparison.OrdinalIgnoreCase)).Select(MapMatch).ToList();

    public IReadOnlyList<MatchDto> GetUpcomingFixtures(int? sportId, int? competitionId, int count = 10)
        => _matches
            .Where(m => !string.Equals(m.Status, "Live", StringComparison.OrdinalIgnoreCase))
            .Where(m => !sportId.HasValue || m.SportId == sportId.Value)
            .Where(m => !competitionId.HasValue || m.CompetitionId == competitionId.Value)
            .OrderBy(m => m.KickoffUtc)
            .Take(Math.Clamp(count, 1, 20))
            .Select(MapMatch)
            .ToList();

    public MatchDetailDto? GetMatchDetail(int matchId)
    {
        var match = _matches.FirstOrDefault(m => m.Id == matchId);
        return match is null
            ? null
            : new MatchDetailDto(MapMatch(match), match.Events.Select(e => new MatchEventDto(e.Minute, e.Type, e.Subject)).ToList());
    }

    public PagedResult<TeamDto> GetTeams(int? sportId, PageRequest request)
        => Paginate(
            _teams
                .Where(t => !sportId.HasValue || t.SportId == sportId.Value)
                .Select(MapTeam),
            request);

    public TeamRosterDto? GetTeamRoster(int teamId)
    {
        var team = _teams.FirstOrDefault(t => t.Id == teamId);
        if (team is null)
        {
            return null;
        }

        var members = _people.Where(p => p.TeamId == teamId).Select(MapPerson).ToList();
        return new TeamRosterDto(MapTeam(team), members);
    }

    public PersonProfileDto? GetPerson(int personId)
    {
        var person = _people.FirstOrDefault(p => p.Id == personId);
        if (person is null)
        {
            return null;
        }

        var teamName = _teams.First(t => t.Id == person.TeamId).Name;
        return new PersonProfileDto(MapPerson(person), person.Bio, teamName);
    }

    public PagedResult<PersonDto> SearchPeople(string? query, PageRequest request)
    {
        var items = _people
            .Where(p => string.IsNullOrWhiteSpace(query)
                || p.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || p.Role.Contains(query, StringComparison.OrdinalIgnoreCase)
                || p.Nationality.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Select(MapPerson);

        return Paginate(items, request);
    }

    public IReadOnlyList<StandingDto> GetStandings(int competitionId)
        => _standings
            .Where(s => s.CompetitionId == competitionId)
            .OrderBy(s => s.Position)
            .Select(s => new StandingDto(s.Position, GetTeamName(s.TeamId), s.Played, s.Won, s.Drawn, s.Lost, s.GoalsFor, s.GoalsAgainst, s.Points))
            .ToList();

    public IReadOnlyList<PlayerRankingDto> GetPlayerRankings(int competitionId, string category)
        => _playerRankings
            .Where(r => r.CompetitionId == competitionId)
            .Where(r => string.IsNullOrWhiteSpace(category) || string.Equals(r.Category, category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.Position)
            .Select(r =>
            {
                var person = _people.First(p => p.Id == r.PersonId);
                return new PlayerRankingDto(r.Position, person.FullName, GetTeamName(person.TeamId), r.Category, r.Value);
            })
            .ToList();

    public IReadOnlyList<ChangeLogDto> GetRecentChanges()
        => _changeLog
            .OrderByDescending(c => c.Timestamp)
            .Select(c => new ChangeLogDto(c.Id, c.Action, c.EntityName, c.AdminUserName, c.Timestamp, c.Summary))
            .ToList();

    public AdminIdentityDto? ValidateCredentials(string userName, string password)
    {
        var admin = _admins.FirstOrDefault(a => a.IsActive && string.Equals(a.UserName, userName, StringComparison.OrdinalIgnoreCase));
        if (admin is null || !BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
        {
            return null;
        }

        return MapAdmin(admin);
    }

    public IReadOnlyList<AdminIdentityDto> GetAdmins() => _admins.Select(MapAdmin).ToList();

    public AdminIdentityDto UpsertAdminUser(AdminUserUpsertRequest request, string performedBy)
    {
        var normalizedRole = request.Role.Trim().ToLowerInvariant();
        if (normalizedRole is not ("superadmin" or "editor" or "readonly"))
        {
            throw new InvalidOperationException("Unsupported admin role supplied.");
        }

        return new AdminIdentityDto(request.UserName.Trim().ToLowerInvariant(), request.DisplayName.Trim(), normalizedRole);
    }

    private static PagedResult<T> Paginate<T>(IEnumerable<T> source, PageRequest request)
    {
        var safePage = Math.Max(request.Page, 1);
        var safePageSize = Math.Clamp(request.PageSize, 1, 100);
        var items = source.ToList();
        var pagedItems = items.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToList();

        return new PagedResult<T>(pagedItems, safePage, safePageSize, items.Count, safePage * safePageSize < items.Count);
    }

    private MatchDto MapMatch(Match match) => new(
        match.Id,
        match.CompetitionId,
        match.SeasonId,
        match.SportId,
        _competitions.First(c => c.Id == match.CompetitionId).Name,
        GetTeamName(match.HomeTeamId),
        GetTeamName(match.AwayTeamId),
        match.KickoffUtc,
        match.Status,
        match.HomeScore,
        match.AwayScore,
        match.Venue);

    private PersonDto MapPerson(Person person) => new(
        person.Id,
        person.TeamId,
        person.FullName,
        person.Role,
        person.Nationality,
        person.BirthDate,
        person.ShirtNumber,
        person.PhotoUrl);

    private TeamDto MapTeam(Team team) => new(
        team.Id,
        team.SportId,
        team.Name,
        team.ShortName,
        team.Country,
        team.Venue,
        team.Founded,
        team.BadgeUrl);

    private AdminIdentityDto MapAdmin(AdminUser admin) => new(admin.UserName, admin.DisplayName, admin.Role.ToString().ToLowerInvariant());

    private string GetSportName(int sportId) => _sports.First(s => s.Id == sportId).Name;

    private string GetTeamName(int teamId) => _teams.First(t => t.Id == teamId).Name;
}