# Feedback

Inclui Alert, EmptyState, ProgressLinear, Skeleton, Snackbar, StatusBanner,
StatusBadge e Toast.

**Quando usar Alert é uma decisão de hierarquia, não de estilo.** Um aviso que
só muda a cor do texto não é aviso: ele some na varredura e não existe para quem
não distingue os tons. Promover é dar FORMA — moldura, ícone, isolamento —, e
promover tudo não promove nada. O critério e a escala de leitura estão em
[DESIGN-VISUAL-HIERARCHY.md](../DESIGN-VISUAL-HIERARCHY.md).

Use `ToneValue` (`SUITone`) em Alert/StatusBadge e `ColorValue` em Progress.
As pontes string/object antigas permanecem obsoletas até v2. Alert diferencia
tom por ícone, texto, fundo e borda uniforme — sem faixa lateral. Progress usa
`transform: scaleX()` e clampa valores em 0–100. Spinner, toast, skeleton,
snackbar, dialog e progress respeitam `prefers-reduced-motion`.

`SUIProgressCircular` reutiliza as regras do spinner de `SUILoadingButton`, para
os dois girarem igual. A raiz é `role="progressbar"`: em `Indeterminate` omite
`aria-valuenow/min/max`; no modo determinado, `Value` é clampado em 0–100 e
desenha um arco SVG. `LoadingIcon` troca o glifo indeterminado, `SpinDirection`
inverte o sentido, `ColorValue` e `SizeValue` seguem os demais componentes.
`AriaLabel` é obrigatório quando o spinner é a única indicação de atividade.

`SUISkeletonLoader` renderiza placeholders por `Type` (`SUISkeletonType.Text`
padrão, Card, Circle ou Table); `Rows` (padrão 3) vale para Table e `Size`
(padrão 48px) para Circle. São apenas `div`s, sem atributos ARIA próprios.

`SUIPendingChangesBar` é uma `section` com `role="region"` e `AriaLabel`
(padrão "Alterações pendentes"); `Title` e `Description` ficam em
`role="status"` com `aria-live="polite"`. O botão de `OnSave` (`SaveText`) é
fixo; o de `OnCancel` (`CancelText`) só aparece quando há delegate. `Disabled`
desabilita os dois.

`SUISnackbarHost` deve existir uma única vez na raiz interativa, com
`AddSufficitUI()` registrado. Ele assina `ISUISnackbar.OnEnqueue`: `Add`,
`Info`, `Success`, `Warning` e `Error` (severidade `danger`) entram numa região
`aria-live="polite"`/`aria-atomic`, cada entrada como `role="status"` com botão
"Fechar". Guarda no máximo cinco entradas, descartando a mais antiga, e remove
cada uma ao vencer `durationMs` (padrão 4000).
