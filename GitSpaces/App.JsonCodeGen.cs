using System.Text.Json.Serialization;
using GitSpaces.Models;
using GitSpaces.ViewModels;

namespace GitSpaces;

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyFields = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(Version))]
[JsonSerializable(typeof(Preference))]
partial class JsonCodeGen : JsonSerializerContext
{
}
