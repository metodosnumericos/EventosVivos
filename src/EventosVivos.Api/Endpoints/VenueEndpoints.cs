using EventosVivos.Api.Contracts;
using EventosVivos.Application.Common;

namespace EventosVivos.Api.Endpoints;

public static class VenueEndpoints
{
    public static RouteGroupBuilder MapVenueEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IVenueRepository venues) =>
        {
            var all = await venues.GetAllAsync();
            return Results.Ok(all.Select(v => new VenueResponse(v.Id, v.Name, v.Capacity, v.City)));
        });

        return group;
    }
}
