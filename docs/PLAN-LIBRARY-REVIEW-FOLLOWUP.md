# Plano — acompanhamento da avaliação de 2026-09-11

Itens do roadmap de [EVALUATION-LIBRARY-REVIEW-20260911.md](EVALUATION-LIBRARY-REVIEW-20260911.md)
ainda abertos. Os itens P1.6, P1.7, P2.9, P2.10, P2.11, P3.12, P3.13 e P3.14
foram entregues em 2026-09-11 e P4.15 (limpeza da API) em 2026-09-12 (ver
`activities/202609120330-completed-api-cleanup.md`); saíram desta lista.

| # | Item | Estado |
| --- | --- | --- |
| P0.2 | Deslistar `1.27.0`, `1.28.0`, `2.0.0`, `2.1.1`, `2.2.1` no NuGet | bloqueado: exige `NUGET_API_KEY` com escopo *Unlist*; rodar `maintenance.yml` com a última versão `2.yy.MMdd.HHmm` publicada em `replacement` e `apply=true` |
| P3.12b | Escrever `/// <summary>` em todos os membros públicos e retirar `CS1591` do `NoWarn` | pendente: 320 membros em 31 tipos (medido com `-p:NoWarn=` em 2026-09-11); os maiores são `SUINavGroup` (39), `SUIAutocomplete<T>` (32), `SUITypography` (29), `SUIDateField` (24), `SUILayout` (22), `SUIDrawer` (21). Atenção ao teto de 450 linhas dos `.razor.cs`: `SUIDateField.razor.cs` (448) e `SUINavGroup.razor.cs` (409) precisam ser divididos antes |
