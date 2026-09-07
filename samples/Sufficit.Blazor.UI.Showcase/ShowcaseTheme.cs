using Sufficit.Blazor.UI.Themes;

namespace Sufficit.Blazor.UI.Showcase;

public sealed record ThemePreference(string Mode, string Brand, string Density, bool Dark)
{
    public SUITheme Build()
    {
        var preset = Dark ? SUITheme.Dark : SUITheme.Light;
        var accent = Brand switch
        {
            "red" => Dark ? "#fca5a5" : "#b91c1c",
            "blue" => Dark ? "#93c5fd" : "#2563eb",
            _ => Dark ? "#fb923c" : "#c2410c",
        };
        return preset with
        {
            Palette = preset.Palette with
            {
                Primary = accent,
                PrimaryContrast = Dark ? "#0f172a" : "#ffffff",
                PrimaryAction = Brand == "amber" ? "#b7440e" : accent,
                PrimaryActionContrast = Brand == "amber" ? "#fff7ed" : Dark ? "#0f172a" : "#ffffff",
            },
            Layout = preset.Layout with { ControlHMd = Density == "compact" ? "32px" : "40px" },
        };
    }
}
