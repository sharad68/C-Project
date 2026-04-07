using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tornois.Application.Contracts;
using Tornois.Infrastructure.Data;

namespace Tornois.Infrastructure;

internal sealed class DatabaseBackedSportsPlatformService(
    TornoisDbContext dbContext,
    SeededSportsPlatformService fallback,
    ILogger<DatabaseBackedSportsPlatformService> logger) : ISportsPlatformService, IAdminAuthService
{
    public PagedResult<SportDto> GetSports(PageRequest request)
    {
        if (!CanUseDatabase())
        {
            return fallback.GetSports(request);
        }

        var query = dbContext.Sports
            .AsNoTracking()
            .OrderBy(sport => sport.Name)
            .Select(sport => new SportDto(sport.Id, sport.Name, sport.Slug, sport.Description, sport.IsOlympic));

        return Paginate(query, request);
    }

    public PagedResult<CompetitionDto> GetCompetitions(int? sportId, PageRequest request)
    {
        if (!CanUseDatabase())
        {
            return fallback.GetCompetitions(sportId, request);
        }

        var sports = dbContext.Sports.AsNoTracking().ToDictionary(sport => sport.Id, sport => sport.Name);
        var items = dbContext.Competitions
            .AsNoTracking()
            .Where(competition => !sportId.HasValue || competition.SportId == sportId.Value)
            .OrderBy(competition => competition.Name)
            .AsEnumerable()
            .Select(competition => new CompetitionDto(
                competition.Id,
                competition.SportId,
                sports.GetValueOrDefault(competition.SportId, "Unknown sport"),
                competition.Name,
                competition.Country,
                competition.Format,
                competition.IsCup))
            .AsQueryable();

        return Paginate(items, request);
    }

    public PagedResult<SeasonDto> GetSeasons(int competitionId, PageRequest request)
    {
        if (!CanUseDatabase())
        {
            return fallback.GetSeasons(competitionId, request);
        }

        var query = dbContext.Seasons
            .AsNoTracking()
            .Where(season => season.CompetitionId == competitionId)
            .OrderByDescending(season => season.YearStart)
            .Select(season => new SeasonDto(season.Id, season.CompetitionId, season.Name, season.YearStart, season.YearEnd, season.IsCurrent));

        return Paginate(query, request);
    }

    public SeasonDetailDto? GetSeasonDetail(int seasonId)
    {
        if (!CanUseDatabase())
        {
            return fallback.GetSeasonDetail(seasonId);
        }

        var season = dbContext.Seasons.AsNoTracking().FirstOrDefault(entry => entry.Id == seasonId);
        if (season is null)
        {
            return fallback.GetSeasonDetail(seasonId);
        }

        var competition = dbContext.Competitions.AsNoTracking().First(entry => entry.Id == season.CompetitionId);
        var sportName = dbContext.Sports.AsNoTracking().Where(entry => entry.Id == competition.SportId).Select(entry => entry.Name).FirstOrDefault() ?? "Unknown sport";

        return new SeasonDetailDto(
            new CompetitionDto(competition.Id, competition.SportId, sportName, competition.Name, competition.Country, competition.Format, competition.IsCup),
            new SeasonDto(season.Id, season.CompetitionId, season.Name, season.YearStart, season.YearEnd, season.IsCurrent),
            GetStandings(competition.Id));
    }

    public IReadOnlyList<MatchDto> GetLiveMatches()
    {
        if (!CanUseDatabase())
        {
            return fallback.GetLiveMatches();
        }

        var matches = dbContext.Matches.AsNoTracking().Where(match => match.Status == "Live").OrderBy(match => match.KickoffUtc).ToList();
        return matches.Count == 0 ? fallback.GetLiveMatches() : MapMatches(matches);
    }

    public IReadOnlyList<MatchDto> GetUpcomingFixtures(int? sportId, int? competitionId, int count = 10)
    {
        if (!CanUseDatabase())
        {
            return fallback.GetUpcomingFixtures(sportId, competitionId, count);
        }

        var matches = dbContext.Matches
            .AsNoTracking()
            .Where(match => match.Status != "Live")
            .Where(match => !sportId.HasValue || match.SportId == sportId.Value)
            .Where(match => !competitionId.HasValue || match.CompetitionId == competitionId.Value)
            .OrderBy(match => match.KickoffUtc)
            .Take(Math.Clamp(count, 1, 20))
            .ToList();

        return matches.Count == 0 ? fallback.GetUpcomingFixtures(sportId, competitionId, count) : MapMatches(matches);
    }

    public MatchDetailDto? GetMatchDetail(int matchId)
    {
        if (!CanUseDatabase())
        {
            return fallback.GetMatchDetail(matchId);
        }

        var match = dbContext.Matches.AsNoTracking().FirstOrDefault(entry => entry.Id == matchId);
        if (match is null)
        {
            return fallback.GetMatchDetail(matchId);
        }

        var events = dbContext.MatchEvents
            .AsNoTracking()
            .Where(entry => entry.MatchId == matchId)
            .OrderBy(entry => entry.Minute)
            .Select(entry => new MatchEventDto(entry.Minute, entry.Type, entry.Subject))
            .ToList();

        return new MatchDetailDto(MapMatches([match]).Single(), events);
    }

    public PagedResult<TeamDto> GetTeams(int? sportId, PageRequest request)
    {
        if (!CanUseDatabase())
        {
            return fallback.GetTeams(sportId, request);
        }

        var query = dbContext.Teams
            .AsNoTracking()
            .Where(team => !sportId.HasValue || team.SportId == sportId.Value)
            .OrderBy(team => team.Name)
            .Select(team => new TeamDto(team.Id, team.SportId, team.Name, team.ShortName, team.Country, team.Venue, team.Founded, team.BadgeUrl));

        return Paginate(query, request);
    }

    public TeamRosterDto? GetTeamRoster(int teamId)
    {
        if (!CanUseDatabase())
        {
            return fallback.GetTeamRoster(teamId);
        }

        var team = dbContext.Teams.AsNoTracking().FirstOrDefault(entry => entry.Id == teamId);
        if (team is null)
        {
            return fallback.GetTeamRoster(teamId);
        }

        var latestTeamSeason = dbContext.TeamSeasons.AsNoTracking().Where(entry => entry.TeamId == teamId).OrderByDescending(entry => entry.SeasonId).FirstOrDefault();
        var members = latestTeamSeason is null
            ? []
            : dbContext.TeamMembers
                .AsNoTracking()
                .Where(entry => entry.TeamSeasonId == latestTeamSeason.Id)
                .Join(
                    dbContext.People.AsNoTracking(),
                    member => member.PersonId,
                    person => person.Id,
                    (member, person) => new PersonDto(
                        person.Id,
                        teamId,
                        person.FullName,
                        person.PrimaryRole,
                        person.Nationality,
                        person.BirthDate,
                        member.ShirtNumber,
                        person.PhotoUrl))
                .OrderBy(member => member.FullName)
                .ToList();

        if (members.Count == 0)
        {
            return fallback.GetTeamRoster(teamId);
        }

        return new TeamRosterDto(new TeamDto(team.Id, team.SportId, team.Name, team.ShortName, team.Country, team.Venue, team.Founded, team.BadgeUrl), members);
    }

    public PersonProfileDto? GetPerson(int personId)
    {
        if (!CanUseDatabase())
        {
            return fallback.GetPerson(personId);
        }

        var person = dbContext.People.AsNoTracking().FirstOrDefault(entry => entry.Id == personId);
        if (person is null)
        {
            return fallback.GetPerson(personId);
        }

        var membership = dbContext.TeamMembers
            .AsNoTracking()
            .Join(dbContext.TeamSeasons.AsNoTracking(), member => member.TeamSeasonId, season => season.Id, (member, season) => new { member, season })
            .Where(entry => entry.member.PersonId == personId)
            .OrderByDescending(entry => entry.season.SeasonId)
            .FirstOrDefault();

        var teamName = membership is null
            ? "Unassigned"
            : dbContext.Teams.AsNoTracking().Where(team => team.Id == membership.season.TeamId).Select(team => team.Name).FirstOrDefault() ?? "Unassigned";

        return new PersonProfileDto(
            new PersonDto(person.Id, membership?.season.TeamId ?? 0, person.FullName, person.PrimaryRole, person.Nationality, person.BirthDate, membership?.member.ShirtNumber, person.PhotoUrl),
            person.Bio,
            teamName);
    }

    public PagedResult<PersonDto> SearchPeople(string? query, PageRequest request)
    {
        if (!CanUseDatabase())
        {
            return fallback.SearchPeople(query, request);
        }

        var memberships = dbContext.TeamMembers
            .AsNoTracking()
            .Join(dbContext.TeamSeasons.AsNoTracking(), member => member.TeamSeasonId, season => season.Id, (member, season) => new { member.PersonId, season.TeamId, member.ShirtNumber, season.SeasonId })
            .ToList()
            .GroupBy(entry => entry.PersonId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(entry => entry.SeasonId).First());

        var people = dbContext.People
            .AsNoTracking()
            .AsEnumerable()
            .Where(person => string.IsNullOrWhiteSpace(query)
                || person.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || person.PrimaryRole.Contains(query, StringComparison.OrdinalIgnoreCase)
                || person.Nationality.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(person => person.FullName)
            .Select(person =>
            {
                memberships.TryGetValue(person.Id, out var membership);
                return new PersonDto(person.Id, membership?.TeamId ?? 0, person.FullName, person.PrimaryRole, person.Nationality, person.BirthDate, membership?.ShirtNumber, person.PhotoUrl);
            })
            .AsQueryable();

        return Paginate(people, request);
    }

    public IReadOnlyList<StandingDto> GetStandings(int competitionId)
    {
        if (!CanUseDatabase())
        {
            return fallback.GetStandings(competitionId);
        }

        var teams = dbContext.Teams.AsNoTracking().ToDictionary(team => team.Id, team => team.Name);
        var standings = dbContext.Standings
            .AsNoTracking()
            .Where(entry => entry.CompetitionId == competitionId)
            .OrderBy(entry => entry.Position)
            .ToList()
            .Select(entry => new StandingDto(entry.Position, teams.GetValueOrDefault(entry.TeamId, "Unknown team"), entry.Played, entry.Won, entry.Drawn, entry.Lost, entry.GoalsFor, entry.GoalsAgainst, entry.Points))
            .ToList();

        return standings.Count == 0 ? fallback.GetStandings(competitionId) : standings;
    }

    public IReadOnlyList<PlayerRankingDto> GetPlayerRankings(int competitionId, string category)
    {
        if (!CanUseDatabase())
        {
            return fallback.GetPlayerRankings(competitionId, category);
        }

        var rankings = dbContext.IndividualRankings
            .AsNoTracking()
            .Where(entry => entry.CompetitionId == competitionId)
            .Where(entry => string.IsNullOrWhiteSpace(category) || entry.Category == category)
            .Join(dbContext.PersonRankings.AsNoTracking(), ranking => ranking.Id, personRanking => personRanking.RankingId, (ranking, personRanking) => new { ranking, personRanking })
            .OrderBy(entry => entry.personRanking.Position)
            .ToList();

        if (rankings.Count == 0)
        {
            return fallback.GetPlayerRankings(competitionId, category);
        }

        var people = dbContext.People.AsNoTracking().ToDictionary(person => person.Id, person => person.FullName);
        var teams = dbContext.Teams.AsNoTracking().ToDictionary(team => team.Id, team => team.Name);

        return rankings
            .Select(entry => new PlayerRankingDto(
                entry.personRanking.Position,
                people.GetValueOrDefault(entry.personRanking.PersonId, "Unknown person"),
                teams.GetValueOrDefault(entry.personRanking.TeamId, "Unknown team"),
                entry.ranking.Category,
                entry.personRanking.Value))
            .ToList();
    }

    public IReadOnlyList<ChangeLogDto> GetRecentChanges()
    {
        if (!CanUseDatabase())
        {
            return fallback.GetRecentChanges();
        }

        var admins = dbContext.AdminUsers.AsNoTracking().ToDictionary(admin => admin.Id, admin => admin.UserName);
        var changes = dbContext.ChangeLogs
            .AsNoTracking()
            .OrderByDescending(change => change.Timestamp)
            .Take(20)
            .ToList()
            .Select(change => new ChangeLogDto(change.Id, change.Action, change.EntityName, change.AdminUserId.HasValue ? admins.GetValueOrDefault(change.AdminUserId.Value, "system") : "system", change.Timestamp, change.Summary))
            .ToList();

        return changes.Count == 0 ? fallback.GetRecentChanges() : changes;
    }

    public AdminIdentityDto? ValidateCredentials(string userName, string password)
    {
        if (!CanUseDatabase())
        {
            return fallback.ValidateCredentials(userName, password);
        }

        var admin = dbContext.AdminUsers.FirstOrDefault(entry => entry.IsActive && entry.UserName == userName);
        if (admin is null || !BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
        {
            return fallback.ValidateCredentials(userName, password);
        }

        admin.LastLoginAtUtc = DateTimeOffset.UtcNow;
        dbContext.SaveChanges();
        return new AdminIdentityDto(admin.UserName, admin.DisplayName, admin.Role);
    }

    public IReadOnlyList<AdminIdentityDto> GetAdmins()
    {
        if (!CanUseDatabase())
        {
            return fallback.GetAdmins();
        }

        return dbContext.AdminUsers
            .AsNoTracking()
            .OrderBy(admin => admin.UserName)
            .Select(admin => new AdminIdentityDto(admin.UserName, admin.DisplayName, admin.Role))
            .ToList();
    }

    public AdminIdentityDto UpsertAdminUser(AdminUserUpsertRequest request, string performedBy)
    {
        if (!CanUseDatabase())
        {
            throw new InvalidOperationException("Admin user management requires a configured PostgreSQL database.");
        }

        var role = request.Role.Trim().ToLowerInvariant();
        if (role is not ("superadmin" or "editor" or "readonly"))
        {
            throw new InvalidOperationException("Unsupported admin role supplied.");
        }

        var normalizedUserName = request.UserName.Trim().ToLowerInvariant();
        var action = "update";
        var admin = dbContext.AdminUsers.FirstOrDefault(entry => entry.UserName == normalizedUserName);

        if (admin is null)
        {
            action = "create";
            admin = new AdminUserEntity
            {
                UserName = normalizedUserName,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            dbContext.AdminUsers.Add(admin);
        }

        admin.DisplayName = request.DisplayName.Trim();
        admin.Role = role;
        admin.IsActive = request.IsActive;
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var actorId = dbContext.AdminUsers.AsNoTracking().Where(entry => entry.UserName == performedBy).Select(entry => (int?)entry.Id).FirstOrDefault();
        dbContext.ChangeLogs.Add(new ChangeLogEntity
        {
            Action = action,
            EntityName = "admin.admin_user",
            AdminUserId = actorId,
            Summary = $"{performedBy} {action}d admin user '{normalizedUserName}' with role '{role}'.",
            AfterJson = $"{{\"userName\":\"{normalizedUserName}\",\"role\":\"{role}\",\"isActive\":{request.IsActive.ToString().ToLowerInvariant()}}}"
        });

        dbContext.SaveChanges();
        return new AdminIdentityDto(admin.UserName, admin.DisplayName, admin.Role);
    }

    private bool CanUseDatabase()
    {
        try
        {
            return dbContext.Database.CanConnect() && dbContext.Sports.AsNoTracking().Any();
        }
        catch (Exception exception)
        {
            logger.LogDebug(exception, "Falling back to the in-memory sports platform service because the PostgreSQL database is unavailable.");
            return false;
        }
    }

    private IReadOnlyList<MatchDto> MapMatches(IEnumerable<MatchEntity> matches)
    {
        var materializedMatches = matches.ToList();
        var competitionIds = materializedMatches.Select(match => match.CompetitionId).Distinct().ToList();
        var teamIds = materializedMatches.SelectMany(match => new[] { match.HomeTeamId, match.AwayTeamId }).Distinct().ToList();

        var competitions = dbContext.Competitions.AsNoTracking().Where(entry => competitionIds.Contains(entry.Id)).ToDictionary(entry => entry.Id, entry => entry.Name);
        var teams = dbContext.Teams.AsNoTracking().Where(entry => teamIds.Contains(entry.Id)).ToDictionary(entry => entry.Id, entry => entry.Name);

        return materializedMatches
            .Select(match => new MatchDto(
                match.Id,
                match.CompetitionId,
                match.SeasonId,
                match.SportId,
                competitions.GetValueOrDefault(match.CompetitionId, "Unknown competition"),
                teams.GetValueOrDefault(match.HomeTeamId, "Unknown team"),
                teams.GetValueOrDefault(match.AwayTeamId, "Unknown team"),
                match.KickoffUtc,
                match.Status,
                match.HomeScore,
                match.AwayScore,
                match.Venue))
            .ToList();
    }

    private static PagedResult<T> Paginate<T>(IQueryable<T> source, PageRequest request)
    {
        var safePage = Math.Max(request.Page, 1);
        var safePageSize = Math.Clamp(request.PageSize, 1, 100);
        var totalCount = source.Count();
        var items = source.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToList();
        return new PagedResult<T>(items, safePage, safePageSize, totalCount, safePage * safePageSize < totalCount);
    }
}