using System.IO.Compression;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Payload budgets for everything the library pushes over the wire. Transfer
/// size is what a consumer actually pays for, so the budgets are asserted on
/// the Brotli and gzip encodings a static file server negotiates, with the raw
/// size kept as a parse-cost ceiling.
/// </summary>
public sealed class AssetBudgetTests
{
    // Headroom rule (2026-09-11): a ceiling is the measured value × 1.03,
    // rounded up to the next 256 B. Less than that turned every unrelated
    // commit into a budget failure (three CI failures on 10–11/09 with nothing
    // over budget in the change itself). Ceilings ratchet DOWN in a dedicated
    // commit when a measurement drops; they go up only with a justification
    // in the commit that adds the bytes. Measured values are recorded beside
    // each constant so the next reader can see the real headroom.
    private const int BundleRawBudget = 56 * 1024;            // measured 56,202 B (2026-09-18, SUIToastHost stack + tone variants + action button)
    private const int BundleGzipBudget = 10 * 1024 + 256;     // measured 10,336 B (2026-09-18)
    private const int BundleBrotliBudget = 9 * 1024;          // measured  9,009 B (2026-09-18)
    private const int JsModuleRawBudget = 12 * 1024;
    // Every colocated module together, Brotli. Numeric interop remains opt-in;
    // this ceiling measures all of them at once. Raised 2026-09-13 for the
    // SUISlidingTabs indicator module (position + resize/font repositioning).
    // Raised 2026-09-19 for the SUITable grid-row keyboard module (WAI-ARIA
    // row focus replacing tr[role=button]) and the clipboard HTTPS message.
    // Raised 2026-09-20 for the SUIPopover module (hover-intent delays, focus,
    // Escape, fixed positioning with viewport flip) — measured 12,387 B.
    private const int JsTotalBrotliBudget = 12 * 1024 + 512;  // measured 12,387 B
    // Raw bytes of every .razor.css (source/parse cost of CSS isolation).
    // Raised 2026-09-13 for SUISlidingTabs (pill track + sliding indicator,
    // micro-interactions, responsive collapse, reduced-motion) — measured
    // 32,217 B; headroom rule keeps ~3% slack.
    // Raised 2026-09-20 for SUIPopover (dark floating surface, per-placement
    // arrows, open state, reduced-motion) — measured 34,491 B.
    private const int IsolatedCssRawBudget = 34 * 1024 + 1024; // measured 34,491 B

    [Fact]
    public void GlobalStylesheet_FitsTheTransferBudget()
    {
        var bundle = File.ReadAllBytes(Path.Combine(RepositoryLayout.WebRoot, "sufficit-ui.css"));

        Assert.True(bundle.Length <= BundleRawBudget,
            $"sufficit-ui.css raw is {bundle.Length} B, budget {BundleRawBudget} B.");
        Assert.True(Gzip(bundle) <= BundleGzipBudget,
            $"sufficit-ui.css gzip is {Gzip(bundle)} B, budget {BundleGzipBudget} B.");
        Assert.True(Brotli(bundle) <= BundleBrotliBudget,
            $"sufficit-ui.css brotli is {Brotli(bundle)} B, budget {BundleBrotliBudget} B.");
    }

    [Fact]
    public void EveryJsModule_FitsTheParseBudget()
    {
        var offenders = RepositoryLayout.Files(RepositoryLayout.Src, "*.razor.js")
            .Select(file => (Path: RepositoryLayout.Relative(file), Size: new FileInfo(file).Length))
            .Where(module => module.Size > JsModuleRawBudget)
            .Select(module => $"{module.Path}: {module.Size} B > {JsModuleRawBudget} B")
            .ToArray();

        Assert.True(offenders.Length == 0, string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void AllJsModulesTogether_FitTheTransferBudget()
    {
        var total = RepositoryLayout.Files(RepositoryLayout.Src, "*.razor.js")
            .Sum(file => Brotli(File.ReadAllBytes(file)));

        Assert.True(total <= JsTotalBrotliBudget,
            $"Colocated JS modules total {total} B brotli, budget {JsTotalBrotliBudget} B.");
    }

    [Fact]
    public void ScopedComponentCss_StaysSmall()
    {
        var total = RepositoryLayout.Files(RepositoryLayout.Src, "*.razor.css")
            .Sum(file => new FileInfo(file).Length);

        Assert.True(total <= IsolatedCssRawBudget,
            $"CSS isolation files total {total} B, budget {IsolatedCssRawBudget} B.");
    }

    [Fact]
    public void GeneratedBundle_IsMinifiedNotAuthored()
    {
        var bundle = File.ReadAllText(Path.Combine(RepositoryLayout.WebRoot, "sufficit-ui.css"));
        var authored = RepositoryLayout.Files(RepositoryLayout.Styles, "*.css")
            .Sum(file => new FileInfo(file).Length);

        Assert.True(bundle.Length < authored,
            "sufficit-ui.css must be the minified build output of src/styles; run `npm run build:css`.");
        Assert.DoesNotContain("\n\n", bundle, StringComparison.Ordinal);
    }

    private static long Gzip(byte[] payload) => Compress(payload, stream =>
        new GZipStream(stream, CompressionLevel.SmallestSize, leaveOpen: true));

    private static long Brotli(byte[] payload) => Compress(payload, stream =>
        new BrotliStream(stream, CompressionLevel.SmallestSize, leaveOpen: true));

    private static long Compress(byte[] payload, Func<Stream, Stream> encoder)
    {
        using var output = new MemoryStream();
        using (var compressor = encoder(output))
            compressor.Write(payload, 0, payload.Length);

        return output.Length;
    }
}
