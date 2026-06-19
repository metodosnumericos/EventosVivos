using EventosVivos.Application.Common;
using EventosVivos.Domain.Reservations;

namespace EventosVivos.Application.Reservations;

public class ListReservationsUseCase
{
    private readonly IReservationRepository _reservations;

    public ListReservationsUseCase(IReservationRepository reservations)
    {
        _reservations = reservations;
    }

    public async Task<IReadOnlyList<Reservation>> ExecuteAsync(
        int eventId, ReservationState? state, CancellationToken ct = default)
    {
        return await _reservations.GetByEventIdAsync(eventId, state, ct);
    }
}
