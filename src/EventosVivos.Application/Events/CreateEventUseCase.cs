using EventosVivos.Application.Common;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Shared;

namespace EventosVivos.Application.Events;

public record CreateEventCommand(
    string Title,
    string Description,
    int VenueId,
    int MaxCapacity,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    decimal TicketPrice,
    EventType Type);

public class CreateEventUseCase
{
    private readonly IEventRepository _events;
    private readonly IVenueRepository _venues;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public CreateEventUseCase(
        IEventRepository events,
        IVenueRepository venues,
        IUnitOfWork uow,
        ITimeProvider time)
    {
        _events = events;
        _venues = venues;
        _uow = uow;
        _time = time;
    }

    public async Task<Event> ExecuteAsync(CreateEventCommand cmd, CancellationToken ct = default)
    {
        var venue = await _venues.GetByIdAsync(cmd.VenueId, ct)
            ?? throw new NotFoundException($"Venue {cmd.VenueId} not found.");

        var now = _time.UtcNow;

        var hasOverlap = await _events.HasActiveOverlapAsync(
            cmd.VenueId, cmd.StartsAt, cmd.EndsAt, excludeEventId: null, now, ct);

        if (hasOverlap)
            throw new VenueConflictException();

        var ev = Event.Create(
            cmd.Title, cmd.Description, cmd.VenueId, venue.Capacity,
            cmd.MaxCapacity, cmd.StartsAt, cmd.EndsAt, cmd.TicketPrice, cmd.Type, now);

        await _events.AddAsync(ev, ct);
        await _uow.SaveChangesAsync(ct);
        return ev;
    }
}
