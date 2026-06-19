using EventosVivos.Domain.Venues;
using EventosVivos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.SeedData;

public static class VenueSeeder
{
    public static async Task SeedAsync(EventosVivosDbContext db)
    {
        var existing = await db.Venues.Select(v => v.Id).ToListAsync();
        var toAdd = new[]
        {
            new Venue(1, "Auditorio Central", 200, "Bogota"),
            new Venue(2, "Sala Norte",         50,  "Bogota"),
            new Venue(3, "Arena Sur",          500, "Medellin")
        }.Where(v => !existing.Contains(v.Id));

        db.Venues.AddRange(toAdd);
        await db.SaveChangesAsync();
    }
}
