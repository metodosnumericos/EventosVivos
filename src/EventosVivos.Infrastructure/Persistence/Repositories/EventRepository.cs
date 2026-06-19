using EventosVivos.Application.Common;
using EventosVivos.Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

public class EventRepository : IEventRepository
{
    private readonly EventosVivosDbContext _db;

    public EventRepository(EventosVivosDbContext db) => _db = db;

    public async Task<IReadOnlyList<Event>> GetAllAsync(
        EventFilter filter, DateTimeOffset utcNow, CancellationToken ct = default)
    {
        var query = _db.Events.Include(e => e.Venue).AsQueryable();

        if (filter.Type.HasValue)
            query = query.Where(e => e.Type == filter.Type.Value);

        if (filter.VenueId.HasValue)
            query = query.Where(e => e.VenueId == filter.VenueId.Value);

        if (filter.StartsFrom.HasValue)
            query = query.Where(e => e.StartsAt >= filter.StartsFrom.Value);

        if (filter.StartsTo.HasValue)
            query = query.Where(e => e.StartsAt <= filter.StartsTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.Title))
            query = query.Where(e => EF.Functions.ILike(e.Title, $"%{filter.Title}%"));

        if (filter.State.HasValue)
        {
            switch (filter.State.Value)
            {
                case EventEffectiveState.Canceled:
                    query = query.Where(e => e.State == EventState.Canceled);
                    break;
                case EventEffectiveState.Completed:
                    query = query.Where(e => e.State == EventState.Active && e.EndsAt < utcNow);
                    break;
                case EventEffectiveState.Active:
                    query = query.Where(e => e.State == EventState.Active && e.EndsAt >= utcNow);
                    break;
            }
        }

        return await query.OrderBy(e => e.StartsAt).ToListAsync(ct);
    }

    public async Task<Event?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Events.Include(e => e.Venue).FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(Event ev, CancellationToken ct = default)
        => await _db.Events.AddAsync(ev, ct);

    public async Task<bool> HasActiveOverlapAsync(
        int venueId, DateTimeOffset startsAt, DateTimeOffset endsAt,
        int? excludeEventId, DateTimeOffset utcNow, CancellationToken ct = default)
    {
        return await _db.Events
            .Where(e => e.VenueId == venueId
                && e.State == EventState.Active
                && e.EndsAt > utcNow       // exclude already-completed events
                && e.StartsAt < endsAt
                && e.EndsAt > startsAt
                && (excludeEventId == null || e.Id != excludeEventId))
            .AnyAsync(ct);
    }
}
