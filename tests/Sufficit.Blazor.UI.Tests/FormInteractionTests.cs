using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class FormInteractionTests
{
    [Fact]
    public async Task CheckboxAssociatesAndRemovesValidationErrors()
    {
        using var context = new BunitContext();
        var model = new Preferences();
        var form = new EditContext(model);
        var cut = context.Render<CascadingValue<EditContext>>(p => p.Add(x => x.Value, form)
            .AddChildContent<SUICheckbox>(p => p.Add(x => x.Label, "Aceitar")
                .Add(x => x.ValueExpression, () => model.Accepted)));
        var messages = new ValidationMessageStore(form);
        messages.Add(form.Field(nameof(model.Accepted)), "Aceite os termos.");
        await cut.InvokeAsync(form.NotifyValidationStateChanged);
        var input = cut.Find("input");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        var error = input.GetAttribute("aria-errormessage");
        Assert.Equal(error, input.GetAttribute("aria-describedby"));
        Assert.Equal("Aceite os termos.", cut.Find("#" + error).TextContent);
        messages.Clear();
        await cut.InvokeAsync(form.NotifyValidationStateChanged);
        Assert.Null(cut.Find("input").GetAttribute("aria-errormessage"));
        Assert.Null(cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public void InvalidFieldsWithoutMessageDoNotReferenceMissingElements()
    {
        using var context = new BunitContext();
        var text = context.Render<SUITextField<string>>(p => p.Add(x => x.Invalid, true));
        var number = context.Render<SUINumericField<int>>(p => p.Add(x => x.Invalid, true));
        foreach (var input in new[] { text.Find("input"), number.Find("input") })
        {
            Assert.Equal("true", input.GetAttribute("aria-invalid"));
            Assert.Null(input.GetAttribute("aria-errormessage"));
        }
    }

    [Fact]
    public async Task DisablingAutocompleteCancelsPendingResults()
    {
        using var context = new BunitContext();
        context.JSInterop.SetupModule("./_content/Sufficit.Blazor.UI/Components/Forms/SUIAutocomplete.razor.js").Mode = JSRuntimeMode.Loose;
        var started = new TaskCompletionSource<CancellationToken>();
        var cut = context.Render<SUIAutocomplete<string>>(p => p.Add(x => x.DebounceInterval, 0)
            .Add(x => x.SearchFuncAsync, async (_, token) => {
                started.SetResult(token);
                await Task.Delay(Timeout.Infinite, token);
                return Array.Empty<string>();
            }));
        var inputTask = cut.Find("input").InputAsync("a");
        var token = await started.Task;
        cut.Render(p => p.Add(x => x.Disabled, true));
        await inputTask.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.True(token.IsCancellationRequested);
        Assert.Equal("false", cut.Find("input").GetAttribute("aria-expanded"));
    }

    private sealed class Preferences { public bool Accepted { get; set; } }
}
