using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class Archive : Popup
{
    [Required(ErrorMessage = "Output file name is required")]
    public string SaveFile
    {
        get => _saveFile;
        set => SetProperty(ref _saveFile, value, true);
    }

    public object BasedOn { get; private set; }

    public Archive(Repository repo, Branch branch)
    {
        _repo = repo;
        _revision = branch.Head;
        _saveFile = $"archive-{Path.GetFileNameWithoutExtension(branch.Name)}.zip";
        BasedOn = branch;
        View = new OldViews.Archive
        {
            DataContext = this
        };
    }

    public Archive(Repository repo, Commit commit)
    {
        _repo = repo;
        _revision = commit.SHA;
        _saveFile = $"archive-{commit.SHA.Substring(0, 10)}.zip";
        BasedOn = commit;
        View = new OldViews.Archive
        {
            DataContext = this
        };
    }

    public Archive(Repository repo, Tag tag)
    {
        _repo = repo;
        _revision = tag.SHA;
        _saveFile = $"archive-{tag.Name}.zip";
        BasedOn = tag;
        View = new OldViews.Archive
        {
            DataContext = this
        };
    }

    public override Task<bool> Sure()
    {
        _repo.SetWatcherEnabled(false);
        ProgressDescription = "Archiving ...";

        return Task.Run(() =>
        {
            var succ = new Commands.Archive(_repo.FullPath, _revision, _saveFile, SetProgressDescription).Exec();
            CallUIThread(() =>
            {
                _repo.SetWatcherEnabled(true);
                if (succ) App.SendNotification(_repo.FullPath, $"Save archive to : {_saveFile}");
            });

            return succ;
        });
    }

    readonly Repository _repo;
    string _saveFile = string.Empty;
    readonly string _revision = string.Empty;
}
