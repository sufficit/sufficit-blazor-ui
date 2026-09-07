using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Sufficit.Blazor.UI.Components;
using Sufficit.Blazor.UI.Themes;

namespace Sufficit.Blazor.UI.Tests;

public sealed class CatalogImprovementTests
{
    [Fact]
    public void NumericField_OmittedBoundsDoNotRestrictNativeFormSubmission()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUINumericField<int>>(p => p.Add(x => x.Value, 3));
        Assert.False(cut.Find("input").HasAttribute("min"));
        Assert.False(cut.Find("input").HasAttribute("max"));
        cut.Render(p => p.Add(x => x.Min, 0).Add(x => x.Max, 10));
        Assert.Equal("0", cut.Find("input").GetAttribute("min"));
        Assert.Equal("10", cut.Find("input").GetAttribute("max"));
    }

    [Fact]
    public void Stack_AppliesStyleAndExplicitZeroSpacing()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIStack>(p => p.Add(x => x.Spacing, 0).Add(x => x.Style, "width:50%"));
        Assert.Equal("gap:0;width:50%", cut.Find("div").GetAttribute("style"));
    }

    [Fact]
    public void TextField_NotifiesEditContextAndDisplaysValidation()
    {
        using var context = new BunitContext();
        var model = new FormModel();
        var form = new EditContext(model);
        var cut = context.Render<CascadingValue<EditContext>>(p => p.Add(x => x.Value, form)
            .AddChildContent<SUITextField<string>>(p => p
                .Add(x => x.ValueExpression, () => model.Name)
                .Add(x => x.ValueChanged, value => model.Name = value ?? "")));
        cut.Find("input").Change("novo");
        Assert.Equal("novo", model.Name);
        Assert.True(form.IsModified(form.Field(nameof(model.Name))));
        var messages = new ValidationMessageStore(form);
        messages.Add(form.Field(nameof(model.Name)), "Nome indisponível");
        cut.InvokeAsync(form.NotifyValidationStateChanged);
        cut.WaitForAssertion(() => Assert.Contains("Nome indisponível", cut.Markup));
    }

    [Fact]
    public void NumericField_InvalidInputBlocksFormValidation()
    {
        using var context = new BunitContext();
        var model = new FormModel();
        var form = new EditContext(model);
        var cut = context.Render<CascadingValue<EditContext>>(p => p.Add(x => x.Value, form)
            .AddChildContent<SUINumericField<int>>(p => p
                .Add(x => x.ValueExpression, () => model.Count)
                .Add(x => x.ValueChanged, value => model.Count = value)));
        cut.Find("input").Change("invalid");
        Assert.False(form.Validate());
        cut.Find("input").Change("5");
        Assert.True(form.Validate());
        Assert.Equal(5, model.Count);
    }

    [Fact]
    public async Task TextField_DoesNotSwallowConsumerExceptions()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUITextField<string>>(p => p.Add(x => x.ValueChanged,
            _ => Task.FromException(new InvalidOperationException("consumer"))));
        await Assert.ThrowsAsync<InvalidOperationException>(() => cut.Find("input").ChangeAsync(new() { Value = "x" }));
    }

    [Fact]
    public async Task Autocomplete_CancelsAnAlreadyRunningSearch()
    {
        await using var context = new BunitContext();
        var started = new TaskCompletionSource<CancellationToken>();
        var cut = context.Render<SUIAutocomplete<string>>(p => p.Add(x => x.DebounceInterval, 0)
            .Add(x => x.SearchFuncAsync, async (_, token) =>
            {
                started.TrySetResult(token);
                await Task.Delay(5000, token);
                return Array.Empty<string>();
            }));
        var input = cut.Find("input").InputAsync("a");
        var token = await started.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await context.DisposeAsync();
        await input.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.True(token.IsCancellationRequested);
    }

    [Fact]
    public void DarkPreset_HasReadableSemanticForegrounds()
    {
        var theme = SUITheme.Dark;
        Assert.True(theme.IsDark);
        Assert.Equal("#0f172a", theme.Palette.SuccessContrast);
        Assert.Contains("var(--sui-color-primary)", theme.Palette.PrimarySoft);
    }

    private sealed class FormModel
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }
}
