using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class AutocompleteReadOnlyTests : BunitContext
{
    public AutocompleteReadOnlyTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public async Task ReadOnlyKeepsTheSelectedValueFocusableAndRejectsSearchSelectionAndClear()
    {
        var reads = 0; var changes = 0;
        var cut = Render<SUIAutocomplete<string>>(p => p.Add(c => c.Value, "Selected customer")
            .Add(c => c.DebounceInterval, 0).Add(c => c.Clearable, true).AddUnmatched("ReadOnly", true)
            .Add(c => c.ValueChanged, (string? _) => changes++)
            .Add(c => c.SearchFunc, _ => { reads++; return Task.FromResult<IEnumerable<string>>(["Other customer"]); }));
        Assert.True(cut.Find("input").HasAttribute("readonly"));
        Assert.False(cut.Find("input").HasAttribute("disabled"));
        Assert.Empty(cut.FindAll(".sui-autocomplete__clear"));
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "" });
        await cut.Find("input").KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown" });
        Assert.Equal("Selected customer", cut.Find("input").GetAttribute("value"));
        Assert.Equal(0, reads); Assert.Equal(0, changes);
        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task SwitchingToReadOnlyCancelsPendingSearchAndRestoresTheSelectedDisplayText()
    {
        var pending = new TaskCompletionSource<IEnumerable<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationToken token = default;
        var cut = Render<SUIAutocomplete<string>>(p => p.Add(c => c.Value, "Selected customer").Add(c => c.DebounceInterval, 0)
            .Add(c => c.SearchFuncAsync, (_, ct) => { token = ct; return pending.Task; }));
        var input = cut.Find("input").InputAsync(new ChangeEventArgs { Value = "search" });
        cut.WaitForAssertion(() => Assert.True(token.CanBeCanceled));
        cut.Render(p => p.AddUnmatched("ReadOnly", true));
        Assert.True(token.IsCancellationRequested);
        pending.SetResult(["Late customer"]); await input;
        Assert.Equal("Selected customer", cut.Find("input").GetAttribute("value"));
        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));
        Assert.Empty(cut.FindAll("[role=option]"));
    }
}
