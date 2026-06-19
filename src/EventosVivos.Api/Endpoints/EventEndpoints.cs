using EventosVivos.Api.Contracts;
using EventosVivos.Application.Common;
using EventosVivos.Application.Events;
using EventosVivos.Application.Reports;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.Api.Endpoints;

public static class EventEndpoints
{
    public static RouteGroupBuilder MapEventEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
            [FromQuery] EventType? type,
            [FromQuery] int? venueId,
            [FromQuery] EventEffectiveState? state,
            [FromQuery] DateTimeOffset? startsFrom,
            [FromQuery] DateTimeOffset? startsTo,
            [FromQuery] string? title,
            ITimeProvider time,
            ListEventsUseCase useCase) =>
        {
            var filter = new EventFilter(type, venueId, state, startsFrom, startsTo, title);
            var events = await useCase.ExecuteAsync(filter);
            return Results.Ok(events.Select(e => ToResponse(e, time.UtcNow)));
        });

        group.MapPost("/", async (
            CreateEventRequest req,
            CreateEventUseCase useCase) =>
        {
            var cmd = new CreateEventCommand(
                req.Title, req.Description, req.VenueId, req.MaxCapacity,
                req.StartsAt, req.EndsAt, req.TicketPrice,
                req.Type ?? throw new InputValidationException("Event type is required."));
            var ev = await useCase.ExecuteAsync(cmd);
            return Results.Created($"/api/events/{ev.Id}", ToResponse(ev, DateTimeOffset.UtcNow));
        }).RequireAuthorization("AdminPolicy");

        group.MapGet("/{id:int}/occupancy", async (
            int id,
            GetOccupancyReportUseCase useCase) =>
        {
            var report = await useCase.ExecuteAsync(id);
            return Results.Ok(report);
        });

        return group;
    }

    private static EventResponse ToResponse(Domain.Events.Event e, DateTimeOffset utcNow) =>
        new(e.Id, e.Title, e.Description, e.VenueId,
            e.Venue?.Name, e.Venue?.City,
            e.MaxCapacity, e.StartsAt, e.EndsAt, e.TicketPrice,
            e.Type.ToString(), e.GetEffectiveState(utcNow).ToString());
}
