using EventosVivos.Domain.Venues;

namespace EventosVivos.Application.Common;

public interface IVenueRepository
{
    Task<IReadOnlyList<Venue>> GetAllAsync(CancellationToken ct = default);
    Task<Venue?> GetByIdAsync(int id, CancellationToken ct = default);
}
