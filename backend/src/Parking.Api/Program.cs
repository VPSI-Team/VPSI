using Asp.Versioning;
using Parking.Api.Auth;
using Parking.Api.Middleware;
using Parking.Application;
using Parking.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Application + Infrastructure ─────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── API Versioning ────────────────────────────────────────────────────────────
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ── OpenAPI / Scalar ──────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── Auth placeholder ──────────────────────────────────────────────────────────
builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(ParkingPolicies.AdminOnly,
        p => p.RequireRole(ParkingRoles.Admin));
    options.AddPolicy(ParkingPolicies.AdminOrTechnician,
        p => p.RequireRole(ParkingRoles.Admin, ParkingRoles.Technician));
    options.AddPolicy(ParkingPolicies.AdminOrFinance,
        p => p.RequireRole(ParkingRoles.Admin, ParkingRoles.Finance));
    options.AddPolicy(ParkingPolicies.AnyStaff,
        p => p.RequireRole(ParkingRoles.Admin, ParkingRoles.Technician, ParkingRoles.Finance));
});

// ── Health checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<Parking.Infrastructure.Persistence.AppDbContext>(
        name: "db",
        tags: ["ready"]);

// ─────────────────────────────────────────────────────────────────────────────

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Parking API";
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false  // liveness: no checks, just app-is-up
}).WithMetadata(new Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute());

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = hc => hc.Tags.Contains("ready")
}).WithMetadata(new Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute());

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
