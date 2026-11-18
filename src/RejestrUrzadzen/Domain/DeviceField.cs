namespace RejestrUrzadzen.Domain;

/// <summary>
/// Opis jednego pola urządzenia. Dzięki tej abstrakcji formularze, widok szczegółów
/// i wyszukiwarka działają tak samo dla każdego typu urządzenia.
/// </summary>
public abstract class DeviceField
{
    protected DeviceField(string label, string hint, bool isOptional)
    {
        Label = label;
        Hint = hint;
        IsOptional = isOptional;
    }

    public string Label { get; }

    public string Hint { get; }

    public bool IsOptional { get; }

    public abstract string DisplayValue { get; }

    /// <summary>Ustawia pole na podstawie tekstu użytkownika.</summary>
    /// <exception cref="DomainValidationException">Gdy tekst jest niepoprawny.</exception>
    public abstract void SetFromText(string text);
}
