using Bunit;
using Sufficit.Blazor.UI.Components;
namespace Sufficit.Blazor.UI.Tests;
public sealed class WrappedChipTests
{
    [Fact]
    public void WrappingPreservesTheCompleteOperandAndIndependentActions()
    {
        using var context=new BunitContext();context.JSInterop.Mode=JSRuntimeMode.Loose;
        var edits=0;var removes=0;const string operand="Período: 17/11/2026 00:00 até 17/11/2026 23:59 (incluindo as datas)";
        var cut=context.Render<SUIChip>(p=>p.Add(c=>c.Wrap,true).Add(c=>c.ChildContent,operand).Add(c=>c.OnClick,()=>edits++).Add(c=>c.OnRemove,()=>removes++));
        Assert.Equal(operand,cut.Find(".sui-chip__label").TextContent);cut.Find(".sui-chip__action").Click();Assert.Equal(1,edits);Assert.Equal(0,removes);
        cut.Find(".sui-chip__remove").Click();Assert.Equal(1,removes);Assert.Equal(1,edits);Assert.Contains("sui-chip--wrap",cut.Find("span.sui-chip").ClassList);
        cut.Render(p=>p.Add(c=>c.Wrap,false));Assert.DoesNotContain("sui-chip--wrap",cut.Find("span.sui-chip").ClassList);
    }
}
