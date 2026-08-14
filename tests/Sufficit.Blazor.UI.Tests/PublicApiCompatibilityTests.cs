using System.Reflection;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class PublicApiCompatibilityTests
{
    [Fact]
    public void VisualObjectBridges_AreDeprecatedAndHaveTypedReplacements()
    {
        var bridges = new (Type Component, string Legacy, string Replacement, Type Typed)[]
        {
            (typeof(SUIButton), "Color", "ColorValue", typeof(SUIColor?)),
            (typeof(SUIButton), "IconColor", "IconColorValue", typeof(SUIColor?)),
            (typeof(SUIButton), "Variant", "VariantValue", typeof(SUIVariant?)),
            (typeof(SUIButton), "Size", "SizeValue", typeof(SUISize?)),
            (typeof(SUIButton), "IconSize", "IconSizeValue", typeof(SUISize?)),
            (typeof(SUIButton), "ButtonType", "ButtonTypeValue", typeof(SUIButtonType?)),
            (typeof(SUIIconButton), "Color", "ColorValue", typeof(SUIColor?)),
            (typeof(SUIIconButton), "Size", "SizeValue", typeof(SUISize?)),
            (typeof(SUIIconButton), "Variant", "VariantValue", typeof(SUIVariant?)),
            (typeof(SUIIconButton), "Edge", "EdgeValue", typeof(SUIEdge?)),
            (typeof(SUIIconButton), "ButtonType", "ButtonTypeValue", typeof(SUIButtonType?)),
            (typeof(SUILoadingButton), "Color", "ColorValue", typeof(SUIColor?)),
            (typeof(SUILoadingButton), "Size", "SizeValue", typeof(SUISize?)),
            (typeof(SUILoadingButton), "Variant", "VariantValue", typeof(SUIVariant?)),
            (typeof(SUILoadingButton), "ButtonType", "ButtonTypeValue", typeof(SUIButtonType?)),
            (typeof(SUIChip), "Color", "ColorValue", typeof(SUIColor?)),
            (typeof(SUIChip), "Size", "SizeValue", typeof(SUISize?)),
            (typeof(SUIChip), "Variant", "VariantValue", typeof(SUIVariant?)),
            (typeof(SUITimelineItem), "Color", "ColorValue", typeof(SUIColor?)),
            (typeof(SUITimelineItem), "Size", "SizeValue", typeof(SUISize?)),
            (typeof(SUITimelineItem), "Variant", "VariantValue", typeof(SUIVariant?)),
            (typeof(SUIProgressLinear), "Color", "ColorValue", typeof(SUIColor?)),
            (typeof(SUISwitch), "Color", "ColorValue", typeof(SUIColor?)),
        };

        Assert.Equal(23, bridges.Length);
        foreach (var (component, legacyName, replacementName, typedType) in bridges)
        {
            var legacy = component.GetProperty(legacyName)!;
            var replacement = component.GetProperty(replacementName)!;
            var obsolete = legacy.GetCustomAttribute<ObsoleteAttribute>();

            Assert.Equal(typeof(object), Nullable.GetUnderlyingType(legacy.PropertyType) ?? legacy.PropertyType);
            Assert.Equal(typedType, replacement.PropertyType);
            Assert.NotNull(obsolete);
            Assert.Contains(replacementName, obsolete!.Message, StringComparison.Ordinal);
            Assert.Contains("v2.0.0", obsolete.Message, StringComparison.Ordinal);
        }

        Assert.Null(typeof(SUISelectItem).GetProperty(nameof(SUISelectItem.Value))!
            .GetCustomAttribute<ObsoleteAttribute>());

        var toneBridges = new (Type Component, string Legacy, string Replacement)[]
        {
            (typeof(SUIAlert), "Severity", "ToneValue"),
            (typeof(SUIStatusBadge), "Tone", "ToneValue"),
        };

        foreach (var (component, legacyName, replacementName) in toneBridges)
        {
            var legacy = component.GetProperty(legacyName)!;
            var replacement = component.GetProperty(replacementName)!;
            var obsolete = legacy.GetCustomAttribute<ObsoleteAttribute>();

            Assert.Equal(typeof(string), legacy.PropertyType);
            Assert.Equal(typeof(SUITone?), replacement.PropertyType);
            Assert.NotNull(obsolete);
            Assert.Contains(replacementName, obsolete!.Message, StringComparison.Ordinal);
            Assert.Contains("v2.0.0", obsolete.Message, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PublicApi_DoesNotRemoveTrackedSignatures()
    {
        var baselinePath = Path.Combine(FindRepositoryRoot(), "eng", "PublicApiBaseline.txt");
        var current = CapturePublicApi();

        if (Environment.GetEnvironmentVariable("SUI_UPDATE_PUBLIC_API") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            File.WriteAllLines(baselinePath, current);
            return;
        }

        Assert.True(File.Exists(baselinePath),
            $"Public API baseline not found: {baselinePath}. Run with SUI_UPDATE_PUBLIC_API=1 after an intentional review.");
        var baseline = File.ReadAllLines(baselinePath)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
            .ToHashSet(StringComparer.Ordinal);
        var removed = baseline.Except(current, StringComparer.Ordinal).Order().ToArray();

        Assert.True(removed.Length == 0,
            "Public API signatures were removed or changed:" + Environment.NewLine
            + string.Join(Environment.NewLine, removed));
    }

    private static SortedSet<string> CapturePublicApi()
    {
        var signatures = new SortedSet<string>(StringComparer.Ordinal);
        var assembly = typeof(SUIButton).Assembly;

        foreach (var type in assembly.GetExportedTypes().OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            var typeName = FormatType(type);
            signatures.Add($"T|{TypeKind(type)}|{typeName}");
            if (type.BaseType is { } baseType)
                signatures.Add($"B|{typeName}|{FormatType(baseType)}");
            foreach (var contract in type.GetInterfaces().OrderBy(FormatType, StringComparer.Ordinal))
                signatures.Add($"I|{typeName}|{FormatType(contract)}");

            const BindingFlags declaredPublic = BindingFlags.Public | BindingFlags.Instance
                | BindingFlags.Static | BindingFlags.DeclaredOnly;

            foreach (var constructor in type.GetConstructors(declaredPublic))
                signatures.Add($"C|{typeName}|({FormatParameters(constructor.GetParameters())})");

            foreach (var property in type.GetProperties(declaredPublic))
            {
                var accessors = string.Concat(property.GetMethod?.IsPublic == true ? "get;" : string.Empty,
                    property.SetMethod?.IsPublic == true ? "set;" : string.Empty);
                signatures.Add($"P|{typeName}|{FormatType(property.PropertyType)}|{property.Name}"
                    + $"|[{FormatParameters(property.GetIndexParameters())}]|{accessors}");
            }

            foreach (var field in type.GetFields(declaredPublic))
                signatures.Add($"F|{typeName}|{FormatType(field.FieldType)}|{field.Name}");

            foreach (var eventInfo in type.GetEvents(declaredPublic))
                signatures.Add($"E|{typeName}|{FormatType(eventInfo.EventHandlerType!)}|{eventInfo.Name}");

            foreach (var method in type.GetMethods(declaredPublic).Where(method => !method.IsSpecialName))
            {
                var genericArity = method.IsGenericMethodDefinition
                    ? $"``{method.GetGenericArguments().Length}"
                    : string.Empty;
                signatures.Add($"M|{typeName}|{FormatType(method.ReturnType)}|{method.Name}{genericArity}"
                    + $"|({FormatParameters(method.GetParameters())})");
            }
        }

        return signatures;
    }

    private static string FormatParameters(IEnumerable<ParameterInfo> parameters)
        => string.Join(',', parameters.Select(parameter =>
        {
            var modifier = parameter.IsOut ? "out "
                : parameter.ParameterType.IsByRef ? "ref "
                : string.Empty;
            return modifier + FormatType(parameter.ParameterType.IsByRef
                ? parameter.ParameterType.GetElementType()!
                : parameter.ParameterType);
        }));

    private static string FormatType(Type type)
    {
        if (type.IsGenericParameter)
            return $"`{type.GenericParameterPosition}:{type.Name}";
        if (type.IsArray)
            return $"{FormatType(type.GetElementType()!)}[{new string(',', type.GetArrayRank() - 1)}]";
        if (!type.IsGenericType)
            return type.FullName ?? type.Name;

        var genericName = type.GetGenericTypeDefinition().FullName!;
        genericName = genericName[..genericName.IndexOf('`')];
        return $"{genericName}<{string.Join(',', type.GetGenericArguments().Select(FormatType))}>";
    }

    private static string TypeKind(Type type)
        => type.IsEnum ? "enum"
            : type.IsInterface ? "interface"
            : typeof(MulticastDelegate).IsAssignableFrom(type.BaseType) ? "delegate"
            : type.IsValueType ? "struct"
            : "class";

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Sufficit.Blazor.UI.slnx")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate the sufficit-blazor-ui repository root.");
    }
}
