using RejestrUrzadzen.Domain;
using RejestrUrzadzen.Services;

namespace RejestrUrzadzen.Ui.Commands;

public class EditDeviceCommand : MenuCommand
{
    private readonly DeviceService _service;
    private readonly DeviceSelector _selector;
    private readonly DeviceForm _form;

    public EditDeviceCommand(ConsoleUi ui, DeviceService service, DeviceSelector selector, DeviceForm form) : base(ui)
    {
        _service = service;
        _selector = selector;
        _form = form;
    }

    public override string Title => "Edytuj urządzenie";

    public override void Execute()
    {
        Device? device = _selector.Select();
        if (device is null)
        {
            return;
        }

        Ui.Line();
        Ui.Muted("Enter = bez zmian, ! = anuluj, - = wyczyść uwagi.");
        _form.Edit(device);

        _service.Update(device);
        Ui.Line();
        Ui.Success($"Zapisano zmiany w urządzeniu \"{device.Name}\" (ID {device.Id}).");
    }
}
