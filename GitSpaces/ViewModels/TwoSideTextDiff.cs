using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using GitSpaces.Models;

namespace GitSpaces.ViewModels;

public class TwoSideTextDiff : ObservableObject
{
    public string File { get; set; } = string.Empty;
    public List<TextDiffLine> Old { get; set; } = new();
    public List<TextDiffLine> New { get; set; } = new();
    public int MaxLineNumber;

    public TwoSideTextDiff(TextDiff diff)
    {
        File = diff.File;
        MaxLineNumber = diff.MaxLineNumber;

        foreach (var line in diff.Lines)
        {
            switch (line.Type)
            {
                case TextDiffLineType.Added:
                    New.Add(line);
                    break;

                case TextDiffLineType.Deleted:
                    Old.Add(line);
                    break;

                default:
                    FillEmptyLines();
                    Old.Add(line);
                    New.Add(line);
                    break;
            }
        }

        FillEmptyLines();
    }

    void FillEmptyLines()
    {
        if (Old.Count < New.Count)
        {
            var diff = New.Count - Old.Count;
            for (var i = 0; i < diff; i++) Old.Add(new());
        }
        else if (Old.Count > New.Count)
        {
            var diff = Old.Count - New.Count;
            for (var i = 0; i < diff; i++) New.Add(new());
        }
    }
}
