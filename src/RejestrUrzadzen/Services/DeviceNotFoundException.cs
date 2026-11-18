namespace RejestrUrzadzen.Services;

public class DeviceNotFoundException : Exception
{
    public DeviceNotFoundException(int id) : base($"Urządzenie o ID {id} nie istnieje.")
    {
    }
}
