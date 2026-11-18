using RejestrUrzadzen.Domain;
using RejestrUrzadzen.Services;

namespace RejestrUrzadzen.Ui;

/// <summary>Formularz oparty na polach urządzenia - wspólny dla wszystkich typów.</summary>
public class DeviceForm
{
    private const string ClearToken = "-";

    private readonly ConsoleUi _ui;
    private readonly DeviceService _service;

    public DeviceForm(ConsoleUi ui, DeviceService service)
    {
        _ui = ui;
        _service = service;
    }

    /// <summary>Wypełnia wszystkie pola nowego urządzenia.</summary>
    public void Fill(Device device)
    {
        foreach (DeviceField field in device.GetFields())
        {
            while (true)
            {
                string input = _ui.Read(FormatPrompt(field, current: null));
                if (input.Trim().Length == 0 && field.IsOptional)
                {
                    break;
                }

                if (TryApply(device, field, input))
                {
                    break;
                }
            }
        }
    }

    /// <summary>Zmienia pola istniejącego urządzenia; pusty wiersz zachowuje obecną wartość.</summary>
    public void Edit(Device device)
    {
        foreach (DeviceField field in device.GetFields())
        {
            while (true)
            {
                string previous = field.DisplayValue;
                string input = _ui.Read(FormatPrompt(field, previous));
                if (input.Trim().Length == 0)
                {
                    break;
                }

                if (field.IsOptional && input.Trim() == ClearToken)
                {
                    field.SetFromText(string.Empty);
                    break;
                }

                if (TryApply(device, field, input))
                {
                    break;
                }

                // Wartość odrzucona przez regułę unikalności nie może zostać w urządzeniu.
                field.SetFromText(previous);
            }
        }
    }

    private bool TryApply(Device device, DeviceField field, string input)
    {
        try
        {
            field.SetFromText(input);
        }
        catch (DomainValidationException exception)
        {
            _ui.Error(exception.Message);
            return false;
        }

        string? conflict = _service.FindConflict(device);
        if (conflict is not null)
        {
            _ui.Error(conflict);
            return false;
        }

        return true;
    }

    private string FormatPrompt(DeviceField field, string? current)
    {
        var details = new List<string>();
        if (field.Hint.Length > 0)
        {
            details.Add(field.Hint);
        }

        if (current is not null && current.Length > 0)
        {
            details.Add($"obecnie: {current}");
        }

        string suffix = details.Count > 0 ? $" [{string.Join("; ", details)}]" : string.Empty;
        return $"{field.Label}{suffix}: ";
    }
}
