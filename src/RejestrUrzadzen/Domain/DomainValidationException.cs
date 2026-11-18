namespace RejestrUrzadzen.Domain;

/// <summary>Zgłaszany, gdy wartość nie spełnia reguł modelu; treść jest pokazywana użytkownikowi.</summary>
public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message)
    {
    }
}
