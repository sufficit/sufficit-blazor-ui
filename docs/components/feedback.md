# Feedback

Inclui Alert, EmptyState, ProgressLinear, Skeleton, Snackbar, StatusBanner,
StatusBadge e Toast.

Use `ToneValue` (`SUITone`) em Alert/StatusBadge e `ColorValue` em Progress.
As pontes string/object antigas permanecem obsoletas até v2. Alert diferencia
tom por ícone, texto, fundo e borda uniforme — sem faixa lateral. Progress usa
`transform: scaleX()` e clampa valores em 0–100. Spinner, toast, skeleton,
snackbar, dialog e progress respeitam `prefers-reduced-motion`.
