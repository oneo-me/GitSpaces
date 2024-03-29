using System.Collections.Generic;

namespace GitSpaces.Models;

public class Locale
{
    public string Name { get; set; }
    public string Key { get; set; }

    public static List<Locale> Supported = new()
    {
        new("English", "en_US"), new("简体中文", "zh_CN")
    };

    public Locale(string name, string key)
    {
        Name = name;
        Key = key;
    }
}
