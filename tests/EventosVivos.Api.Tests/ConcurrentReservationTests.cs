using System.Net;
using System.Net.Http.Json;
using EventosVivos.Api.Contracts;
using EventosVivos.Domain.Events;

namespace EventosVivos.Api.Tests;

public class ConcurrentReservationTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;

    public ConcurrentReservationTests(ApiFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ConcurrentReservations_NeverOversell()
    {
        // Create an event with exactly 1 ticket available
        var admin = _fixture.CreateAdminClient();
        var eventReq = new CreateEventRequest(
            "Race Condition Test",
            "Testing concurrent reservation handling.",
            3, // Arena Sur, capacity 500
            1, // max capacity = 1
            DateTimeOffset.UtcNow.AddDays(10),
            DateTimeOffset.UtcNow.AddDays(10).AddHours(2),
            50m,
            EventType.Concierto);
        var createResp = await admin.PostAsJsonAsync("/api/events", eventReq);
        createResp.EnsureSuccessStatusCode();
        var ev = await createResp.Content.ReadFromJsonAsync<EventResponse>();

        // Fire 10 concurrent reservation attempts
        var tasks = Enumerable.Range(0, 10).Select(i =>
            _fixture.CreateClient().PostAsJsonAsync("/api/reservations",
                new CreateReservationRequest(ev!.Id, 1, $"Buyer{i}", $"buyer{i}@test.com")));

        var responses = await Task.WhenAll(tasks);

        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        // Exactly 1 should succeed
        Assert.Equal(1, successCount);
        // The rest should be conflicts (or possibly 422 for completed/started events)
        Assert.Equal(9, conflictCount + responses.Count(r =>
            r.StatusCode == HttpStatusCode.UnprocessableEntity));

        // Verify the occupancy report shows exactly 1 held ticket
        var reportResp = await _fixture.CreateClient().GetAsync($"/api/events/{ev!.Id}/occupancy");
        reportResp.EnsureSuccessStatusCode();
        var report = await reportResp.Content.ReadFromJsonAsync<OccupancyReport>();
        Assert.NotNull(report);
        // pending + confirmed + lost should not exceed maxCapacity
        Assert.True(report.PendingTicketsHeld + report.ConfirmedTicketsSold + report.LostTickets <= 1);
    }
}

// Minimal type to deserialize occupancy report
record OccupancyReport(
    int EventId, string EventTitle, string EventState,
    int ConfirmedTicketsSold, int PendingTicketsHeld, int LostTickets,
    int RemainingTickets, double OccupancyPercentage, decimal TotalIncome);
