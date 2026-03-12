using System.Text.Json;
using Parking.Contracts.Enums;

namespace Parking.Application.Abstractions;

public interface IEventProcessor
{
    Task ProcessAsync(Guid deviceId, HwEventType eventType, JsonElement payload, CancellationToken ct = default);
}
