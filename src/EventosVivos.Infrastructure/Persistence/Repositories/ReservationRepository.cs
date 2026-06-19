using EventosVivos.Application.Common;
using EventosVivos.Domain.Reservations;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly EventosVivosDbContext _db;

    public ReservationRepository(EventosVivosDbContext db) => _db = db;

    public async Task<Reservation?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Reservations.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(Reservation reservation, CancellationToken ct = default)
        => await _db.Reservations.AddAsync(reservation, ct);

    public async Task<IReadOnlyList<Reservation>> GetByEventIdAsync(
        int? eventId, ReservationState? state, CancellationToken ct = default)
    {
        var query = _db.Reservations.AsQueryable();
        if (eventId.HasValue)
            query = query.Where(r => r.EventId == eventId.Value);
        if (state.HasValue)
            query = query.Where(r => r.State == state.Value);
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
    }

    public async Task<int> GetHeldCapacityAsync(int eventId, CancellationToken ct = default)
    {
        return await _db.Reservations
            .Where(r => r.EventId == eventId
                && (r.State == ReservationState.PendingPayment
                    || r.State == ReservationState.Confirmed
                    || (r.State == ReservationState.Canceled && r.LostCapacity)))
            .SumAsync(r => r.Quantity, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
        => await _db.Reservations.AnyAsync(r => r.ReservationCode == code, ct);
}
