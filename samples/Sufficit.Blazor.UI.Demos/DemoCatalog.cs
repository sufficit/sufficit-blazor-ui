using System.Text.Json;

namespace Sufficit.Blazor.UI.Demos;

public static partial class DemoCatalog
{
    public static IReadOnlyList<DemoEntry> Entries { get; } = Load();

    private static IReadOnlyList<DemoEntry> Load()
    {
        using var stream = typeof(DemoCatalog).Assembly.GetManifestResourceStream("Sufficit.Blazor.UI.Demos.catalog.json")!;
        return JsonSerializer.Deserialize(stream, CatalogJsonContext.Default.ListDemoEntry)!;
    }
}

public sealed record DemoEntry(string Name, string Family, string Source, List<DemoParameter> Parameters);
public sealed record DemoParameter(string Name, string Type, string Default, bool Required, bool Deprecated);

[System.Text.Json.Serialization.JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[System.Text.Json.Serialization.JsonSerializable(typeof(List<DemoEntry>))]
internal partial class CatalogJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
