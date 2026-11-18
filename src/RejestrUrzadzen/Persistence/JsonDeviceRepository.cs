using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using RejestrUrzadzen.Domain;

namespace RejestrUrzadzen.Persistence;

/// <summary>Przechowuje rejestr w pliku JSON.</summary>
public class JsonDeviceRepository : IDeviceRepository
{
    private readonly string _path;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    private RegistryData _data;

    public JsonDeviceRepository(string path)
    {
        _path = path;
        _data = Load();
    }

    public IReadOnlyList<Device> GetAll() => _data.Devices.Select(device => device.Clone()).ToList();

    public Device? GetById(int id) => _data.Devices.FirstOrDefault(device => device.Id == id)?.Clone();

    public void Add(Device device)
    {
        Device stored = device.Clone();
        stored.AssignId(_data.NextId);

        var devices = new List<Device>(_data.Devices) { stored };
        Commit(new RegistryData { NextId = _data.NextId + 1, Devices = devices });

        device.AssignId(stored.Id);
    }

    public bool Update(Device device)
    {
        int index = _data.Devices.FindIndex(existing => existing.Id == device.Id);
        if (index < 0)
        {
            return false;
        }

        var devices = new List<Device>(_data.Devices);
        devices[index] = device.Clone();
        Commit(new RegistryData { NextId = _data.NextId, Devices = devices });
        return true;
    }

    public bool Delete(int id)
    {
        List<Device> devices = _data.Devices.Where(device => device.Id != id).ToList();
        if (devices.Count == _data.Devices.Count)
        {
            return false;
        }

        Commit(new RegistryData { NextId = _data.NextId, Devices = devices });
        return true;
    }

    private RegistryData Load()
    {
        if (!File.Exists(_path))
        {
            return new RegistryData();
        }

        RegistryData? data;
        try
        {
            data = JsonSerializer.Deserialize<RegistryData>(File.ReadAllText(_path), _jsonOptions);
        }
        catch (JsonException exception)
        {
            throw new DataFileException($"Plik danych \"{_path}\" jest uszkodzony lub ma niepoprawny format.", exception);
        }
        catch (Exception exception) when (exception is DomainValidationException or IOException or UnauthorizedAccessException)
        {
            throw new DataFileException($"Nie można odczytać pliku danych \"{_path}\": {exception.Message}", exception);
        }

        if (data is null)
        {
            throw new DataFileException($"Plik danych \"{_path}\" jest pusty lub niepoprawny.");
        }

        if (data.Devices.Select(device => device.Id).Distinct().Count() != data.Devices.Count)
        {
            throw new DataFileException($"Plik danych \"{_path}\" zawiera powtórzone identyfikatory urządzeń.");
        }

        // Chroni przed ponownym użyciem identyfikatora po ręcznej edycji pliku.
        int highestId = data.Devices.Count == 0 ? 0 : data.Devices.Max(device => device.Id);
        data.NextId = Math.Max(data.NextId, highestId + 1);
        return data;
    }

    /// <summary>Stan w pamięci zmienia się dopiero po udanym zapisie na dysku.</summary>
    private void Commit(RegistryData next)
    {
        string temporaryPath = _path + ".tmp";
        try
        {
            string? folder = Path.GetDirectoryName(Path.GetFullPath(_path));
            if (folder is not null)
            {
                Directory.CreateDirectory(folder);
            }

            // Zapis przez plik tymczasowy: awaria w trakcie zapisu nie uszkodzi istniejących danych.
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(next, _jsonOptions));
            File.Move(temporaryPath, _path, overwrite: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new DataFileException($"Nie można zapisać pliku danych \"{_path}\": {exception.Message}", exception);
        }

        _data = next;
    }
}
