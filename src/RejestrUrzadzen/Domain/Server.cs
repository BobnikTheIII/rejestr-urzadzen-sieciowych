using System.Text.Json.Serialization;

namespace RejestrUrzadzen.Domain;

public class Server : Device
{
    private const int MaxSystemLength = 40;

    private int _cpuCores = 1;
    private int _ramGb = 1;
    private string _operatingSystem = string.Empty;

    public Server() : base("Serwer")
    {
    }

    [JsonRequired]
    public int CpuCores
    {
        get => _cpuCores;
        set => _cpuCores = RequireRange(value, "Rdzenie CPU", 1, 1024);
    }

    [JsonRequired]
    public int RamGb
    {
        get => _ramGb;
        set => _ramGb = RequireRange(value, "RAM [GB]", 1, 65536);
    }

    [JsonRequired]
    public string OperatingSystem
    {
        get => _operatingSystem;
        set => _operatingSystem = RequireText(value, "System operacyjny", MaxSystemLength);
    }

    protected override IEnumerable<DeviceField> GetSpecificFields()
    {
        yield return new IntegerField("Rdzenie CPU", () => CpuCores, value => CpuCores = value, "1-1024");
        yield return new IntegerField("RAM [GB]", () => RamGb, value => RamGb = value, "1-65536");
        yield return new TextField("System operacyjny", () => OperatingSystem, value => OperatingSystem = value, "np. Debian 13");
    }
}
