﻿using System.ComponentModel.DataAnnotations;
using GitSpaces.Models;
using Tag = GitSpaces.Commands.Tag;

namespace GitSpaces.ViewModels;

public class CreateTag : Popup
{
    [Required(ErrorMessage = "Tag name is required!")]
    [RegularExpression(@"^[\w\-\.]+$", ErrorMessage = "Bad tag name format!")]
    [CustomValidation(typeof(CreateTag), nameof(ValidateTagName))]
    public string TagName
    {
        get => _tagName;
        set => SetProperty(ref _tagName, value, true);
    }

    public string Message { get; set; }

    public object BasedOn { get; private set; }

    public CreateTag(Repository repo, Branch branch)
    {
        _repo = repo;
        _basedOn = branch.Head;

        BasedOn = branch;
        View = new OldViews.CreateTag
        {
            DataContext = this
        };
    }

    public CreateTag(Repository repo, Commit commit)
    {
        _repo = repo;
        _basedOn = commit.SHA;

        BasedOn = commit;
        View = new OldViews.CreateTag
        {
            DataContext = this
        };
    }

    public static ValidationResult ValidateTagName(string name, ValidationContext ctx)
    {
        var creator = ctx.ObjectInstance as CreateTag;
        if (creator != null)
        {
            var found = creator._repo.Tags.Find(x => x.Name == name);
            if (found != null) return new("A tag with same name already exists!");
        }

        return ValidationResult.Success;
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Create tag...";

        return Task.Run(() =>
        {
            Tag.Add(_repo.FullPath, TagName, _basedOn, Message);
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return true;
        });
    }

    readonly Repository _repo;
    string _tagName = string.Empty;
    readonly string _basedOn = string.Empty;
}
