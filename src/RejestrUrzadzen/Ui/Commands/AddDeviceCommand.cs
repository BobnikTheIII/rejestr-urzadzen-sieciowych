using RejestrUrzadzen.Domain;
using RejestrUrzadzen.Services;

namespace RejestrUrzadzen.Ui.Commands;

public class AddDeviceCommand : MenuCommand
{
    private readonly DeviceService _service;
    private readonly DeviceFactory _factory;
    private readonly DeviceForm _form;

    public AddDeviceCommand(ConsoleUi ui, DeviceService service, DeviceFactory factory, DeviceForm form) : base(ui)
    {
        _service = service;
        _factory = factory;
        _form = form;
    }

    public override string Title => "Dodaj urządzenie";

    public override void Execute()
    {
        Device device = _factory.Create(ReadType());
        Ui.Line();
        Ui.Muted($"Nowe urządzenie: {device.TypeName}. Wpisz ! aby anulować.");
        _form.Fill(device);

        _service.Add(device);
        Ui.Line();
        Ui.Success($"Dodano urządzenie \"{device.Name}\" (ID {device.Id}).");
    }

    private int ReadType()
    {
        IReadOnlyList<string> types = _factory.TypeNames;
        for (int index = 0; index < types.Count; index++)
        {
            Ui.Line($"  {index + 1}. {types[index]}");
        }

        Ui.Line();
        while (true)
        {
            string input = Ui.Read("Typ urządzenia (! = anuluj): ");
            if (int.TryParse(input.Trim(), out int number) && number >= 1 && number <= types.Count)
            {
                return number - 1;
            }

            Ui.Error($"Podaj numer od 1 do {types.Count}.");
        }
    }
}
