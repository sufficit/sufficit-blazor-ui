# Consolidação de design — P1/P2: SUIComponentBase e splatting canônico

**Data:** 2026-10-04  
**Branch:** `work/design-consolidation`  
**Escopo:** classe base fina; migração piloto de 8 componentes (SUILink, SUICheckbox, SUISection, SUIPagination, SUIStat, SUIProgressSteps, SUISelect<T>, SUITextField<T>); catálogo/baseline/testes.

## Problema

`Class`, `Style` e o dicionário de atributos eram redeclarados por dezenas de componentes. `UserAttributes` coexistia com `AdditionalAttributes`. A classe base não deve decidir em qual elemento splattar, pois um campo passa os atributos ao `<input>`, enquanto um link os aplica ao `<a>`.

## Implementado

- `SUIComponentBase : ComponentBase` (público, sem markup):
  - `[Parameter] Class`, `[Parameter] Style`, `[Parameter(CaptureUnmatchedValues = true)] AdditionalAttributes`;
  - `EffectiveClass(owned)` para compor classes sem concatenação manual;
  - `MergeLegacyAttributes(legacy)` para compatibilidade BL0007: parâmetros devem ser auto-properties, logo o alias antigo não pode encaminhar no setter. O merge é feito após captura de unmatched por Blazor, sem mutar dicionários do consumidor; o canônico prevalece em colisões.
- Pilotos abrangem actions, forms, layout, navigation e data display; `SUISelect<T>` e `SUITextField<T>` comprovam que a base funciona com `@typeparam T` e code-behind parcial.
- `SUILink`, `SUISelect<T>` e `SUITextField<T>` preservam `UserAttributes` como alias `[Parameter, Obsolete]`; passam `@attributes` herdados para o mesmo elemento que antes recebia o splat. `Style` herdado segue para a âncora/raiz. O restante da migração de aliases fica para uma próxima revisão antes da janela de quebra.
- `NamingConventionTests.LegacyUserAttributes_DoesNotGrowBeyondTheFrozenList` congela os 17 componentes antigos para que novos adotem `AdditionalAttributes`.
- `scripts/generate-catalog.py` agora inclui parâmetros herdados ao processar componentes com `@inherits SUIComponentBase`; `catalog.json` gerado atualizado.
- `eng/PublicApiBaseline.txt` regenerado com revisão: declarações dos parâmetros passam da classe concreta para a base (permanecem acessíveis via herança), e o tipo base direto muda de `ComponentBase` para `SUIComponentBase` nos oito pilotos.

## Obstáculos e decisões

- Alias com setter manual falhou com **BL0007** (Blazor exige auto-property em `[Parameter]`): corrigido usando `OnParametersSet` + helper de merge protegido na base.
- A migração disparou falha de catálogo porque o gerador lia somente declarações diretas: corrigido para ler a base, sem edição manual do JSON.
- Baseline de API detectou mudanças deliberadas de herança; regenerado pelo mecanismo do projeto (`SUI_UPDATE_PUBLIC_API=1`) após revisão do diff.

## Validação

- `python3 scripts/generate-catalog.py --check`: passou (73 componentes).
- `node scripts/build-css.mjs --check`: passou (CSS não alterado nesta etapa; raw=58993, gzip=10848, brotli=9476).
- `dotnet test tests/Sufficit.Blazor.UI.Tests`: 924 passaram, 0 falhas.
- `dotnet build` de BrowserTests e Demos: 0 erros/avisos.

## Próximo

P4: sobrecargas `SUITone` em snackbar/toast, normalização única; depois F1/F4/L4. A migração dos demais componentes para a base deve ser progressiva, com atenção ao destino do splat e ao baseline.
