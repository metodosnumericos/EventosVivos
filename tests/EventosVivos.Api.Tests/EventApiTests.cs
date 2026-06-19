using System.Net;
using System.Net.Http.Json;
using EventosVivos.Api.Contracts;
using EventosVivos.Domain.Events;

namespace EventosVivos.Api.Tests;

public class EventApiTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;
    private static int _eventSequence;

    public EventApiTests(ApiFixture fixture) => _fixture = fixture;

    private static DateTimeOffset NextStart()
    {
        var offset = Interlocked.Increment(ref _eventSequence);
        return DateTimeOffset.UtcNow.AddDays(14 + offset).AddMinutes(offset);
    }

    private static CreateEventRequest ValidEvent(
        DateTimeOffset? startsAt = null,
        int venueId = 1,
        int capacity = 50,
        decimal price = 20m)
    {
        var start = startsAt ?? NextStart();
        return new("Test Conference",
            "A valid description for the test event.",
            venueId,
            capacity,
            start,
            start.AddHours(3),
            price,
            EventType.Conferencia);
    }

    [Fact]
    public async Task CreateEvent_Returns201_WhenValid()
    {
        var admin = _fixture.CreateAdminClient();
        var resp = await admin.PostAsJsonAsync("/api/events", ValidEvent());

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var ev = await resp.Content.ReadFromJsonAsync<EventResponse>();
        Assert.NotNull(ev);
        Assert.Equal("Active", ev.EffectiveState);
    }

    [Fact]
    public async Task CreateEvent_Returns401_Without_AdminKey()
    {
        var client = _fixture.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/events", ValidEvent());

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_Returns409_WhenVenueOverlap()
    {
        var admin = _fixture.CreateAdminClient();
        var startsAt = DateTimeOffset.UtcNow.AddDays(14);
        await admin.PostAsJsonAsync("/api/events", ValidEvent(startsAt, venueId: 2));

        // Second event overlaps
        var resp = await admin.PostAsJsonAsync("/api/events", ValidEvent(startsAt.AddHours(1), venueId: 2));

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task ListEvents_Returns200_WithFilters()
    {
        var client = _fixture.CreateClient();
        var resp = await client.GetAsync("/api/events?type=Conferencia");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task GetOccupancyReport_Returns200_ForExistingEvent()
    {
        var admin = _fixture.CreateAdminClient();
        var createResp = await admin.PostAsJsonAsync("/api/events", ValidEvent());
        createResp.EnsureSuccessStatusCode();
        var ev = await createResp.Content.ReadFromJsonAsync<EventResponse>();

        var client = _fixture.CreateClient();
        var resp = await client.GetAsync($"/api/events/{ev!.Id}/occupancy");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }
}
