using EventosVivos.Domain.Events;

namespace EventosVivos.Application.Common;

public record EventFilter(
    EventType? Type = null,
    int? VenueId = null,
    EventEffectiveState? State = null,
    DateTimeOffset? StartsFrom = null,
    DateTimeOffset? StartsTo = null,
    string? Title = null);

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetAllAsync(EventFilter filter, DateTimeOffset utcNow, CancellationToken ct = default);
    Task<Event?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Event ev, CancellationToken ct = default);
    Task<bool> HasActiveOverlapAsync(int venueId, DateTimeOffset startsAt, DateTimeOffset endsAt, int? excludeEventId, DateTimeOffset utcNow, CancellationToken ct = default);
}
