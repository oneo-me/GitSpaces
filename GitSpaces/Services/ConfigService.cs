using System.Text.Json;
using System.Text.Json.Serialization;
using OpenUI.Desktop;

namespace GitSpaces.Services;

public class ConfigService
{
    readonly string _configFile = Path.Combine(DesktopApp.Folder, "config.json");
    readonly Dictionary<string, object> _config;

    public ConfigService()
    {
        if (File.Exists(_configFile) is false)
        {
            _config = new();
            return;
        }

        var configJson = File.ReadAllText(_configFile);
        _config = Deserialize<Dictionary<string, object>>(configJson);
    }

    public T Get<T>(string? configName = null) where T : class, new()
    {
        configName ??= typeof(T).Name;
        configName = JsonNamingPolicy.SnakeCaseLower.ConvertName(configName);

        if (_config.TryGetValue(configName, out var value) && value is T result)
            return result;

        if (value is null)
        {
            result = new();
            _config[configName] = result;
            return result;
        }

        var json = Serialize(value);
        result = Deserialize<T>(json);
        _config[configName] = result;

        return result;
    }

    public void Save()
    {
        var configJson = Serialize(_config);
        File.WriteAllText(_configFile, configJson);
    }

    static JsonSerializerOptions JsonSerializerOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
        }
    };

    string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonSerializerOptions);
    }

    T Deserialize<T>(string json) where T : class, new()
    {
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions) ?? new();
    }
}
