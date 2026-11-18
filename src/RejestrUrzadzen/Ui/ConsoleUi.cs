namespace RejestrUrzadzen.Ui;

/// <summary>Jedyna klasa komunikująca się bezpośrednio z konsolą.</summary>
public class ConsoleUi
{
    private const string CancelToken = "!";
    private const int FallbackWidth = 100;

    public int Width
    {
        get
        {
            if (Console.IsOutputRedirected)
            {
                return FallbackWidth;
            }

            int width = Console.WindowWidth;
            return width > 0 ? width : FallbackWidth;
        }
    }

    public void Clear()
    {
        if (!Console.IsOutputRedirected)
        {
            Console.Clear();
        }
    }

    public void Title(string text)
    {
        WriteColored($"=== {text} ===", ConsoleColor.Cyan);
        Line();
    }

    public void Line(string text = "") => Console.WriteLine(text);

    public void Header(string text) => WriteColored(text, ConsoleColor.Cyan);

    public void Muted(string text) => WriteColored(text, ConsoleColor.DarkGray);

    public void Success(string text) => WriteColored(text, ConsoleColor.Green);

    public void Error(string text) => WriteColored(text, ConsoleColor.Red);

    /// <summary>Wczytuje wiersz, w którym znak anulowania nie ma specjalnego znaczenia.</summary>
    public string ReadRaw(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? throw new EndOfInputException();
    }

    /// <summary>Wczytuje wiersz; "!" przerywa bieżącą operację.</summary>
    public string Read(string prompt)
    {
        string input = ReadRaw(prompt);
        if (input.Trim() == CancelToken)
        {
            throw new InputCancelledException();
        }

        return input;
    }

    public bool Confirm(string question)
    {
        while (true)
        {
            switch (Read($"{question} (t/n): ").Trim().ToLowerInvariant())
            {
                case "t":
                case "tak":
                    return true;
                case "n":
                case "nie":
                    return false;
                default:
                    Error("Odpowiedz t (tak) lub n (nie).");
                    break;
            }
        }
    }

    public void WaitForEnter()
    {
        Line();
        ReadRaw("Naciśnij Enter, aby wrócić do menu...");
    }

    private void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}
