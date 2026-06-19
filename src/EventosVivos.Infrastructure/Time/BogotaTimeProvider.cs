using EventosVivos.Domain.Shared;

namespace EventosVivos.Infrastructure.Time;

public class BogotaTimeProvider : ITimeProvider
{
    private static readonly TimeZoneInfo BogotaTz =
        TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateTimeOffset BogotaNow =>
        TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, BogotaTz);
}
