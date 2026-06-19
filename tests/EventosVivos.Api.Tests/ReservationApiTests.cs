using System.Net;
using System.Net.Http.Json;
using EventosVivos.Api.Contracts;
using EventosVivos.Domain.Events;

namespace EventosVivos.Api.Tests;

public class ReservationApiTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;
    private static int _eventSequence;

    public ReservationApiTests(ApiFixture fixture) => _fixture = fixture;

    private static DateTimeOffset NextStart()
    {
        var offset = Interlocked.Increment(ref _eventSequence);
        return DateTimeOffset.UtcNow.AddDays(14 + offset).AddMinutes(offset);
    }

    private async Task<EventResponse> CreateTestEvent(
        DateTimeOffset? startsAt = null, int capacity = 10, decimal price = 20m)
    {
        var admin = _fixture.CreateAdminClient();
        var start = startsAt ?? NextStart();
        var req = new CreateEventRequest(
            "Reservation Test Event",
            "A valid description for testing reservations.",
            1,
            capacity,
            start,
            start.AddHours(3),
            price,
            EventType.Conferencia);
        var resp = await admin.PostAsJsonAsync("/api/events", req);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<EventResponse>())!;
    }

    [Fact]
    public async Task CreateReservation_Returns201_WhenValid()
    {
        var ev = await CreateTestEvent();
        var client = _fixture.CreateClient();
        var req = new CreateReservationRequest(ev.Id, 2, "Juan", "juan@test.com");
        var resp = await client.PostAsJsonAsync("/api/reservations", req);

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var res = await resp.Content.ReadFromJsonAsync<ReservationResponse>();
        Assert.Equal("PendingPayment", res!.State);
    }

    [Fact]
    public async Task CreateReservation_Returns409_WhenCapacityExceeded()
    {
        var ev = await CreateTestEvent(capacity: 1);
        var client = _fixture.CreateClient();

        // Reserve the single ticket
        await client.PostAsJsonAsync("/api/reservations",
            new CreateReservationRequest(ev.Id, 1, "First", "first@test.com"));

        // Try to reserve again
        var resp = await client.PostAsJsonAsync("/api/reservations",
            new CreateReservationRequest(ev.Id, 1, "Second", "second@test.com"));

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task CreateReservation_Returns422_WhenEventStartsInLessThan1Hour()
    {
        var ev = await CreateTestEvent(startsAt: DateTimeOffset.UtcNow.AddMinutes(30));
        var client = _fixture.CreateClient();

        var resp = await client.PostAsJsonAsync("/api/reservations",
            new CreateReservationRequest(ev.Id, 1, "Juan", "juan@test.com"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task ConfirmPayment_Returns200_WithReservationCode()
    {
        var ev = await CreateTestEvent();
        var client = _fixture.CreateClient();
        var admin = _fixture.CreateAdminClient();

        var resResp = await client.PostAsJsonAsync("/api/reservations",
            new CreateReservationRequest(ev.Id, 1, "Juan", "juan@test.com"));
        var res = await resResp.Content.ReadFromJsonAsync<ReservationResponse>();

        var confirmResp = await admin.PostAsync($"/api/reservations/{res!.Id}/confirm", null);
        Assert.Equal(HttpStatusCode.OK, confirmResp.StatusCode);

        var confirmed = await confirmResp.Content.ReadFromJsonAsync<ReservationResponse>();
        Assert.Equal("Confirmed", confirmed!.State);
        Assert.NotNull(confirmed.ReservationCode);
        Assert.StartsWith("EV-", confirmed.ReservationCode);
    }

    [Fact]
    public async Task ConfirmPayment_Returns409_WhenAlreadyConfirmed()
    {
        var ev = await CreateTestEvent();
        var client = _fixture.CreateClient();
        var admin = _fixture.CreateAdminClient();

        var resResp = await client.PostAsJsonAsync("/api/reservations",
            new CreateReservationRequest(ev.Id, 1, "Juan", "juan@test.com"));
        var res = await resResp.Content.ReadFromJsonAsync<ReservationResponse>();

        await admin.PostAsync($"/api/reservations/{res!.Id}/confirm", null);

        // Confirm again
        var resp2 = await admin.PostAsync($"/api/reservations/{res.Id}/confirm", null);
        Assert.Equal(HttpStatusCode.Conflict, resp2.StatusCode);
    }

    [Fact]
    public async Task AdminCancel_Returns204_AndReleasesCapacity()
    {
        var ev = await CreateTestEvent(capacity: 1);
        var client = _fixture.CreateClient();
        var admin = _fixture.CreateAdminClient();

        var resResp = await client.PostAsJsonAsync("/api/reservations",
            new CreateReservationRequest(ev.Id, 1, "Juan", "juan@test.com"));
        var res = await resResp.Content.ReadFromJsonAsync<ReservationResponse>();

        var cancelResp = await admin.PostAsync($"/api/reservations/{res!.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.NoContent, cancelResp.StatusCode);

        // Now capacity should be free — new reservation should succeed
        var newRes = await client.PostAsJsonAsync("/api/reservations",
            new CreateReservationRequest(ev.Id, 1, "New Buyer", "new@test.com"));
        Assert.Equal(HttpStatusCode.Created, newRes.StatusCode);
    }

    [Fact]
    public async Task BuyerCancel_Pending_Returns204()
    {
        var ev = await CreateTestEvent();
        var client = _fixture.CreateClient();

        var resResp = await client.PostAsJsonAsync("/api/reservations",
            new CreateReservationRequest(ev.Id, 1, "Juan", "buyer@test.com"));
        var res = await resResp.Content.ReadFromJsonAsync<ReservationResponse>();

        var cancelResp = await client.PostAsJsonAsync(
            $"/api/reservations/{res!.Id}/buyer-cancel",
            new BuyerCancelRequest("buyer@test.com", null));

        Assert.Equal(HttpStatusCode.NoContent, cancelResp.StatusCode);
    }
}
