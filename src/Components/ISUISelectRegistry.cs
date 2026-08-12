namespace Sufficit.Blazor.UI.Components;

internal interface ISUISelectRegistry
{
    void Register(SUISelectItem item);

    void Unregister(SUISelectItem item);
}
