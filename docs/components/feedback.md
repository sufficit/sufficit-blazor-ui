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
