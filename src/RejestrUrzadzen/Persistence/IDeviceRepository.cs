using RejestrUrzadzen.Domain;

namespace RejestrUrzadzen.Persistence;

/// <summary>Trwały magazyn urządzeń. Każda zmiana jest zapisana, zanim metoda się zakończy.</summary>
public interface IDeviceRepository
{
    IReadOnlyList<Device> GetAll();

    Device? GetById(int id);

    /// <summary>Zapisuje nowe urządzenie i nadaje mu identyfikator.</summary>
    void Add(Device device);

    /// <returns><c>false</c>, gdy urządzenie o tym identyfikatorze nie istnieje.</returns>
    bool Update(Device device);

    /// <returns><c>false</c>, gdy urządzenie o tym identyfikatorze nie istnieje.</returns>
    bool Delete(int id);
}
