using EventosVivos.Application.Common;
using EventosVivos.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly EventosVivosDbContext _db;

    public VenueRepository(EventosVivosDbContext db) => _db = db;

    public async Task<IReadOnlyList<Venue>> GetAllAsync(CancellationToken ct = default)
        => await _db.Venues.AsNoTracking().ToListAsync(ct);

    public async Task<Venue?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _db.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);
}
