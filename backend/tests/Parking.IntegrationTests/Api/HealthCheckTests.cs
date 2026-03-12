using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Parking.Infrastructure.Persistence;

namespace Parking.IntegrationTests.Api;

public sealed class HealthCheckTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private HttpClient CreateClient() =>
        factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace real Postgres DbContext with InMemory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor is not null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(opts =>
                    opts.UseInMemoryDatabase($"parking-test-{Guid.NewGuid()}"));

                // Replace the "db" health check with an always-healthy stub for tests
                services.PostConfigure<HealthCheckServiceOptions>(options =>
                {
                    var existing = options.Registrations.FirstOrDefault(r => r.Name == "db");
                    if (existing is not null) options.Registrations.Remove(existing);

                    options.Registrations.Add(new HealthCheckRegistration(
                        name: "db",
                        factory: _ => new AlwaysHealthyCheck(),
                        failureStatus: null,
                        tags: ["ready"]));
                });
            });
        }).CreateClient();

    [Fact]
    public async Task LivenessEndpoint_ReturnsOk()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReadinessEndpoint_ReturnsOk()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/health/ready");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed class AlwaysHealthyCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default) =>
            Task.FromResult(HealthCheckResult.Healthy());
    }
}
