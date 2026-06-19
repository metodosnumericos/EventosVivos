namespace EventosVivos.Domain.Events;

public enum EventState
{
    Active,
    Canceled
}

public enum EventEffectiveState
{
    Active,
    Canceled,
    Completed
}
