using EventosVivos.Application.Common;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Shared;

namespace EventosVivos.Application.Events;

public class ListEventsUseCase
{
    private readonly IEventRepository _events;
    private readonly ITimeProvider _time;

    public ListEventsUseCase(IEventRepository events, ITimeProvider time)
    {
        _events = events;
        _time = time;
    }

    public async Task<IReadOnlyList<Event>> ExecuteAsync(EventFilter filter, CancellationToken ct = default)
    {
        return await _events.GetAllAsync(filter, _time.UtcNow, ct);
    }
}
