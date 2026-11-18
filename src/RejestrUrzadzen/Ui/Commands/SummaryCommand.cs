using RejestrUrzadzen.Services;

namespace RejestrUrzadzen.Ui.Commands;

public class SummaryCommand : MenuCommand
{
    private readonly DeviceService _service;

    public SummaryCommand(ConsoleUi ui, DeviceService service) : base(ui)
    {
        _service = service;
    }

    public override string Title => "Podsumowanie rejestru";

    public override void Execute()
    {
        RegistrySummary summary = _service.GetSummary();
        Ui.Line($"Liczba urządzeń: {summary.Total}");
        if (summary.Total == 0)
        {
            return;
        }

        PrintGroup("Według typu", summary.ByType);
        PrintGroup("Według statusu", summary.ByStatus);
    }

    private void PrintGroup(string title, IReadOnlyDictionary<string, int> counts)
    {
        Ui.Line();
        Ui.Header(title);
        int width = counts.Keys.Max(key => key.Length);
        foreach (KeyValuePair<string, int> entry in counts)
        {
            Ui.Line($"  {entry.Key.PadRight(width)} : {entry.Value}");
        }
    }
}
