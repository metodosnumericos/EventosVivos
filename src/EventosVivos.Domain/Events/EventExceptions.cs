using EventosVivos.Domain.Shared;

namespace EventosVivos.Domain.Events;

public class VenueConflictException : ConflictException
{
    public VenueConflictException() : base("The venue already has an active event during that time slot.") { }
}

public class WeekendCutoffException : BusinessRuleException
{
    public WeekendCutoffException() : base("Weekend events cannot start after 22:00 (America/Bogota).") { }
}

public class EventCapacityExceedsVenueException : BusinessRuleException
{
    public EventCapacityExceedsVenueException(int maxCapacity, int venueCapacity)
        : base($"Event capacity {maxCapacity} exceeds venue capacity {venueCapacity}.") { }
}

public class InvalidEventDateException : InputValidationException
{
    public InvalidEventDateException(string message) : base(message) { }
}
