using System.Reflection;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Showcase;

/// <summary>Read the version of the very assembly rendered by the documentation.</summary>
public static class CatalogBuildInfo
{
    private static readonly string Information = typeof(SUIButton).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0-local";
    public static string Version => Information.Split('+')[0];
    public static bool IsDevelopment => Version.StartsWith("0.0.0", StringComparison.Ordinal);
    public static string Revision => Information.Split('+').Skip(1).FirstOrDefault() ?? "";
    public static string SourceUrl => "https://github.com/sufficit/sufficit-blazor-ui/tree/" +
        (Revision.Length >= 7 && Revision.All(Uri.IsHexDigit) ? Revision : "main");
    public static string Label => IsDevelopment ? "Documentação de desenvolvimento" : $"API do pacote {Version}";
}
