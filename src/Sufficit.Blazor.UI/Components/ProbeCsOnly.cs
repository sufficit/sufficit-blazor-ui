using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Sufficit.Blazor.UI.Components;

/// <summary>Probe: a C#-only component of ours, mirroring how MudElement is declared.</summary>
public class ProbeCsOnly : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
        => builder.AddContent(0, "probe");
}
