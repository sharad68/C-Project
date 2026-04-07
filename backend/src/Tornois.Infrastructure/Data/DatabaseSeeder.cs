using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tornois.Infrastructure.Data;

public sealed class DatabaseSeeder(TornoisDbContext dbContext, ILogger<DatabaseSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var shouldRefreshReferenceData = await dbContext.Sports.AnyAsync(cancellationToken);
        if (shouldRefreshReferenceData)
        {
            await dbContext.ChangeLogs.ExecuteDeleteAsync(cancellationToken);
            await dbContext.PersonRankings.ExecuteDeleteAsync(cancellationToken);
            await dbContext.IndividualRankings.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Standings.ExecuteDeleteAsync(cancellationToken);
            await dbContext.MatchBroadcasts.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Broadcasters.ExecuteDeleteAsync(cancellationToken);
            await dbContext.PlayerStats.ExecuteDeleteAsync(cancellationToken);
            await dbContext.MatchEvents.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Matches.ExecuteDeleteAsync(cancellationToken);
            await dbContext.TeamMembers.ExecuteDeleteAsync(cancellationToken);
            await dbContext.TeamSeasons.ExecuteDeleteAsync(cancellationToken);
            await dbContext.People.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Teams.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Seasons.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Competitions.ExecuteDeleteAsync(cancellationToken);
            await dbContext.Sports.ExecuteDeleteAsync(cancellationToken);
        }

        var sports = new[]
        {
            new SportEntity { Id = 1, Name = "League of Legends", Slug = "league-of-legends", Description = "Publisher-backed MOBA circuit with regional leagues and global finals.", IsOlympic = true },
            new SportEntity { Id = 2, Name = "Counter-Strike 2", Slug = "counter-strike-2", Description = "Open FPS ecosystem built around majors, circuits, and qualifiers.", IsOlympic = false },
            new SportEntity { Id = 3, Name = "Valorant", Slug = "valorant", Description = "Structured tactical shooter league with regional stages and Masters events.", IsOlympic = true }
        };

        var competitions = new[]
        {
            new CompetitionEntity { Id = 101, SportId = 1, Name = "LEC Spring Split", Country = "Europe", Format = "League", IsCup = false, ExternalSource = "manual-seed" },
            new CompetitionEntity { Id = 102, SportId = 1, Name = "Mid-Season Invitational", Country = "Global", Format = "International", IsCup = true, ExternalSource = "manual-seed" },
            new CompetitionEntity { Id = 201, SportId = 2, Name = "PGL Major Copenhagen", Country = "Denmark", Format = "Major", IsCup = true, ExternalSource = "manual-seed" },
            new CompetitionEntity { Id = 202, SportId = 2, Name = "ESL Pro League", Country = "Global", Format = "League", IsCup = false, ExternalSource = "manual-seed" },
            new CompetitionEntity { Id = 301, SportId = 3, Name = "VCT EMEA Stage 1", Country = "Europe", Format = "League", IsCup = false, ExternalSource = "manual-seed" },
            new CompetitionEntity { Id = 302, SportId = 3, Name = "Valorant Masters Toronto", Country = "Canada", Format = "International", IsCup = true, ExternalSource = "manual-seed" }
        };

        var seasons = new[]
        {
            new SeasonEntity { Id = 1001, CompetitionId = 101, Name = "Spring 2026", YearStart = 2026, YearEnd = 2026, IsCurrent = true },
            new SeasonEntity { Id = 1002, CompetitionId = 102, Name = "2026", YearStart = 2026, YearEnd = 2026, IsCurrent = true },
            new SeasonEntity { Id = 2001, CompetitionId = 201, Name = "2026", YearStart = 2026, YearEnd = 2026, IsCurrent = true },
            new SeasonEntity { Id = 2002, CompetitionId = 202, Name = "Season 22", YearStart = 2026, YearEnd = 2026, IsCurrent = true },
            new SeasonEntity { Id = 3001, CompetitionId = 301, Name = "Stage 1 2026", YearStart = 2026, YearEnd = 2026, IsCurrent = true },
            new SeasonEntity { Id = 3002, CompetitionId = 302, Name = "2026", YearStart = 2026, YearEnd = 2026, IsCurrent = true }
        };

        var teams = new[]
        {
            new TeamEntity { Id = 1, SportId = 1, Name = "T1", ShortName = "T1", Country = "South Korea", Venue = "LoL Park, Seoul", Founded = 2003 },
            new TeamEntity { Id = 2, SportId = 1, Name = "G2 Esports", ShortName = "G2", Country = "Europe", Venue = "Berlin Team House", Founded = 2015 },
            new TeamEntity { Id = 3, SportId = 2, Name = "Natus Vincere", ShortName = "NAVI", Country = "Ukraine", Venue = "NAVI Campus Kyiv", Founded = 2009 },
            new TeamEntity { Id = 4, SportId = 2, Name = "Team Vitality", ShortName = "VIT", Country = "France", Venue = "V.Hive Paris", Founded = 2013 },
            new TeamEntity { Id = 5, SportId = 3, Name = "Fnatic", ShortName = "FNC", Country = "United Kingdom", Venue = "London HQ", Founded = 2004 },
            new TeamEntity { Id = 6, SportId = 3, Name = "Sentinels", ShortName = "SEN", Country = "United States", Venue = "Los Angeles HQ", Founded = 2016 }
        };

        var people = new[]
        {
            new PersonEntity { Id = 1, FullName = "Lee \"Faker\" Sang-hyeok", FirstName = "Lee", LastName = "Sang-hyeok", Nationality = "South Korea", BirthDate = new DateOnly(1996, 5, 7), PrimaryRole = "Mid Laner", Bio = "Legendary shot-caller and franchise player for T1's League of Legends roster." },
            new PersonEntity { Id = 2, FullName = "Rasmus \"Caps\" Winther", FirstName = "Rasmus", LastName = "Winther", Nationality = "Denmark", BirthDate = new DateOnly(1999, 11, 17), PrimaryRole = "Mid Laner", Bio = "Aggressive playmaker known for high-pressure international performances." },
            new PersonEntity { Id = 3, FullName = "Oleksandr \"s1mple\" Kostyliev", FirstName = "Oleksandr", LastName = "Kostyliev", Nationality = "Ukraine", BirthDate = new DateOnly(1997, 10, 2), PrimaryRole = "AWPer", Bio = "Elite Counter-Strike superstar with unmatched clutch potential." },
            new PersonEntity { Id = 4, FullName = "Mathieu \"ZywOo\" Herbaut", FirstName = "Mathieu", LastName = "Herbaut", Nationality = "France", BirthDate = new DateOnly(2000, 11, 9), PrimaryRole = "AWPer", Bio = "Precision-focused star fragger anchoring Vitality's late-round setups." },
            new PersonEntity { Id = 5, FullName = "Jake \"Boaster\" Howlett", FirstName = "Jake", LastName = "Howlett", Nationality = "United Kingdom", BirthDate = new DateOnly(1995, 5, 25), PrimaryRole = "IGL", Bio = "Calm in-game leader coordinating Fnatic's tactical rounds and utility usage." },
            new PersonEntity { Id = 6, FullName = "Tyson \"TenZ\" Ngo", FirstName = "Tyson", LastName = "Ngo", Nationality = "Canada", BirthDate = new DateOnly(2001, 5, 5), PrimaryRole = "Duelist", Bio = "Explosive entry player known for opening duel wins and stream appeal." }
        };

        var teamSeasons = new[]
        {
            new TeamSeasonEntity { Id = 1, TeamId = 1, SeasonId = 1001, CoachName = "kkOma", Notes = "Macro-focused lineup built around controlled objective fights." },
            new TeamSeasonEntity { Id = 2, TeamId = 2, SeasonId = 1001, CoachName = "Dylan Falco", Notes = "Creative drafts and high-tempo side lane pressure." },
            new TeamSeasonEntity { Id = 3, TeamId = 3, SeasonId = 2001, CoachName = "B1ad3", Notes = "Disciplined default-heavy Counter-Strike structure." },
            new TeamSeasonEntity { Id = 4, TeamId = 4, SeasonId = 2001, CoachName = "XTQZZZ", Notes = "Star-powered fragging core with explosive exec timing." },
            new TeamSeasonEntity { Id = 5, TeamId = 5, SeasonId = 3001, CoachName = "mini", Notes = "Utility-heavy tactical identity for long series." },
            new TeamSeasonEntity { Id = 6, TeamId = 6, SeasonId = 3001, CoachName = "kaplan", Notes = "Aggressive opening duels backed by momentum-based mid rounds." }
        };

        var teamMembers = new[]
        {
            new TeamMemberEntity { Id = 1, TeamSeasonId = 1, PersonId = 1, SquadRole = "Captain" },
            new TeamMemberEntity { Id = 2, TeamSeasonId = 2, PersonId = 2, SquadRole = "Starter" },
            new TeamMemberEntity { Id = 3, TeamSeasonId = 3, PersonId = 3, SquadRole = "Starter" },
            new TeamMemberEntity { Id = 4, TeamSeasonId = 4, PersonId = 4, SquadRole = "Starter" },
            new TeamMemberEntity { Id = 5, TeamSeasonId = 5, PersonId = 5, SquadRole = "Captain" },
            new TeamMemberEntity { Id = 6, TeamSeasonId = 6, PersonId = 6, SquadRole = "Starter" }
        };

        var matches = new[]
        {
            new MatchEntity { Id = 5001, SportId = 1, CompetitionId = 101, SeasonId = 1001, HomeTeamId = 1, AwayTeamId = 2, KickoffUtc = DateTimeOffset.UtcNow.AddMinutes(-12), Status = "Live", HomeScore = 1, AwayScore = 0, Venue = "LoL Park, Seoul", Source = "manual-seed" },
            new MatchEntity { Id = 5002, SportId = 2, CompetitionId = 201, SeasonId = 2001, HomeTeamId = 3, AwayTeamId = 4, KickoffUtc = DateTimeOffset.UtcNow.AddDays(1), Status = "Scheduled", Venue = "Royal Arena, Copenhagen", Source = "manual-seed" },
            new MatchEntity { Id = 5003, SportId = 3, CompetitionId = 301, SeasonId = 3001, HomeTeamId = 5, AwayTeamId = 6, KickoffUtc = DateTimeOffset.UtcNow.AddHours(6), Status = "Scheduled", Venue = "Riot Games Arena, Berlin", Source = "manual-seed" },
            new MatchEntity { Id = 5004, SportId = 1, CompetitionId = 102, SeasonId = 1002, HomeTeamId = 2, AwayTeamId = 1, KickoffUtc = DateTimeOffset.UtcNow.AddDays(2), Status = "Scheduled", Venue = "MSI Main Stage", Source = "manual-seed" }
        };

        var matchEvents = new[]
        {
            new MatchEventEntity { Id = 1, MatchId = 5001, Minute = 6, Type = "first blood", Subject = "Faker opens the series with a solo kill in mid.", TeamId = 1, PersonId = 1 },
            new MatchEventEntity { Id = 2, MatchId = 5001, Minute = 18, Type = "objective", Subject = "T1 secure soul point after a clean dragon fight.", TeamId = 1 },
            new MatchEventEntity { Id = 3, MatchId = 5001, Minute = 29, Type = "baron", Subject = "G2 hold the base after a tense Baron standoff.", TeamId = 2, PersonId = 2 }
        };

        var playerStats = new[]
        {
            new PlayerStatEntity { Id = 1, MatchId = 5001, PersonId = 1, TeamId = 1, Category = "league-of-legends", MetricValue = 8, StatPayloadJson = "{\"kills\":8,\"assists\":11,\"deaths\":2}" },
            new PlayerStatEntity { Id = 2, MatchId = 5002, PersonId = 4, TeamId = 4, Category = "counter-strike-2", MetricValue = 23, StatPayloadJson = "{\"headshots\":23,\"openingKills\":6,\"rating\":132}" },
            new PlayerStatEntity { Id = 3, MatchId = 5003, PersonId = 6, TeamId = 6, Category = "valorant", MetricValue = 267, StatPayloadJson = "{\"acs\":267,\"firstKills\":5,\"plants\":2}" }
        };

        var broadcasters = new[]
        {
            new BroadcasterEntity { Id = 1, Name = "Twitch Esports", Country = "Global", WebsiteUrl = "https://www.twitch.tv" },
            new BroadcasterEntity { Id = 2, Name = "YouTube Gaming", Country = "Global", WebsiteUrl = "https://www.youtube.com/gaming" }
        };

        var matchBroadcasts = new[]
        {
            new MatchBroadcastEntity { Id = 1, MatchId = 5001, BroadcasterId = 1, Region = "Global", ChannelName = "twitch.tv/lec", StreamUrl = "https://www.twitch.tv/lec" },
            new MatchBroadcastEntity { Id = 2, MatchId = 5003, BroadcasterId = 2, Region = "Global", ChannelName = "youtube.com/valorantesports", StreamUrl = "https://www.youtube.com/@valorantesports" }
        };

        var standings = new[]
        {
            new StandingEntity { Id = 1, CompetitionId = 101, SeasonId = 1001, TeamId = 1, Position = 1, Played = 9, Won = 8, Drawn = 0, Lost = 1, GoalsFor = 18, GoalsAgainst = 7, Points = 24 },
            new StandingEntity { Id = 2, CompetitionId = 101, SeasonId = 1001, TeamId = 2, Position = 2, Played = 9, Won = 7, Drawn = 0, Lost = 2, GoalsFor = 15, GoalsAgainst = 9, Points = 21 },
            new StandingEntity { Id = 3, CompetitionId = 201, SeasonId = 2001, TeamId = 4, Position = 1, Played = 5, Won = 4, Drawn = 0, Lost = 1, GoalsFor = 10, GoalsAgainst = 5, Points = 12 },
            new StandingEntity { Id = 4, CompetitionId = 201, SeasonId = 2001, TeamId = 3, Position = 2, Played = 5, Won = 3, Drawn = 0, Lost = 2, GoalsFor = 8, GoalsAgainst = 6, Points = 9 },
            new StandingEntity { Id = 5, CompetitionId = 301, SeasonId = 3001, TeamId = 5, Position = 1, Played = 4, Won = 3, Drawn = 0, Lost = 1, GoalsFor = 7, GoalsAgainst = 3, Points = 9 },
            new StandingEntity { Id = 6, CompetitionId = 301, SeasonId = 3001, TeamId = 6, Position = 2, Played = 4, Won = 2, Drawn = 0, Lost = 2, GoalsFor = 5, GoalsAgainst = 5, Points = 6 }
        };

        var rankingGroups = new[]
        {
            new IndividualRankingEntity { Id = 1, CompetitionId = 101, SeasonId = 1001, Category = "kills", Label = "Kill leaders" },
            new IndividualRankingEntity { Id = 2, CompetitionId = 201, SeasonId = 2001, Category = "headshots", Label = "Headshot leaders" },
            new IndividualRankingEntity { Id = 3, CompetitionId = 301, SeasonId = 3001, Category = "acs", Label = "ACS leaders" }
        };

        var personRankings = new[]
        {
            new PersonRankingEntity { Id = 1, RankingId = 1, PersonId = 1, TeamId = 1, Position = 1, Value = 44 },
            new PersonRankingEntity { Id = 2, RankingId = 2, PersonId = 4, TeamId = 4, Position = 1, Value = 23 },
            new PersonRankingEntity { Id = 3, RankingId = 3, PersonId = 6, TeamId = 6, Position = 1, Value = 267 }
        };

        var admins = new[]
        {
            new AdminUserEntity { Id = 1, UserName = "superadmin", DisplayName = "Platform Superadmin", Role = "superadmin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123"), IsActive = true },
            new AdminUserEntity { Id = 2, UserName = "editor", DisplayName = "Content Editor", Role = "editor", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Editor@123"), IsActive = true },
            new AdminUserEntity { Id = 3, UserName = "readonly", DisplayName = "Read Only Analyst", Role = "readonly", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Viewer@123"), IsActive = true }
        };

        if (!await dbContext.AdminUsers.AnyAsync(cancellationToken))
        {
            await dbContext.AdminUsers.AddRangeAsync(admins, cancellationToken);
        }

        var changeLogs = new[]
        {
            new ChangeLogEntity { Id = 1, AdminUserId = 1, Action = "sync", EntityName = "matches.match", Summary = "Seeded initial esports live-ops snapshot.", AfterJson = "{\"records\":4}" },
            new ChangeLogEntity { Id = 2, AdminUserId = 2, Action = "update", EntityName = "broadcast.match_broadcast", Summary = "Updated official stream metadata for T1 versus G2.", AfterJson = "{\"channel\":\"twitch.tv/lec\"}" }
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
        await dbContext.ChangeLogs.AddRangeAsync(changeLogs, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await ResetIdentitySequencesAsync(cancellationToken);

        logger.LogInformation("Seeded PostgreSQL esports reference data across the Tornois schemas.");
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