using HwSimulator.App.Models;

namespace HwSimulator.App.Transport;

public interface IEventSender
{
    Task SendAsync(HwEvent hwEvent, CancellationToken ct);
}
