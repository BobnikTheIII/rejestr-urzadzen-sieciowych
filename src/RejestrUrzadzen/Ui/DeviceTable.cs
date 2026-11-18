using RejestrUrzadzen.Domain;

namespace RejestrUrzadzen.Ui;

/// <summary>Wypisuje urządzenia jako tabelę dopasowaną do szerokości konsoli.</summary>
public class DeviceTable
{
    private const string ColumnSeparator = "  ";
    private const int MinimumColumnWidth = 4;

    private static readonly string[] Headers = { "ID", "Typ", "Nazwa", "Adres IP", "Lokalizacja", "Status" };

    private readonly ConsoleUi _ui;

    public DeviceTable(ConsoleUi ui)
    {
        _ui = ui;
    }

    public void Print(IReadOnlyList<Device> devices)
    {
        List<string[]> rows = devices
            .Select(device => new[]
            {
                device.Id.ToString(),
                device.TypeName,
                device.Name,
                device.IpAddress,
                device.Location,
                device.StatusName,
            })
            .ToList();

        int[] widths = FitToConsole(MeasureColumns(rows));

        string header = FormatRow(Headers, widths);
        _ui.Header(header);
        _ui.Line(new string('-', header.Length));
        foreach (string[] row in rows)
        {
            _ui.Line(FormatRow(row, widths));
        }
    }

    private int[] MeasureColumns(IReadOnlyList<string[]> rows)
    {
        var widths = new int[Headers.Length];
        for (int column = 0; column < Headers.Length; column++)
        {
            widths[column] = Math.Max(Headers[column].Length, rows.Max(row => row[column].Length));
        }

        return widths;
    }

    /// <summary>Gdy tabela jest za szeroka, skraca po jednym znaku zawsze najszerszą kolumnę.</summary>
    private int[] FitToConsole(int[] widths)
    {
        int available = _ui.Width - 1 - (ColumnSeparator.Length * (widths.Length - 1));
        while (widths.Sum() > available && widths.Max() > MinimumColumnWidth)
        {
            widths[Array.IndexOf(widths, widths.Max())]--;
        }

        return widths;
    }

    private string FormatRow(IReadOnlyList<string> cells, IReadOnlyList<int> widths)
    {
        IEnumerable<string> formatted = cells.Select((cell, column) => FitCell(cell, widths[column]).PadRight(widths[column]));
        return string.Join(ColumnSeparator, formatted).TrimEnd();
    }

    private string FitCell(string text, int width)
    {
        return text.Length <= width ? text : text[..(width - 1)] + "…";
    }
}
