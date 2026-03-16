using Microsoft.EntityFrameworkCore;
using Parking.Application.Abstractions;
using Parking.Domain.Entities;

namespace Parking.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ParkingSession> ParkingSessions => Set<ParkingSession>();
    public DbSet<Device> Devices => Set<Device>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
