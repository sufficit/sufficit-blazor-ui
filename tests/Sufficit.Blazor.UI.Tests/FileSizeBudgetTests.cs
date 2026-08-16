namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// File size budgets. A component that outgrows its budget is a component that
/// is doing more than one thing; the fix is to split it, not to raise the
/// number. Files listed in <see cref="Debt"/> are known offenders frozen at
/// their current size: they may shrink, never grow, and the entry must be
/// removed once the file drops back under the category budget.
/// </summary>
public sealed class FileSizeBudgetTests
{
    private const int RazorBudget = 250;
    private const int CSharpBudget = 450;
    private const int ComponentCssBudget = 200;
    private const int JsModuleBudget = 320;
    private const int TestFileBudget = 400;

    /// <summary>Path (repo-relative) to the frozen ceiling of a known offender.</summary>
    private static readonly Dictionary<string, int> Debt = new(StringComparer.Ordinal)
    {
        // Single stylesheet holding every non-isolated component rule. Should be
        // split per component alongside the .razor.css files it already has.
        ["src/styles/sui-components.css"] = 1430,
        ["src/styles/sui-foundations.css"] = 420,
        // One NUnit class covering the whole catalog surface; split by concern.
        ["tests/Sufficit.Blazor.UI.BrowserTests/CatalogBrowserTests.cs"] = 940,
    };

    public static TheoryData<string, int> BudgetedFiles()
    {
        var data = new TheoryData<string, int>();

        foreach (var file in RepositoryLayout.Files(RepositoryLayout.Src, "*.razor"))
            data.Add(RepositoryLayout.Relative(file), RazorBudget);

        foreach (var file in RepositoryLayout.Files(RepositoryLayout.Src, "*.cs"))
            data.Add(RepositoryLayout.Relative(file), CSharpBudget);

        foreach (var file in RepositoryLayout.Files(RepositoryLayout.Src, "*.css"))
            data.Add(RepositoryLayout.Relative(file), ComponentCssBudget);

        foreach (var file in RepositoryLayout.Files(RepositoryLayout.Src, "*.js"))
            data.Add(RepositoryLayout.Relative(file), JsModuleBudget);

        foreach (var file in RepositoryLayout.Files(Path.Combine(RepositoryLayout.Root, "tests"), "*.cs"))
            data.Add(RepositoryLayout.Relative(file), TestFileBudget);

        return data;
    }

    [Theory]
    [MemberData(nameof(BudgetedFiles))]
    public void File_StaysWithinItsLineBudget(string relativePath, int budget)
    {
        var effective = Debt.TryGetValue(relativePath, out var frozen) ? frozen : budget;
        var lines = File.ReadAllLines(Path.Combine(RepositoryLayout.Root, relativePath)).Length;

        Assert.True(lines <= effective,
            $"{relativePath} has {lines} lines, budget is {effective}. Split the file instead of raising the budget.");
    }

    [Fact]
    public void DebtEntries_StillPointAtRealOffenders()
    {
        var stale = new List<string>();

        foreach (var (relativePath, frozen) in Debt)
        {
            var absolute = Path.Combine(RepositoryLayout.Root, relativePath);
            if (!File.Exists(absolute))
            {
                stale.Add($"{relativePath}: file no longer exists, drop the debt entry");
                continue;
            }

            var lines = File.ReadAllLines(absolute).Length;
            if (lines > frozen)
                stale.Add($"{relativePath}: grew to {lines} lines, over its frozen ceiling of {frozen}");
        }

        Assert.True(stale.Count == 0, string.Join(Environment.NewLine, stale));
    }

    [Fact]
    public void GeneratedStylesheet_StaysWithinItsSizeBudget()
    {
        var bundle = Path.Combine(RepositoryLayout.WebRoot, "sufficit-ui.css");
        var bytes = new FileInfo(bundle).Length;

        Assert.True(bytes <= 56 * 1024,
            $"sufficit-ui.css is {bytes} bytes, budget is {56 * 1024}. See AssetBudgetTests for the compressed budgets.");
    }
}
