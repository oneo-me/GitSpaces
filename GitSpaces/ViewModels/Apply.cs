using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class Apply : Popup
{
    [Required(ErrorMessage = "Patch file is required!!!")]
    [CustomValidation(typeof(Apply), nameof(ValidatePatchFile))]
    public string PatchFile
    {
        get => _patchFile;
        set => SetProperty(ref _patchFile, value, true);
    }

    public bool IgnoreWhiteSpace
    {
        get => _ignoreWhiteSpace;
        set => SetProperty(ref _ignoreWhiteSpace, value);
    }

    public List<ApplyWhiteSpaceMode> WhiteSpaceModes { get; }

    public ApplyWhiteSpaceMode SelectedWhiteSpaceMode { get; set; }

    public Apply(Repository repo)
    {
        _repo = repo;

        WhiteSpaceModes = new()
        {
            new("Apply.NoWarn", "Apply.NoWarn.Desc", "nowarn"), new("Apply.Warn", "Apply.Warn.Desc", "warn"), new("Apply.Error", "Apply.Error.Desc", "error"), new("Apply.ErrorAll", "Apply.ErrorAll.Desc", "error-all")
        };
        SelectedWhiteSpaceMode = WhiteSpaceModes[0];

        View = new OldViews.Apply
        {
            DataContext = this
        };
    }

    public static ValidationResult ValidatePatchFile(string file, ValidationContext _)
    {
        if (File.Exists(file))
        {
            return ValidationResult.Success;
        }

        return new($"File '{file}' can NOT be found!!!");
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Apply patch...";

        return Task.Run(() =>
        {
            var succ = new Commands.Apply(_repo.FullPath, _patchFile, _ignoreWhiteSpace, SelectedWhiteSpaceMode.Arg, null).Exec();
            CallUIThread(() => _repo.SetWatcherEnabled(true));
            return succ;
        });
    }

    readonly Repository _repo;
    string _patchFile = string.Empty;
    bool _ignoreWhiteSpace = true;
}
