namespace RejestrUrzadzen.Services;

/// <summary>Liczba urządzeń ogółem oraz w podziale na typy i statusy.</summary>
public class RegistrySummary
{
    public RegistrySummary(int total, IReadOnlyDictionary<string, int> byType, IReadOnlyDictionary<string, int> byStatus)
    {
        Total = total;
        ByType = byType;
        ByStatus = byStatus;
    }

    public int Total { get; }

    public IReadOnlyDictionary<string, int> ByType { get; }

    public IReadOnlyDictionary<string, int> ByStatus { get; }
}
