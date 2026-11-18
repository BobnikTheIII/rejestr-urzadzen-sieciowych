using RejestrUrzadzen.Domain;

namespace RejestrUrzadzen.Ui;

/// <summary>Wypisuje wszystkie pola jednego urządzenia.</summary>
public class DeviceDetailsView
{
    private readonly ConsoleUi _ui;

    public DeviceDetailsView(ConsoleUi ui)
    {
        _ui = ui;
    }

    public void Print(Device device)
    {
        IReadOnlyList<DeviceField> fields = device.GetFields();
        int labelWidth = fields.Max(field => field.Label.Length);

        _ui.Header($"Urządzenie #{device.Id}: {device.TypeName}");
        foreach (DeviceField field in fields)
        {
            string value = field.DisplayValue.Length > 0 ? field.DisplayValue : "-";
            _ui.Line($"  {field.Label.PadRight(labelWidth)} : {value}");
        }
    }
}
