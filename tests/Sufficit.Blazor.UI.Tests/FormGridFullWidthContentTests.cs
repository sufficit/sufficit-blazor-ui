using Bunit;
using Microsoft.AspNetCore.Components;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

public sealed class FormGridFullWidthContentTests
{
    [Fact]
    public void FullWidthFieldKeepsItsValueChangeCallback()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        string? observed = null;
        RenderFragment notes = builder =>
        {
            builder.OpenComponent<SUITextField<string>>(0);
            builder.AddAttribute(1, nameof(SUITextField<string>.Label), "Observações");
            builder.AddAttribute(2, nameof(SUITextField<string>.Multiline), true);
            builder.AddAttribute(3, nameof(SUITextField<string>.ValueChanged),
                EventCallback.Factory.Create<string?>(this, value => observed = value));
            builder.CloseComponent();
        };
        var cut = context.Render<SUIFormGrid>(parameters => parameters.Add(x => x.FullWidthContent, notes));
        cut.Find("textarea").Change("Informação do serviço");
        Assert.Equal("Informação do serviço", observed);
    }
}
