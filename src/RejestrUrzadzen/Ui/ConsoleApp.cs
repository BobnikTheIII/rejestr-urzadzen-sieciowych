using RejestrUrzadzen.Domain;
using RejestrUrzadzen.Persistence;
using RejestrUrzadzen.Services;
using RejestrUrzadzen.Ui.Commands;

namespace RejestrUrzadzen.Ui;

/// <summary>Główna pętla programu: menu, wykonanie polecenia, obsługa błędów.</summary>
public class ConsoleApp
{
    private readonly ConsoleUi _ui;
    private readonly IReadOnlyList<MenuCommand> _commands;
    private readonly string _dataFile;

    public ConsoleApp(ConsoleUi ui, IReadOnlyList<MenuCommand> commands, string dataFile)
    {
        _ui = ui;
        _commands = commands;
        _dataFile = dataFile;
    }

    public void Run()
    {
        try
        {
            while (true)
            {
                PrintMenu();
                string choice = _ui.ReadRaw("Wybierz opcję: ").Trim();
                if (choice == "0")
                {
                    _ui.Line("Do widzenia!");
                    return;
                }

                if (int.TryParse(choice, out int number) && number >= 1 && number <= _commands.Count)
                {
                    Execute(_commands[number - 1]);
                }
                else
                {
                    _ui.Error($"Nieznana opcja: \"{choice}\".");
                    _ui.WaitForEnter();
                }
            }
        }
        catch (EndOfInputException)
        {
            _ui.Line();
        }
    }

    private void PrintMenu()
    {
        _ui.Clear();
        _ui.Title("Rejestr urządzeń sieciowych");
        _ui.Muted($"Plik danych: {_dataFile}");
        _ui.Line();
        for (int index = 0; index < _commands.Count; index++)
        {
            _ui.Line($"  {index + 1}. {_commands[index].Title}");
        }

        _ui.Line("  0. Wyjście");
        _ui.Line();
    }

    private void Execute(MenuCommand command)
    {
        _ui.Clear();
        _ui.Title(command.Title);
        try
        {
            command.Execute();
        }
        catch (InputCancelledException)
        {
            _ui.Line();
            _ui.Muted("Anulowano - nie wprowadzono żadnych zmian.");
        }
        catch (Exception exception) when (exception is DataFileException or DomainValidationException or DeviceNotFoundException)
        {
            _ui.Error(exception.Message);
        }

        _ui.WaitForEnter();
    }
}
