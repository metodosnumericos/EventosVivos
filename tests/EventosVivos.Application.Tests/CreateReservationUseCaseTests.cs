using EventosVivos.Application.Common;
using EventosVivos.Application.Reservations;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Shared;
using NSubstitute;

namespace EventosVivos.Application.Tests;

public class CreateReservationUseCaseTests
{
    private readonly IEventRepository _events = Substitute.For<IEventRepository>();
    private readonly IReservationRepository _reservations = Substitute.For<IReservationRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ITimeProvider _time = Substitute.For<ITimeProvider>();

    private CreateReservationUseCase CreateUseCase() =>
        new(_events, _reservations, _uow, _time);

    private Event CreateFutureEvent(
        DateTimeOffset? startsAt = null,
        int maxCapacity = 100,
        decimal price = 50m)
    {
        var now = DateTimeOffset.UtcNow;
        startsAt ??= now.AddDays(7);
        return Event.Create("Test Event", "A long enough description", 1, 200, maxCapacity,
            startsAt.Value, startsAt.Value.AddHours(3), price, EventType.Conferencia, now.AddDays(-1));
    }

    [Fact]
    public async Task Execute_Creates_Reservation_When_Valid()
    {
        var now = DateTimeOffset.UtcNow;
        _time.UtcNow.Returns(now);
        var ev = CreateFutureEvent(startsAt: now.AddDays(7));
        _events.GetByIdAsync(1, default).Returns(ev);
        _reservations.GetHeldCapacityAsync(1, default).Returns(0);
        _uow.SaveChangesAsync(default).Returns(1);

        var cmd = new CreateReservationCommand(1, 5, "Juan", "juan@example.com");
        var result = await CreateUseCase().ExecuteAsync(cmd);

        Assert.Equal(ReservationState.PendingPayment, result.State);
        Assert.Equal(5, result.Quantity);
    }

    [Fact]
    public async Task Execute_Throws_WhenNoCapacity()
    {
        var now = DateTimeOffset.UtcNow;
        _time.UtcNow.Returns(now);
        var ev = CreateFutureEvent(startsAt: now.AddDays(7), maxCapacity: 3);
        _events.GetByIdAsync(1, default).Returns(ev);
        _reservations.GetHeldCapacityAsync(1, default).Returns(3); // fully booked

        var cmd = new CreateReservationCommand(1, 1, "Juan", "juan@example.com");
        await Assert.ThrowsAsync<CapacityExceededException>(() =>
            CreateUseCase().ExecuteAsync(cmd));
    }

    [Fact]
    public async Task Execute_Throws_WhenEventStartsInLessThan1Hour()
    {
        var now = DateTimeOffset.UtcNow;
        _time.UtcNow.Returns(now);
        var ev = CreateFutureEvent(startsAt: now.AddMinutes(30));
        _events.GetByIdAsync(1, default).Returns(ev);

        var cmd = new CreateReservationCommand(1, 1, "Juan", "juan@example.com");
        await Assert.ThrowsAsync<ReservationWindowClosedException>(() =>
            CreateUseCase().ExecuteAsync(cmd));
    }

    [Fact]
    public async Task Execute_Throws_WhenOver5Tickets_LessThan24h()
    {
        var now = DateTimeOffset.UtcNow;
        _time.UtcNow.Returns(now);
        var ev = CreateFutureEvent(startsAt: now.AddHours(12), price: 50m); // < 24h, price <= 100
        _events.GetByIdAsync(1, default).Returns(ev);
        _reservations.GetHeldCapacityAsync(1, default).Returns(0);

        var cmd = new CreateReservationCommand(1, 6, "Juan", "juan@example.com");
        await Assert.ThrowsAsync<TicketQuantityLimitException>(() =>
            CreateUseCase().ExecuteAsync(cmd));
    }

    [Fact]
    public async Task Execute_AppliesStrictestLimit_24hAnd_PriceOver100()
    {
        var now = DateTimeOffset.UtcNow;
        _time.UtcNow.Returns(now);
        // 12 hours away (triggers 5-ticket limit) AND price > 100 (triggers 10-ticket limit)
        // Strictest = 5
        var ev = CreateFutureEvent(startsAt: now.AddHours(12), price: 150m);
        _events.GetByIdAsync(1, default).Returns(ev);
        _reservations.GetHeldCapacityAsync(1, default).Returns(0);

        // 6 tickets should fail (strictest limit is 5)
        var cmd = new CreateReservationCommand(1, 6, "Juan", "juan@example.com");
        await Assert.ThrowsAsync<TicketQuantityLimitException>(() =>
            CreateUseCase().ExecuteAsync(cmd));
    }

    [Fact]
    public async Task Execute_Throws_WhenOver10Tickets_PriceOver100()
    {
        var now = DateTimeOffset.UtcNow;
        _time.UtcNow.Returns(now);
        var ev = CreateFutureEvent(startsAt: now.AddDays(5), price: 150m); // far future, price > 100
        _events.GetByIdAsync(1, default).Returns(ev);
        _reservations.GetHeldCapacityAsync(1, default).Returns(0);

        var cmd = new CreateReservationCommand(1, 11, "Juan", "juan@example.com");
        await Assert.ThrowsAsync<TicketQuantityLimitException>(() =>
            CreateUseCase().ExecuteAsync(cmd));
    }

    [Fact]
    public async Task Execute_Succeeds_WhenReservingExactlyLastSeat()
    {
        var now = DateTimeOffset.UtcNow;
        _time.UtcNow.Returns(now);
        var ev = CreateFutureEvent(startsAt: now.AddDays(7), maxCapacity: 5);
        _events.GetByIdAsync(1, default).Returns(ev);
        _reservations.GetHeldCapacityAsync(1, default).Returns(4); // 4 of 5 held, 1 remaining
        _uow.SaveChangesAsync(default).Returns(1);

        var cmd = new CreateReservationCommand(1, 1, "Juan", "juan@example.com");
        var result = await CreateUseCase().ExecuteAsync(cmd);

        Assert.Equal(ReservationState.PendingPayment, result.State);
        Assert.Equal(1, result.Quantity);
    }

    [Fact]
    public async Task Execute_Throws_WhenEventNotFound()
    {
        var now = DateTimeOffset.UtcNow;
        _time.UtcNow.Returns(now);
        _events.GetByIdAsync(99, default).Returns((Event?)null);

        var cmd = new CreateReservationCommand(99, 1, "Juan", "juan@example.com");
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateUseCase().ExecuteAsync(cmd));
    }
}
