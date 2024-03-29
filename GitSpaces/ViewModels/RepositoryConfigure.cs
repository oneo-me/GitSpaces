﻿using System.Collections.Generic;
using System.Threading.Tasks;
using GitSpaces.Commands;

namespace GitSpaces.ViewModels;

public class RepositoryConfigure : Popup
{
    public string UserName { get; set; }

    public string UserEmail { get; set; }

    public bool GPGSigningEnabled { get; set; }

    public string GPGUserSigningKey { get; set; }

    public string HttpProxy { get; set; }

    public RepositoryConfigure(Repository repo)
    {
        _repo = repo;

        _cached = new Config(repo.FullPath).ListAll();
        if (_cached.ContainsKey("user.name")) UserName = _cached["user.name"];
        if (_cached.ContainsKey("user.email")) UserEmail = _cached["user.email"];
        if (_cached.ContainsKey("commit.gpgsign")) GPGSigningEnabled = _cached["commit.gpgsign"] == "true";
        if (_cached.ContainsKey("user.signingkey")) GPGUserSigningKey = _cached["user.signingkey"];
        if (_cached.ContainsKey("http.proxy")) HttpProxy = _cached["http.proxy"];

        View = new OldViews.RepositoryConfigure
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        SetIfChanged("user.name", UserName);
        SetIfChanged("user.email", UserEmail);
        SetIfChanged("commit.gpgsign", GPGSigningEnabled ? "true" : "false");
        SetIfChanged("user.signingkey", GPGUserSigningKey);
        SetIfChanged("http.proxy", HttpProxy);
        return null;
    }

    void SetIfChanged(string key, string value)
    {
        var changed = false;
        if (_cached.ContainsKey(key))
        {
            changed = value != _cached[key];
        }
        else if (!string.IsNullOrEmpty(value))
        {
            changed = true;
        }

        if (changed)
        {
            new Config(_repo.FullPath).Set(key, value);
        }
    }

    readonly Repository _repo;
    readonly Dictionary<string, string> _cached;
}
