﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GitSpaces.ViewModels;

public class Popup : ObservableValidator
{
    public string HostPageId { get; set; }

    public object View { get; set; }

    public bool InProgress
    {
        get => _inProgress;
        set => SetProperty(ref _inProgress, value);
    }

    public string ProgressDescription
    {
        get => _progressDescription;
        set => SetProperty(ref _progressDescription, value);
    }

    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2026:RequiresUnreferencedCode")]
    public bool Check()
    {
        if (HasErrors) return false;
        ValidateAllProperties();
        return !HasErrors;
    }

    public virtual Task<bool> Sure()
    {
        return null;
    }

    protected void CallUIThread(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }

    protected void SetProgressDescription(string description)
    {
        CallUIThread(() => ProgressDescription = description);
    }

    bool _inProgress;
    string _progressDescription = string.Empty;
}
