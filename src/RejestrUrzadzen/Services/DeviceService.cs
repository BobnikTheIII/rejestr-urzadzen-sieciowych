using RejestrUrzadzen.Domain;
using RejestrUrzadzen.Persistence;

namespace RejestrUrzadzen.Services;

/// <summary>Operacje CRUD na rejestrze oraz reguły obejmujące wiele urządzeń.</summary>
public class DeviceService
{
    private readonly IDeviceRepository _repository;

    public DeviceService(IDeviceRepository repository)
    {
        _repository = repository;
    }

    public void Add(Device device)
    {
        RequireNoConflict(device);
        _repository.Add(device);
    }

    public IReadOnlyList<Device> GetAll() => _repository.GetAll();

    public Device? Find(int id) => _repository.GetById(id);

    public IReadOnlyList<Device> Search(string text)
    {
        return _repository.GetAll().Where(device => device.Matches(text.Trim())).ToList();
    }

    public void Update(Device device)
    {
        RequireNoConflict(device);
        if (!_repository.Update(device))
        {
            throw new DeviceNotFoundException(device.Id);
        }
    }

    public void Delete(int id)
    {
        if (!_repository.Delete(id))
        {
            throw new DeviceNotFoundException(id);
        }
    }

    /// <summary>Nazwa i adres IP muszą być w rejestrze unikalne.</summary>
    /// <returns>Opis konfliktu z innym urządzeniem albo <c>null</c>, gdy go nie ma.</returns>
    public string? FindConflict(Device device)
    {
        foreach (Device other in _repository.GetAll().Where(other => other.Id != device.Id))
        {
            if (device.Name.Length > 0 && other.Name.Equals(device.Name, StringComparison.OrdinalIgnoreCase))
            {
                return $"Nazwa \"{device.Name}\" jest już zajęta przez urządzenie o ID {other.Id}.";
            }

            if (device.IpAddress.Length > 0 && other.IpAddress == device.IpAddress)
            {
                return $"Adres {device.IpAddress} jest już przypisany do urządzenia \"{other.Name}\" (ID {other.Id}).";
            }
        }

        return null;
    }

    public RegistrySummary GetSummary()
    {
        IReadOnlyList<Device> devices = _repository.GetAll();
        return new RegistrySummary(
            devices.Count,
            CountBy(devices, device => device.TypeName),
            CountBy(devices, device => device.StatusName));
    }

    private void RequireNoConflict(Device device)
    {
        string? conflict = FindConflict(device);
        if (conflict is not null)
        {
            throw new DomainValidationException(conflict);
        }
    }

    private IReadOnlyDictionary<string, int> CountBy(IEnumerable<Device> devices, Func<Device, string> key)
    {
        return devices
            .GroupBy(key)
            .OrderBy(group => group.Key)
            .ToDictionary(group => group.Key, group => group.Count());
    }
}
