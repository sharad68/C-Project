using Microsoft.Extensions.Options;
using Quartz;

namespace Tornois.Api.BackgroundJobs;

public sealed class SyncJobOptions
{
    public const string SectionName = "SyncJobs";

    public bool Enabled { get; init; } = true;
    public int DailyApiQuota { get; init; } = 500;
    public string LiveScoresCron { get; init; } = "0 0/5 * * * ?";
    public string FixturesCron { get; init; } = "0 0 3 * * ?";
    public string EnrichmentCron { get; init; } = "0 0 2 ? * SUN";
}

public sealed class ExternalApiOptions
{
    public const string SectionName = "ExternalApis";

    public string ApiSportsBaseUrl { get; init; } = "https://v3.football.api-sports.io";
    public string TheSportsDbBaseUrl { get; init; } = "https://www.thesportsdb.com/api/v1/json";
    public string ApiSportsApiKey { get; init; } = string.Empty;
    public string TheSportsDbApiKey { get; init; } = string.Empty;
}

public interface ISyncQuotaGuard
{
    bool TryConsumeQuota(string jobName, out int remainingCalls);
}

internal sealed class InMemorySyncQuotaGuard(IOptions<SyncJobOptions> options, ILogger<InMemorySyncQuotaGuard> logger) : ISyncQuotaGuard
{
    private readonly object _gate = new();
    private DateOnly _currentDay = DateOnly.FromDateTime(DateTime.UtcNow);
    private int _consumedCalls;

    public bool TryConsumeQuota(string jobName, out int remainingCalls)
    {
        lock (_gate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (today != _currentDay)
            {
                _currentDay = today;
                _consumedCalls = 0;
            }

            if (_consumedCalls >= options.Value.DailyApiQuota)
            {
                remainingCalls = 0;
                logger.LogWarning("Skipping {JobName} because the configured daily API quota has been exhausted.", jobName);
                return false;
            }

            _consumedCalls++;
            remainingCalls = Math.Max(options.Value.DailyApiQuota - _consumedCalls, 0);
            return true;
        }
    }
}

public static class BackgroundJobRegistrationExtensions
{
    public static IServiceCollection AddTornoisBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SyncJobOptions>(configuration.GetSection(SyncJobOptions.SectionName));
        services.Configure<ExternalApiOptions>(configuration.GetSection(ExternalApiOptions.SectionName));
        services.AddSingleton<ISyncQuotaGuard, InMemorySyncQuotaGuard>();

        var options = configuration.GetSection(SyncJobOptions.SectionName).Get<SyncJobOptions>() ?? new SyncJobOptions();
        if (!options.Enabled)
        {
            return services;
        }

        services.AddQuartz(quartz =>
        {
            var liveScoresJobKey = new JobKey(nameof(LiveScoresSyncJob));
            quartz.AddJob<LiveScoresSyncJob>(job => job.WithIdentity(liveScoresJobKey));
            quartz.AddTrigger(trigger => trigger
                .ForJob(liveScoresJobKey)
                .WithIdentity($"{nameof(LiveScoresSyncJob)}-trigger")
                .WithCronSchedule(options.LiveScoresCron));

            var fixturesJobKey = new JobKey(nameof(FixturesSyncJob));
            quartz.AddJob<FixturesSyncJob>(job => job.WithIdentity(fixturesJobKey));
            quartz.AddTrigger(trigger => trigger
                .ForJob(fixturesJobKey)
                .WithIdentity($"{nameof(FixturesSyncJob)}-trigger")
                .WithCronSchedule(options.FixturesCron));

            var enrichmentJobKey = new JobKey(nameof(EnrichmentSyncJob));
            quartz.AddJob<EnrichmentSyncJob>(job => job.WithIdentity(enrichmentJobKey));
            quartz.AddTrigger(trigger => trigger
                .ForJob(enrichmentJobKey)
                .WithIdentity($"{nameof(EnrichmentSyncJob)}-trigger")
                .WithCronSchedule(options.EnrichmentCron));
        });

        services.AddQuartzHostedService(settings => settings.WaitForJobsToComplete = true);
        return services;
    }
}

internal abstract class LoggedSyncJob(
    ISyncQuotaGuard quotaGuard,
    IOptions<ExternalApiOptions> externalApis,
    ILogger logger) : IJob
{
    protected ExternalApiOptions ApiSettings => externalApis.Value;

    protected Task RunAsync(string jobName, string targetUrl)
    {
        if (!quotaGuard.TryConsumeQuota(jobName, out var remainingCalls))
        {
            return Task.CompletedTask;
        }

        var hasApiKey = !string.IsNullOrWhiteSpace(ApiSettings.ApiSportsApiKey)
            || !string.IsNullOrWhiteSpace(ApiSettings.TheSportsDbApiKey);

        logger.LogInformation(
            "{JobName} executed at {UtcNow}. Target={TargetUrl}. Remaining quota={RemainingCalls}. KeysConfigured={KeysConfigured}",
            jobName,
            DateTimeOffset.UtcNow,
            targetUrl,
            remainingCalls,
            hasApiKey);

        return Task.CompletedTask;
    }

    public abstract Task Execute(IJobExecutionContext context);
}

internal sealed class LiveScoresSyncJob(
    ISyncQuotaGuard quotaGuard,
    IOptions<ExternalApiOptions> externalApis,
    ILogger<LiveScoresSyncJob> logger)
    : LoggedSyncJob(quotaGuard, externalApis, logger)
{
    public override Task Execute(IJobExecutionContext context)
        => RunAsync(nameof(LiveScoresSyncJob), $"{ApiSettings.ApiSportsBaseUrl}/fixtures?live=all");
}

internal sealed class FixturesSyncJob(
    ISyncQuotaGuard quotaGuard,
    IOptions<ExternalApiOptions> externalApis,
    ILogger<FixturesSyncJob> logger)
    : LoggedSyncJob(quotaGuard, externalApis, logger)
{
    public override Task Execute(IJobExecutionContext context)
        => RunAsync(nameof(FixturesSyncJob), $"{ApiSettings.ApiSportsBaseUrl}/fixtures?next=25");
}

internal sealed class EnrichmentSyncJob(
    ISyncQuotaGuard quotaGuard,
    IOptions<ExternalApiOptions> externalApis,
    ILogger<EnrichmentSyncJob> logger)
    : LoggedSyncJob(quotaGuard, externalApis, logger)
{
    public override Task Execute(IJobExecutionContext context)
        => RunAsync(nameof(EnrichmentSyncJob), ApiSettings.TheSportsDbBaseUrl);
}