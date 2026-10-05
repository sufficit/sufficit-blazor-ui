# Consolidação de design — F3: decisão da rampa tipográfica

**Data:** 2026-10-05
**Branch:** `main`

## Escopo

Item 9 do roadmap (F3): `SUITypo` tinha duas famílias — Material (`h1`–`h6`,
`body1/2`, …) e semântica (`display`/`headline`/`title`/`body`/`label`/`mono`) —
e a avaliação pedia uma decisão explícita em vez de manter as duas "só por
garantia". O item era de produto, não de engenharia: a entrega é a decisão, a
orientação ao consumidor e um contrato que impede a decisão de se desfazer.

## Evidência re-verificada antes de decidir

- A avaliação dizia que nenhum componente usa os 6 membros semânticos. Confirmado:
  `SUIDrawer`, `SUIPagination`, `SUIStat` e `SUITableEmpty` usam só a família
  Material; os tokens semânticos têm consumidores CSS (`sui-shared-text.css` e
  `SUIStatusBanner` via `--sui-fs-headline`).
- A família semântica tem suporte completo de tema (`SUITypography.Fs*` e
  `LineHeight*`, publicados por `SUIThemeCssWriter`); a Material tem só os
  tamanhos.

## Decisão

- **Direção:** papéis semânticos para telas operacionais novas — são
  tematizáveis por papel e o tamanho segue a função, não o nível do documento.
- **Material:** suportada e **não** `[Obsolete]` antes da próxima major. Os
  componentes internos e consumidores publicados dependem dela; marcar agora
  seria quebra de fonte sem janela. Alinhado à janela única de quebra que já
  remove as pontes `[Obsolete]` da migração para `SUIComponentBase`.
- **Opção (b) da avaliação (remover os 6 membros) descartada:** eles têm
  suporte de tema completo e consumo em CSS; removê-los jogaria fora trabalho
  funcional sem ganho.
- **Não são intercambiáveis:** tamanhos diferem, só `h1`–`h6` viram heading
  nativo sob `SUITextTag.Auto` (os semânticos renderizam `div`; para título de
  página, `Tag` explícita) e pesos/alturas de linha divergem. O documento traz a
  tabela de vizinho mais próximo, com as linhas sem equivalente marcadas.
- **Migração dos defaults internos adiada:** mudar o tamanho de `SUIPagination`
  ou `SUIStat` altera a aparência de todos os consumidores; fica para a major.

## Entregas

- `docs/DESIGN-TYPOGRAPHY-RAMP.md` (decisão, mapeamento, regras para código
  novo) e entrada no índice `docs/README.md`.
- XML docs do `SUITypo`: cabeçalho do enum com a decisão e cada papel
  semântico com token, peso e degrau Material mais próximo.
- `TypographyRampContractTests` (5 contratos): todo membro tem regra
  `.sui-text--*`; todo papel tem token de tamanho/altura, propriedade em
  `SUITypography` e escrita no tema; nenhum membro `[Obsolete]`; os semânticos
  não renderizam heading sob `Auto`; o documento cita os seis papéis.
- Nenhuma mudança de comportamento, CSS ou API pública: baseline de API e
  orçamentos de CSS intactos.

## Validação

- Suíte unitária 949/949.
