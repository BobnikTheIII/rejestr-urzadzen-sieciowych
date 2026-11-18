namespace RejestrUrzadzen.Persistence;

/// <summary>Zgłaszany, gdy pliku danych nie da się odczytać lub zapisać.</summary>
public class DataFileException : Exception
{
    public DataFileException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DataFileException(string message) : base(message)
    {
    }
}
