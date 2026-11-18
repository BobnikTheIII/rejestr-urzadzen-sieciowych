using System.Text.Json.Serialization;

namespace RejestrUrzadzen.Domain;

public class Router : Device
{
    private int _wanPorts = 1;

    public Router() : base("Router")
    {
    }

    [JsonRequired]
    public int WanPorts
    {
        get => _wanPorts;
        set => _wanPorts = RequireRange(value, "Porty WAN", 1, 64);
    }

    public bool SupportsVpn { get; set; }

    protected override IEnumerable<DeviceField> GetSpecificFields()
    {
        yield return new IntegerField("Porty WAN", () => WanPorts, value => WanPorts = value, "1-64");
        yield return new YesNoField("Obsługa VPN", () => SupportsVpn, value => SupportsVpn = value);
    }
}
