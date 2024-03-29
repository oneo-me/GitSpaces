﻿using System.Text.RegularExpressions;

namespace GitSpaces.Models;

public partial class Remote
{
    [GeneratedRegex(@"^http[s]?://([\w\-]+@)?[\w\.\-]+(\:[0-9]+)?/[\w\-]+/[\w\-\.]+\.git$")]
    private static partial Regex regex1();

    [GeneratedRegex(@"^[\w\-]+@[\w\.\-]+(\:[0-9]+)?:[\w\-]+/[\w\-\.]+\.git$")]
    private static partial Regex regex2();

    [GeneratedRegex(@"^ssh://([\w\-]+@)?[\w\.\-]+(\:[0-9]+)?/[\w\-]+/[\w\-\.]+\.git$")]
    private static partial Regex regex3();

    static readonly Regex[] URL_FORMATS =
    [
        regex1(),
        regex2(),
        regex3()
    ];

    public string Name { get; set; }
    public string URL { get; set; }

    public static bool IsSSH(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        for (var i = 1; i < URL_FORMATS.Length; i++)
        {
            if (URL_FORMATS[i].IsMatch(url)) return true;
        }

        return false;
    }

    public static bool IsValidURL(string url)
    {
        foreach (var fmt in URL_FORMATS)
        {
            if (fmt.IsMatch(url)) return true;
        }

        return false;
    }
}
