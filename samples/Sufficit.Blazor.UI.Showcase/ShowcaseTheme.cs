using Sufficit.Blazor.UI.Themes;

namespace Sufficit.Blazor.UI.Showcase;

public sealed record ThemePreference(string Mode, string Brand, string Density, bool Dark)
{
    public int FontSize { get; init; } = 16;
    public int SpaceUnit { get; init; } = 4;
    public int Radius { get; init; } = 8;
    public string Font { get; init; } = "system";
    public string? Success { get; init; }
    public string? Error { get; init; }

    private static string Foreground(string background)
    {
        SUIColorContrast.TryGetRatio("#ffffff", background, out var white);
        SUIColorContrast.TryGetRatio("#111827", background, out var dark);
        return white >= dark ? "#ffffff" : "#111827";
    }

    public SUITheme Build()
    {
        var preset = Dark ? SUITheme.Dark : SUITheme.Light;
        var accent = Brand switch
        {
            "red" => Dark ? "#fca5a5" : "#b91c1c",
            "blue" => Dark ? "#93c5fd" : "#2563eb",
            _ => Dark ? "#fb923c" : "#c2410c",
        };
        var size = Math.Clamp(FontSize, 14, 20);
        var space = Math.Clamp(SpaceUnit, 2, 6);
        var success = SUIColorContrast.IsHexColor(Success) ? Success! : preset.Palette.Success;
        var error = SUIColorContrast.IsHexColor(Error) ? Error! : preset.Palette.Error;
        return preset with
        {
            Palette = preset.Palette with
            {
                Success = success, SuccessContrast = Foreground(success),
                Error = error, ErrorContrast = Foreground(error),
                Primary = accent,
                PrimaryContrast = Dark ? "#0f172a" : "#ffffff",
                PrimaryAction = Brand == "amber" ? "#b7440e" : accent,
                PrimaryActionContrast = Brand == "amber" ? "#fff7ed" : Dark ? "#0f172a" : "#ffffff",
            },
            Typography = preset.Typography with
            {
                FontFamily = Font switch { "serif" => "Georgia, serif", "mono" => preset.Typography.FontFamilyMono, _ => preset.Typography.FontFamily },
                FsBody1 = $"{size}px", FsBody2 = $"{size - 2}px", FsBody = $"{size - 2}px", FsButton = $"{size - 2}px"
            },
            Layout = preset.Layout with
            {
                ControlHMd = Density == "compact" ? "32px" : "40px",
                Radius = $"{Math.Clamp(Radius, 0, 16)}px", RadiusLg = $"{Math.Clamp(Radius, 0, 16) + 6}px",
                Space1 = $"{space}px", Space2 = $"{space * 2}px", Space3 = $"{space * 3}px",
                Space4 = $"{space * 4}px", Space5 = $"{space * 6}px", Space6 = $"{space * 8}px"
            },
        };
    }
}
