namespace EventosVivos.Application.Common;

public class OptimisticConcurrencyException : Exception
{
    public OptimisticConcurrencyException() : base("Concurrency conflict detected. Please retry.") { }
}
