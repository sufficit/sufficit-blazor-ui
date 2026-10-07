using System.Text.RegularExpressions;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// The next-major breaking window is inventoried in
/// docs/PLAN-MAJOR3-BREAKING-WINDOW.md. These contracts keep that inventory
/// honest in both directions: every <c>[Obsolete]</c> in src is a bridge the
/// plan lists (no silent new bridges, no forgotten removal), the counts match
/// exactly, and each bridge message points at its canonical replacement.
/// Nothing is removed before the window — that is the plan's own rule.
/// </summary>
public sealed class BreakingWindowContractTests
{
    private sealed record Bridge(string RelativePath, string Message);

    private const string AttributeBridgeNeedle = "Use AdditionalAttributes instead";
    private const string SeverityBridgeNeedle = "string severity bridge";

    private static string Plan
        => File.ReadAllText(Path.Combine(RepositoryLayout.Root, "docs", "PLAN-MAJOR3-BREAKING-WINDOW.md"));

    /// <summary>
    /// Every [Obsolete] in src, with the file that carries it. A parameterless
    /// [Obsolete] has no message and shows up with an empty one.
    /// </summary>
    private static List<Bridge> CollectBridges()
        => Directory.EnumerateFiles(RepositoryLayout.Src, "*", SearchOption.AllDirectories)
            .Where(path => (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                        || path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
                        && !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                            .Contains("obj")
                        && !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                            .Contains("bin"))
            .Select(path => (Path: path, Text: File.ReadAllText(path)))
            .SelectMany(file => Regex.Matches(file.Text, @"\bObsolete(?:\(\s*""(?<msg>[^""]*)""\s*\))?")
                .Select(match => new Bridge(
                    RepositoryLayout.Relative(file.Path),
                    match.Groups["msg"].Success ? match.Groups["msg"].Value : string.Empty)))
            .ToList();

    [Fact]
    public void SrcCarriesExactlyTheBridgesThePlanInventories()
    {
        var bridges = CollectBridges();

        var attributeBridges = bridges.Where(b => b.Message.Contains(AttributeBridgeNeedle)).ToArray();
        var severityBridges = bridges.Where(b => b.Message.Contains(SeverityBridgeNeedle)).ToArray();
        var unexpected = bridges
            .Where(b => !b.Message.Contains(AttributeBridgeNeedle) && !b.Message.Contains(SeverityBridgeNeedle))
            .ToArray();

        // A new [Obsolete] must either be a documented bridge or update the plan.
        Assert.True(unexpected.Length == 0,
            "Unexpected [Obsolete] entries in src (not inventory bridges):\n  "
            + string.Join("\n  ", unexpected.Select(b => $"{b.RelativePath}: {b.Message}")));
        Assert.Equal(21, attributeBridges.Length);
        Assert.Equal(2, severityBridges.Length);
    }

    [Fact]
    public void PlanListsEveryBridgedComponentAndTheCanonicalTargets()
    {
        var plan = Plan;

        // The counts the plan claims are the counts the code carries.
        Assert.Contains("As 21 pontes", plan);
        Assert.Contains("As 2 sobrecargas", plan);

        // Each component carrying an attribute bridge is named in the plan.
        foreach (var bridge in CollectBridges().Where(b => b.Message.Contains(AttributeBridgeNeedle)))
        {
            var file = Path.GetFileName(bridge.RelativePath);
            var component = file.EndsWith(".razor.cs", StringComparison.OrdinalIgnoreCase)
                ? file[..^".razor.cs".Length]
                : Path.GetFileNameWithoutExtension(file);
            Assert.True(plan.Contains(component), $"{component} carries a bridge but is not in the plan ({bridge.RelativePath})");
        }

        // Canonical replacements are stated, not just implied.
        Assert.Contains("SUIComponentBase.AdditionalAttributes", plan);
        Assert.Contains("SUITone", plan);
    }

    [Fact]
    public void BridgeMessagesPointAtTheCanonicalReplacement()
    {
        var bridges = CollectBridges();

        foreach (var bridge in bridges.Where(b => b.Message.Contains(AttributeBridgeNeedle)))
            Assert.Contains("AdditionalAttributes", bridge.Message);

        foreach (var bridge in bridges.Where(b => b.Message.Contains(SeverityBridgeNeedle)))
            Assert.Contains("SUITone", bridge.Message);
    }

    [Fact]
    public void PlanKeepsTheMaterialFamilyAndSelectItemValueOutOfTheWindow()
    {
        var plan = Plan;

        // F3 decision: the Material family stays supported until the window
        // decides its future — the plan must not turn removal into a done deal.
        Assert.Contains("não é removida nesta janela", plan);

        // SUISelectItem.Value stays object? by design (ARCHITECTURE-VERSIONING-AND-TFM).
        Assert.Contains("SUISelectItem.Value", plan);
        Assert.Contains("por desenho", plan);
    }
}
