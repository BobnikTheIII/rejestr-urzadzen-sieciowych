namespace RejestrUrzadzen.Domain;

public class TextField : DeviceField
{
    private readonly Func<string> _get;
    private readonly Action<string> _set;

    public TextField(string label, Func<string> get, Action<string> set, string hint = "", bool isOptional = false)
        : base(label, hint, isOptional)
    {
        _get = get;
        _set = set;
    }

    public override string DisplayValue => _get();

    public override void SetFromText(string text) => _set(text);
}
