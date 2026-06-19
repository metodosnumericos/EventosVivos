using EventosVivos.Domain.Reservations;

namespace EventosVivos.Domain.Tests;

public class ReservationTests
{
    [Fact]
    public void Cancel_Confirmed_LessThan48h_Sets_LostCapacity()
    {
        var now = DateTimeOffset.UtcNow;
        var eventStart = now.AddHours(24);

        var res = Reservation.Create(1, 2, "Juan", "juan@example.com", now.AddDays(-2));
        res.Confirm("EV-000001", now.AddDays(-1));
        res.Cancel(now, eventStart);

        Assert.Equal(ReservationState.Canceled, res.State);
        Assert.True(res.LostCapacity);
    }

    [Fact]
    public void Cancel_Confirmed_MoreThan48h_ReleasesCapacity()
    {
        var now = DateTimeOffset.UtcNow;
        var eventStart = now.AddDays(3);

        var res = Reservation.Create(1, 2, "Juan", "juan@example.com", now.AddDays(-2));
        res.Confirm("EV-000002", now.AddDays(-1));
        res.Cancel(now, eventStart);

        Assert.Equal(ReservationState.Canceled, res.State);
        Assert.False(res.LostCapacity);
    }

    [Fact]
    public void Cancel_Pending_Does_Not_Set_LostCapacity()
    {
        var now = DateTimeOffset.UtcNow;
        var eventStart = now.AddHours(12); // less than 48h, but pending

        var res = Reservation.Create(1, 2, "Juan", "juan@example.com", now.AddDays(-1));
        res.Cancel(now, eventStart);

        Assert.Equal(ReservationState.Canceled, res.State);
        Assert.False(res.LostCapacity);
    }

    [Fact]
    public void Cancel_Already_Canceled_Throws()
    {
        var now = DateTimeOffset.UtcNow;
        var eventStart = now.AddDays(5);
        var res = Reservation.Create(1, 2, "Juan", "juan@example.com", now.AddDays(-1));
        res.Cancel(now, eventStart);

        Assert.Throws<InvalidStateTransitionException>(() => res.Cancel(now, eventStart));
    }

    [Fact]
    public void Confirm_Twice_Throws()
    {
        var now = DateTimeOffset.UtcNow;
        var res = Reservation.Create(1, 2, "Juan", "juan@example.com", now.AddDays(-1));
        res.Confirm("EV-000003", now);

        Assert.Throws<InvalidStateTransitionException>(() => res.Confirm("EV-000004", now));
    }

    [Fact]
    public void BuyerOwnership_Fails_WrongEmail()
    {
        var now = DateTimeOffset.UtcNow;
        var res = Reservation.Create(1, 2, "Juan", "juan@example.com", now);

        Assert.Throws<OwnershipProofFailedException>(() =>
            res.ValidateBuyerOwnership("other@example.com", null));
    }

    [Fact]
    public void BuyerOwnership_Confirmed_Fails_Without_Code()
    {
        var now = DateTimeOffset.UtcNow;
        var res = Reservation.Create(1, 2, "Juan", "juan@example.com", now);
        res.Confirm("EV-000005", now);

        Assert.Throws<OwnershipProofFailedException>(() =>
            res.ValidateBuyerOwnership("juan@example.com", null));
    }

    [Fact]
    public void BuyerOwnership_Confirmed_Passes_With_Code()
    {
        var now = DateTimeOffset.UtcNow;
        var res = Reservation.Create(1, 2, "Juan", "juan@example.com", now);
        res.Confirm("EV-000006", now);

        // Should not throw
        res.ValidateBuyerOwnership("juan@example.com", "EV-000006");
    }
}
