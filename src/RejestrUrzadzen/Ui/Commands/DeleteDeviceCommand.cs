using RejestrUrzadzen.Domain;
using RejestrUrzadzen.Services;

namespace RejestrUrzadzen.Ui.Commands;

public class DeleteDeviceCommand : MenuCommand
{
    private readonly DeviceService _service;
    private readonly DeviceSelector _selector;
    private readonly DeviceDetailsView _details;

    public DeleteDeviceCommand(ConsoleUi ui, DeviceService service, DeviceSelector selector, DeviceDetailsView details) : base(ui)
    {
        _service = service;
        _selector = selector;
        _details = details;
    }

    public override string Title => "Usuń urządzenie";

    public override void Execute()
    {
        Device? device = _selector.Select();
        if (device is null)
        {
            return;
        }

        Ui.Line();
        _details.Print(device);
        Ui.Line();

        if (!Ui.Confirm($"Czy na pewno usunąć urządzenie \"{device.Name}\"?"))
        {
            throw new InputCancelledException();
        }

        _service.Delete(device.Id);
        Ui.Line();
        Ui.Success($"Usunięto urządzenie \"{device.Name}\".");
    }
}
