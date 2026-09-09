using Microsoft.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests
{
    [TestCase(1280)]
    [TestCase(390)]
    public async Task ThemeNumbersApplyTypingArrowsAndWheelWithoutBlur(int width)
    {
        await Page.SetViewportSizeAsync(width, 1000);
        await Page.GotoAsync(BaseUrl + "?view=themes");
        var field = Page.GetByLabel("Unidade de espaçamento (px)", new() { Exact = true });
        await field.FillAsync("5");
        await Expect(field).ToBeFocusedAsync();
        await Expect(Page.Locator(".sui-root")).ToHaveCSSAsync("--sui-space-1", "5px");
        await field.PressAsync("ArrowUp");
        await Expect(field).ToHaveValueAsync("6");
        await Expect(Page.Locator(".sui-root")).ToHaveCSSAsync("--sui-space-1", "6px");
        await field.HoverAsync();
        await Page.Mouse.WheelAsync(0, 100);
        await Expect(field).ToHaveValueAsync("5");
        await Expect(Page.Locator(".sui-root")).ToHaveCSSAsync("--sui-space-1", "5px");
        await field.FillAsync("6");
        await Page.Mouse.WheelAsync(0, -100);
        await Expect(field).ToHaveValueAsync("6");
        var font = Page.GetByLabel("Tamanho do texto (px)", new() { Exact = true });
        await font.FillAsync("");
        await font.PressSequentiallyAsync("18");
        await Expect(font).ToHaveValueAsync("18");
        await Expect(Page.Locator(".sui-root")).ToHaveCSSAsync("--sui-fs-button", "16px");
        await Page.ReloadAsync();
        await Expect(field).ToHaveValueAsync("6");
        await Expect(font).ToHaveValueAsync("18");
    }

    [Test]
    public async Task NumericWheelRespectsNativeStepFocusModifiersAndDisconnect()
    {
        var results = await Page.EvaluateAsync<bool[]>(
            """
            async () => {
                const module = await import(new URL('_content/Sufficit.Blazor.UI/Components/Forms/SUINumericField.razor.js', document.baseURI));
                const input = document.createElement('input');
                input.type='number'; input.min='0'; input.max='1'; input.step='0.25'; input.value='0.5';
                document.body.append(input); module.configure(input,true); module.configure(input,true);
                let events=0; input.addEventListener('input',()=>events++);
                const wheel=(options={})=>input.dispatchEvent(new WheelEvent('wheel',{deltaY:-100,cancelable:true,...options}));
                const ok=[];
                try {
                    ok.push(wheel() && input.value==='0.5');
                    input.focus(); ok.push(!wheel() && input.value==='0.75' && events===1);
                    wheel(); wheel(); ok.push(input.value==='1' && events===2);
                    wheel({deltaY:100}); ok.push(input.value==='0.75');
                    ok.push(wheel({ctrlKey:true}) && input.value==='0.75');
                    ok.push(wheel({metaKey:true}) && input.value==='0.75');
                    input.readOnly=true; ok.push(wheel() && input.value==='0.75'); input.readOnly=false;
                    input.disabled=true; ok.push(wheel() && input.value==='0.75'); input.disabled=false; input.focus();
                    input.step='any'; ok.push(wheel() && input.value==='0.75'); input.step='0.25';
                    input.value='0'; wheel({deltaY:100}); ok.push(input.value==='0');
                    module.configure(input,false); ok.push(wheel() && input.value==='0');
                    module.configure(input,true); module.disconnect(input); ok.push(wheel() && input.value==='0');
                    return ok;
                } finally { module.disconnect(input); input.remove(); }
            }
            """);
        Assert.That(results, Has.Length.EqualTo(12));
        Assert.That(results, Is.All.True);
    }
}
