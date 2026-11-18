using RejestrUrzadzen.Domain;
using RejestrUrzadzen.Services;

namespace RejestrUrzadzen.Ui.Commands;

public class ListDevicesCommand : MenuCommand
{
    private readonly DeviceService _service;
    private readonly DeviceTable _table;

    public ListDevicesCommand(ConsoleUi ui, DeviceService service, DeviceTable table) : base(ui)
    {
        _service = service;
        _table = table;
    }

    public override string Title => "Lista urządzeń";

    public override void Execute()
    {
        IReadOnlyList<Device> devices = _service.GetAll();
        if (devices.Count == 0)
        {
            Ui.Muted("Rejestr jest pusty.");
            return;
        }

        _table.Print(devices);
        Ui.Line();
        Ui.Line($"Liczba urządzeń: {devices.Count}");
    }
}
