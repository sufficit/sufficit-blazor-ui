using System.Text.RegularExpressions;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Regression guard: the library is standalone Blazor + CSS + ES modules. No
/// third-party UI framework may come back as a package, as vendored source, as
/// a CSS class or as a CSS custom property fallback. The custom property case
/// is the subtle one: <c>var(--other-lib-primary, var(--sui-color-primary))</c>
/// compiles and renders, but silently hands theming control to the other
/// library whenever a consumer still loads it.
/// </summary>
public sealed class NoThirdPartyUiFrameworkTests
{
    private static readonly string[] BannedIdentifiers =
    [
        "MudBlazor",
        "Radzen",
        "Syncfusion",
        "Blazorise",
        "AntDesign",
        "MatBlazor",
        "Telerik",
        "Icons.Material",
    ];

    private static readonly Regex BannedCssHook =
        new(@"--mud-|\.mud-|--rz-|\.rz-|\.e-(?:btn|input)\b|\.mat-", RegexOptions.Compiled);

    [Fact]
    public void LibrarySources_DoNotMentionAnyThirdPartyUiFramework()
    {
        var offenders = new List<string>();

        foreach (var file in RepositoryLayout.Files(RepositoryLayout.Src, "*.cs", "*.razor", "*.css", "*.js", "*.csproj"))
        {
            var text = File.ReadAllText(file);
            foreach (var identifier in BannedIdentifiers)
            {
                if (text.Contains(identifier, StringComparison.OrdinalIgnoreCase))
                    offenders.Add($"{RepositoryLayout.Relative(file)}: mentions '{identifier}'");
            }
        }

        Assert.True(offenders.Count == 0, string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void Stylesheets_DoNotReadOrTargetThirdPartyDesignTokens()
    {
        var offenders = new List<string>();

        foreach (var file in RepositoryLayout.Files(RepositoryLayout.Src, "*.css"))
        {
            var lines = File.ReadAllLines(file);
            for (var index = 0; index < lines.Length; index++)
            {
                if (BannedCssHook.IsMatch(lines[index]))
                    offenders.Add($"{RepositoryLayout.Relative(file)}:{index + 1}: {lines[index].Trim()}");
            }
        }

        Assert.True(offenders.Count == 0, string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void ProjectFile_ReferencesOnlyAspNetCoreComponents()
    {
        var project = File.ReadAllText(Path.Combine(RepositoryLayout.Src, "Sufficit.Blazor.UI.csproj"));
        var references = Regex.Matches(project, @"<PackageReference\s+Include=""([^""]+)""")
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.All(references, reference =>
            Assert.StartsWith("Microsoft.AspNetCore.Components", reference, StringComparison.Ordinal));
    }

    [Fact]
    public void CompiledAssembly_ReferencesNoThirdPartyUiAssembly()
    {
        var referenced = typeof(Components.SUIButton).Assembly
            .GetReferencedAssemblies()
            .Select(name => name.Name ?? string.Empty)
            .ToArray();

        var offenders = referenced
            .Where(name => BannedIdentifiers.Any(banned =>
                name.Contains(banned, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        Assert.True(offenders.Length == 0, string.Join(", ", offenders));
    }
}
