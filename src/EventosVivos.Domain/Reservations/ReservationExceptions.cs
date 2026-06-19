using EventosVivos.Domain.Shared;

namespace EventosVivos.Domain.Reservations;

public class InvalidStateTransitionException : ConflictException
{
    public InvalidStateTransitionException(ReservationState current, string targetAction)
        : base($"Cannot {targetAction} a reservation in state {current}.") { }
}

public class CapacityExceededException : ConflictException
{
    public CapacityExceededException() : base("Not enough capacity available for this reservation.") { }
}

public class ReservationWindowClosedException : BusinessRuleException
{
    public ReservationWindowClosedException(string reason) : base(reason) { }
}

public class TicketQuantityLimitException : BusinessRuleException
{
    public TicketQuantityLimitException(int limit, string reason)
        : base($"Maximum {limit} tickets per transaction. {reason}") { }
}

public class OwnershipProofFailedException : ConflictException
{
    public OwnershipProofFailedException() : base("Reservation ownership proof is invalid.") { }
}
