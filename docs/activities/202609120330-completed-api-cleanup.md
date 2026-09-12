# Atividade concluída — limpeza da API pública (antigo plano "v2")

Concluída em 2026-09-12. Plano original: `PLAN-SUI-V2.md` (agosto/2026),
renomeado para `PLAN-API-CLEANUP.md` em 2026-09-11 e arquivado aqui.

## Entregue

| Item | Release | Consumidores tocados |
| --- | --- | --- |
| 26 pontes `object`/`string` removidas (`Color`, `IconColor`, `Variant`, `Size`, `IconSize`, `ButtonType`, `Edge`, `Severity`, `Tone`) | `v2.26.911.2356` | Background e Network Control migraram para `*Value`/`ToneValue` (com `ToneMapping.Parse` para strings da API); os demais já não usavam |
| `NavAccordionScope` → `SUINavAccordionScope` | `v2.26.911.2356` | nenhum |
| `SUIAlert.OnClose` (com `CloseIconClicked` obsoleto encaminhando) | `v2.26.911.2356` | Fleet, AI, Services Run, Blazor, Cloud Mobile (main e ramo google-accounts) |
| `SUIItem.xs/sm/md/lg/xl` → `Xs/Sm/Md/Lg/Xl` (rename duro) | `v2.26.912.328` | Blazor (4 arquivos), AI (3), Cloud Mobile main (12) e ramo google-accounts (11), samples (4) |
| `CloseIconClicked` removido | release seguinte a `v2.26.912.328` | nenhum (todos já em `OnClose`) |

Última versão com as pontes: `2.26.911.2323`. Última com `SUIItem.xs`
minúsculo e com `CloseIconClicked`: `2.26.911.2356`.

## Por que o rename do `SUIItem` não teve transição

O Razor casa atributos de componente sem distinguir maiúsculas; `xs` obsoleto
ao lado de `Xs` tornaria toda chamada ambígua. O rename foi feito em lockstep
nos consumidores no mesmo dia, cada um compilado contra o fonte novo antes do
commit.

## Listas de débito

`NamingConventionTests.LegacyParameterNames` ficou vazia e
`PrefixExemptTypes` perdeu `NavAccordionScope`. `PublicApiCompatibilityTests`
agora afirma que as pontes **não** existem.

## Fora de escopo (permanece)

- renomear o prefixo `SUI`;
- transformar a biblioteca em Material Design;
- incorporar identidade visual de um consumer no tema default;
- remover `sufficit-ui.css` ou exigir scripts globais;
- alterar fields controlados para herdar `InputBase<T>`.
