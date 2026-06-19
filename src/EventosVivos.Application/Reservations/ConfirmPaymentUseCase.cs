using EventosVivos.Application.Common;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Shared;

namespace EventosVivos.Application.Reservations;

public class ConfirmPaymentUseCase
{
    private readonly IReservationRepository _reservations;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public ConfirmPaymentUseCase(
        IReservationRepository reservations,
        IUnitOfWork uow,
        ITimeProvider time)
    {
        _reservations = reservations;
        _uow = uow;
        _time = time;
    }

    public async Task<Reservation> ExecuteAsync(int reservationId, CancellationToken ct = default)
    {
        var reservation = await _reservations.GetByIdAsync(reservationId, ct)
            ?? throw new NotFoundException($"Reservation {reservationId} not found.");

        var now = _time.UtcNow;
        const int maxCodeAttempts = 5;

        for (int i = 0; i < maxCodeAttempts; i++)
        {
            var code = GenerateCode();
            if (await _reservations.CodeExistsAsync(code, ct))
                continue;

            reservation.Confirm(code, now);
            await _uow.SaveChangesAsync(ct);
            return reservation;
        }

        throw new ConfirmConflictException("Could not generate a unique reservation code. Please retry.");
    }

    private static string GenerateCode()
    {
        var number = Random.Shared.Next(0, 1_000_000);
        return $"EV-{number:D6}";
    }

    private sealed class ConfirmConflictException : Domain.Shared.ConflictException
    {
        public ConfirmConflictException(string message) : base(message) { }
    }
}
