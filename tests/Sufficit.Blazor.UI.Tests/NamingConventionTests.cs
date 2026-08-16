using Microsoft.AspNetCore.Components;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Enforces the SUI naming and layout contract. These rules are what let a
/// consumer predict an API without reading it: every public surface is
/// <c>SUI</c>-prefixed, every namespace mirrors its folder, and every
/// colocated asset belongs to a component that exists.
/// </summary>
public sealed class NamingConventionTests
{
    private static readonly Assembly Library = typeof(Components.SUIButton).Assembly;

    private static readonly string[] AllowedNamespaces =
    [
        "Sufficit.Blazor.UI",
        "Sufficit.Blazor.UI.Components",
        "Sufficit.Blazor.UI.Services",
        "Sufficit.Blazor.UI.Themes",
        "Sufficit.Blazor.UI.Utilities",
    ];

    /// <summary>Public types that predate the prefix rule or follow a framework convention.</summary>
    private static readonly string[] PrefixExemptTypes =
    [
        "ServiceCollectionExtensions",
        "DefaultSUITheme",
        // Debt: renaming is a public API break, scheduled for v2 (docs/PLAN-SUI-V2.md).
        "NavAccordionScope",
    ];

    /// <summary>
    /// Parameters whose names shipped before the convention was enforced.
    /// Renaming them breaks Razor call sites, so they are frozen until v2.
    /// </summary>
    private static readonly string[] LegacyParameterNames =
    [
        "SUIItem.xs", "SUIItem.sm", "SUIItem.md", "SUIItem.lg", "SUIItem.xl",
        "SUIAlert.CloseIconClicked",
    ];

    private static readonly string[] ComponentCategories =
    [
        "Actions", "DataDisplay", "Feedback", "Forms", "Layout", "Navigation", "Overlays",
    ];

    [Fact]
    public void ExportedTypes_UseTheSuiPrefix()
    {
        var offenders = Library.GetExportedTypes()
            .Where(type => !type.IsNested)
            .Where(type => !type.Name.StartsWith('_')) // Razor _Imports scaffolding
            .Where(type => !PrefixExemptTypes.Contains(type.Name, StringComparer.Ordinal))
            .Where(type => !Regex.IsMatch(StripArity(type.Name), "^I?SUI[A-Z]"))
            .Select(type => type.FullName!)
            .ToArray();

        Assert.True(offenders.Length == 0,
            "Public types must be named SUIXxx (or ISUIXxx): " + string.Join(", ", offenders));
    }

    [Fact]
    public void ExportedTypes_LiveInAnApprovedNamespace()
    {
        var offenders = Library.GetExportedTypes()
            .Where(type => !type.IsNested)
            .Where(type => !AllowedNamespaces.Contains(type.Namespace, StringComparer.Ordinal))
            .Select(type => type.FullName!)
            .ToArray();

        Assert.True(offenders.Length == 0, string.Join(", ", offenders));
    }

    [Fact]
    public void Components_AreDeclaredPublic()
    {
        var offenders = Library.GetTypes()
            .Where(type => typeof(ComponentBase).IsAssignableFrom(type) && !type.IsAbstract)
            .Where(type => !type.IsPublic && !type.IsNestedPublic)
            .Select(type => type.FullName!)
            .ToArray();

        Assert.True(offenders.Length == 0,
            "Components must be public to be usable from a consumer: " + string.Join(", ", offenders));
    }

    [Fact]
    public void ComponentParameters_ArePublicPascalCaseProperties()
    {
        var offenders = new List<string>();

        foreach (var type in ComponentTypes())
        {
            // Cascading parameters are wired by the framework, never by the
            // consumer, so they are allowed to stay private.
            var parameters = type
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(property => property.GetCustomAttribute<ParameterAttribute>() is not null);

            foreach (var parameter in parameters)
            {
                var name = $"{type.Name}.{parameter.Name}";
                if (LegacyParameterNames.Contains(name, StringComparer.Ordinal))
                    continue;

                if (parameter.GetMethod?.IsPublic != true || parameter.SetMethod?.IsPublic != true)
                    offenders.Add($"{name}: parameters need a public getter and setter");

                if (!char.IsUpper(parameter.Name[0]))
                    offenders.Add($"{name}: parameters are PascalCase");
            }
        }

        Assert.True(offenders.Count == 0, string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void EventCallbackParameters_UseOnXxxOrXxxChanged()
    {
        var offenders = new List<string>();

        foreach (var type in ComponentTypes())
        {
            var callbacks = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.GetCustomAttribute<ParameterAttribute>() is not null)
                .Where(property => property.PropertyType == typeof(EventCallback)
                    || (property.PropertyType.IsGenericType
                        && property.PropertyType.GetGenericTypeDefinition() == typeof(EventCallback<>)));

            foreach (var callback in callbacks)
            {
                if (LegacyParameterNames.Contains($"{type.Name}.{callback.Name}", StringComparer.Ordinal))
                    continue;

                if (!callback.Name.StartsWith("On", StringComparison.Ordinal)
                    && !callback.Name.EndsWith("Changed", StringComparison.Ordinal))
                {
                    offenders.Add($"{type.Name}.{callback.Name}");
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "EventCallback parameters are named OnXxx or XxxChanged: " + string.Join(", ", offenders));
    }

    [Fact]
    public void RazorFiles_AreSuiPrefixedAndFiledUnderACategory()
    {
        var componentsRoot = Path.Combine(RepositoryLayout.Src, "Components");
        var offenders = new List<string>();

        foreach (var file in RepositoryLayout.Files(componentsRoot, "*.razor"))
        {
            var relative = RepositoryLayout.Relative(file);
            var name = Path.GetFileNameWithoutExtension(file);

            if (!name.StartsWith("SUI", StringComparison.Ordinal))
                offenders.Add($"{relative}: component files are named SUIXxx.razor");

            var category = Path.GetFileName(Path.GetDirectoryName(file)!);
            if (!ComponentCategories.Contains(category, StringComparer.Ordinal))
                offenders.Add($"{relative}: unknown category folder '{category}'");
        }

        Assert.True(offenders.Count == 0, string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void ColocatedAssets_BelongToAnExistingComponent()
    {
        var offenders = new List<string>();

        foreach (var file in RepositoryLayout.Files(RepositoryLayout.Src, "*.razor.cs", "*.razor.css", "*.razor.js"))
        {
            var component = Path.ChangeExtension(file, null);
            if (!File.Exists(component))
                offenders.Add($"{RepositoryLayout.Relative(file)}: no sibling {Path.GetFileName(component)}");
        }

        Assert.True(offenders.Count == 0, string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void CodeBehindFiles_DeclareOnlyTheirOwnComponent()
    {
        var offenders = new List<string>();

        foreach (var file in RepositoryLayout.Files(RepositoryLayout.Src, "*.razor.cs"))
        {
            var expected = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file));
            var text = File.ReadAllText(file);

            if (!Regex.IsMatch(text, $@"partial class {Regex.Escape(expected)}\b"))
                offenders.Add($"{RepositoryLayout.Relative(file)}: expected 'partial class {expected}'");
        }

        Assert.True(offenders.Count == 0, string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void JsModules_AreColocatedAndNeverLoadedGlobally()
    {
        var offenders = RepositoryLayout.Files(RepositoryLayout.Src, "*.js")
            .Where(file => !file.EndsWith(".razor.js", StringComparison.Ordinal))
            .Select(RepositoryLayout.Relative)
            .ToArray();

        Assert.True(offenders.Length == 0,
            "Scripts ship as colocated .razor.js modules only: " + string.Join(", ", offenders));
    }

    private static IEnumerable<Type> ComponentTypes()
        => Library.GetTypes()
            .Where(type => typeof(ComponentBase).IsAssignableFrom(type) && !type.IsAbstract);

    private static string StripArity(string name)
    {
        var index = name.IndexOf('`');
        return index < 0 ? name : name[..index];
    }
}
