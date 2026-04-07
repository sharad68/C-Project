using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tornois.Infrastructure.Data;

public sealed class DatabaseSeeder(TornoisDbContext dbContext, ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Sports.AnyAsync(cancellationToken))
        {
            await ResetIdentitySequencesAsync(cancellationToken);
            return;
        }

        var sports = new[]
        {
            new SportEntity { Id = 1, Name = "Football", Slug = "football", Description = "Live football coverage across leagues and cups.", IsOlympic = true },
            new SportEntity { Id = 2, Name = "Basketball", Slug = "basketball", Description = "Top leagues, standings, and player leaderboards.", IsOlympic = true },
            new SportEntity { Id = 3, Name = "Cricket", Slug = "cricket", Description = "International and franchise cricket schedules.", IsOlympic = false }
        };

        var competitions = new[]
        {
            new CompetitionEntity { Id = 101, SportId = 1, Name = "Premier League", Country = "England", Format = "League", IsCup = false, ExternalSource = "api-sports" },
            new CompetitionEntity { Id = 102, SportId = 1, Name = "UEFA Champions League", Country = "Europe", Format = "Cup", IsCup = true, ExternalSource = "api-sports" },
            new CompetitionEntity { Id = 201, SportId = 2, Name = "NBA", Country = "United States", Format = "League", IsCup = false, ExternalSource = "the-sports-db" },
            new CompetitionEntity { Id = 301, SportId = 3, Name = "Indian Premier League", Country = "India", Format = "League", IsCup = false, ExternalSource = "the-sports-db" }
        };

        var seasons = new[]
        {
            new SeasonEntity { Id = 1001, CompetitionId = 101, Name = "2025/26", YearStart = 2025, YearEnd = 2026, IsCurrent = true },
            new SeasonEntity { Id = 1002, CompetitionId = 102, Name = "2025/26", YearStart = 2025, YearEnd = 2026, IsCurrent = true },
            new SeasonEntity { Id = 2001, CompetitionId = 201, Name = "2025/26", YearStart = 2025, YearEnd = 2026, IsCurrent = true },
            new SeasonEntity { Id = 3001, CompetitionId = 301, Name = "2026", YearStart = 2026, YearEnd = 2026, IsCurrent = true }
        };

        var teams = new[]
        {
            new TeamEntity { Id = 1, SportId = 1, Name = "Arsenal", ShortName = "ARS", Country = "England", Venue = "Emirates Stadium", Founded = 1886 },
            new TeamEntity { Id = 2, SportId = 1, Name = "Liverpool", ShortName = "LIV", Country = "England", Venue = "Anfield", Founded = 1892 },
            new TeamEntity { Id = 3, SportId = 1, Name = "Real Madrid", ShortName = "RMA", Country = "Spain", Venue = "Santiago Bernabéu", Founded = 1902 },
            new TeamEntity { Id = 4, SportId = 2, Name = "Boston Celtics", ShortName = "BOS", Country = "USA", Venue = "TD Garden", Founded = 1946 },
            new TeamEntity { Id = 5, SportId = 2, Name = "Los Angeles Lakers", ShortName = "LAL", Country = "USA", Venue = "Crypto.com Arena", Founded = 1947 },
            new TeamEntity { Id = 6, SportId = 3, Name = "Mumbai Indians", ShortName = "MI", Country = "India", Venue = "Wankhede Stadium", Founded = 2008 }
        };

        var people = new[]
        {
            new PersonEntity { Id = 1, FullName = "Bukayo Saka", FirstName = "Bukayo", LastName = "Saka", Nationality = "England", BirthDate = new DateOnly(2001, 9, 5), PrimaryRole = "Forward", Bio = "Creative winger with elite 1v1 ability and strong final-third output." },
            new PersonEntity { Id = 2, FullName = "Martin Ødegaard", FirstName = "Martin", LastName = "Ødegaard", Nationality = "Norway", BirthDate = new DateOnly(1998, 12, 17), PrimaryRole = "Midfielder", Bio = "Ball-progressing playmaker and team captain." },
            new PersonEntity { Id = 3, FullName = "Virgil van Dijk", FirstName = "Virgil", LastName = "van Dijk", Nationality = "Netherlands", BirthDate = new DateOnly(1991, 7, 8), PrimaryRole = "Defender", Bio = "Dominant centre-back known for aerial control and leadership." },
            new PersonEntity { Id = 4, FullName = "Jayson Tatum", FirstName = "Jayson", LastName = "Tatum", Nationality = "USA", BirthDate = new DateOnly(1998, 3, 3), PrimaryRole = "Forward", Bio = "Primary scoring option with two-way impact." },
            new PersonEntity { Id = 5, FullName = "LeBron James", FirstName = "LeBron", LastName = "James", Nationality = "USA", BirthDate = new DateOnly(1984, 12, 30), PrimaryRole = "Forward", Bio = "Veteran playmaker and all-around leader." },
            new PersonEntity { Id = 6, FullName = "Jasprit Bumrah", FirstName = "Jasprit", LastName = "Bumrah", Nationality = "India", BirthDate = new DateOnly(1993, 12, 6), PrimaryRole = "Bowler", Bio = "Elite fast bowler with deceptive release and death-over accuracy." }
        };

        var teamSeasons = new[]
        {
            new TeamSeasonEntity { Id = 1, TeamId = 1, SeasonId = 1001, CoachName = "Mikel Arteta", Notes = "Title challenge in progress." },
            new TeamSeasonEntity { Id = 2, TeamId = 2, SeasonId = 1001, CoachName = "Arne Slot", Notes = "High-intensity pressing setup." },
            new TeamSeasonEntity { Id = 3, TeamId = 3, SeasonId = 1002, CoachName = "Carlo Ancelotti", Notes = "European knockout specialist." },
            new TeamSeasonEntity { Id = 4, TeamId = 4, SeasonId = 2001, CoachName = "Joe Mazzulla", Notes = "Balanced two-way squad." },
            new TeamSeasonEntity { Id = 5, TeamId = 5, SeasonId = 2001, CoachName = "JJ Redick", Notes = "Veteran-led roster." },
            new TeamSeasonEntity { Id = 6, TeamId = 6, SeasonId = 3001, CoachName = "Mahela Jayawardene", Notes = "Power-heavy T20 lineup." }
        };

        var teamMembers = new[]
        {
            new TeamMemberEntity { Id = 1, TeamSeasonId = 1, PersonId = 1, SquadRole = "Starter", ShirtNumber = 7 },
            new TeamMemberEntity { Id = 2, TeamSeasonId = 1, PersonId = 2, SquadRole = "Captain", ShirtNumber = 8 },
            new TeamMemberEntity { Id = 3, TeamSeasonId = 2, PersonId = 3, SquadRole = "Starter", ShirtNumber = 4 },
            new TeamMemberEntity { Id = 4, TeamSeasonId = 4, PersonId = 4, SquadRole = "Starter", ShirtNumber = 0 },
            new TeamMemberEntity { Id = 5, TeamSeasonId = 5, PersonId = 5, SquadRole = "Starter", ShirtNumber = 23 },
            new TeamMemberEntity { Id = 6, TeamSeasonId = 6, PersonId = 6, SquadRole = "Starter" }
        };

        var matches = new[]
        {
            new MatchEntity { Id = 5001, SportId = 1, CompetitionId = 101, SeasonId = 1001, HomeTeamId = 1, AwayTeamId = 2, KickoffUtc = DateTimeOffset.UtcNow.AddMinutes(-15), Status = "Live", HomeScore = 2, AwayScore = 1, Venue = "Emirates Stadium", Source = "api-sports" },
            new MatchEntity { Id = 5002, SportId = 1, CompetitionId = 102, SeasonId = 1002, HomeTeamId = 3, AwayTeamId = 1, KickoffUtc = DateTimeOffset.UtcNow.AddDays(1), Status = "Scheduled", Venue = "Santiago Bernabéu", Source = "api-sports" },
            new MatchEntity { Id = 5003, SportId = 2, CompetitionId = 201, SeasonId = 2001, HomeTeamId = 4, AwayTeamId = 5, KickoffUtc = DateTimeOffset.UtcNow.AddHours(6), Status = "Scheduled", Venue = "TD Garden", Source = "the-sports-db" }
        };

        var matchEvents = new[]
        {
            new MatchEventEntity { Id = 1, MatchId = 5001, Minute = 11, Type = "goal", Subject = "Bukayo Saka", TeamId = 1, PersonId = 1 },
            new MatchEventEntity { Id = 2, MatchId = 5001, Minute = 19, Type = "goal", Subject = "Luis Díaz", TeamId = 2 },
            new MatchEventEntity { Id = 3, MatchId = 5001, Minute = 31, Type = "goal", Subject = "Martin Ødegaard", TeamId = 1, PersonId = 2 }
        };

        var playerStats = new[]
        {
            new PlayerStatEntity { Id = 1, MatchId = 5001, PersonId = 1, TeamId = 1, Category = "football", MetricValue = 1, StatPayloadJson = "{\"shotsOnTarget\": 2, \"keyPasses\": 3}" },
            new PlayerStatEntity { Id = 2, MatchId = 5003, PersonId = 4, TeamId = 4, Category = "basketball", MetricValue = 28, StatPayloadJson = "{\"points\": 28, \"rebounds\": 8, \"assists\": 5}" }
        };

        var broadcasters = new[]
        {
            new BroadcasterEntity { Id = 1, Name = "Sky Sports", Country = "United Kingdom", WebsiteUrl = "https://www.skysports.com" },
            new BroadcasterEntity { Id = 2, Name = "ESPN", Country = "United States", WebsiteUrl = "https://www.espn.com" }
        };

        var matchBroadcasts = new[]
        {
            new MatchBroadcastEntity { Id = 1, MatchId = 5001, BroadcasterId = 1, Region = "UK", ChannelName = "Sky Sports Main Event", StreamUrl = "https://example.com/sky" },
            new MatchBroadcastEntity { Id = 2, MatchId = 5003, BroadcasterId = 2, Region = "US", ChannelName = "ESPN", StreamUrl = "https://example.com/espn" }
        };

        var standings = new[]
        {
            new StandingEntity { Id = 1, CompetitionId = 101, SeasonId = 1001, TeamId = 1, Position = 1, Played = 29, Won = 20, Drawn = 5, Lost = 4, GoalsFor = 61, GoalsAgainst = 28, Points = 65 },
            new StandingEntity { Id = 2, CompetitionId = 101, SeasonId = 1001, TeamId = 2, Position = 2, Played = 29, Won = 19, Drawn = 6, Lost = 4, GoalsFor = 58, GoalsAgainst = 30, Points = 63 },
            new StandingEntity { Id = 3, CompetitionId = 201, SeasonId = 2001, TeamId = 4, Position = 1, Played = 62, Won = 47, Drawn = 0, Lost = 15, GoalsFor = 7421, GoalsAgainst = 7014, Points = 94 }
        };

        var rankingGroups = new[]
        {
            new IndividualRankingEntity { Id = 1, CompetitionId = 101, SeasonId = 1001, Category = "goals", Label = "Golden Boot race" },
            new IndividualRankingEntity { Id = 2, CompetitionId = 201, SeasonId = 2001, Category = "points", Label = "Scoring leaders" },
            new IndividualRankingEntity { Id = 3, CompetitionId = 301, SeasonId = 3001, Category = "wickets", Label = "Bowling leaders" }
        };

        var personRankings = new[]
        {
            new PersonRankingEntity { Id = 1, RankingId = 1, PersonId = 1, TeamId = 1, Position = 1, Value = 16 },
            new PersonRankingEntity { Id = 2, RankingId = 2, PersonId = 4, TeamId = 4, Position = 1, Value = 28 },
            new PersonRankingEntity { Id = 3, RankingId = 3, PersonId = 6, TeamId = 6, Position = 1, Value = 18 }
        };

        var admins = new[]
        {
            new AdminUserEntity { Id = 1, UserName = "superadmin", DisplayName = "Platform Superadmin", Role = "superadmin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123"), IsActive = true },
            new AdminUserEntity { Id = 2, UserName = "editor", DisplayName = "Content Editor", Role = "editor", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Editor@123"), IsActive = true },
            new AdminUserEntity { Id = 3, UserName = "readonly", DisplayName = "Read Only Analyst", Role = "readonly", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Viewer@123"), IsActive = true }
        };

        var changeLogs = new[]
        {
            new ChangeLogEntity { Id = 1, AdminUserId = 1, Action = "sync", EntityName = "matches.match", Summary = "Seeded initial live-score snapshot.", AfterJson = "{\"records\":3}" },
            new ChangeLogEntity { Id = 2, AdminUserId = 2, Action = "update", EntityName = "teams.team", Summary = "Seeded venue metadata for Arsenal.", AfterJson = "{\"venue\":\"Emirates Stadium\"}" }
        };

        await dbContext.Sports.AddRangeAsync(sports, cancellationToken);
        await dbContext.Competitions.AddRangeAsync(competitions, cancellationToken);
        await dbContext.Seasons.AddRangeAsync(seasons, cancellationToken);
        await dbContext.Teams.AddRangeAsync(teams, cancellationToken);
        await dbContext.People.AddRangeAsync(people, cancellationToken);
        await dbContext.TeamSeasons.AddRangeAsync(teamSeasons, cancellationToken);
        await dbContext.TeamMembers.AddRangeAsync(teamMembers, cancellationToken);
        await dbContext.Matches.AddRangeAsync(matches, cancellationToken);
        await dbContext.MatchEvents.AddRangeAsync(matchEvents, cancellationToken);
        await dbContext.PlayerStats.AddRangeAsync(playerStats, cancellationToken);
        await dbContext.Broadcasters.AddRangeAsync(broadcasters, cancellationToken);
        await dbContext.MatchBroadcasts.AddRangeAsync(matchBroadcasts, cancellationToken);
        await dbContext.Standings.AddRangeAsync(standings, cancellationToken);
        await dbContext.IndividualRankings.AddRangeAsync(rankingGroups, cancellationToken);
        await dbContext.PersonRankings.AddRangeAsync(personRankings, cancellationToken);
        await dbContext.AdminUsers.AddRangeAsync(admins, cancellationToken);
        await dbContext.ChangeLogs.AddRangeAsync(changeLogs, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await ResetIdentitySequencesAsync(cancellationToken);

        logger.LogInformation("Seeded PostgreSQL reference data across the Tornois schemas.");
    }

    private async Task ResetIdentitySequencesAsync(CancellationToken cancellationToken)
    {
        var tables = new[]
        {
            "sports.sport",
            "sports.competition",
            "sports.season",
            "teams.team",
            "teams.team_season",
            "teams.team_member",
            "people.person",
            "matches.match",
            "matches.match_event",
            "stats.player_stat",
            "broadcast.broadcaster",
            "broadcast.match_broadcast",
            "rankings.standing",
            "rankings.individual_ranking",
            "rankings.person_ranking",
            "admin.admin_user",
            "admin.change_log"
        };

        foreach (var table in tables)
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                $"SELECT setval(pg_get_serial_sequence('{table}', 'id'), COALESCE((SELECT MAX(id) FROM {table}), 1), true);",
                cancellationToken);
        }
    }
}

public static class DatabaseInitializationExtensions
{
    public static async Task InitialiseTornoisDatabaseAsync(this IServiceProvider services, ILogger logger, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TornoisDbContext>();
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

            await dbContext.Database.MigrateAsync(cancellationToken);
            await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "PostgreSQL initialisation was skipped. Start the database container to apply migrations and seed data.");
        }
    }
}