# Marca e tema — Sufficit Blazor

Referência de tokens committed e do contrato de tema SUI. Carregue ao projetar ou auditar uma UI Sufficit.

## Índice
- Biblioteca é brand-agnóstica
- Contrato de tema (`ISUITheme`)
- Cor primária Sufficit (âmbar)
- Modelo de cor: hex + `color-mix` (NÃO OKLCH)
- Neutros e surfaces
- Raio e layout
- Tipografia (armadilha conhecida)
- Dark mode (bomba latente)
- Arquivos-chave por projeto

## Biblioteca é brand-agnóstica

O SUI **não impõe marca**. Cada app consumidora implementa `ISUITheme` e registra no DI. Identidades ativas:

| Consumer | Primária | Notas |
|---|---|---|
| `sufficit-blazor` (app principal, MudBlazor 9.8) | `#ee6321` (âmbar) | tema authoritative em `MudThemeContainer.razor` |
| `sufficit-cloud-mobile` (SUI, puro) | `#ee6321` (âmbar) | `CloudMobileSUITheme` |
| `Sufficit.Identity.UI.Management` | `#cc0000` (vermelho) | `IdentitySUITheme` |

**Identity-preservation vence.** Se o projeto já tem cor committed, use-a; não rode gerador de paleta.

## Contrato de tema (`ISUITheme`)

Namespace `Sufficit.Blazor.UI.Themes`. Implemente e registre:

```csharp
// Program.cs do consumer
builder.Services.AddSufficitUI(opts => opts.Theme = new CloudMobileSUITheme());
```

```razor
<SUIThemeProvider>
    <Routes />
</SUIThemeProvider>
```

O `SUIThemeProvider` injeta um `<style>` com todas as variáveis `--sui-*` em `:root` a partir do tema ativo. Sem provider, cai em `DefaultSUITheme` (azul `#2563eb`, claro). Composição: `SUIPalette`, `SUITypografia`, `SUILayout` (records).

> Casing real no código: `ISUITheme`, `SUIThemeProvider`, `SUIPalette`, `SUITypography`, `SUILayout`, `DefaultSUITheme`. (O README do library às vezes usa `ISuiTheme`/`SuiThemeProvider`; o código é o `SUI*`.)

Exemplo (`CloudMobileSUITheme.cs`):

```csharp
public sealed class CloudMobileSUITheme : ISUITheme
{
    public SUIPalette Palette { get; } = new()
    {
        Primary = "#ee6321",
        PrimaryContrast = "#ffffff",
        PrimarySoft = "color-mix(in srgb, #ee6321 14%, transparent)",
        Secondary = "#475569",
        Info = "#0284c7", Success = "#15803d", Warning = "#b45309", Error = "#b91c1c",
        Surface = "#ffffff", Surface2 = "#f8fafc", Surface3 = "#e2e8f0",
        TextPrimary = "#111827", TextSecondary = "#475569", TextDisabled = "#94a3b8",
        Border = "#e2e8f0", BorderStrong = "#cbd5e1",
        Overlay = "rgba(15, 23, 42, .45)"
    };
    public SUITypography Typography { get; } = new()
    {
        FontFamily = "\"Open Sans\", -apple-system, BlinkMacSystemFont, \"Segoe UI\", sans-serif",
        FontFamilyMono = "ui-monospace, SFMono-Regular, Menlo, Consolas, monospace"
    };
    public SUILayout Layout { get; } = SUILayout.Default with { Radius = "10px", RadiusLg = "14px", /* shadows */ };
    public bool IsDark => false;
}
```

## Cor primária Sufficit (âmbar)

`#ee6321` — "âmbar Sufficit". Token canonical `--sufficit-amber`. O SUI lê via `--sui-color-primary: var(--sufficit-amber, #ee6321)`.

Rampa (brand CSS):

```css
--sufficit-amber: #ee6321;
--sufficit-amber-hover: #d1530e;
--sufficit-amber-light: color-mix(in srgb, #ee6321 10%, white);
--sufficit-amber-subtle: color-mix(in srgb, #ee6321 8%, transparent);
```

| Token | Claro | Escuro |
|---|---|---|
| Primary | `#ee6321` | `#f4854a` |
| Primary hover/darken | `#d1530e` | `#d1530e` |
| Primary lighten | `#f58a4b` | `#f69a66` |
| Secondary | `#3a3f45` | `#9aa1a9` |

## Modelo de cor: hex + `color-mix` (NÃO OKLCH)

O CSS Sufficit usa **hex + `color-mix(in srgb, …)`** em tudo. **Não há OKLCH.** Ao estender tokens, mantenha o modelo — não introduza `oklch()` só porque regras genêricas de design preferem. Overlays translúcidos: `color-mix(in srgb, var(--sui-color-primary-contrast) 86%, transparent)` em vez de `rgba(255,255,255,.86)` fixo. Matemática de contraste segue valendo (≥4.5:1 corpo).

## Neutros e surfaces

| | Claro | Escuro |
|---|---|---|
| Background/canvas | `#f7f8fa` | `#0e0f12` |
| Surface | `#ffffff` | `#1f2226` |
| Surface-2 / drawer | `#f4f5f7` | `#15171a` |
| Text primary (ink) | `#1f2226` | `#f9fafb` |
| Text secondary | `#475569` | `#9ca3af` |
| Muted / action | `#6b7178` | `#9ca3af` |
| Divider (tintado âmbar) | `#ebdbd5` | `#453933` |

Bordas tintadas de âmbar são intencionais — são quentes, não cinza neutro.

## Raio e layout

- Default `0.75rem` (`MudThemeContainer` / `SUILayout.Radius`).
- Escala CSS: `--radius-sm 4px` · `--radius-md 8px` · `--radius-lg 12px` · `--radius-full 9999px`.
- Drawers (app principal): esquerdo `260px`, mini `60px`, direito `300px`.

## Tipografia (armadilha conhecida)

Três camadas **discordam** — sinalize, não faz de conta:

- **MudTheme (C#, app MudBlazor):** Poppins (headings) + Open Sans (corpo).
- **Brand CSS tokens:** Ubuntu (headings) + Roboto (corpo).
- **Google Fonts de fato carregadas no `App.razor`:** Roboto, Ubuntu, Montserrat.
- **SUI default:** system stack.

⚠️ **Poppins e Open Sans são referenciados pelo MudTheme mas não carregados** — caem pro fallback. Ao tocar tipografia: ou carregue a fonte referenciada no `App.razor`, ou alinhe o tema ao que está carregado. Para títulos, uma opção limpa é `font-family: "Poppins", var(--sui-font)` sobre `h1/h2/h3` (e carregar Poppins). Monospace: JetBrains Mono / Fira Code.

## Dark mode (bomba latente)

- `CloudMobileSUITheme.IsDark => false` (hardcoded). Sem `prefers-color-scheme`. Sem toggle.
- O SUI **tem** dark palette em `sufficit-ui.css` `[data-sui-theme="dark"]`, **mas** o primário escuro é **`#3b82f6` (azul)** — off-brand. Se for ligar dark num consumer âmbar, **sobrescreva o primário** no tema do consumer antes, senão a marca vira azul silenciosamente.

## Arquivos-chave por projeto

- `sufficit-blazor`: `src/Components/Layout/MudThemeContainer.razor` (tema authoritative), `src/wwwroot/assets/css/mudblazor-customize.min.css` (tokens âmbar).
- `sufficit-cloud-mobile`: `src/Sufficit.Cloud.Mobile.Web/CloudMobileSUITheme.cs`, `wwwroot/cloud-mobile.css`.
- `sufficit-blazor-ui` (esta lib): `src/Themes/` (`ISUITheme.cs`, `SUIPalette.cs`, `SUITypography.cs`, `SUILayout.cs`, `SUIThemeProvider.razor`), `src/Components/` (componentes + `SUIEnums.cs`), `src/Utilities/SUIClassBuilder.cs`, `src/wwwroot/sufficit-ui.css`. (Layout achatado em 2026-08: sem o prefixo `Sufficit.Blazor.UI/`.)
