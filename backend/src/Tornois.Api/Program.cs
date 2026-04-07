using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tornois.Api.BackgroundJobs;
using Tornois.Api.Infrastructure;
using Tornois.Infrastructure;
using Tornois.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"];
var syncJobOptions = builder.Configuration.GetSection(SyncJobOptions.SectionName).Get<SyncJobOptions>() ?? new SyncJobOptions();
var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? "Host=localhost;Port=5433;Database=tornois;Username=tornois;Password=tornois_dev_password";

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("public", limiterOptions =>
    {
        limiterOptions.PermitLimit = 60;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddDbContext<TornoisDbContext>(options =>
    options.UseNpgsql(postgresConnectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__ef_migrations_history", "admin")));
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddTornoisPlatformServices();
builder.Services.AddTornoisBackgroundJobs(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

await app.Services.InitialiseTornoisDatabaseAsync(app.Logger);

app.MapGet("/api/health", async (TornoisDbContext dbContext, CancellationToken cancellationToken) =>
{
    bool databaseCanConnect;
    string? lastDatabaseError = null;

    try
    {
        databaseCanConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
    }
    catch (Exception exception)
    {
        databaseCanConnect = false;
        lastDatabaseError = exception.Message;
    }

    return Results.Ok(new
    {
        status = "ok",
        service = "Tornois.Api",
        utcNow = DateTimeOffset.UtcNow,
        databaseProvider = dbContext.Database.ProviderName ?? "unavailable",
        databaseCanConnect,
        backgroundJobsEnabled = syncJobOptions.Enabled,
        connectionStringConfigured = !string.IsNullOrWhiteSpace(postgresConnectionString),
        lastDatabaseError
    });
});
app.MapControllers();

app.Run();

public partial class Program;
