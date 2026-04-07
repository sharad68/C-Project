using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tornois.Application.Contracts;
using Tornois.Infrastructure.Data;

namespace Tornois.Infrastructure;

internal sealed class TournamentManagementService(TornoisDbContext dbContext) : ITournamentManagementService
{
    public SportDto UpsertSport(int? sportId, SportUpsertRequest request, string performedBy)
    {
        EnsureDatabase();

        var isCreate = !sportId.HasValue;
        var sport = isCreate
            ? new SportEntity { CreatedAtUtc = DateTimeOffset.UtcNow }
            : dbContext.Sports.FirstOrDefault(entry => entry.Id == sportId!.Value)
                ?? throw new KeyNotFoundException("Game title not found.");

        var beforeJson = isCreate ? null : JsonSerializer.Serialize(MapSport(sport));

        sport.Name = request.Name.Trim();
        sport.Slug = request.Slug.Trim().ToLowerInvariant();
        sport.Description = request.Description.Trim();
        sport.IsOlympic = request.IsOlympic;

        if (isCreate)
        {
            dbContext.Sports.Add(sport);
        }

        dbContext.SaveChanges();

        var dto = MapSport(sport);
        AppendChangeLog(
            isCreate ? "create" : "update",
            "sports.sport",
            performedBy,
            beforeJson,
            JsonSerializer.Serialize(dto),
            $"{performedBy} {(isCreate ? "created" : "updated")} game title '{dto.Name}'.");

        dbContext.SaveChanges();
        return dto;
    }

    public void DeleteSport(int sportId, string performedBy)
    {
        EnsureDatabase();

        var sport = dbContext.Sports.FirstOrDefault(entry => entry.Id == sportId)
            ?? throw new KeyNotFoundException("Game title not found.");

        var beforeJson = JsonSerializer.Serialize(MapSport(sport));
        var competitionIds = dbContext.Competitions
            .Where(entry => entry.SportId == sportId)
            .Select(entry => entry.Id)
            .ToList();

        var seasonIds = dbContext.Seasons
            .Where(entry => competitionIds.Contains(entry.CompetitionId))
            .Select(entry => entry.Id)
            .ToList();

        var teamIds = dbContext.Teams
            .Where(entry => entry.SportId == sportId)
            .Select(entry => entry.Id)
            .ToList();

        var rankingIds = dbContext.IndividualRankings
            .Where(entry => competitionIds.Contains(entry.CompetitionId) || seasonIds.Contains(entry.SeasonId))
            .Select(entry => entry.Id)
            .ToList();

        var matchIds = dbContext.Matches
            .Where(entry => entry.SportId == sportId || competitionIds.Contains(entry.CompetitionId) || teamIds.Contains(entry.HomeTeamId) || teamIds.Contains(entry.AwayTeamId))
            .Select(entry => entry.Id)
            .ToList();

        DeleteMatchDependencies(matchIds);
        dbContext.Matches.RemoveRange(dbContext.Matches.Where(entry => matchIds.Contains(entry.Id)));

        dbContext.PersonRankings.RemoveRange(dbContext.PersonRankings.Where(entry => rankingIds.Contains(entry.RankingId) || teamIds.Contains(entry.TeamId)));
        dbContext.IndividualRankings.RemoveRange(dbContext.IndividualRankings.Where(entry => rankingIds.Contains(entry.Id)));
        dbContext.Standings.RemoveRange(dbContext.Standings.Where(entry => competitionIds.Contains(entry.CompetitionId) || seasonIds.Contains(entry.SeasonId) || teamIds.Contains(entry.TeamId)));

        var teamSeasonIds = dbContext.TeamSeasons.Where(entry => teamIds.Contains(entry.TeamId)).Select(entry => entry.Id).ToList();
        dbContext.TeamMembers.RemoveRange(dbContext.TeamMembers.Where(entry => teamSeasonIds.Contains(entry.TeamSeasonId)));
        dbContext.TeamSeasons.RemoveRange(dbContext.TeamSeasons.Where(entry => teamIds.Contains(entry.TeamId)));
        dbContext.Teams.RemoveRange(dbContext.Teams.Where(entry => teamIds.Contains(entry.Id)));

        dbContext.Seasons.RemoveRange(dbContext.Seasons.Where(entry => seasonIds.Contains(entry.Id)));
        dbContext.Competitions.RemoveRange(dbContext.Competitions.Where(entry => competitionIds.Contains(entry.Id)));
        dbContext.Sports.Remove(sport);

        AppendChangeLog(
            "delete",
            "sports.sport",
            performedBy,
            beforeJson,
            null,
            $"{performedBy} deleted game title '{sport.Name}' and its dependent tournament data.");

        dbContext.SaveChanges();
    }

    public CompetitionDto UpsertCompetition(int? competitionId, CompetitionUpsertRequest request, string performedBy)
    {
        EnsureDatabase();

        var sport = dbContext.Sports.FirstOrDefault(entry => entry.Id == request.SportId)
            ?? throw new InvalidOperationException("Select a valid game title before saving a tournament.");

        var isCreate = !competitionId.HasValue;
        var competition = isCreate
            ? new CompetitionEntity { CreatedAtUtc = DateTimeOffset.UtcNow }
            : dbContext.Competitions.FirstOrDefault(entry => entry.Id == competitionId!.Value)
                ?? throw new KeyNotFoundException("Tournament not found.");

        var beforeJson = isCreate ? null : JsonSerializer.Serialize(MapCompetition(competition));

        competition.SportId = request.SportId;
        competition.Name = request.Name.Trim();
        competition.Country = request.Country.Trim();
        competition.Format = request.Format.Trim();
        competition.IsCup = request.IsCup;
        competition.ExternalSource = "manual-admin";

        if (isCreate)
        {
            dbContext.Competitions.Add(competition);
        }

        dbContext.SaveChanges();

        var season = dbContext.Seasons
            .Where(entry => entry.CompetitionId == competition.Id)
            .OrderByDescending(entry => entry.IsCurrent)
            .ThenByDescending(entry => entry.YearStart)
            .FirstOrDefault();

        if (season is null)
        {
            season = new SeasonEntity
            {
                CompetitionId = competition.Id,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            dbContext.Seasons.Add(season);
        }

        season.Name = request.SeasonName.Trim();
        season.YearStart = request.YearStart;
        season.YearEnd = request.YearEnd;
        season.IsCurrent = request.IsCurrent;

        dbContext.SaveChanges();

        var dto = MapCompetition(competition);
        AppendChangeLog(
            isCreate ? "create" : "update",
            "sports.competition",
            performedBy,
            beforeJson,
            JsonSerializer.Serialize(new { competition = dto, season = new SeasonDto(season.Id, season.CompetitionId, season.Name, season.YearStart, season.YearEnd, season.IsCurrent) }),
            $"{performedBy} {(isCreate ? "created" : "updated")} tournament '{dto.Name}'.");

        dbContext.SaveChanges();
        return dto;
    }

    public void DeleteCompetition(int competitionId, string performedBy)
    {
        EnsureDatabase();

        var competition = dbContext.Competitions.FirstOrDefault(entry => entry.Id == competitionId)
            ?? throw new KeyNotFoundException("Tournament not found.");

        var beforeJson = JsonSerializer.Serialize(MapCompetition(competition));
        var seasonIds = dbContext.Seasons.Where(entry => entry.CompetitionId == competitionId).Select(entry => entry.Id).ToList();
        var rankingIds = dbContext.IndividualRankings.Where(entry => entry.CompetitionId == competitionId || seasonIds.Contains(entry.SeasonId)).Select(entry => entry.Id).ToList();
        var matchIds = dbContext.Matches.Where(entry => entry.CompetitionId == competitionId).Select(entry => entry.Id).ToList();

        DeleteMatchDependencies(matchIds);
        dbContext.Matches.RemoveRange(dbContext.Matches.Where(entry => matchIds.Contains(entry.Id)));
        dbContext.PersonRankings.RemoveRange(dbContext.PersonRankings.Where(entry => rankingIds.Contains(entry.RankingId)));
        dbContext.IndividualRankings.RemoveRange(dbContext.IndividualRankings.Where(entry => rankingIds.Contains(entry.Id)));
        dbContext.Standings.RemoveRange(dbContext.Standings.Where(entry => entry.CompetitionId == competitionId || seasonIds.Contains(entry.SeasonId)));
        dbContext.Seasons.RemoveRange(dbContext.Seasons.Where(entry => seasonIds.Contains(entry.Id)));
        dbContext.Competitions.Remove(competition);

        AppendChangeLog(
            "delete",
            "sports.competition",
            performedBy,
            beforeJson,
            null,
            $"{performedBy} deleted tournament '{competition.Name}'.");

        dbContext.SaveChanges();
    }

    public TeamDto UpsertTeam(int? teamId, TeamUpsertRequest request, string performedBy)
    {
        EnsureDatabase();

        _ = dbContext.Sports.FirstOrDefault(entry => entry.Id == request.SportId)
            ?? throw new InvalidOperationException("Select a valid game title before saving a team.");

        var isCreate = !teamId.HasValue;
        var team = isCreate
            ? new TeamEntity { CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow }
            : dbContext.Teams.FirstOrDefault(entry => entry.Id == teamId!.Value)
                ?? throw new KeyNotFoundException("Team not found.");

        var beforeJson = isCreate ? null : JsonSerializer.Serialize(MapTeam(team));

        team.SportId = request.SportId;
        team.Name = request.Name.Trim();
        team.ShortName = request.ShortName.Trim();
        team.Country = request.Country.Trim();
        team.Venue = request.Venue.Trim();
        team.Founded = request.Founded;
        team.BadgeUrl = request.BadgeUrl.Trim();
        team.UpdatedAtUtc = DateTimeOffset.UtcNow;

        if (isCreate)
        {
            dbContext.Teams.Add(team);
        }

        dbContext.SaveChanges();

        var dto = MapTeam(team);
        AppendChangeLog(
            isCreate ? "create" : "update",
            "teams.team",
            performedBy,
            beforeJson,
            JsonSerializer.Serialize(dto),
            $"{performedBy} {(isCreate ? "created" : "updated")} team '{dto.Name}'.");

        dbContext.SaveChanges();
        return dto;
    }

    public void DeleteTeam(int teamId, string performedBy)
    {
        EnsureDatabase();

        var team = dbContext.Teams.FirstOrDefault(entry => entry.Id == teamId)
            ?? throw new KeyNotFoundException("Team not found.");

        var beforeJson = JsonSerializer.Serialize(MapTeam(team));
        var matchIds = dbContext.Matches.Where(entry => entry.HomeTeamId == teamId || entry.AwayTeamId == teamId).Select(entry => entry.Id).ToList();
        DeleteMatchDependencies(matchIds);
        dbContext.Matches.RemoveRange(dbContext.Matches.Where(entry => matchIds.Contains(entry.Id)));

        dbContext.PersonRankings.RemoveRange(dbContext.PersonRankings.Where(entry => entry.TeamId == teamId));
        dbContext.Standings.RemoveRange(dbContext.Standings.Where(entry => entry.TeamId == teamId));

        var teamSeasonIds = dbContext.TeamSeasons.Where(entry => entry.TeamId == teamId).Select(entry => entry.Id).ToList();
        dbContext.TeamMembers.RemoveRange(dbContext.TeamMembers.Where(entry => teamSeasonIds.Contains(entry.TeamSeasonId)));
        dbContext.TeamSeasons.RemoveRange(dbContext.TeamSeasons.Where(entry => teamSeasonIds.Contains(entry.Id)));
        dbContext.Teams.Remove(team);

        AppendChangeLog(
            "delete",
            "teams.team",
            performedBy,
            beforeJson,
            null,
            $"{performedBy} deleted team '{team.Name}'.");

        dbContext.SaveChanges();
    }

    public PersonDto UpsertPerson(int? personId, PersonUpsertRequest request, string performedBy)
    {
        EnsureDatabase();

        var isCreate = !personId.HasValue;
        var person = isCreate
            ? new PersonEntity { CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow }
            : dbContext.People.FirstOrDefault(entry => entry.Id == personId!.Value)
                ?? throw new KeyNotFoundException("Player or staff member not found.");

        var beforeJson = isCreate ? null : JsonSerializer.Serialize(BuildPersonDto(person.Id));

        person.FullName = request.FullName.Trim();
        person.FirstName = string.IsNullOrWhiteSpace(request.FirstName)
            ? request.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? request.FullName.Trim()
            : request.FirstName.Trim();
        person.LastName = string.IsNullOrWhiteSpace(request.LastName)
            ? request.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? request.FullName.Trim()
            : request.LastName.Trim();
        person.Nationality = request.Nationality.Trim();
        person.BirthDate = request.BirthDate;
        person.PrimaryRole = request.PrimaryRole.Trim();
        person.Bio = request.Bio.Trim();
        person.PhotoUrl = request.PhotoUrl.Trim();
        person.UpdatedAtUtc = DateTimeOffset.UtcNow;

        if (isCreate)
        {
            dbContext.People.Add(person);
        }

        dbContext.SaveChanges();
        SyncTeamMembership(person.Id, request);
        dbContext.SaveChanges();

        var dto = BuildPersonDto(person.Id);
        AppendChangeLog(
            isCreate ? "create" : "update",
            "people.person",
            performedBy,
            beforeJson,
            JsonSerializer.Serialize(dto),
            $"{performedBy} {(isCreate ? "created" : "updated")} roster profile '{dto.FullName}'.");

        dbContext.SaveChanges();
        return dto;
    }

    public void DeletePerson(int personId, string performedBy)
    {
        EnsureDatabase();

        var person = dbContext.People.FirstOrDefault(entry => entry.Id == personId)
            ?? throw new KeyNotFoundException("Player or staff member not found.");

        var beforeJson = JsonSerializer.Serialize(BuildPersonDto(personId));

        foreach (var matchEvent in dbContext.MatchEvents.Where(entry => entry.PersonId == personId))
        {
            matchEvent.PersonId = null;
        }

        dbContext.TeamMembers.RemoveRange(dbContext.TeamMembers.Where(entry => entry.PersonId == personId));
        dbContext.PlayerStats.RemoveRange(dbContext.PlayerStats.Where(entry => entry.PersonId == personId));
        dbContext.PersonRankings.RemoveRange(dbContext.PersonRankings.Where(entry => entry.PersonId == personId));
        dbContext.People.Remove(person);

        AppendChangeLog(
            "delete",
            "people.person",
            performedBy,
            beforeJson,
            null,
            $"{performedBy} deleted roster profile '{person.FullName}'.");

        dbContext.SaveChanges();
    }

    public MatchDto UpsertMatch(int? matchId, MatchUpsertRequest request, string performedBy)
    {
        EnsureDatabase();

        var competition = dbContext.Competitions.FirstOrDefault(entry => entry.Id == request.CompetitionId)
            ?? throw new InvalidOperationException("Select a valid tournament before saving a series.");

        if (request.HomeTeamId == request.AwayTeamId)
        {
            throw new InvalidOperationException("Home and away teams must be different.");
        }

        _ = dbContext.Teams.FirstOrDefault(entry => entry.Id == request.HomeTeamId)
            ?? throw new InvalidOperationException("Select a valid home team.");
        _ = dbContext.Teams.FirstOrDefault(entry => entry.Id == request.AwayTeamId)
            ?? throw new InvalidOperationException("Select a valid away team.");

        var seasonId = request.SeasonId
            ?? dbContext.Seasons
                .Where(entry => entry.CompetitionId == request.CompetitionId)
                .OrderByDescending(entry => entry.IsCurrent)
                .ThenByDescending(entry => entry.YearStart)
                .Select(entry => (int?)entry.Id)
                .FirstOrDefault()
            ?? throw new InvalidOperationException("Create a season for this tournament before adding a series.");

        var isCreate = !matchId.HasValue;
        var match = isCreate
            ? new MatchEntity { CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow }
            : dbContext.Matches.FirstOrDefault(entry => entry.Id == matchId!.Value)
                ?? throw new KeyNotFoundException("Series not found.");

        var beforeJson = isCreate ? null : JsonSerializer.Serialize(MapMatch(match));

        match.SportId = competition.SportId;
        match.CompetitionId = request.CompetitionId;
        match.SeasonId = seasonId;
        match.HomeTeamId = request.HomeTeamId;
        match.AwayTeamId = request.AwayTeamId;
        match.KickoffUtc = request.KickoffUtc;
        match.Status = request.Status.Trim();
        match.HomeScore = request.HomeScore;
        match.AwayScore = request.AwayScore;
        match.Venue = request.Venue.Trim();
        match.Source = "manual-admin";
        match.UpdatedAtUtc = DateTimeOffset.UtcNow;

        if (isCreate)
        {
            dbContext.Matches.Add(match);
        }

        dbContext.SaveChanges();

        var dto = MapMatch(match);
        AppendChangeLog(
            isCreate ? "create" : "update",
            "matches.match",
            performedBy,
            beforeJson,
            JsonSerializer.Serialize(dto),
            $"{performedBy} {(isCreate ? "created" : "updated")} series '{dto.HomeTeam} vs {dto.AwayTeam}'.");

        dbContext.SaveChanges();
        return dto;
    }

    public void DeleteMatch(int matchId, string performedBy)
    {
        EnsureDatabase();

        var match = dbContext.Matches.FirstOrDefault(entry => entry.Id == matchId)
            ?? throw new KeyNotFoundException("Series not found.");

        var beforeJson = JsonSerializer.Serialize(MapMatch(match));
        DeleteMatchDependencies([matchId]);
        dbContext.Matches.Remove(match);

        AppendChangeLog(
            "delete",
            "matches.match",
            performedBy,
            beforeJson,
            null,
            $"{performedBy} deleted series '{GetTeamName(match.HomeTeamId)} vs {GetTeamName(match.AwayTeamId)}'.");

        dbContext.SaveChanges();
    }

    private void SyncTeamMembership(int personId, PersonUpsertRequest request)
    {
        var existingMemberships = dbContext.TeamMembers.Where(entry => entry.PersonId == personId).ToList();
        if (!request.TeamId.HasValue)
        {
            dbContext.TeamMembers.RemoveRange(existingMemberships);
            return;
        }

        var team = dbContext.Teams.FirstOrDefault(entry => entry.Id == request.TeamId.Value)
            ?? throw new InvalidOperationException("Select a valid team before assigning a player.");

        var teamSeason = dbContext.TeamSeasons
            .Where(entry => entry.TeamId == team.Id)
            .OrderByDescending(entry => entry.SeasonId)
            .FirstOrDefault();

        if (teamSeason is null)
        {
            var currentSeasonId = dbContext.Seasons
                .Join(dbContext.Competitions, season => season.CompetitionId, competition => competition.Id, (season, competition) => new { season, competition })
                .Where(entry => entry.competition.SportId == team.SportId && entry.season.IsCurrent)
                .OrderByDescending(entry => entry.season.YearStart)
                .Select(entry => (int?)entry.season.Id)
                .FirstOrDefault()
                ?? throw new InvalidOperationException("Create a tournament season before assigning players to this team.");

            teamSeason = new TeamSeasonEntity
            {
                TeamId = team.Id,
                SeasonId = currentSeasonId,
                CoachName = string.Empty,
                Notes = "Auto-created from admin roster management.",
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            dbContext.TeamSeasons.Add(teamSeason);
            dbContext.SaveChanges();
        }

        dbContext.TeamMembers.RemoveRange(existingMemberships.Where(entry => entry.TeamSeasonId != teamSeason.Id));

        var membership = dbContext.TeamMembers.FirstOrDefault(entry => entry.TeamSeasonId == teamSeason.Id && entry.PersonId == personId);
        if (membership is null)
        {
            membership = new TeamMemberEntity
            {
                TeamSeasonId = teamSeason.Id,
                PersonId = personId,
                JoinedAtUtc = DateTimeOffset.UtcNow
            };
            dbContext.TeamMembers.Add(membership);
        }

        membership.SquadRole = string.IsNullOrWhiteSpace(request.SquadRole) ? "Starter" : request.SquadRole.Trim();
        membership.ShirtNumber = request.ShirtNumber;
    }

    private void DeleteMatchDependencies(IEnumerable<int> matchIds)
    {
        var materializedMatchIds = matchIds.Distinct().ToList();
        if (materializedMatchIds.Count == 0)
        {
            return;
        }

        dbContext.MatchBroadcasts.RemoveRange(dbContext.MatchBroadcasts.Where(entry => materializedMatchIds.Contains(entry.MatchId)));
        dbContext.PlayerStats.RemoveRange(dbContext.PlayerStats.Where(entry => materializedMatchIds.Contains(entry.MatchId)));
        dbContext.MatchEvents.RemoveRange(dbContext.MatchEvents.Where(entry => materializedMatchIds.Contains(entry.MatchId)));
    }

    private void AppendChangeLog(string action, string entityName, string performedBy, string? beforeJson, string? afterJson, string summary)
    {
        var actorId = dbContext.AdminUsers
            .AsNoTracking()
            .Where(entry => entry.UserName == performedBy)
            .Select(entry => (int?)entry.Id)
            .FirstOrDefault();

        dbContext.ChangeLogs.Add(new ChangeLogEntity
        {
            Action = action,
            EntityName = entityName,
            AdminUserId = actorId,
            Summary = summary,
            BeforeJson = beforeJson,
            AfterJson = afterJson,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    private void EnsureDatabase()
    {
        if (!dbContext.Database.CanConnect())
        {
            throw new InvalidOperationException("Tournament management requires the PostgreSQL database to be running.");
        }
    }

    private SportDto MapSport(SportEntity sport)
        => new(sport.Id, sport.Name, sport.Slug, sport.Description, sport.IsOlympic);

    private CompetitionDto MapCompetition(CompetitionEntity competition)
        => new(
            competition.Id,
            competition.SportId,
            dbContext.Sports.AsNoTracking().Where(entry => entry.Id == competition.SportId).Select(entry => entry.Name).FirstOrDefault() ?? "Unknown sport",
            competition.Name,
            competition.Country,
            competition.Format,
            competition.IsCup);

    private TeamDto MapTeam(TeamEntity team)
        => new(team.Id, team.SportId, team.Name, team.ShortName, team.Country, team.Venue, team.Founded, team.BadgeUrl);

    private PersonDto BuildPersonDto(int personId)
    {
        var person = dbContext.People.AsNoTracking().First(entry => entry.Id == personId);
        var membership = dbContext.TeamMembers
            .AsNoTracking()
            .Join(dbContext.TeamSeasons.AsNoTracking(), member => member.TeamSeasonId, season => season.Id, (member, season) => new { member, season })
            .Where(entry => entry.member.PersonId == personId)
            .OrderByDescending(entry => entry.season.SeasonId)
            .FirstOrDefault();

        return new PersonDto(
            person.Id,
            membership?.season.TeamId ?? 0,
            person.FullName,
            person.PrimaryRole,
            person.Nationality,
            person.BirthDate,
            membership?.member.ShirtNumber,
            person.PhotoUrl);
    }

    private MatchDto MapMatch(MatchEntity match)
        => new(
            match.Id,
            match.CompetitionId,
            match.SeasonId,
            match.SportId,
            dbContext.Competitions.AsNoTracking().Where(entry => entry.Id == match.CompetitionId).Select(entry => entry.Name).FirstOrDefault() ?? "Unknown competition",
            GetTeamName(match.HomeTeamId),
            GetTeamName(match.AwayTeamId),
            match.KickoffUtc,
            match.Status,
            match.HomeScore,
            match.AwayScore,
            match.Venue);

    private string GetTeamName(int teamId)
        => dbContext.Teams.AsNoTracking().Where(entry => entry.Id == teamId).Select(entry => entry.Name).FirstOrDefault() ?? "Unknown team";
}
