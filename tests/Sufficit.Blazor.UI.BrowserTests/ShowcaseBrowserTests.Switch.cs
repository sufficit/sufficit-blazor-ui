using Microsoft.Playwright;

namespace Sufficit.Blazor.UI.BrowserTests;

public sealed partial class ShowcaseBrowserTests
{
    [Test]
    public async Task SwitchHelpKeepsItsNameAndKeyboardBehavior()
    {
        await Page.GotoAsync(BaseUrl + "?component=SUISwitch");
        var input = Page.GetByRole(AriaRole.Checkbox, new() { Name = "Ativar notificações", Exact = true });
        await Expect(input).ToBeCheckedAsync();
        await Expect(input).ToHaveAccessibleDescriptionAsync("Receber avisos sobre operações e alterações de estado.");
        await input.PressAsync("Space");
        await Expect(input).Not.ToBeCheckedAsync();
        await input.PressAsync("Space");
        await Expect(input).ToBeCheckedAsync();
    }
}
