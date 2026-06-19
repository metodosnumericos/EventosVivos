using EventosVivos.Domain.Venues;
using EventosVivos.Domain.Shared;

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
        title = title?.Trim() ?? string.Empty;
        description = description?.Trim() ?? string.Empty;

        if (title.Length is < 5 or > 100)
            throw new InputValidationException("Event title must contain between 5 and 100 characters.");

        if (description.Length is < 10 or > 500)
            throw new InputValidationException("Event description must contain between 10 and 500 characters.");

        if (venueId < 1)
            throw new InputValidationException("Venue id must be greater than zero.");

        if (maxCapacity < 1)
            throw new InputValidationException("Event capacity must be greater than zero.");

        if (ticketPrice <= 0)
            throw new InputValidationException("Ticket price must be greater than zero.");

        if (!Enum.IsDefined(type))
            throw new InputValidationException("Event type is invalid.");

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
