using EventosVivos.Domain.Venues;

namespace EventosVivos.Domain.Events;

public class Event
{
    private static readonly TimeZoneInfo BogotaTz =
        TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

    public int Id { get; private set; }
    public int VenueId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public int MaxCapacity { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public decimal TicketPrice { get; private set; }
    public EventType Type { get; private set; }
    public EventState State { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public int Version { get; private set; }

    public Venue? Venue { get; private set; }

    private Event() { }

    public static Event Create(
        string title,
        string description,
        int venueId,
        int venueCapacity,
        int maxCapacity,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        decimal ticketPrice,
        EventType type,
        DateTimeOffset now)
    {
        if (maxCapacity > venueCapacity)
            throw new EventCapacityExceedsVenueException(maxCapacity, venueCapacity);

        if (startsAt <= now)
            throw new InvalidEventDateException("Event start date must be in the future.");

        if (endsAt <= startsAt)
            throw new InvalidEventDateException("Event end date must be after start date.");

        var localStart = TimeZoneInfo.ConvertTime(startsAt, BogotaTz);
        if (IsWeekend(localStart.DayOfWeek) && localStart.Hour >= 22)
            throw new WeekendCutoffException();

        return new Event
        {
            Title = title,
            Description = description,
            VenueId = venueId,
            MaxCapacity = maxCapacity,
            StartsAt = startsAt,
            EndsAt = endsAt,
            TicketPrice = ticketPrice,
            Type = type,
            State = EventState.Active,
            CreatedAt = now,
            Version = 0
        };
    }

    public EventEffectiveState GetEffectiveState(DateTimeOffset utcNow)
    {
        if (State == EventState.Canceled)
            return EventEffectiveState.Canceled;

        var bogotaNow = TimeZoneInfo.ConvertTime(utcNow, BogotaTz);
        var bogotaEndsAt = TimeZoneInfo.ConvertTime(EndsAt, BogotaTz);
        if (bogotaNow > bogotaEndsAt)
            return EventEffectiveState.Completed;

        return EventEffectiveState.Active;
    }

    public void IncrementVersion() => Version++;

    private static bool IsWeekend(DayOfWeek day) =>
        day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;
}
