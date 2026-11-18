using System.Text.Json.Serialization;

namespace RejestrUrzadzen.Domain;

public class AccessPoint : Device
{
    private const int MaxSsidLength = 32;

    private static readonly IReadOnlyList<ChoiceOption<WifiBand>> BandOptions = new[]
    {
        new ChoiceOption<WifiBand>(WifiBand.Ghz2_4, "2,4 GHz"),
        new ChoiceOption<WifiBand>(WifiBand.Ghz5, "5 GHz"),
        new ChoiceOption<WifiBand>(WifiBand.Ghz6, "6 GHz"),
        new ChoiceOption<WifiBand>(WifiBand.DualBand, "2,4 + 5 GHz"),
        new ChoiceOption<WifiBand>(WifiBand.TriBand, "2,4 + 5 + 6 GHz"),
    };

    private string _ssid = string.Empty;

    public AccessPoint() : base("Punkt dostępowy")
    {
    }

    [JsonRequired]
    public string Ssid
    {
        get => _ssid;
        set => _ssid = RequireText(value, "SSID", MaxSsidLength);
    }

    public WifiBand Band { get; set; } = WifiBand.DualBand;

    protected override IEnumerable<DeviceField> GetSpecificFields()
    {
        yield return new TextField("SSID", () => Ssid, value => Ssid = value, $"do {MaxSsidLength} znaków");
        yield return new ChoiceField<WifiBand>("Pasmo", () => Band, value => Band = value, BandOptions);
    }
}
