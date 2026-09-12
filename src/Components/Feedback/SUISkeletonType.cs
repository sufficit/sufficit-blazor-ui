namespace Sufficit.Blazor.UI.Components;

/// <summary>Placeholder shape rendered by <c>SUISkeletonLoader</c> while content is loading.</summary>
public enum SUISkeletonType
{
    /// <summary>One card-sized block (<c>sui-skeleton-card</c>).</summary>
    Card,
    /// <summary>One text line (<c>sui-skeleton-text</c>); the loader default.</summary>
    Text,
    /// <summary>Circle sized by the loader's <c>Size</c> parameter (48px default), for avatars.</summary>
    Circle,
    /// <summary>Stack of text lines, one per <c>Rows</c> (default 3), imitating table rows.</summary>
    Table
}
