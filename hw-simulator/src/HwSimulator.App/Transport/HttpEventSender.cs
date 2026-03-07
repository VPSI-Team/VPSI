using System.Net.Http.Json;
using HwSimulator.App.Models;
using Microsoft.Extensions.Options;

namespace HwSimulator.App.Transport;

public sealed class HttpEventSender(
    HttpClient httpClient,
    IOptions<SimulatorOptions> options,
    ILogger<HttpEventSender> logger) : IEventSender
{
    private readonly string _endpoint = $"{options.Value.ApiBaseUrl.TrimEnd('/')}/api/v1/hw/events";

    public async Task SendAsync(HwEvent hwEvent, CancellationToken ct)
    {
        logger.LogInformation(
            "Odesílám event {EventType} z {DeviceId} (key: {Key})",
            hwEvent.EventType, hwEvent.DeviceId, hwEvent.IdempotencyKey);

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(_endpoint, hwEvent, ct);

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation(
                        "Event {Key} přijat — HTTP {StatusCode}",
                        hwEvent.IdempotencyKey, (int)response.StatusCode);
                    return;
                }

                logger.LogWarning(
                    "Event {Key} — HTTP {StatusCode} (pokus {Attempt}/2)",
                    hwEvent.IdempotencyKey, (int)response.StatusCode, attempt);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex,
                    "Event {Key} — chyba spojení (pokus {Attempt}/2)",
                    hwEvent.IdempotencyKey, attempt);
            }

            if (attempt < 2)
                await Task.Delay(500, ct);
        }

        logger.LogError("Event {Key} se nepodařilo odeslat po 2 pokusech — pokračuji.", hwEvent.IdempotencyKey);
    }
}
