using FluentAssertions;
using NSubstitute;
using Parking.Application.Abstractions;
using Parking.Application.Sessions.Commands.RegisterEntry;
using Parking.Domain.Entities;
using Parking.Domain.ValueObjects;

namespace Parking.UnitTests.Application;

public sealed class RegisterEntryCommandHandlerTests
{
    private readonly IVehicleRepository _vehicleRepo = Substitute.For<IVehicleRepository>();
    private readonly IParkingSessionRepository _sessionRepo = Substitute.For<IParkingSessionRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private RegisterEntryCommandHandler CreateHandler() =>
        new(_vehicleRepo, _sessionRepo, _uow);

    [Fact]
    public async Task Handle_NewVehicle_CreatesVehicleAndSession()
    {
        var lotId = Guid.NewGuid();
        var command = new RegisterEntryCommand("1AB2345", "CZ", lotId, null);

        _vehicleRepo.GetByPlateAsync(Arg.Any<PlateNumber>(), Arg.Any<CancellationToken>())
            .Returns((Vehicle?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.SessionId.Should().NotBeEmpty();
        result.PlateNumber.Should().Be("1AB2345");
        await _vehicleRepo.Received(1).AddAsync(Arg.Any<Vehicle>(), Arg.Any<CancellationToken>());
        await _sessionRepo.Received(1).AddAsync(Arg.Any<ParkingSession>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingVehicle_ReusesVehicle()
    {
        var lotId = Guid.NewGuid();
        var plate = PlateNumber.Create("1AB2345");
        var existingVehicle = Vehicle.Create(plate, "CZ");
        var command = new RegisterEntryCommand("1AB2345", "CZ", lotId, null);

        _vehicleRepo.GetByPlateAsync(Arg.Any<PlateNumber>(), Arg.Any<CancellationToken>())
            .Returns(existingVehicle);

        var handler = CreateHandler();
        await handler.Handle(command, CancellationToken.None);

        await _vehicleRepo.DidNotReceive().AddAsync(Arg.Any<Vehicle>(), Arg.Any<CancellationToken>());
    }
}
