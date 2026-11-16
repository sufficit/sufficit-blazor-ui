using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;
using System.ComponentModel.DataAnnotations;

namespace Sufficit.Blazor.UI.Tests;

public sealed class FormValidationTests
{
    private sealed class Model { [Required] public string Name { get; set; } = ""; }

    [Fact]
    public async Task AnnotationsBlockSubmissionUntilTheModelIsValid()
    {
        using var context = new BunitContext();
        var model = new Model();
        var submitted = 0;
        var form = context.Render<SUIForm>(p => p.Add(c => c.Model, model)
            .Add(c => c.OnValidSubmit, _ => submitted++));
        await form.InvokeAsync(() => form.Find("form").Submit());
        Assert.False(form.Instance.IsValid);
        Assert.Equal(0, submitted);
        model.Name = "Equipe";
        await form.InvokeAsync(() => form.Find("form").Submit());
        Assert.True(form.Instance.IsValid);
        Assert.Equal(1, submitted);
    }

    [Fact]
    public async Task TypedRequiredRulesParticipateInImperativeValidationAndStayScoped()
    {
        using var context = new BunitContext();
        string value = "";
        RenderFragment Content() => builder =>
        {
            builder.OpenComponent<SUIValidatedField<string>>(0);
            builder.AddAttribute(1, "Value", value);
            builder.AddAttribute(2, "Required", true);
            builder.AddAttribute(3, "ChildContent", (RenderFragment<string?>)(error => child => child.AddContent(0, error)));
            builder.CloseComponent();
        };
        var first = context.Render<SUIForm>(p => p.Add(c => c.ChildContent, Content()));
        var second = context.Render<SUIForm>();
        await first.InvokeAsync(async () => Assert.False(await first.Instance.ValidateAsync()));
        Assert.Contains("Preencha este campo", first.Markup);
        await second.InvokeAsync(async () => Assert.True(await second.Instance.ValidateAsync()));
        value = "Equipe";
        first.Render(p => p.Add(c => c.ChildContent, Content()));
        await first.InvokeAsync(async () => Assert.True(await first.Instance.ValidateAsync()));
        Assert.DoesNotContain("Preencha este campo", first.Markup);
    }

    [Fact]
    public async Task AsyncRulesCompleteWithoutSelfCancellingOrRevalidatingInARenderLoop()
    {
        using var context = new BunitContext();
        var calls = 0;
        Func<string?, CancellationToken, Task<string?>> rule = async (_, token) =>
        {
            calls++;
            await Task.Delay(5, token);
            return null;
        };
        RenderFragment content = builder =>
        {
            builder.OpenComponent<SUIValidatedField<string>>(0);
            builder.AddAttribute(1, "Value", "Equipe");
            builder.AddAttribute(2, "AsyncValidator", rule);
            builder.CloseComponent();
        };
        var form = context.Render<SUIForm>(p => p.Add(c => c.ChildContent, content));
        form.WaitForAssertion(() => Assert.True(form.Instance.IsValid));
        await form.InvokeAsync(async () => Assert.True(await form.Instance.ValidateAsync()));
        Assert.Equal(2, calls);
    }

    [Fact]
    public void DisposalCancelsAsynchronousFieldValidation()
    {
        using var context = new BunitContext();
        CancellationToken observed = default;
        var field = context.Render<SUIValidatedField<string>>(p => p.Add(c => c.Value, "Equipe")
            .Add(c => c.AsyncValidator, async (_, token) =>
            {
                observed = token;
                await Task.Delay(Timeout.Infinite, token);
                return null;
            }));
        Assert.True(observed.CanBeCanceled);
        field.Instance.Dispose();
        Assert.True(observed.IsCancellationRequested);
    }
    [Fact]
    public async Task AnnotationOnlyFieldsPublishChangesAndDetachTheOldContext()
    {
        using var context = new BunitContext();
        var model = new Model();
        var edit = new Microsoft.AspNetCore.Components.Forms.EditContext(model);
        var form = context.Render<SUIForm>(p => p.Add(c => c.EditContext, edit));
        await form.InvokeAsync(() => form.Instance.ValidateAsync());
        Assert.False(form.Instance.IsValid);
        model.Name = "Changed";
        await form.InvokeAsync(() => edit.NotifyFieldChanged(new(model, nameof(Model.Name))));
        form.WaitForAssertion(() => { Assert.True(form.Instance.IsTouched); Assert.True(form.Instance.IsValid); });
        var nextModel = new Model();
        var next = new Microsoft.AspNetCore.Components.Forms.EditContext(nextModel);
        form.Render(p => p.Add(c => c.EditContext, next));
        await form.InvokeAsync(() => edit.NotifyFieldChanged(new(model, nameof(Model.Name))));
        Assert.Same(next, form.Instance.Context);
        Assert.False(form.Instance.IsTouched);
    }

    [Fact]
    public async Task DoubleSubmitWhileTheOwnerIsSavingDoesNotRepeatTheAction()
    {
        using var context = new BunitContext();
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var saves = 0;
        var form = context.Render<SUIForm>(p => p.Add(c => c.OnValidSubmit,
            async _ => { saves++; await pending.Task; }));
        var first = form.InvokeAsync(() => form.Find("form").Submit());
        await form.InvokeAsync(() => form.Find("form").Submit());
        Assert.Equal(1, saves);
        pending.SetResult();
        await first;
    }

}
