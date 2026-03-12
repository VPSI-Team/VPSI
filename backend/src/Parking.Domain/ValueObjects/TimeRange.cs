namespace Parking.Domain.ValueObjects;

public sealed record TimeRange
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset? End { get; }

    private TimeRange(DateTimeOffset start, DateTimeOffset? end)
    {
        Start = start;
        End = end;
    }

    public static TimeRange StartAt(DateTimeOffset start) => new(start, null);

    public TimeRange Close(DateTimeOffset end)
    {
        if (end < Start)
            throw new ArgumentException("End time cannot be before start time.", nameof(end));
        return new TimeRange(Start, end);
    }

    public TimeSpan Duration => (End ?? DateTimeOffset.UtcNow) - Start;

    public bool IsOpen => End is null;
}
