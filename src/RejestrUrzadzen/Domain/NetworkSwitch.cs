using System.Text.Json.Serialization;

namespace RejestrUrzadzen.Domain;

public class NetworkSwitch : Device
{
    private int _portCount = 1;

    public NetworkSwitch() : base("Przełącznik")
    {
    }

    [JsonRequired]
    public int PortCount
    {
        get => _portCount;
        set => _portCount = RequireRange(value, "Liczba portów", 1, 256);
    }

    public bool IsManaged { get; set; }

    protected override IEnumerable<DeviceField> GetSpecificFields()
    {
        yield return new IntegerField("Liczba portów", () => PortCount, value => PortCount = value, "1-256");
        yield return new YesNoField("Zarządzalny", () => IsManaged, value => IsManaged = value);
    }
}
