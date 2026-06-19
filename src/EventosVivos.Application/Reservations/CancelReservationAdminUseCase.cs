using EventosVivos.Application.Common;
using EventosVivos.Domain.Shared;

namespace EventosVivos.Application.Reservations;

public class CancelReservationAdminUseCase
{
    private readonly IReservationRepository _reservations;
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public CancelReservationAdminUseCase(
        IReservationRepository reservations,
        IEventRepository events,
        IUnitOfWork uow,
        ITimeProvider time)
    {
        _reservations = reservations;
        _events = events;
        _uow = uow;
        _time = time;
    }

    public async Task ExecuteAsync(int reservationId, CancellationToken ct = default)
    {
        var reservation = await _reservations.GetByIdAsync(reservationId, ct)
            ?? throw new NotFoundException($"Reservation {reservationId} not found.");

        var ev = await _events.GetByIdAsync(reservation.EventId, ct)
            ?? throw new NotFoundException($"Event {reservation.EventId} not found.");

        var now = _time.UtcNow;
        var releasesCapacity = reservation.ReleasesCapacityOnCancel(now, ev.StartsAt);

        reservation.Cancel(now, ev.StartsAt);

        if (releasesCapacity)
            ev.IncrementVersion();

        await _uow.SaveChangesAsync(ct);
    }
}
