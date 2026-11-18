namespace RejestrUrzadzen.Domain;

public class IntegerField : DeviceField
{
    private readonly Func<int> _get;
    private readonly Action<int> _set;

    public IntegerField(string label, Func<int> get, Action<int> set, string hint = "")
        : base(label, hint, isOptional: false)
    {
        _get = get;
        _set = set;
    }

    public override string DisplayValue => _get().ToString();

    public override void SetFromText(string text)
    {
        if (!int.TryParse(text.Trim(), out int value))
        {
            throw new DomainValidationException($"Pole \"{Label}\" musi być liczbą całkowitą.");
        }

        _set(value);
    }
}
