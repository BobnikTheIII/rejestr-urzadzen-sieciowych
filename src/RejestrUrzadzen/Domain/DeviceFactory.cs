namespace RejestrUrzadzen.Domain;

/// <summary>Tworzy puste urządzenia typów dostępnych w rejestrze.</summary>
public class DeviceFactory
{
    private readonly IReadOnlyList<Func<Device>> _creators = new Func<Device>[]
    {
        () => new Router(),
        () => new NetworkSwitch(),
        () => new AccessPoint(),
        () => new Server(),
    };

    public IReadOnlyList<string> TypeNames => _creators.Select(create => create().TypeName).ToList();

    public Device Create(int typeIndex) => _creators[typeIndex]();
}
