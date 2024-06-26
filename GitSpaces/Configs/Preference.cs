using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Collections;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using GitSpaces.Commands;
using GitSpaces.Models;
using GitSpaces.Services;
using GitSpaces.ViewModels;
using OpenUI.Desktop;
using OpenUI.Services;

namespace GitSpaces.Configs;

public class Preference : ObservableObject
{
    [JsonIgnore]
    public static Preference Instance
    {
        get
        {
            if (_instance == null)
            {
                if (!File.Exists(_savePath))
                    _instance = new();
                else
                    try
                    {
                        _instance = JsonSerializer.Deserialize(File.ReadAllText(_savePath), JsonCodeGen.Default.Preference);
                    }
                    catch
                    {
                        _instance = new();
                    }
            }

            _instance.Repositories.RemoveAll(x => !Directory.Exists(x.FullPath));

            if (_instance.DefaultFont == null)
                _instance.DefaultFont = FontManager.Current.DefaultFontFamily;

            if (_instance.MonospaceFont == null)
                _instance.MonospaceFont = new("fonts:GitSpaces#JetBrains Mono");

            if (!_instance.IsGitConfigured)
            {
            }

            return _instance;
        }
    }

    public string Locale
    {
        get => _locale;
        set
        {
            if (SetProperty(ref _locale, value))
                App123.SetLocale(value);
        }
    }

    public string Theme
    {
        get => _theme;
        set
        {
            if (SetProperty(ref _theme, value))
                App123.SetTheme(value);
        }
    }

    [JsonConverter(typeof(FontFamilyConverter))]
    public FontFamily DefaultFont
    {
        get => _defaultFont;
        set => SetProperty(ref _defaultFont, value);
    }

    [JsonConverter(typeof(FontFamilyConverter))]
    public FontFamily MonospaceFont
    {
        get => _monospaceFont;
        set => SetProperty(ref _monospaceFont, value);
    }

    public double DefaultFontSize
    {
        get => _defaultFontSize;
        set => SetProperty(ref _defaultFontSize, value);
    }

    public string AvatarServer
    {
        get => AvatarManager.SelectedServer;
        set
        {
            if (AvatarManager.SelectedServer != value)
            {
                AvatarManager.SelectedServer = value;
                OnPropertyChanged();
            }
        }
    }

    public int MaxHistoryCommits
    {
        get => _maxHistoryCommits;
        set => SetProperty(ref _maxHistoryCommits, value);
    }

    public bool RestoreTabs
    {
        get => _restoreTabs;
        set => SetProperty(ref _restoreTabs, value);
    }

    public bool UseFixedTabWidth
    {
        get => _useFixedTabWidth;
        set => SetProperty(ref _useFixedTabWidth, value);
    }

    public bool Check4UpdatesOnStartup
    {
        get => _check4UpdatesOnStartup;
        set => SetProperty(ref _check4UpdatesOnStartup, value);
    }

    public string IgnoreUpdateTag { get; set; } = string.Empty;

    public bool UseTwoColumnsLayoutInHistories
    {
        get => _useTwoColumnsLayoutInHistories;
        set => SetProperty(ref _useTwoColumnsLayoutInHistories, value);
    }

    public bool UseSideBySideDiff
    {
        get => _useSideBySideDiff;
        set => SetProperty(ref _useSideBySideDiff, value);
    }

    public bool UseSyntaxHighlighting
    {
        get => _useSyntaxHighlighting;
        set => SetProperty(ref _useSyntaxHighlighting, value);
    }

    public ChangeViewMode UnstagedChangeViewMode
    {
        get => _unstagedChangeViewMode;
        set => SetProperty(ref _unstagedChangeViewMode, value);
    }

    public ChangeViewMode StagedChangeViewMode
    {
        get => _stagedChangeViewMode;
        set => SetProperty(ref _stagedChangeViewMode, value);
    }

    public ChangeViewMode CommitChangeViewMode
    {
        get => _commitChangeViewMode;
        set => SetProperty(ref _commitChangeViewMode, value);
    }

    [JsonIgnore]
    public bool IsGitConfigured => !string.IsNullOrEmpty(GitInstallPath) && File.Exists(GitInstallPath);

    public string GitInstallPath
    {
        get => string.Empty;
        set
        {
            var OS = Service.Get<ISystemService>();
            if (OS.GitInstallPath != value)
            {
                OS.GitInstallPath = value;
                OnPropertyChanged();
            }
        }
    }

    public string GitDefaultCloneDir
    {
        get => _gitDefaultCloneDir;
        set => SetProperty(ref _gitDefaultCloneDir, value);
    }

    public bool GitAutoFetch
    {
        get => AutoFetch.IsEnabled;
        set
        {
            if (AutoFetch.IsEnabled != value)
            {
                AutoFetch.IsEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public int ExternalMergeToolType
    {
        get => _externalMergeToolType;
        set
        {
            var changed = SetProperty(ref _externalMergeToolType, value);
            if (changed && !OperatingSystem.IsWindows() && value > 0 && value < ExternalMergeTools.Supported.Count)
            {
                var tool = ExternalMergeTools.Supported[value];
                if (File.Exists(tool.Exec)) ExternalMergeToolPath = tool.Exec;
                else ExternalMergeToolPath = string.Empty;
            }
        }
    }

    public string ExternalMergeToolPath
    {
        get => _externalMergeToolPath;
        set => SetProperty(ref _externalMergeToolPath, value);
    }

    public string ExternalMergeToolCmd
    {
        get => _externalMergeToolCmd;
        set => SetProperty(ref _externalMergeToolCmd, value);
    }

    public string ExternalMergeToolDiffCmd
    {
        get => _externalMergeToolDiffCmd;
        set => SetProperty(ref _externalMergeToolDiffCmd, value);
    }

    public List<Repository> Repositories { get; set; } = new();

    public AvaloniaList<RepositoryNode> RepositoryNodes
    {
        get => _repositoryNodes;
        set => SetProperty(ref _repositoryNodes, value);
    }

    public List<string> OpenedTabs { get; set; } = new();

    public int LastActiveTabIdx { get; set; } = 0;

    public static void AddNode(RepositoryNode node, RepositoryNode to = null)
    {
        var collection = to == null ? _instance._repositoryNodes : to.SubNodes;
        var list = new List<RepositoryNode>();
        list.AddRange(collection);
        list.Add(node);
        list.Sort((l, r) =>
        {
            if (l.IsRepository != r.IsRepository)
                return l.IsRepository ? 1 : -1;

            return l.Name.CompareTo(r.Name);
        });

        collection.Clear();
        foreach (var one in list)
            collection.Add(one);
    }

    public static RepositoryNode FindNode(string id)
    {
        return FindNodeRecursive(id, _instance.RepositoryNodes);
    }

    public static void MoveNode(RepositoryNode node, RepositoryNode to = null)
    {
        if (to == null && _instance._repositoryNodes.Contains(node)) return;
        if (to != null && to.SubNodes.Contains(node)) return;

        RemoveNode(node);
        AddNode(node, to);
    }

    public static void RemoveNode(RepositoryNode node)
    {
        RemoveNodeRecursive(node, _instance._repositoryNodes);
    }

    public static Repository FindRepository(string path)
    {
        foreach (var repo in _instance.Repositories)
            if (repo.FullPath == path)
                return repo;

        return null;
    }

    public static Repository AddRepository(string rootDir, string gitDir)
    {
        var normalized = rootDir.Replace('\\', '/');
        var repo = FindRepository(normalized);
        if (repo != null)
        {
            repo.GitDir = gitDir;
            return repo;
        }

        repo = new()
        {
            FullPath = normalized, GitDir = gitDir
        };

        _instance.Repositories.Add(repo);
        return repo;
    }

    public static void Save()
    {
        var dir = Path.GetDirectoryName(_savePath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var data = JsonSerializer.Serialize(_instance, JsonCodeGen.Default.Preference);
        File.WriteAllText(_savePath, data);
    }

    static RepositoryNode FindNodeRecursive(string id, AvaloniaList<RepositoryNode> collection)
    {
        foreach (var node in collection)
        {
            if (node.Id == id) return node;

            var sub = FindNodeRecursive(id, node.SubNodes);
            if (sub != null) return sub;
        }

        return null;
    }

    static bool RemoveNodeRecursive(RepositoryNode node, AvaloniaList<RepositoryNode> collection)
    {
        if (collection.Contains(node))
        {
            collection.Remove(node);
            return true;
        }

        foreach (var one in collection)
            if (RemoveNodeRecursive(node, one.SubNodes))
                return true;

        return false;
    }

    static Preference _instance;

    static readonly string _savePath = Path.Combine(
        DesktopApp.Folder,
        "OldConfigs",
        "preference.json");

    string _locale = "en_US";
    string _theme = "Default";
    FontFamily _defaultFont;
    FontFamily _monospaceFont;
    double _defaultFontSize = 13;

    int _maxHistoryCommits = 20000;
    bool _restoreTabs;
    bool _useFixedTabWidth = true;
    bool _check4UpdatesOnStartup = true;
    bool _useTwoColumnsLayoutInHistories;
    bool _useSideBySideDiff;
    bool _useSyntaxHighlighting;

    ChangeViewMode _unstagedChangeViewMode = ChangeViewMode.List;
    ChangeViewMode _stagedChangeViewMode = ChangeViewMode.List;
    ChangeViewMode _commitChangeViewMode = ChangeViewMode.List;

    string _gitDefaultCloneDir = string.Empty;

    int _externalMergeToolType;
    string _externalMergeToolPath = string.Empty;
    string _externalMergeToolCmd = string.Empty;
    string _externalMergeToolDiffCmd = string.Empty;

    AvaloniaList<RepositoryNode> _repositoryNodes = new();
}

public class FontFamilyConverter : JsonConverter<FontFamily>
{
    public override FontFamily Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var name = reader.GetString();
        return new(name);
    }

    public override void Write(Utf8JsonWriter writer, FontFamily value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
