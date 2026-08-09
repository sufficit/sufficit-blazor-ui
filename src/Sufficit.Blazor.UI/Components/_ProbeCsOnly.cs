using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Sufficit.Blazor.UI.Components;

/// <summary>
/// Diagnostic probe: a component of our own defined purely in C#, mirroring how
/// MudElement is declared. If this fails to resolve too, the problem is not the
/// vendored tree but how this project discovers C#-only components.
/// </summary>
public class ProbeCsOnly : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
        => builder.AddContent(0, "probe");
}
