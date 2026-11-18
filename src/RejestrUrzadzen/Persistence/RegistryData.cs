using RejestrUrzadzen.Domain;

namespace RejestrUrzadzen.Persistence;

/// <summary>Zawartość pliku danych.</summary>
public class RegistryData
{
    /// <summary>Licznik jest zapisywany, aby identyfikator usuniętego urządzenia nie wrócił do użycia.</summary>
    public int NextId { get; set; } = 1;

    public List<Device> Devices { get; set; } = new();
}
