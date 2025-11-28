namespace Shared.Domain.Exceptions;

/// <summary>
/// Base domain exception for all domain-specific errors
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found")
    {
        EntityName = entityName;
        Key = key;
    }

    public string EntityName { get; }
    public object Key { get; }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    public BusinessRuleValidationException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when there's a conflict (e.g., duplicate entity)
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Exception thrown for unauthorized access
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(message)
    {
    }
}

/// <summary>
/// Exception thrown for forbidden operations
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "Access forbidden")
        : base(message)
    {
    }
}
