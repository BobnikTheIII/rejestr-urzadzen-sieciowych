using RejestrUrzadzen.Domain;
using RejestrUrzadzen.Services;

namespace RejestrUrzadzen.Ui.Commands;

public class SearchDevicesCommand : MenuCommand
{
    private readonly DeviceService _service;
    private readonly DeviceTable _table;

    public SearchDevicesCommand(ConsoleUi ui, DeviceService service, DeviceTable table) : base(ui)
    {
        _service = service;
        _table = table;
    }

    public override string Title => "Wyszukaj urządzenia";

    public override void Execute()
    {
        string text = ReadSearchText();
        IReadOnlyList<Device> devices = _service.Search(text);

        Ui.Line();
        if (devices.Count == 0)
        {
            Ui.Muted($"Brak urządzeń pasujących do \"{text}\".");
            return;
        }

        _table.Print(devices);
        Ui.Line();
        Ui.Line($"Znaleziono: {devices.Count}");
    }

    private string ReadSearchText()
    {
        while (true)
        {
            string text = Ui.Read("Szukany tekst (! = anuluj): ").Trim();
            if (text.Length > 0)
            {
                return text;
            }

            Ui.Error("Wpisz co najmniej jeden znak.");
        }
    }
}
