using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parking.Application.Abstractions;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Persistence.Repositories;
using Parking.Infrastructure.Services;

namespace Parking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IParkingSessionRepository, ParkingSessionRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IPlateRecognizer, PlateRecognizerStub>();
        services.AddScoped<IPaymentGateway, PaymentGatewayStub>();
        services.AddScoped<IEventProcessor, EventProcessorService>();

        return services;
    }
}
