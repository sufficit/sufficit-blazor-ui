using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Sufficit.Blazor.UI.Components;
using Sufficit.Blazor.UI.Services;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Behaviour promoted from sufficit-blazor's CopyToClipBoard (17 usages).
/// Pins the two rendering modes and the toast wiring: with child content the
/// wrapper is semantically transparent; without it a real, labelled button is
/// rendered — the accessibility upgrade over the original clickable span.
/// </summary>
public sealed class CopyToClipboardTests
{
    private sealed class RecordingSnackbar : ISUISnackbar
    {
        public readonly List<(string Message, string Severity)> Entries = new();

        public void Add(string message, string severity = "info", int durationMs = 4000)
            => Entries.Add((message, severity));

        // Required by the interface; the host subscribes to it, these tests do not.
        public event Action<SUISnackbarEntry>? OnEnqueue { add { } remove { } }
    }

    private static (BunitContext Context, RecordingSnackbar Snackbar) CreateContext()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var snackbar = new RecordingSnackbar();
        context.Services.AddSingleton<ISUISnackbar>(snackbar);
        return (context, snackbar);
    }

    [Fact]
    public void WithChildContent_WrapperCarriesNoInteractiveSemantics()
    {
        var (context, _) = CreateContext();
        using var _1 = context;

        var cut = context.Render<SUICopyToClipboard>(parameters => parameters
            .Add(component => component.Text, "abc")
            .AddChildContent("<button type='button'>Copiar</button>"));

        var wrapper = cut.Find(".sui-copy-clip");
        // The child is the interactive element; role or tabindex on the wrapper
        // would nest interactive semantics, which assistive tech rejects.
        Assert.Null(wrapper.GetAttribute("role"));
        Assert.Null(wrapper.GetAttribute("tabindex"));
        Assert.NotNull(cut.Find("button"));
    }

    [Fact]
    public void WithoutChildContent_RendersALabelledButton()
    {
        var (context, _) = CreateContext();
        using var _1 = context;

        var cut = context.Render<SUICopyToClipboard>(parameters => parameters
            .Add(component => component.Text, "abc")
            .Add(component => component.Title, "Token"));

        var button = cut.Find("button");
        Assert.Equal("Copiar Token", button.GetAttribute("aria-label"));
    }

    [Fact]
    public void SuccessfulCopy_RaisesASuccessToastNamingTheItem()
    {
        var (context, snackbar) = CreateContext();
        using var _1 = context;

        var cut = context.Render<SUICopyToClipboard>(parameters => parameters
            .Add(component => component.Text, "abc")
            .Add(component => component.Title, "Token"));

        cut.Find("button").Click();

        var entry = Assert.Single(snackbar.Entries);
        Assert.Equal("success", entry.Severity);
        Assert.Contains("Token", entry.Message);
    }

    [Fact]
    public void FullWidth_StretchesTheWrapper()
    {
        var (context, _) = CreateContext();
        using var _1 = context;

        var cut = context.Render<SUICopyToClipboard>(parameters => parameters
            .Add(component => component.Text, "abc")
            .Add(component => component.FullWidth, true)
            .AddChildContent("<button type='button'>Copiar</button>"));

        Assert.NotNull(cut.Find(".sui-copy-clip--full"));
    }
}
