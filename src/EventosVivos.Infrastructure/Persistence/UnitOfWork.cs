using EventosVivos.Application.Common;
using EventosVivos.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        catch (DbUpdateException ex) when (IsDuplicateReservationCode(ex))
        {
            throw new DuplicateReservationCodeException();
        }
        catch (DbUpdateException ex) when (IsVenueExclusionViolation(ex))
        {
            throw new VenueConflictException();
        }
    }

    public void Detach<T>(T entity) where T : class =>
        _db.Entry(entity).State = EntityState.Detached;

    private static bool IsDuplicateReservationCode(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg
            && pg.SqlState == "23505"
            && pg.ConstraintName?.Contains("reservation_code", StringComparison.OrdinalIgnoreCase) == true;

    private static bool IsVenueExclusionViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg && pg.SqlState == "23P01";
}
