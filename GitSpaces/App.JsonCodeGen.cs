using System.Text.Json.Serialization;
using GitSpaces.Configs;
using GitSpaces.ViewModels;
using Version = GitSpaces.Models.Version;

namespace GitSpaces;

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyFields = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(Version))]
[JsonSerializable(typeof(Preference))]
partial class JsonCodeGen : JsonSerializerContext
{
}
