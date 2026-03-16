using Parking.Domain.Enums;

namespace Parking.Domain.Entities;

public sealed class Device
{
    public Guid Id { get; private set; }
    public Guid ParkingLotId { get; private set; }
    public DeviceType Type { get; private set; }
    public DeviceProtocol Protocol { get; private set; }
    public DateTimeOffset? LastSeenAt { get; private set; }
    public bool IsActive { get; private set; }

    private Device() { }

    public static Device Create(Guid parkingLotId, DeviceType type, DeviceProtocol protocol) =>
        new()
        {
            Id = Guid.NewGuid(),
            ParkingLotId = parkingLotId,
            Type = type,
            Protocol = protocol,
            IsActive = true
        };

    public void RecordHeartbeat() => LastSeenAt = DateTimeOffset.UtcNow;

    public void Deactivate() => IsActive = false;
}
