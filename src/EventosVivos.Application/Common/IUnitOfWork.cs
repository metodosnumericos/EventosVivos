namespace EventosVivos.Application.Common;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    void Detach<T>(T entity) where T : class;
}
