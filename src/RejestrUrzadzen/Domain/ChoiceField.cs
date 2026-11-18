namespace RejestrUrzadzen.Domain;

/// <summary>Pole z zamkniętą listą wartości, wybieranych numerem lub nazwą.</summary>
public class ChoiceField<TValue> : DeviceField where TValue : notnull
{
    private readonly Func<TValue> _get;
    private readonly Action<TValue> _set;
    private readonly IReadOnlyList<ChoiceOption<TValue>> _options;

    public ChoiceField(string label, Func<TValue> get, Action<TValue> set, IReadOnlyList<ChoiceOption<TValue>> options)
        : base(label, string.Join(", ", options.Select((option, index) => $"{index + 1} = {option.Name}")), isOptional: false)
    {
        _get = get;
        _set = set;
        _options = options;
    }

    public override string DisplayValue => _options.First(option => option.Value.Equals(_get())).Name;

    public override void SetFromText(string text)
    {
        string input = text.Trim();
        ChoiceOption<TValue>? selected = int.TryParse(input, out int number) && number >= 1 && number <= _options.Count
            ? _options[number - 1]
            : _options.FirstOrDefault(option => option.Name.Equals(input, StringComparison.OrdinalIgnoreCase));

        if (selected is null)
        {
            throw new DomainValidationException($"Pole \"{Label}\": podaj numer z listy albo pełną nazwę.");
        }

        _set(selected.Value);
    }
}
