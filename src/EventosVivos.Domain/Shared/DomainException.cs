namespace EventosVivos.Domain.Shared;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

// Maps to HTTP 409 Conflict — conflict with current resource state
public abstract class ConflictException : DomainException
{
    protected ConflictException(string message) : base(message) { }
}

// Maps to HTTP 422 Unprocessable Entity — semantic business rule rejection
public abstract class BusinessRuleException : DomainException
{
    protected BusinessRuleException(string message) : base(message) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }
}
