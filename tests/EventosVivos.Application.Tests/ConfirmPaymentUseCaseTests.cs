using EventosVivos.Application.Common;
using EventosVivos.Application.Reservations;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Shared;
using NSubstitute;

namespace EventosVivos.Application.Tests;

public class ConfirmPaymentUseCaseTests
{
    private readonly IReservationRepository _reservations = Substitute.For<IReservationRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ITimeProvider _time = Substitute.For<ITimeProvider>();

    private ConfirmPaymentUseCase CreateUseCase() => new(_reservations, _uow, _time);

    [Fact]
    public async Task Execute_Confirms_And_Assigns_Code()
    {
        var now = DateTimeOffset.UtcNow;
        _time.UtcNow.Returns(now);
        var res = Reservation.Create(1, 2, "Juan", "juan@example.com", now.AddHours(-1));
        _reservations.GetByIdAsync(1, default).Returns(res);
        _reservations.CodeExistsAsync(Arg.Any<string>(), default).Returns(false);
        _uow.SaveChangesAsync(default).Returns(1);

        var result = await CreateUseCase().ExecuteAsync(1);

        Assert.Equal(ReservationState.Confirmed, result.State);
        Assert.NotNull(result.ReservationCode);
        Assert.StartsWith("EV-", result.ReservationCode);
        Assert.Equal(9, result.ReservationCode.Length); // EV-######
    }

    [Fact]
    public async Task Execute_Throws_WhenAlreadyConfirmed()
    {
        var now = DateTimeOffset.UtcNow;
        _time.UtcNow.Returns(now);
        var res = Reservation.Create(1, 2, "Juan", "juan@example.com", now.AddHours(-2));
        res.Confirm("EV-000001", now.AddHours(-1));
        _reservations.GetByIdAsync(1, default).Returns(res);

        await Assert.ThrowsAsync<InvalidStateTransitionException>(() =>
            CreateUseCase().ExecuteAsync(1));
    }

    [Fact]
    public async Task Execute_Throws_WhenNotFound()
    {
        var now = DateTimeOffset.UtcNow;
        _time.UtcNow.Returns(now);
        _reservations.GetByIdAsync(99, default).Returns((Reservation?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateUseCase().ExecuteAsync(99));
    }
}
