using EventosVivos.Domain.Shared;

namespace EventosVivos.Application.Common;

public sealed class DuplicateReservationCodeException : ConflictException
{
    public DuplicateReservationCodeException()
        : base("The generated reservation code is already in use.") { }
}
