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
        services.AddScoped<TournamentManagementService>();
        services.AddScoped<ISportsPlatformService>(provider => provider.GetRequiredService<DatabaseBackedSportsPlatformService>());
        services.AddScoped<IAdminAuthService>(provider => provider.GetRequiredService<DatabaseBackedSportsPlatformService>());
        services.AddScoped<ITournamentManagementService>(provider => provider.GetRequiredService<TournamentManagementService>());
        return services;
    }
}

internal sealed class SeededSportsPlatformService : ISportsPlatformService, IAdminAuthService
{
    private readonly IReadOnlyList<Sport> _sports =
    [
        new(1, "League of Legends", "league-of-legends", "Publisher-backed MOBA circuit with regional leagues and global finals.", true),
        new(2, "Counter-Strike 2", "counter-strike-2", "Open FPS ecosystem built around majors, circuits, and qualifiers.", false),
        new(3, "Valorant", "valorant", "Structured tactical shooter league with regional stages and Masters events.", true)
    ];

    private readonly IReadOnlyList<Competition> _competitions =
    [
        new(101, 1, "LEC Spring Split", "Europe", "League", false),
        new(102, 1, "Mid-Season Invitational", "Global", "International", true),
        new(201, 2, "PGL Major Copenhagen", "Denmark", "Major", true),
        new(202, 2, "ESL Pro League", "Global", "League", false),
        new(301, 3, "VCT EMEA Stage 1", "Europe", "League", false),
        new(302, 3, "Valorant Masters Toronto", "Canada", "International", true)
    ];

    private readonly IReadOnlyList<Season> _seasons =
    [
        new(1001, 101, "Spring 2026", 2026, 2026, true),
        new(1002, 102, "2026", 2026, 2026, true),
        new(2001, 201, "2026", 2026, 2026, true),
        new(2002, 202, "Season 22", 2026, 2026, true),
        new(3001, 301, "Stage 1 2026", 2026, 2026, true),
        new(3002, 302, "2026", 2026, 2026, true)
    ];

    private readonly IReadOnlyList<Team> _teams =
    [
        new(1, 1, "T1", "T1", "South Korea", "LoL Park, Seoul", 2003, "https://images.unsplash.com/photo-1540747913346-19e32dc3e97e?w=200&q=80"),
        new(2, 1, "G2 Esports", "G2", "Europe", "Berlin Team House", 2015, "https://images.unsplash.com/photo-1518604666860-9ed391f76460?w=200&q=80"),
        new(3, 2, "Natus Vincere", "NAVI", "Ukraine", "NAVI Campus Kyiv", 2009, "https://images.unsplash.com/photo-1574629810360-7efbbe195018?w=200&q=80"),
        new(4, 2, "Team Vitality", "VIT", "France", "V.Hive Paris", 2013, "https://images.unsplash.com/photo-1546519638-68e109498ffc?w=200&q=80"),
        new(5, 3, "Fnatic", "FNC", "United Kingdom", "London HQ", 2004, "https://images.unsplash.com/photo-1519861531473-9200262188bf?w=200&q=80"),
        new(6, 3, "Sentinels", "SEN", "United States", "Los Angeles HQ", 2016, "https://images.unsplash.com/photo-1531415074968-036ba1b575da?w=200&q=80")
    ];

    private readonly IReadOnlyList<Person> _people =
    [
        new(1, 1, "Lee \"Faker\" Sang-hyeok", "Mid Laner", "South Korea", new DateOnly(1996, 5, 7), null, "Legendary shot-caller and franchise player for T1's League of Legends roster.", "https://images.unsplash.com/photo-1522778119026-d647f0596c20?w=200&q=80"),
        new(2, 2, "Rasmus \"Caps\" Winther", "Mid Laner", "Denmark", new DateOnly(1999, 11, 17), null, "Aggressive playmaker known for high-pressure international performances.", "https://images.unsplash.com/photo-1508098682722-e99c643e7485?w=200&q=80"),
        new(3, 3, "Oleksandr \"s1mple\" Kostyliev", "AWPer", "Ukraine", new DateOnly(1997, 10, 2), null, "Elite Counter-Strike superstar with unmatched clutch potential.", "https://images.unsplash.com/photo-1517466787929-bc90951d0974?w=200&q=80"),
        new(4, 4, "Mathieu \"ZywOo\" Herbaut", "AWPer", "France", new DateOnly(2000, 11, 9), null, "Precision-focused star fragger anchoring Vitality's late-round setups.", "https://images.unsplash.com/photo-1519766304817-4f37bda74a26?w=200&q=80"),
        new(5, 5, "Jake \"Boaster\" Howlett", "IGL", "United Kingdom", new DateOnly(1995, 5, 25), null, "Calm in-game leader coordinating Fnatic's tactical rounds and utility usage.", "https://images.unsplash.com/photo-1517649763962-0c623066013b?w=200&q=80"),
        new(6, 6, "Tyson \"TenZ\" Ngo", "Duelist", "Canada", new DateOnly(2001, 5, 5), null, "Explosive entry player known for opening duel wins and stream appeal.", "https://images.unsplash.com/photo-1543357480-c60d40007a3f?w=200&q=80")
    ];

    private readonly IReadOnlyList<Match> _matches =
    [
        new(5001, 1, 101, 1001, 1, 2, DateTimeOffset.UtcNow.AddMinutes(-12), "Live", 1, 0, "LoL Park, Seoul",
        [
            new MatchEvent(6, "first blood", "Faker opens the series with a solo kill in mid."),
            new MatchEvent(18, "objective", "T1 secure soul point after a clean dragon fight."),
            new MatchEvent(29, "baron", "G2 hold the base after a tense Baron standoff.")
        ]),
        new(5002, 2, 201, 2001, 3, 4, DateTimeOffset.UtcNow.AddDays(1), "Scheduled", 0, 0, "Royal Arena, Copenhagen",
        [
            new MatchEvent(0, "preview", "NAVI and Vitality headline tomorrow's major playoff slate.")
        ]),
        new(5003, 3, 301, 3001, 5, 6, DateTimeOffset.UtcNow.AddHours(6), "Scheduled", 0, 0, "Riot Games Arena, Berlin",
        [
            new MatchEvent(0, "preview", "Fnatic and Sentinels meet in a high-pressure cross-regional showcase.")
        ]),
        new(5004, 1, 102, 1002, 2, 1, DateTimeOffset.UtcNow.AddDays(2), "Scheduled", 0, 0, "MSI Main Stage",
        [
            new MatchEvent(0, "preview", "A rematch between G2 and T1 is lined up for the international stage.")
        ])
    ];

    private readonly IReadOnlyList<Standing> _standings =
    [
        new(101, 1, 1, 9, 8, 0, 1, 18, 7, 24),
        new(101, 2, 2, 9, 7, 0, 2, 15, 9, 21),
        new(201, 4, 1, 5, 4, 0, 1, 10, 5, 12),
        new(201, 3, 2, 5, 3, 0, 2, 8, 6, 9),
        new(301, 5, 1, 4, 3, 0, 1, 7, 3, 9),
        new(301, 6, 2, 4, 2, 0, 2, 5, 5, 6)
    ];

    private readonly IReadOnlyList<PlayerRanking> _playerRankings =
    [
        new(101, 1, 1, "kills", 44),
        new(101, 2, 2, "assists", 72),
        new(201, 4, 1, "headshots", 23),
        new(301, 6, 1, "acs", 267)
    ];

    private readonly IReadOnlyList<ChangeLogEntry> _changeLog =
    [
        new(1, "sync", "matches.match", "system", DateTimeOffset.UtcNow.AddMinutes(-12), "Live esports series sync completed for the current tournament slate."),
        new(2, "update", "broadcast.match_broadcast", "editor", DateTimeOffset.UtcNow.AddHours(-3), "Published official stream metadata for VCT EMEA Stage 1."),
        new(3, "verify", "people.person", "superadmin", DateTimeOffset.UtcNow.AddDays(-1), "Verified roster profile enrichment for Faker.")
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