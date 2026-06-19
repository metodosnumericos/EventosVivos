using EventosVivos.Application.Common;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Shared;

namespace EventosVivos.Application.Reservations;

public record CreateReservationCommand(
    int EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail);

public class CreateReservationUseCase
{
    private readonly IEventRepository _events;
    private readonly IReservationRepository _reservations;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public CreateReservationUseCase(
        IEventRepository events,
        IReservationRepository reservations,
        IUnitOfWork uow,
        ITimeProvider time)
    {
        _events = events;
        _reservations = reservations;
        _uow = uow;
        _time = time;
    }

    public async Task<Reservation> ExecuteAsync(CreateReservationCommand cmd, CancellationToken ct = default)
    {
        var now = _time.UtcNow;

        var ev = await _events.GetByIdAsync(cmd.EventId, ct)
            ?? throw new NotFoundException($"Event {cmd.EventId} not found.");

        var effectiveState = ev.GetEffectiveState(now);

        if (effectiveState != EventEffectiveState.Active)
            throw new ReservationWindowClosedException($"Reservations are not available for events in state {effectiveState}.");

        var timeUntilStart = ev.StartsAt - now;

        if (timeUntilStart < TimeSpan.FromHours(1))
            throw new ReservationWindowClosedException("Reservations close 1 hour before the event starts.");

        // Compute the strictest quantity limit
        int? limit = null;

        if (timeUntilStart < TimeSpan.FromHours(24))
            limit = Min(limit, 5);

        if (ev.TicketPrice > 100m)
            limit = Min(limit, 10);

        if (limit.HasValue && cmd.Quantity > limit.Value)
            throw new TicketQuantityLimitException(limit.Value,
                timeUntilStart < TimeSpan.FromHours(24)
                    ? "Event starts in less than 24 hours."
                    : "Ticket price exceeds 100.");

        // Optimistic concurrency loop
        const int maxRetries = 3;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            var held = await _reservations.GetHeldCapacityAsync(cmd.EventId, ct);
            var available = ev.MaxCapacity - held;

            if (cmd.Quantity > available)
                throw new CapacityExceededException();

            var reservation = Reservation.Create(cmd.EventId, cmd.Quantity, cmd.BuyerName, cmd.BuyerEmail, now);
            await _reservations.AddAsync(reservation, ct);

            ev.IncrementVersion();

            try
            {
                await _uow.SaveChangesAsync(ct);
                return reservation;
            }
            catch (OptimisticConcurrencyException) when (attempt < maxRetries - 1)
            {
                // Detach both entities so the identity map doesn't return stale data on reload.
                // The zombie reservation must not be re-inserted on the next attempt.
                _uow.Detach(reservation);
                _uow.Detach(ev);
                var freshEvent = await _events.GetByIdAsync(cmd.EventId, ct);
                if (freshEvent is null) throw new NotFoundException($"Event {cmd.EventId} not found.");
                ev = freshEvent;
            }
        }

        throw new CapacityExceededException();
    }

    private static int Min(int? current, int candidate) =>
        current.HasValue ? Math.Min(current.Value, candidate) : candidate;
}
