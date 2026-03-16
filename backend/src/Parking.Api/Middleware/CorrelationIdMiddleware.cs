namespace Parking.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var correlationId)
            || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items[HeaderName] = correlationId.ToString();
        context.Response.Headers[HeaderName] = correlationId.ToString();

        await next(context);
    }
}
