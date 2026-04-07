using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tornois.Infrastructure.Data;

public sealed class TornoisDbContextFactory : IDesignTimeDbContextFactory<TornoisDbContext>
{
    public TornoisDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5433;Database=tornois;Username=tornois;Password=tornois_dev_password";

        var optionsBuilder = new DbContextOptionsBuilder<TornoisDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable("__ef_migrations_history", "admin"));

        return new TornoisDbContext(optionsBuilder.Options);
    }
}