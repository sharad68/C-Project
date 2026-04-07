using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Tornois.Infrastructure.Data;

public sealed class TornoisDbContext(DbContextOptions<TornoisDbContext> options) : DbContext(options)
{
    public DbSet<SportEntity> Sports => Set<SportEntity>();
    public DbSet<CompetitionEntity> Competitions => Set<CompetitionEntity>();
    public DbSet<SeasonEntity> Seasons => Set<SeasonEntity>();
    public DbSet<TeamEntity> Teams => Set<TeamEntity>();
    public DbSet<TeamSeasonEntity> TeamSeasons => Set<TeamSeasonEntity>();
    public DbSet<TeamMemberEntity> TeamMembers => Set<TeamMemberEntity>();
    public DbSet<PersonEntity> People => Set<PersonEntity>();
    public DbSet<MatchEntity> Matches => Set<MatchEntity>();
    public DbSet<MatchEventEntity> MatchEvents => Set<MatchEventEntity>();
    public DbSet<PlayerStatEntity> PlayerStats => Set<PlayerStatEntity>();
    public DbSet<BroadcasterEntity> Broadcasters => Set<BroadcasterEntity>();
    public DbSet<MatchBroadcastEntity> MatchBroadcasts => Set<MatchBroadcastEntity>();
    public DbSet<StandingEntity> Standings => Set<StandingEntity>();
    public DbSet<IndividualRankingEntity> IndividualRankings => Set<IndividualRankingEntity>();
    public DbSet<PersonRankingEntity> PersonRankings => Set<PersonRankingEntity>();
    public DbSet<AdminUserEntity> AdminUsers => Set<AdminUserEntity>();
    public DbSet<ChangeLogEntity> ChangeLogs => Set<ChangeLogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SportEntity>(entity =>
        {
            entity.ToTable("sport", "sports");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<CompetitionEntity>(entity =>
        {
            entity.ToTable("competition", "sports");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Country).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Format).HasMaxLength(60).IsRequired();
            entity.Property(x => x.ExternalSource).HasMaxLength(80);
            entity.Property(x => x.ExternalId).HasMaxLength(80);
            entity.HasOne<SportEntity>().WithMany().HasForeignKey(x => x.SportId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.SportId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<SeasonEntity>(entity =>
        {
            entity.ToTable("season", "sports");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(60).IsRequired();
            entity.HasOne<CompetitionEntity>().WithMany().HasForeignKey(x => x.CompetitionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.CompetitionId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<TeamEntity>(entity =>
        {
            entity.ToTable("team", "teams");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(140).IsRequired();
            entity.Property(x => x.ShortName).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Country).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Venue).HasMaxLength(160);
            entity.Property(x => x.BadgeUrl).HasMaxLength(500);
            entity.HasOne<SportEntity>().WithMany().HasForeignKey(x => x.SportId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.SportId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<TeamSeasonEntity>(entity =>
        {
            entity.ToTable("team_season", "teams");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CoachName).HasMaxLength(160);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.HasOne<TeamEntity>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<SeasonEntity>().WithMany().HasForeignKey(x => x.SeasonId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.TeamId, x.SeasonId }).IsUnique();
        });

        modelBuilder.Entity<TeamMemberEntity>(entity =>
        {
            entity.ToTable("team_member", "teams");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SquadRole).HasMaxLength(60).IsRequired();
            entity.HasOne<TeamSeasonEntity>().WithMany().HasForeignKey(x => x.TeamSeasonId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<PersonEntity>().WithMany().HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.TeamSeasonId, x.PersonId }).IsUnique();
        });

        modelBuilder.Entity<PersonEntity>(entity =>
        {
            entity.ToTable("person", "people");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100);
            entity.Property(x => x.Nationality).HasMaxLength(120).IsRequired();
            entity.Property(x => x.PrimaryRole).HasMaxLength(80).IsRequired();
            entity.Property(x => x.PhotoUrl).HasMaxLength(500);
            entity.Property(x => x.Bio).HasMaxLength(2000);
            entity.HasIndex(x => x.FullName);
        });

        modelBuilder.Entity<MatchEntity>(entity =>
        {
            entity.ToTable("match", "matches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Venue).HasMaxLength(160);
            entity.Property(x => x.Source).HasMaxLength(80);
            entity.HasOne<SportEntity>().WithMany().HasForeignKey(x => x.SportId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<CompetitionEntity>().WithMany().HasForeignKey(x => x.CompetitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<SeasonEntity>().WithMany().HasForeignKey(x => x.SeasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TeamEntity>().WithMany().HasForeignKey(x => x.HomeTeamId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TeamEntity>().WithMany().HasForeignKey(x => x.AwayTeamId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.CompetitionId, x.SeasonId, x.KickoffUtc });
        });

        modelBuilder.Entity<MatchEventEntity>(entity =>
        {
            entity.ToTable("match_event", "matches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasMaxLength(60).IsRequired();
            entity.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            entity.HasOne<MatchEntity>().WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<TeamEntity>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<PersonEntity>().WithMany().HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PlayerStatEntity>(entity =>
        {
            entity.ToTable("player_stat", "stats");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Category).HasMaxLength(60).IsRequired();
            entity.Property(x => x.StatPayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasOne<MatchEntity>().WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<PersonEntity>().WithMany().HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<TeamEntity>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.MatchId, x.PersonId, x.Category });
        });

        modelBuilder.Entity<BroadcasterEntity>(entity =>
        {
            entity.ToTable("broadcaster", "broadcast");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Country).HasMaxLength(120).IsRequired();
            entity.Property(x => x.WebsiteUrl).HasMaxLength(300);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<MatchBroadcastEntity>(entity =>
        {
            entity.ToTable("match_broadcast", "broadcast");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Region).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ChannelName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.StreamUrl).HasMaxLength(300);
            entity.HasOne<MatchEntity>().WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<BroadcasterEntity>().WithMany().HasForeignKey(x => x.BroadcasterId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StandingEntity>(entity =>
        {
            entity.ToTable("standing", "rankings");
            entity.HasKey(x => x.Id);
            entity.HasOne<CompetitionEntity>().WithMany().HasForeignKey(x => x.CompetitionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<SeasonEntity>().WithMany().HasForeignKey(x => x.SeasonId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<TeamEntity>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.CompetitionId, x.SeasonId, x.TeamId }).IsUnique();
        });

        modelBuilder.Entity<IndividualRankingEntity>(entity =>
        {
            entity.ToTable("individual_ranking", "rankings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Category).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Label).HasMaxLength(120).IsRequired();
            entity.HasOne<CompetitionEntity>().WithMany().HasForeignKey(x => x.CompetitionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<SeasonEntity>().WithMany().HasForeignKey(x => x.SeasonId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.CompetitionId, x.SeasonId, x.Category, x.Label }).IsUnique();
        });

        modelBuilder.Entity<PersonRankingEntity>(entity =>
        {
            entity.ToTable("person_ranking", "rankings");
            entity.HasKey(x => x.Id);
            entity.HasOne<IndividualRankingEntity>().WithMany().HasForeignKey(x => x.RankingId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<PersonEntity>().WithMany().HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<TeamEntity>().WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.RankingId, x.PersonId }).IsUnique();
        });

        modelBuilder.Entity<AdminUserEntity>(entity =>
        {
            entity.ToTable("admin_user", "admin");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.UserName).IsUnique();
        });

        modelBuilder.Entity<ChangeLogEntity>(entity =>
        {
            entity.ToTable("change_log", "admin");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(60).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Summary).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.BeforeJson).HasColumnType("jsonb");
            entity.Property(x => x.AfterJson).HasColumnType("jsonb");
            entity.HasOne<AdminUserEntity>().WithMany().HasForeignKey(x => x.AdminUserId).OnDelete(DeleteBehavior.SetNull);
        });

        ApplySnakeCaseConventions(modelBuilder);
    }

    private static void ApplySnakeCaseConventions(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrWhiteSpace(columnName))
                {
                    property.SetColumnName(ToSnakeCase(columnName));
                }
            }

            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (!string.IsNullOrWhiteSpace(keyName))
                {
                    key.SetName(ToSnakeCase(keyName));
                }
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var constraintName = foreignKey.GetConstraintName();
                if (!string.IsNullOrWhiteSpace(constraintName))
                {
                    foreignKey.SetConstraintName(ToSnakeCase(constraintName));
                }
            }

            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (!string.IsNullOrWhiteSpace(indexName))
                {
                    index.SetDatabaseName(ToSnakeCase(indexName));
                }
            }
        }
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var builder = new StringBuilder(value.Length + 8);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsUpper(character) && index > 0 && value[index - 1] != '_')
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }
}

public sealed class SportEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsOlympic { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class CompetitionEntity
{
    public int Id { get; set; }
    public int SportId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public bool IsCup { get; set; }
    public string ExternalSource { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class SeasonEntity
{
    public int Id { get; set; }
    public int CompetitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int YearStart { get; set; }
    public int YearEnd { get; set; }
    public bool IsCurrent { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class TeamEntity
{
    public int Id { get; set; }
    public int SportId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public int Founded { get; set; }
    public string BadgeUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class TeamSeasonEntity
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int SeasonId { get; set; }
    public string CoachName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class TeamMemberEntity
{
    public int Id { get; set; }
    public int TeamSeasonId { get; set; }
    public int PersonId { get; set; }
    public string SquadRole { get; set; } = string.Empty;
    public int? ShirtNumber { get; set; }
    public DateTimeOffset JoinedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PersonEntity
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PrimaryRole { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class MatchEntity
{
    public int Id { get; set; }
    public int SportId { get; set; }
    public int CompetitionId { get; set; }
    public int SeasonId { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public DateTimeOffset KickoffUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class MatchEventEntity
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int Minute { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int? TeamId { get; set; }
    public int? PersonId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PlayerStatEntity
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int PersonId { get; set; }
    public int TeamId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string StatPayloadJson { get; set; } = "{}";
    public decimal MetricValue { get; set; }
    public DateTimeOffset RecordedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class BroadcasterEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class MatchBroadcastEntity
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int BroadcasterId { get; set; }
    public string Region { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
}

public sealed class StandingEntity
{
    public int Id { get; set; }
    public int CompetitionId { get; set; }
    public int SeasonId { get; set; }
    public int TeamId { get; set; }
    public int Position { get; set; }
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int Points { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class IndividualRankingEntity
{
    public int Id { get; set; }
    public int CompetitionId { get; set; }
    public int SeasonId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class PersonRankingEntity
{
    public int Id { get; set; }
    public int RankingId { get; set; }
    public int PersonId { get; set; }
    public int TeamId { get; set; }
    public int Position { get; set; }
    public int Value { get; set; }
}

public sealed class AdminUserEntity
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAtUtc { get; set; }
}

public sealed class ChangeLogEntity
{
    public int Id { get; set; }
    public int? AdminUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}