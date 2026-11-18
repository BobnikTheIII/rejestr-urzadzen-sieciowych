using RejestrUrzadzen.Domain;
using RejestrUrzadzen.Services;

namespace RejestrUrzadzen.Ui;

/// <summary>Pokazuje listę urządzeń i prosi o wskazanie jednego z nich.</summary>
public class DeviceSelector
{
    private readonly ConsoleUi _ui;
    private readonly DeviceService _service;
    private readonly DeviceTable _table;

    public DeviceSelector(ConsoleUi ui, DeviceService service, DeviceTable table)
    {
        _ui = ui;
        _service = service;
        _table = table;
    }

    /// <returns>Wybrane urządzenie albo <c>null</c>, gdy rejestr jest pusty.</returns>
    public Device? Select()
    {
        IReadOnlyList<Device> devices = _service.GetAll();
        if (devices.Count == 0)
        {
            _ui.Muted("Rejestr jest pusty.");
            return null;
        }

        _table.Print(devices);
        _ui.Line();

        while (true)
        {
            string input = _ui.Read("Podaj ID urządzenia (! = anuluj): ");
            if (!int.TryParse(input.Trim(), out int id))
            {
                _ui.Error("ID musi być liczbą całkowitą.");
                continue;
            }

            Device? device = _service.Find(id);
            if (device is not null)
            {
                return device;
            }

            _ui.Error($"Urządzenie o ID {id} nie istnieje.");
        }
    }
}
