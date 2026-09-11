using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Sufficit.Blazor.UI.Components;
using Sufficit.Blazor.UI.Services;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Minimal render contracts for the feedback family: root element, base class,
/// attribute forwarding and the live-region / progressbar / alert semantics.
/// </summary>
public sealed class RenderContractFeedbackTests
{
    [Fact]
    public void StatusBanner_IsAPoliteStatusRegionAndForwardsAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIStatusBanner>(parameters => parameters
            .Add(component => component.Title, "Tudo operacional")
            .Add(component => component.Description, "Sem incidentes")
            .Add(component => component.Class, "probe")
            .AddUnmatched("data-test", "banner"));

        var root = cut.Find("section.sui-status-banner");
        Assert.Equal("status", root.GetAttribute("role"));
        Assert.Equal("polite", root.GetAttribute("aria-live"));
        Assert.Equal("banner", root.GetAttribute("data-test"));
        Assert.True(root.ClassList.Contains("sui-status-banner--neutral"));
        Assert.True(root.ClassList.Contains("probe"));
        Assert.Equal("Tudo operacional", cut.Find("h2.sui-status-banner__title").TextContent);
        Assert.Equal("Sem incidentes", cut.Find(".sui-status-banner__description").TextContent);
        Assert.Equal("true", cut.Find(".sui-status-banner__signal").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void StatusBanner_AppliesToneAndOptionalActionLink()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIStatusBanner>(parameters => parameters
            .Add(component => component.Title, "Degradado")
            .Add(component => component.Tone, SUITone.Warning)
            .Add(component => component.ShowSignal, false)
            .Add(component => component.AriaLive, "assertive")
            .Add(component => component.ActionText, "Ver status")
            .Add(component => component.ActionHref, "/status"));

        var root = cut.Find(".sui-status-banner");
        Assert.True(root.ClassList.Contains("sui-status-banner--warning"));
        Assert.True(root.ClassList.Contains("sui-status-banner--without-signal"));
        Assert.Equal("assertive", root.GetAttribute("aria-live"));
        Assert.Empty(cut.FindAll(".sui-status-banner__signal"));
        Assert.Equal("/status", cut.Find("a.sui-status-banner__action").GetAttribute("href"));
    }

    [Fact]
    public void SnackbarHost_RendersLiveRegionAndShowsEnqueuedMessages()
    {
        using var context = new BunitContext();
        context.Services.AddSufficitUI();
        var cut = context.Render<SUISnackbarHost>();

        var root = cut.Find(".sui-snackbar-host");
        Assert.Equal("polite", root.GetAttribute("aria-live"));
        Assert.Equal("true", root.GetAttribute("aria-atomic"));
        Assert.Empty(cut.FindAll(".sui-snackbar"));

        context.Services.GetRequiredService<ISUISnackbar>().Success("Salvo");

        cut.WaitForAssertion(() =>
        {
            var snackbar = cut.Find(".sui-snackbar");
            Assert.Equal("status", snackbar.GetAttribute("role"));
            Assert.True(snackbar.ClassList.Contains("sui-snackbar--success"));
            Assert.Equal("Salvo", cut.Find(".sui-snackbar__message").TextContent);
            Assert.Equal("Fechar", cut.Find(".sui-snackbar__close").GetAttribute("aria-label"));
        });
    }

    [Fact]
    public void ProgressCircular_DeterminateExposesProgressbarValuesAndForwardsAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIProgressCircular>(parameters => parameters
            .Add(component => component.Value, 40)
            .Add(component => component.AriaLabel, "Carregando")
            .Add(component => component.SizeValue, SUISize.Small)
            .Add(component => component.ColorValue, SUIColor.Primary)
            .Add(component => component.Class, "probe")
            .AddUnmatched("data-test", "progress"));

        var root = cut.Find("span.sui-progress-circular");
        Assert.Equal("progressbar", root.GetAttribute("role"));
        Assert.Equal("40", root.GetAttribute("aria-valuenow"));
        Assert.Equal("0", root.GetAttribute("aria-valuemin"));
        Assert.Equal("100", root.GetAttribute("aria-valuemax"));
        Assert.Equal("Carregando", root.GetAttribute("aria-label"));
        Assert.Equal("progress", root.GetAttribute("data-test"));
        Assert.True(root.ClassList.Contains("sui-progress-circular--sm"));
        Assert.True(root.ClassList.Contains("sui-progress-circular--static"));
        Assert.True(root.ClassList.Contains("sui-color-primary"));
        Assert.True(root.ClassList.Contains("probe"));
        Assert.Equal(2, cut.FindAll("svg circle").Count);
    }

    [Fact]
    public void ProgressCircular_IndeterminateOmitsValueAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIProgressCircular>(parameters => parameters
            .Add(component => component.Indeterminate, true)
            .Add(component => component.SpinDirection, SUISpinDirection.CounterClockwise));

        var root = cut.Find("[role=progressbar]");
        Assert.False(root.HasAttribute("aria-valuenow"));
        Assert.False(root.HasAttribute("aria-valuemin"));
        Assert.False(root.HasAttribute("aria-valuemax"));
        Assert.True(root.ClassList.Contains("sui-progress-circular--md"));
        Assert.True(root.ClassList.Contains("sui-progress-circular--reverse"));
        Assert.False(root.ClassList.Contains("sui-progress-circular--static"));
        Assert.Equal("true", cut.Find("svg").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void SkeletonLoader_DefaultsToASingleTextBlock()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUISkeletonLoader>(parameters => parameters
            .Add(component => component.Class, "probe"));

        var root = cut.Find("div.sui-skeleton-loader");
        Assert.True(root.ClassList.Contains("probe"));
        Assert.Single(cut.FindAll(".sui-skeleton.sui-skeleton-text"));
    }

    [Fact]
    public void SkeletonLoader_RendersOneRowPerTableLineAndSizedCircle()
    {
        using var context = new BunitContext();
        var table = context.Render<SUISkeletonLoader>(parameters => parameters
            .Add(component => component.Type, SUISkeletonType.Table)
            .Add(component => component.Rows, 4));
        var circle = context.Render<SUISkeletonLoader>(parameters => parameters
            .Add(component => component.Type, SUISkeletonType.Circle)
            .Add(component => component.Size, "32px"));

        Assert.Equal(4, table.FindAll(".sui-skeleton-text").Count);
        Assert.Contains("width: 32px", circle.Find(".sui-skeleton-circle").GetAttribute("style"), StringComparison.Ordinal);
    }

    [Fact]
    public void Toast_IsAnAssertiveAlertAndForwardsAttributes()
    {
        using var context = new BunitContext();
        var dismissed = false;
        var cut = context.Render<SUIToast>(parameters => parameters
            .Add(component => component.Id, "toast-1")
            .Add(component => component.Class, "probe")
            .Add(component => component.OnDismiss, EventCallback.Factory.Create(this, () => dismissed = true))
            .AddUnmatched("data-test", "toast")
            .AddChildContent("Falhou"));

        var root = cut.Find("div.sui-toast");
        Assert.Equal("alert", root.GetAttribute("role"));
        Assert.Equal("assertive", root.GetAttribute("aria-live"));
        Assert.Equal("toast-1", root.Id);
        Assert.Equal("toast", root.GetAttribute("data-test"));
        Assert.True(root.ClassList.Contains("sui-toast--error"));
        Assert.True(root.ClassList.Contains("probe"));
        Assert.Equal("true", cut.Find(".sui-toast__icon").GetAttribute("aria-hidden"));
        Assert.Equal("Falhou", cut.Find(".sui-toast__content").TextContent.Trim());

        var dismiss = cut.Find("button.sui-toast__dismiss");
        Assert.Equal("Fechar", dismiss.GetAttribute("aria-label"));
        dismiss.Click();

        Assert.True(dismissed);
    }

    [Fact]
    public void Toast_HidesDismissButtonWhenNotDismissible()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIToast>(parameters => parameters
            .Add(component => component.Color, SUIColor.Success)
            .Add(component => component.Dismissible, false));

        Assert.True(cut.Find(".sui-toast").ClassList.Contains("sui-toast--success"));
        Assert.Empty(cut.FindAll("button"));
    }

    [Fact]
    public void EmptyState_RendersCopyAndForwardsAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIEmptyState>(parameters => parameters
            .Add(component => component.Title, "Nada por aqui")
            .Add(component => component.Description, "Crie o primeiro registro")
            .Add(component => component.IconPath, SUIEmptyState.Icons.Inbox)
            .Add(component => component.Class, "probe")
            .Add(component => component.Actions, builder => builder.AddMarkupContent(0, "<button>Criar</button>"))
            .AddUnmatched("data-test", "empty"));

        var root = cut.Find("div.sui-empty");
        Assert.True(root.ClassList.Contains("probe"));
        Assert.Equal("empty", root.GetAttribute("data-test"));
        Assert.Equal("Nada por aqui", cut.Find(".sui-empty__title").TextContent);
        Assert.Equal("Crie o primeiro registro", cut.Find(".sui-empty__description").TextContent);
        Assert.NotEmpty(cut.FindAll("svg.sui-empty__icon path"));
        Assert.Equal("Criar", cut.Find(".sui-empty__actions button").TextContent);
    }

    [Fact]
    public void EmptyState_OmitsOptionalSectionsWhenEmpty()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIEmptyState>();

        Assert.NotNull(cut.Find(".sui-empty"));
        Assert.Empty(cut.FindAll(".sui-empty__icon"));
        Assert.Empty(cut.FindAll(".sui-empty__title"));
        Assert.Empty(cut.FindAll(".sui-empty__description"));
        Assert.Empty(cut.FindAll(".sui-empty__actions"));
    }
}
