using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class CatalogMetadataTests
{
    [Fact]
    public void Catalog_DocumentsEveryPublicComponentParameter()
    {
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(RepositoryLayout.Root,
            "samples", "Sufficit.Blazor.UI.Demos", "catalog.json")));
        var assembly = typeof(SUIButton).Assembly;
        foreach (var entry in json.RootElement.EnumerateArray())
        {
            var name = entry.GetProperty("name").GetString()!;
            var type = assembly.GetTypes().Single(type => type.Name.Split('`')[0] == name && typeof(ComponentBase).IsAssignableFrom(type));
            var parameters = type.GetProperties().Where(p => p.IsDefined(typeof(ParameterAttribute)))
                .Select(p => p.Name).Order().ToArray();
            var documented = entry.GetProperty("parameters").EnumerateArray()
                .Select(p => p.GetProperty("name").GetString()!).Order().ToArray();
            Assert.True(parameters.SequenceEqual(documented),
                name + " missing: " + string.Join(", ", parameters.Except(documented)));
        }
    }
}
