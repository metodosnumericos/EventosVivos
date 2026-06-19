using EventosVivos.Application.Common;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Shared;

namespace EventosVivos.Application.Reports;

public record OccupancyReport(
    int EventId,
    string EventTitle,
    string EventState,
    int ConfirmedTicketsSold,
    int PendingTicketsHeld,
    int LostTickets,
    int RemainingTickets,
    double OccupancyPercentage,
    decimal TotalIncome);

public class GetOccupancyReportUseCase
{
    private readonly IEventRepository _events;
    private readonly IReservationRepository _reservations;
    private readonly ITimeProvider _time;

    public GetOccupancyReportUseCase(
        IEventRepository events,
        IReservationRepository reservations,
        ITimeProvider time)
    {
        _events = events;
        _reservations = reservations;
        _time = time;
    }

    public async Task<OccupancyReport> ExecuteAsync(int eventId, CancellationToken ct = default)
    {
        var ev = await _events.GetByIdAsync(eventId, ct)
            ?? throw new NotFoundException($"Event {eventId} not found.");

        var all = await _reservations.GetByEventIdAsync(eventId, state: null, ct);

        var confirmed = all.Where(r => r.State == ReservationState.Confirmed).Sum(r => r.Quantity);
        var pending = all.Where(r => r.State == ReservationState.PendingPayment).Sum(r => r.Quantity);
        var lost = all.Where(r => r.State == ReservationState.Canceled && r.LostCapacity).Sum(r => r.Quantity);

        // Remaining = max - (pending held + confirmed sold + lost tickets still blocking)
        var remaining = ev.MaxCapacity - pending - confirmed - lost;
        var occupancy = ev.MaxCapacity > 0 ? confirmed * 100.0 / ev.MaxCapacity : 0;
        var income = ev.TicketPrice * confirmed;

        var effectiveState = ev.GetEffectiveState(_time.UtcNow).ToString();

        return new OccupancyReport(
            ev.Id, ev.Title, effectiveState,
            confirmed, pending, lost, remaining,
            Math.Round(occupancy, 2), income);
    }
}
