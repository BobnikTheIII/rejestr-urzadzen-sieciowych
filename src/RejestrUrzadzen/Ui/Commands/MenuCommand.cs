namespace RejestrUrzadzen.Ui.Commands;

/// <summary>Pozycja menu głównego.</summary>
public abstract class MenuCommand
{
    protected MenuCommand(ConsoleUi ui)
    {
        Ui = ui;
    }

    public abstract string Title { get; }

    protected ConsoleUi Ui { get; }

    public abstract void Execute();
}
