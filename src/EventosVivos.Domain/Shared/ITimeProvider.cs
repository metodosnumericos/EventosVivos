namespace EventosVivos.Domain.Shared;

public interface ITimeProvider
{
    DateTimeOffset UtcNow { get; }
    DateTimeOffset BogotaNow { get; }
}
