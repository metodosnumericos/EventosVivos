using EventosVivos.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly EventosVivosDbContext _db;

    public UnitOfWork(EventosVivosDbContext db) => _db = db;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new OptimisticConcurrencyException();
        }
    }
}
