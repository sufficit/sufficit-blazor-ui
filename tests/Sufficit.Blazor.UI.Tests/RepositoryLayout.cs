using System.Runtime.CompilerServices;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Locates the repository on disk so the architecture, naming, file size and
/// asset budget tests can inspect the real sources instead of the compiled
/// output only. Resolution prefers the compile-time path of this file and
/// falls back to walking up from the test binaries, so the suite works both
/// from an IDE and from <c>dotnet test</c> in CI.
/// </summary>
internal static class RepositoryLayout
{
    private static readonly Lazy<string> LazyRoot = new(Resolve);

    /// <summary>Absolute path of the repository root.</summary>
    public static string Root => LazyRoot.Value;

    /// <summary>Absolute path of the library sources.</summary>
    public static string Src => Path.Combine(Root, "src");

    /// <summary>Absolute path of the authored (unbundled) stylesheets.</summary>
    public static string Styles => Path.Combine(Src, "styles");

    /// <summary>Absolute path of the published static web assets.</summary>
    public static string WebRoot => Path.Combine(Src, "wwwroot");

    /// <summary>Path relative to the repository root, using forward slashes.</summary>
    public static string Relative(string absolutePath)
        => Path.GetRelativePath(Root, absolutePath).Replace(Path.DirectorySeparatorChar, '/');

    /// <summary>
    /// Enumerates authored source files under <paramref name="directory"/>,
    /// skipping build output.
    /// </summary>
    public static IEnumerable<string> Files(string directory, params string[] patterns)
    {
        if (!Directory.Exists(directory))
            return [];

        return patterns
            .SelectMany(pattern => Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories))
            .Where(path => !IsBuildOutput(path))
            .OrderBy(path => path, StringComparer.Ordinal);
    }

    private static bool IsBuildOutput(string path)
    {
        var normalized = path.Replace(Path.DirectorySeparatorChar, '/');
        return normalized.Contains("/bin/", StringComparison.Ordinal)
            || normalized.Contains("/obj/", StringComparison.Ordinal);
    }

    private static string Resolve()
    {
        foreach (var candidate in Candidates())
        {
            var root = WalkUp(candidate);
            if (root is not null)
                return root;
        }

        throw new InvalidOperationException(
            "Repository root not found: no ancestor directory contains Sufficit.Blazor.UI.slnx.");
    }

    private static IEnumerable<string> Candidates()
    {
        yield return Path.GetDirectoryName(ThisFile())!;
        yield return AppContext.BaseDirectory;
        yield return Directory.GetCurrentDirectory();
    }

    private static string? WalkUp(string start)
    {
        var directory = new DirectoryInfo(start);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Sufficit.Blazor.UI.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }

    private static string ThisFile([CallerFilePath] string path = "") => path;
}
