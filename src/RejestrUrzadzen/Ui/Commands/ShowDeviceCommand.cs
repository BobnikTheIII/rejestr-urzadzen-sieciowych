using RejestrUrzadzen.Domain;

namespace RejestrUrzadzen.Ui.Commands;

public class ShowDeviceCommand : MenuCommand
{
    private readonly DeviceSelector _selector;
    private readonly DeviceDetailsView _details;

    public ShowDeviceCommand(ConsoleUi ui, DeviceSelector selector, DeviceDetailsView details) : base(ui)
    {
        _selector = selector;
        _details = details;
    }

    public override string Title => "Szczegóły urządzenia";

    public override void Execute()
    {
        Device? device = _selector.Select();
        if (device is not null)
        {
            Ui.Line();
            _details.Print(device);
        }
    }
}
