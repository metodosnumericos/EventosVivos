using System.ComponentModel.DataAnnotations;
using EventosVivos.Domain.Events;

namespace EventosVivos.Api.Contracts;

public record CreateEventRequest(
    [Required, StringLength(100, MinimumLength = 5)] string Title,
    [Required, StringLength(500, MinimumLength = 10)] string Description,
    [Required] int VenueId,
    [Required, Range(1, int.MaxValue)] int MaxCapacity,
    [Required] DateTimeOffset StartsAt,
    [Required] DateTimeOffset EndsAt,
    [Required, Range(0.01, double.MaxValue)] decimal TicketPrice,
    [Required] EventType Type);

public record EventResponse(
    int Id,
    string Title,
    string Description,
    int VenueId,
    string? VenueName,
    string? VenueCity,
    int MaxCapacity,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    decimal TicketPrice,
    string Type,
    string EffectiveState);
