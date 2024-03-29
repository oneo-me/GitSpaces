using System.Text.Json;
using GitSpaces.Configs;
using GitSpaces.Native;

namespace GitSpaces.Services;

public class ConfigService
{
    readonly string _configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GitSpaces", "preference.json");

    public Preference Instance { get; }

    public ConfigService()
    {
        if (File.Exists(_configFile) is false)
            Instance = new();
        else
            try
            {
                Instance = JsonSerializer.Deserialize(File.ReadAllText(_configFile), JsonCodeGen.Default.Preference) ?? new Preference();
            }
            catch (Exception)
            {
                Instance = new();
            }

        if (Instance.IsGitConfigured)
            Instance.GitInstallPath = OS.FindGitExecutable();
    }
}
