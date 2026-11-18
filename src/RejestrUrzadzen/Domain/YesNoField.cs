namespace RejestrUrzadzen.Domain;

public class YesNoField : DeviceField
{
    private readonly Func<bool> _get;
    private readonly Action<bool> _set;

    public YesNoField(string label, Func<bool> get, Action<bool> set)
        : base(label, "t = tak, n = nie", isOptional: false)
    {
        _get = get;
        _set = set;
    }

    public override string DisplayValue => _get() ? "tak" : "nie";

    public override void SetFromText(string text)
    {
        switch (text.Trim().ToLowerInvariant())
        {
            case "t":
            case "tak":
                _set(true);
                break;
            case "n":
            case "nie":
                _set(false);
                break;
            default:
                throw new DomainValidationException($"Pole \"{Label}\" przyjmuje wartość t (tak) lub n (nie).");
        }
    }
}
