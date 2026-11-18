using System.Text;
using RejestrUrzadzen.Domain;
using RejestrUrzadzen.Persistence;
using RejestrUrzadzen.Services;
using RejestrUrzadzen.Ui;
using RejestrUrzadzen.Ui.Commands;

namespace RejestrUrzadzen;

/// <summary>Punkt wejścia: składa obiekty aplikacji i uruchamia pętlę menu.</summary>
public class Program
{
    public static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        if (OperatingSystem.IsWindows())
        {
            // Konsola Windows poprawnie wczytuje polskie znaki tylko w trybie UTF-16.
            Console.InputEncoding = Encoding.Unicode;
        }

        return new Program().Run(args);
    }

    private int Run(string[] args)
    {
        var ui = new ConsoleUi();
        string dataFile = args.Length > 0 ? args[0] : DefaultDataFile();

        JsonDeviceRepository repository;
        try
        {
            repository = new JsonDeviceRepository(dataFile);
        }
        catch (DataFileException exception)
        {
            // Plik nie jest nadpisywany - użytkownik może go naprawić i spróbować ponownie.
            ui.Error(exception.Message);
            return 1;
        }

        var service = new DeviceService(repository);
        var factory = new DeviceFactory();
        var table = new DeviceTable(ui);
        var details = new DeviceDetailsView(ui);
        var form = new DeviceForm(ui, service);
        var selector = new DeviceSelector(ui, service, table);

        var commands = new List<MenuCommand>
        {
            new AddDeviceCommand(ui, service, factory, form),
            new ListDevicesCommand(ui, service, table),
            new ShowDeviceCommand(ui, selector, details),
            new SearchDevicesCommand(ui, service, table),
            new EditDeviceCommand(ui, service, selector, form),
            new DeleteDeviceCommand(ui, service, selector, details),
            new SummaryCommand(ui, service),
        };

        new ConsoleApp(ui, commands, dataFile).Run();
        return 0;
    }

    private string DefaultDataFile()
    {
        string folder = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData,
            Environment.SpecialFolderOption.Create);
        return Path.Combine(folder, "RejestrUrzadzen", "urzadzenia.json");
    }
}
