namespace RejestrUrzadzen.Domain;

/// <summary>Wartość wyliczeniowa wraz z nazwą pokazywaną użytkownikowi.</summary>
public class ChoiceOption<TValue>
{
    public ChoiceOption(TValue value, string name)
    {
        Value = value;
        Name = name;
    }

    public TValue Value { get; }

    public string Name { get; }
}
