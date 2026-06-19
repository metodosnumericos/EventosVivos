using System.ComponentModel.DataAnnotations;

namespace EventosVivos.Api.Contracts;

public record CreateReservationRequest(
    [Required] int EventId,
    [Required, Range(1, int.MaxValue)] int Quantity,
    [Required, StringLength(200, MinimumLength = 1)] string BuyerName,
    [Required, EmailAddress] string BuyerEmail);

public record BuyerCancelRequest(
    [Required, EmailAddress] string BuyerEmail,
    string? ReservationCode);

public record ReservationResponse(
    int Id,
    int EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail,
    string State,
    string? ReservationCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? CanceledAt,
    bool LostCapacity);
