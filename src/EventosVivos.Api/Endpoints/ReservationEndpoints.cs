using EventosVivos.Api.Contracts;
using EventosVivos.Application.Reservations;
using EventosVivos.Domain.Reservations;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.Api.Endpoints;

public static class ReservationEndpoints
{
    public static RouteGroupBuilder MapReservationEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
            [FromQuery] int eventId,
            [FromQuery] ReservationState? state,
            ListReservationsUseCase useCase) =>
        {
            var list = await useCase.ExecuteAsync(eventId, state);
            return Results.Ok(list.Select(ToResponse));
        }).RequireAuthorization("AdminPolicy");

        group.MapPost("/", async (
            CreateReservationRequest req,
            CreateReservationUseCase useCase) =>
        {
            var cmd = new CreateReservationCommand(req.EventId, req.Quantity, req.BuyerName, req.BuyerEmail);
            var res = await useCase.ExecuteAsync(cmd);
            return Results.Created($"/api/reservations/{res.Id}", ToResponse(res));
        });

        group.MapPost("/{id:int}/confirm", async (
            int id,
            ConfirmPaymentUseCase useCase) =>
        {
            var res = await useCase.ExecuteAsync(id);
            return Results.Ok(ToResponse(res));
        }).RequireAuthorization("AdminPolicy");

        group.MapPost("/{id:int}/cancel", async (
            int id,
            CancelReservationAdminUseCase useCase) =>
        {
            await useCase.ExecuteAsync(id);
            return Results.NoContent();
        }).RequireAuthorization("AdminPolicy");

        group.MapPost("/{id:int}/buyer-cancel", async (
            int id,
            BuyerCancelRequest req,
            CancelReservationBuyerUseCase useCase) =>
        {
            var cmd = new BuyerCancelCommand(id, req.BuyerEmail, req.ReservationCode);
            await useCase.ExecuteAsync(cmd);
            return Results.NoContent();
        });

        return group;
    }

    private static ReservationResponse ToResponse(Reservation r) =>
        new(r.Id, r.EventId, r.Quantity, r.BuyerName, r.BuyerEmail,
            r.State.ToString(), r.ReservationCode,
            r.CreatedAt, r.ConfirmedAt, r.CanceledAt, r.LostCapacity);
}
