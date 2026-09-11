using System.Text.RegularExpressions;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// The README component table is documentation a consumer reads before the
/// code, so it must not drift from <c>src/Components</c>. Each family row must
/// list exactly the components found in the matching directory, and the
/// advertised component count must match the sources.
/// </summary>
public sealed class ReadmeCatalogTests
{
    private static readonly Dictionary<string, string> FamilyLabels = new(StringComparer.Ordinal)
    {
        ["Actions"] = "Ações",
        ["Forms"] = "Formulários",
        ["Layout"] = "Layout",
        ["Navigation"] = "Navegação",
        ["DataDisplay"] = "Exibição de dados",
        ["Feedback"] = "Feedback",
        ["Overlays"] = "Overlays",
    };

    private static string Readme => File.ReadAllText(Path.Combine(RepositoryLayout.Root, "README.md"));

    public static TheoryData<string> Families()
    {
        var data = new TheoryData<string>();
        foreach (var family in FamilyLabels.Keys)
            data.Add(family);
        return data;
    }

    [Theory]
    [MemberData(nameof(Families))]
    public void FamilyRow_ListsExactlyTheComponentsInItsDirectory(string family)
    {
        var expected = Directory.GetFiles(Path.Combine(RepositoryLayout.Src, "Components", family), "*.razor")
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        var row = Regex.Match(Readme, $@"^\| {Regex.Escape(FamilyLabels[family])} \| (.+) \|$", RegexOptions.Multiline);
        Assert.True(row.Success, $"README has no table row for family '{FamilyLabels[family]}'.");

        var listed = Regex.Matches(row.Groups[1].Value, "`([^`]+)`")
            .Select(match => match.Groups[1].Value)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected, listed);
    }

    [Fact]
    public void EveryComponentDirectory_HasAFamilyRow()
    {
        var directories = Directory.GetDirectories(Path.Combine(RepositoryLayout.Src, "Components"))
            .Select(Path.GetFileName)
            .Where(name => name is not ("bin" or "obj"))
            .OrderBy(name => name, StringComparer.Ordinal);

        Assert.Equal(FamilyLabels.Keys.OrderBy(name => name, StringComparer.Ordinal), directories);
    }

    [Fact]
    public void AdvertisedComponentCount_MatchesTheSources()
    {
        var components = RepositoryLayout.Files(Path.Combine(RepositoryLayout.Src, "Components"), "*.razor").Count()
            + RepositoryLayout.Files(Path.Combine(RepositoryLayout.Src, "Themes"), "*.razor").Count();

        var advertised = Regex.Match(Readme, @"\*\*(\d+) componentes\*\*");
        Assert.True(advertised.Success, "README must advertise the component count in bold (\"**N componentes**\").");
        Assert.Equal(components, int.Parse(advertised.Groups[1].Value));
    }
}
