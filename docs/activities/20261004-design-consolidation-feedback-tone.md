# Consolidação de design — P4: SUITone em snackbar/toast + normalizador único

**Data:** 2026-10-04  
**Branch:** `work/design-consolidation`  
**Escopo:** `ISUISnackbar`, `ISUIToast`, `SUISnackbarService`, `SUIToastService`, novo `SUIToneNormalizer` interno, samples e testes.

## Contexto

Achado **P4** da avaliação: os dois serviços de feedback transitório aceitavam
`string severity` solta ("info"/"success"/"warning"/"danger"), mantinham o
sinônimo histórico `"error"` e **duplicavam** a normalização
`"error"→"danger"` em `SUISnackbarService.cs` e `SUIToastService.cs`. O
restante da biblioteca usa enums tipados (`SUITone` já existia para badges,
banners e alerts) — menos os dois serviços de feedback.

## Mudanças

1. **`SUIToneNormalizer` (novo, `src/Services/SUIToneNormalizer.cs`, interno)**
   — ponto único da regra:
   - `Parse(string?)`: ponte do histórico — `"success"`/`"warning"`/
     `"danger"`/`"error"`→`SUITone` correspondente; neutro, vazio e
     desconhecido → `Info` (as superfícies de feedback **não definem**
     variante neutra em CSS: não existe `sui-snackbar--neutral` nem
     `sui-toast--neutral`; o snackbar tem apenas info/success/warning/danger
     e o toast info/success/warning/error).
   - `Slug(SUITone)`: slug CSS (`info`/`success`/`warning`/`danger`),
     mapeando `Neutral`→`info` pela mesma razão.

2. **Interfaces e serviços** — sobrecarga tipada `Add(message, SUITone tone, …)`
   como caminho preferido; a assinatura `string severity` permanece como
   **ponte DIM default `[Obsolete]`** (compila e delega ao normalizador;
   será removida numa futura major). A implementação dos serviços agora é
   única por serviço, sem normalização duplicada.

3. **Consumidores migrados** — `DemoBase.Alert` (Demos) e testes
   (`RenderContractFeedbackTests`, `CopyToClipboardTests` — o
   `RecordingSnackbar` agora grava `SUITone`). O teste da ponte string foi
   mantido de propósito com `#pragma warning disable CS0618`, pinando que
   `"Error"` continua normalizando para slug `"danger"`.

4. **Contratos guardados** — `eng/PublicApiBaseline.txt` regenerado: as duas
   sobrecargas `Add` (tipada e ponte) aparecem nos dois serviços e nas duas
   interfaces; entry records (`SUISnackbarEntry.Severity`,
   `SUIToastEntry.Severity`) seguem `string` slug para não quebrar os hosts
   nem consumidores que leem entradas.

5. **CHANGELOG** — seção *Changed* do `[Unreleased]` documenta a sobrecarga
   preferida, o comportamento de `Neutral`, a ponte obsoleta e o
   normalizador único.

## Validação

- Biblioteca: `dotnet build` — 0 warnings, 0 erros (warnings como erros).
- Suíte unitária: **926/926** (era 924; +1 teste novo de snackbar
  `Neutral`→slug `info`; +2 líquido inclui o rename da ponte string que
  ganhou caso próprio).
- `Sufficit.Blazor.UI.Demos`, `Sufficit.Blazor.UI.Showcase` e
  `Sufficit.Blazor.UI.BrowserTests`: builds verdes, 0 warnings (nenhum
  consumidor interno usa mais a forma string fora do teste da ponte).

## Não feito (próximos passos)

- **F5** (convergência interna dos hosts em um serviço único + pausa em
  hover/focus, requisito WCAG 2.2.1) ficou fora deste commit — a avaliação
  pede verificação de timer antes, e o P4 já alterava superfície pública;
  separar mantém a janela de revisão pequena.
- P3 (generics `TValue`/`TItem`), F1/F4/L4 seguem pendentes na ordem da
  avaliação.
