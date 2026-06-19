namespace EventosVivos.Domain.Reservations;

public class Reservation
{
    public int Id { get; private set; }
    public int EventId { get; private set; }
    public int Quantity { get; private set; }
    public string BuyerName { get; private set; } = default!;
    public string BuyerEmail { get; private set; } = default!;
    public ReservationState State { get; private set; }
    public string? ReservationCode { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }
    public bool LostCapacity { get; private set; }

    private Reservation() { }

    public static Reservation Create(int eventId, int quantity, string buyerName, string buyerEmail, DateTimeOffset now)
    {
        return new Reservation
        {
            EventId = eventId,
            Quantity = quantity,
            BuyerName = buyerName,
            BuyerEmail = buyerEmail,
            State = ReservationState.PendingPayment,
            CreatedAt = now
        };
    }

    public void Confirm(string code, DateTimeOffset now)
    {
        if (State != ReservationState.PendingPayment)
            throw new InvalidStateTransitionException(State, "confirm");

        State = ReservationState.Confirmed;
        ReservationCode = code;
        ConfirmedAt = now;
    }

    public bool ReleasesCapacityOnCancel(DateTimeOffset now, DateTimeOffset eventStartsAt)
    {
        if (State == ReservationState.PendingPayment)
            return true;

        if (State == ReservationState.Confirmed)
        {
            var timeUntilEvent = eventStartsAt - now;
            return timeUntilEvent >= TimeSpan.FromHours(48);
        }

        return false;
    }

    public void Cancel(DateTimeOffset now, DateTimeOffset eventStartsAt)
    {
        if (State == ReservationState.Canceled)
            throw new InvalidStateTransitionException(State, "cancel");

        var wasConfirmed = State == ReservationState.Confirmed;
        State = ReservationState.Canceled;
        CanceledAt = now;

        if (wasConfirmed)
        {
            var timeUntilEvent = eventStartsAt - now;
            LostCapacity = timeUntilEvent < TimeSpan.FromHours(48);
        }
    }

    public void ValidateBuyerOwnership(string buyerEmail, string? reservationCode)
    {
        if (!string.Equals(BuyerEmail, buyerEmail, StringComparison.OrdinalIgnoreCase))
            throw new OwnershipProofFailedException();

        if (State == ReservationState.Confirmed)
        {
            if (string.IsNullOrWhiteSpace(reservationCode) ||
                !string.Equals(ReservationCode, reservationCode, StringComparison.OrdinalIgnoreCase))
                throw new OwnershipProofFailedException();
        }
    }
}
