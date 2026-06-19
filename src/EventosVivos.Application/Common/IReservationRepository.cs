using EventosVivos.Domain.Reservations;

namespace EventosVivos.Application.Common;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Reservation reservation, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetByEventIdAsync(int? eventId, ReservationState? state, CancellationToken ct = default);
    Task<int> GetHeldCapacityAsync(int eventId, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
}
