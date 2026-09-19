using System.Reflection;
using Bunit;
using Sufficit.Blazor.UI.Components;

namespace Sufficit.Blazor.UI.Tests;

/// <summary>
/// Icon parameters accept a raw SVG fragment and render it as markup, so any
/// application that stores icon names in a table has an HTML sink wherever it
/// binds that column. These tests decide whether such a column is data.
/// </summary>
public sealed class IconInjectionTests
{
    public static TheoryData<string, string> HostileFragments() => new()
    {
        { "script element", "<script>alert(1)</script>" },
        { "script after a shape", "<path d=\"M0 0\"/><script>alert(1)</script>" },
        { "image with error handler", "<image href=\"x\" onerror=\"alert(1)\"/>" },
        { "event handler on a shape", "<path d=\"M0 0\" onload=\"alert(1)\"/>" },
        { "mixed-case event handler", "<path d=\"M0 0\" OnLoad=\"alert(1)\"/>" },
        { "click handler", "<circle cx=\"1\" cy=\"1\" r=\"1\" onclick=\"alert(1)\"/>" },
        { "foreign object", "<foreignObject><body xmlns=\"http://www.w3.org/1999/xhtml\"><script>alert(1)</script></body></foreignObject>" },
        { "style element", "<style>*{display:none}</style>" },
        { "animation element", "<animate attributeName=\"x\" onbegin=\"alert(1)\"/>" },
        { "anchor with script url", "<a href=\"javascript:alert(1)\"><path d=\"M0 0\"/></a>" },
        { "external reference", "<use xlink:href=\"//attacker.test/x.svg#i\"/>" },
        { "data url reference", "<use href=\"data:image/svg+xml;base64,AAAA\"/>" },
        { "url in inline style", "<path d=\"M0 0\" style=\"background:url(//attacker.test/p.png)\"/>" },
        { "comment hiding a payload", "<!-- --><script>alert(1)</script>" },
        { "cdata", "<![CDATA[<script>alert(1)</script>]]>" },
        { "unbalanced element", "<g><path d=\"M0 0\"/>" },
        { "stray closing tag", "<path d=\"M0 0\"/></svg><script>alert(1)</script>" },
        { "text content", "<path d=\"M0 0\"/>plain text" },
        { "angle bracket in an attribute", "<path d=\"M0 0\" class=\"a<script>\"/>" },
    };

    [Theory]
    [MemberData(nameof(HostileFragments))]
    public void HostileFragment_IsRejected(string scenario, string fragment)
    {
        Assert.False(SUIIconMarkup.IsSafe(fragment), scenario);
        Assert.Equal(default, SUIIconMarkup.Render(fragment));
    }

    [Theory]
    [MemberData(nameof(HostileFragments))]
    public void HostileFragment_NeverReachesTheDom(string scenario, string fragment)
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIIcon>(parameters => parameters
            .Add(component => component.Name, fragment));

        var markup = cut.Markup;

        Assert.DoesNotContain("<script", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onerror", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onload", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("javascript:", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("foreignObject", markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("attacker.test", markup, StringComparison.OrdinalIgnoreCase);
        // The icon still renders its own svg wrapper; only the payload is gone.
        Assert.Single(cut.FindAll("svg"));
        Assert.Equal(scenario, scenario);
    }

    [Theory]
    [MemberData(nameof(HostileFragments))]
    public void HostileFragment_IsRejectedInEveryIconSlot(string scenario, string fragment)
    {
        using var context = new BunitContext();

        var button = context.Render<SUIButton>(parameters => parameters
            .Add(component => component.StartIcon, fragment)
            .Add(component => component.EndIcon, fragment));
        var navLink = context.Render<SUINavLink>(parameters => parameters
            .Add(component => component.Title, "x")
            .Add(component => component.Href, "/x")
            .Add(component => component.Icon, fragment));
        var loading = context.Render<SUILoadingButton>(parameters => parameters
            .Add(component => component.IsLoading, true)
            .Add(component => component.LoadingIcon, fragment));

        foreach (var markup in new[] { button.Markup, navLink.Markup, loading.Markup })
        {
            Assert.DoesNotContain("<script", markup, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("onerror", markup, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("attacker.test", markup, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Equal(scenario, scenario);
    }

    [Theory]
    // Shapes and attributes taken from what the library's own constants and the
    // icon set consumers use actually contain. A validator that rejected these
    // would be reverted by the first person who needed an icon.
    [InlineData("<path d=\"M12 2 2 22h20z\"/>")]
    [InlineData("<path d=\"M0 0\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\"/>")]
    [InlineData("<circle cx=\"12\" cy=\"12\" r=\"9\"/>")]
    [InlineData("<ellipse cx=\"12\" cy=\"5\" rx=\"8\" ry=\"3\"/>")]
    [InlineData("<rect x=\"3\" y=\"3\" width=\"7\" height=\"7\" rx=\"1.5\"/>")]
    [InlineData("<line x1=\"0\" y1=\"0\" x2=\"10\" y2=\"10\"/>")]
    [InlineData("<polyline points=\"1,2 3,4\"/>")]
    [InlineData("<polygon points=\"1,2 3,4 5,6\"/>")]
    [InlineData("<g fill-rule=\"evenodd\" clip-rule=\"evenodd\"><path d=\"M0 0\"/></g>")]
    [InlineData("<defs><clipPath id=\"a\"><rect width=\"24\" height=\"24\"/></clipPath></defs><g clip-path=\"url\"/>")]
    [InlineData("<use xlink:href=\"#local\"/>")]
    [InlineData("<use href=\"#local\"/>")]
    [InlineData("<path d=\"M0 0\" opacity=\".5\" fill-opacity=\".25\" transform=\"rotate(45)\"/>")]
    [InlineData("<path d=\"M0 0\" style=\"fill:currentColor\"/>")]
    [InlineData("  <path d=\"M0 0\"/>  ")]
    [InlineData("<path d=\"M0 0\"/>\n<path d=\"M1 1\"/>")]
    public void LegitimateFragment_IsAccepted(string fragment)
    {
        Assert.True(SUIIconMarkup.IsSafe(fragment), fragment);
        Assert.Equal(fragment, SUIIconMarkup.Render(fragment).Value);
    }

    [Fact]
    public void EveryLibraryIconConstant_PassesValidation()
    {
        // The library's own constants are the baseline: if the validator
        // rejected one of these, every component using it would lose its glyph.
        var constants = typeof(SUIIcons)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(string))
            .Select(field => (field.Name, Value: (string)field.GetValue(null)!))
            .ToArray();

        Assert.NotEmpty(constants);
        var rejected = constants.Where(entry => !SUIIconMarkup.IsSafe(entry.Value)).ToArray();

        Assert.True(rejected.Length == 0,
            "SUIIcons constants rejected by the validator: "
            + string.Join(", ", rejected.Select(entry => entry.Name)));
    }

    [Fact]
    public void MalformedFragment_IsRejectedEvenWhenHarmless()
    {
        // Validation is fail-closed: a fragment that does not parse is rejected
        // rather than patched up, because "mostly parses" is exactly the state
        // an injection tries to reach. This costs a real glyph — of the 10,667
        // icon constants in the set consumers use, one (a chess bishop nobody in
        // this codebase references) carries a stray '>' after its self-closing
        // path and is dropped. Losing that glyph is the right trade for not
        // having to reason about how a browser recovers from broken markup.
        const string strayBracket = "<path d=\"M19,22H5V20H19V22Z\"/>>";

        Assert.False(SUIIconMarkup.IsSafe(strayBracket));
    }

    [Fact]
    public void ShortNames_AreNotTreatedAsMarkup()
    {
        // SUIIcon resolves a name through its own switch; only a value that
        // starts a tag takes the markup path.
        Assert.False(SUIIconMarkup.IsMarkup("dashboard"));
        Assert.False(SUIIconMarkup.IsMarkup(null));
        Assert.True(SUIIconMarkup.IsMarkup("<path d=\"M0 0\"/>"));
        Assert.True(SUIIconMarkup.IsMarkup("  <path d=\"M0 0\"/>"));
    }

    [Fact]
    public void NamedIcon_StillRendersItsShapes()
    {
        using var context = new BunitContext();
        var cut = context.Render<SUIIcon>(parameters => parameters
            .Add(component => component.Name, "dashboard"));

        Assert.Equal(4, cut.FindAll("rect").Count);
    }

    [Fact]
    public void OverlongFragment_IsRejected()
    {
        Assert.False(SUIIconMarkup.IsSafe("<path d=\"" + new string('0', 21_000) + "\"/>"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyFragment_IsRejected(string? fragment)
    {
        Assert.False(SUIIconMarkup.IsSafe(fragment));
    }
}
