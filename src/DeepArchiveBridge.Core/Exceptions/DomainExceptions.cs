namespace DeepArchiveBridge.Core.Exceptions;

/// <summary>
/// Exceção base para o domínio de aplicação
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Exceção para recursos não encontrados
/// </summary>
public class NotFoundException : DomainException
{
    public string ResourceName { get; }
    public object ResourceId { get; }

    public NotFoundException(string resourceName, object resourceId) 
        : base($"{resourceName} com ID '{resourceId}' não foi encontrado")
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Exceção para violações de validação
/// </summary>
public class ValidationException : DomainException
{
    public IEnumerable<string> Errors { get; }

    public ValidationException(string message, IEnumerable<string>? errors = null) 
        : base(message)
    {
        Errors = errors ?? new List<string>();
    }
}

/// <summary>
/// Exceção para conflitos de negócio
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>
/// Exceção para operações não permitidas
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message) : base(message) { }
}
