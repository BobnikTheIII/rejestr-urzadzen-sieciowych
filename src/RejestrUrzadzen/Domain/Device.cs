using System.Net;
using System.Net.Sockets;
using System.Text.Json.Serialization;

namespace RejestrUrzadzen.Domain;

/// <summary>Wspólna część każdego urządzenia w rejestrze.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Router), "router")]
[JsonDerivedType(typeof(NetworkSwitch), "switch")]
[JsonDerivedType(typeof(AccessPoint), "accessPoint")]
[JsonDerivedType(typeof(Server), "server")]
public abstract class Device
{
    private const int MaxNameLength = 40;
    private const int MaxNotesLength = 200;

    private static readonly IReadOnlyList<ChoiceOption<DeviceStatus>> StatusOptions = new[]
    {
        new ChoiceOption<DeviceStatus>(DeviceStatus.Active, "Aktywne"),
        new ChoiceOption<DeviceStatus>(DeviceStatus.InStorage, "W magazynie"),
        new ChoiceOption<DeviceStatus>(DeviceStatus.InService, "W serwisie"),
        new ChoiceOption<DeviceStatus>(DeviceStatus.Retired, "Wycofane"),
    };

    private string _name = string.Empty;
    private string _ipAddress = string.Empty;
    private string _location = string.Empty;
    private string _notes = string.Empty;

    protected Device(string typeName)
    {
        TypeName = typeName;
    }

    [JsonIgnore]
    public string TypeName { get; }

    [JsonInclude]
    [JsonRequired]
    public int Id { get; private set; }

    [JsonRequired]
    public string Name
    {
        get => _name;
        set => _name = RequireText(value, "Nazwa", MaxNameLength);
    }

    [JsonRequired]
    public string IpAddress
    {
        get => _ipAddress;
        set => _ipAddress = NormalizeIpAddress(value);
    }

    [JsonRequired]
    public string Location
    {
        get => _location;
        set => _location = RequireText(value, "Lokalizacja", MaxNameLength);
    }

    public DeviceStatus Status { get; set; } = DeviceStatus.Active;

    public string Notes
    {
        get => _notes;
        set
        {
            string text = (value ?? string.Empty).Trim();
            if (text.Length > MaxNotesLength)
            {
                throw new DomainValidationException($"Uwagi mogą mieć najwyżej {MaxNotesLength} znaków.");
            }

            _notes = text;
        }
    }

    [JsonIgnore]
    public string StatusName => StatusOptions.First(option => option.Value == Status).Name;

    /// <summary>Wszystkie pola urządzenia: wspólne i właściwe dla danego typu.</summary>
    public IReadOnlyList<DeviceField> GetFields()
    {
        var fields = new List<DeviceField>
        {
            new TextField("Nazwa", () => Name, value => Name = value, $"do {MaxNameLength} znaków"),
            new TextField("Adres IP", () => IpAddress, value => IpAddress = value, "np. 192.168.1.1"),
            new TextField("Lokalizacja", () => Location, value => Location = value, "np. Serwerownia A"),
            new ChoiceField<DeviceStatus>("Status", () => Status, value => Status = value, StatusOptions),
        };
        fields.AddRange(GetSpecificFields());
        fields.Add(new TextField("Uwagi", () => Notes, value => Notes = value, "opcjonalne", isOptional: true));
        return fields;
    }

    /// <summary>Czy typ urządzenia lub wartość któregokolwiek pola zawiera szukany tekst.</summary>
    public bool Matches(string text)
    {
        return TypeName.Contains(text, StringComparison.OrdinalIgnoreCase)
            || GetFields().Any(field => field.DisplayValue.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    public Device Clone() => (Device)MemberwiseClone();

    internal void AssignId(int id) => Id = id;

    /// <summary>Pola właściwe tylko dla danego typu urządzenia.</summary>
    protected abstract IEnumerable<DeviceField> GetSpecificFields();

    protected static string RequireText(string? value, string label, int maxLength)
    {
        string text = (value ?? string.Empty).Trim();
        if (text.Length == 0)
        {
            throw new DomainValidationException($"Pole \"{label}\" nie może być puste.");
        }

        if (text.Length > maxLength)
        {
            throw new DomainValidationException($"Pole \"{label}\" może mieć najwyżej {maxLength} znaków.");
        }

        return text;
    }

    protected static int RequireRange(int value, string label, int min, int max)
    {
        if (value < min || value > max)
        {
            throw new DomainValidationException($"Pole \"{label}\" musi mieścić się w zakresie {min}-{max}.");
        }

        return value;
    }

    private static string NormalizeIpAddress(string? value)
    {
        string text = (value ?? string.Empty).Trim();

        // IPAddress.TryParse akceptuje też skróty typu "10.1", dlatego IPv4 wymaga czterech oktetów.
        bool isValid = IPAddress.TryParse(text, out IPAddress? address)
            && (address.AddressFamily != AddressFamily.InterNetwork || text.Count(c => c == '.') == 3);
        if (!isValid)
        {
            throw new DomainValidationException($"\"{text}\" nie jest poprawnym adresem IPv4 ani IPv6.");
        }

        return address!.ToString();
    }
}
