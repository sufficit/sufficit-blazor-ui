using Bunit;
using Sufficit.Blazor.UI.Components;
namespace Sufficit.Blazor.UI.Tests;
public sealed class RecordManagementTests {
 [Fact] public void Mobile_item_contains_only_title_and_secondary_and_accessible_state() {
  using var c=new BunitContext();c.JSInterop.Mode=JSRuntimeMode.Loose;
  var cut=c.Render<SUIRecordTable<string>>(p=>p.Add(x=>x.Items,new[]{"Greeting"})
   .Add(x=>x.TitleSelector,x=>x).Add(x=>x.SecondarySelector,x=>"Voice")
   .Add(x=>x.ActiveSelector,x=>false).Add(x=>x.KeySelector,x=>x)
   .Add(x=>x.StatusTemplate,x=>b=>b.AddContent(0,"Desativado"))
   .Add(x=>x.Actions,x=>b=>b.AddContent(0,"Editar")));
  var trigger=cut.Find(".sui-records__mobile button");
  Assert.Equal("GreetingVoice",trigger.TextContent);
  Assert.Contains("desativado",trigger.GetAttribute("aria-label"));
  Assert.NotNull(trigger.GetAttribute("popovertarget"));
  Assert.Equal("Ações",cut.Find("th.sui-records__actions-heading").TextContent);
 }
}
