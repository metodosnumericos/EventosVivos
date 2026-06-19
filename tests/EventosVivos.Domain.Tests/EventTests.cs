using EventosVivos.Domain.Events;
using EventosVivos.Domain.Shared;

namespace EventosVivos.Domain.Tests;

public class EventTests
{
    private static readonly TimeZoneInfo BogotaTz =
        TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

    [Fact]
    public void EffectiveState_Returns_Active_For_Future_Active_Event()
    {
        var now = DateTimeOffset.UtcNow;
        var ev = Event.Create("My Event", "A description here", 1, 200, 100,
            now.AddDays(7), now.AddDays(8), 50m, EventType.Conferencia, now);

        Assert.Equal(EventEffectiveState.Active, ev.GetEffectiveState(now));
    }

    [Fact]
    public void EffectiveState_Returns_Completed_When_Past_EndsAt()
    {
        var now = DateTimeOffset.UtcNow;
        var past = now.AddDays(-30);
        var ev = Event.Create("Past Event", "A past description", 1, 200, 50,
            past.AddDays(-2), past.AddDays(-1), 10m, EventType.Taller, past.AddDays(-10));

        Assert.Equal(EventEffectiveState.Completed, ev.GetEffectiveState(now));
    }

    [Fact]
    public void Create_Throws_WeekendCutoffException_For_Saturday_After_22()
    {
        // Find a Saturday at 22:30 in Bogota timezone
        var saturday = GetNextWeekday(DayOfWeek.Saturday);
        // Build a local DateTime for Saturday 22:30 in Bogota
        var localDateTime = new DateTime(saturday.Year, saturday.Month, saturday.Day, 22, 30, 0);
        var bogotaOffset = BogotaTz.GetUtcOffset(localDateTime);
        var bogotaStartsAt22h30 = new DateTimeOffset(localDateTime, bogotaOffset);
        var now = bogotaStartsAt22h30.AddDays(-10);

        Assert.Throws<WeekendCutoffException>(() =>
            Event.Create("Weekend Late", "Test description long enough", 1, 200, 50,
                bogotaStartsAt22h30, bogotaStartsAt22h30.AddHours(3), 10m, EventType.Concierto, now));
    }

    [Fact]
    public void Create_Accepts_Saturday_Before_22()
    {
        var saturday = GetNextWeekday(DayOfWeek.Saturday);
        var localDateTime = new DateTime(saturday.Year, saturday.Month, saturday.Day, 21, 0, 0);
        var bogotaOffset = BogotaTz.GetUtcOffset(localDateTime);
        var bogotaStartsAt21h00 = new DateTimeOffset(localDateTime, bogotaOffset);
        var now = bogotaStartsAt21h00.AddDays(-10);

        var ev = Event.Create("Saturday Concert", "Valid Saturday concert", 1, 200, 50,
            bogotaStartsAt21h00, bogotaStartsAt21h00.AddHours(2), 10m, EventType.Concierto, now);

        Assert.NotNull(ev);
    }

    [Fact]
    public void Create_Throws_When_Capacity_Exceeds_Venue()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.Throws<EventCapacityExceedsVenueException>(() =>
            Event.Create("Over Capacity", "Test description long enough", 1, 50, 100,
                now.AddDays(7), now.AddDays(8), 10m, EventType.Conferencia, now));
    }

    [Fact]
    public void Create_Throws_When_StartDate_In_Past()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.Throws<InvalidEventDateException>(() =>
            Event.Create("Past Start", "Test description long enough", 1, 200, 100,
                now.AddDays(-1), now.AddDays(1), 10m, EventType.Conferencia, now));
    }

    private static DateTime GetNextWeekday(DayOfWeek day)
    {
        var date = DateTime.Today.AddDays(10);
        while (date.DayOfWeek != day) date = date.AddDays(1);
        return date;
    }
}
