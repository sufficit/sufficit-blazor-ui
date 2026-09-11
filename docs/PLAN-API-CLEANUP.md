# Plano — limpeza da API pública (antigo "v2")

> Este plano se chamava `PLAN-SUI-V2.md`. O nome mudou em 2026-09-11 porque a
> série SemVer `2.0.0`–`2.2.1` foi publicada por engano em agosto com as pontes
> ainda presentes, e a linha corporativa agora é `2.yy.MMdd.HHmm`. "Limpeza da
> API" é a release calendário em que as pontes saem; ela não tem número
> reservado. Só itens pendentes ficam aqui; o histórico está em
> `docs/activities/`.

**Status:** planejado; nenhuma quebra aplicada  
**Pré-requisito:** todos os consumidores conhecidos compilando com
`Version="2.*"` e sem `CS0618` da SUI (ver [checklist](#checklist-de-migração-de-consumer))

## Remover pontes visuais obsoletas

| Componentes | Remover | Usar antes da migração |
| --- | --- | --- |
| `SUIButton` | `Color`, `IconColor`, `Variant`, `Size`, `IconSize`, `ButtonType` | `ColorValue`, `IconColorValue`, `VariantValue`, `SizeValue`, `IconSizeValue`, `ButtonTypeValue` |
| `SUIIconButton` | `Color`, `Size`, `Variant`, `Edge`, `ButtonType` | equivalentes terminados em `Value` |
| `SUILoadingButton` | `Color`, `Size`, `Variant`, `ButtonType` | equivalentes terminados em `Value` |
| `SUIChip`, `SUITimelineItem` | `Color`, `Size`, `Variant` | equivalentes terminados em `Value` |
| `SUIProgressLinear`, `SUISwitch`, `SUICheckbox` | `Color` | `ColorValue` |
| `SUIAlert` | `Severity` string | `ToneValue` |
| `SUIStatusBadge` | `Tone` string | `ToneValue` |

Total: 23 pontes `object` (contadas em `PublicApiCompatibilityTests`) e duas
pontes `string`. Não remover `SUISelectItem.Value`: o item não genérico
registra o valor no `SUISelect<T>` pai, que faz a comparação tipada.

As mensagens `[Obsolete]` apontam para este arquivo; ao remover uma ponte,
remover também a linha correspondente da lista em
`PublicApiCompatibilityTests.VisualObjectBridges_AreDeprecatedAndHaveTypedReplacements`
no mesmo commit.

## Renomes bloqueados pela compatibilidade

Cada item é uma violação real de convenção, congelada em lista de débito nos
testes (`NamingConventionTests.PrefixExemptTypes` e `LegacyParameterNames`).
Renomear quebra call sites Razor, então só entra junto com a remoção das pontes.

| Atual | Alvo | Motivo |
| --- | --- | --- |
| `NavAccordionScope` | `SUINavAccordionScope` | tipo público sem o prefixo da biblioteca |
| `SUIItem.xs/sm/md/lg/xl` | `Xs/Sm/Md/Lg/Xl` | parâmetros Blazor são PascalCase |
| `SUIAlert.CloseIconClicked` | `OnClose` | callbacks são `OnXxx` ou `XxxChanged` |

Ao aplicar, remover a entrada da lista de débito no mesmo commit: a lista só
pode encolher.

## Checklist de migração de consumer

1. Referenciar `Version="2.*"` (feito em todos os consumidores conhecidos em
   2026-09-11).
2. Compilar com warnings como erro e substituir todos os `CS0618` SUI.
3. Procurar no Razor os parâmetros da tabela e converter valores legados/string
   para enums SUI.
4. Executar testes, smoke de assets e auditoria geométrica de forms.
5. Instalar o pacote candidato, repetir os gates e registrar diferenças.

Pesquisas de apoio, sempre revisadas em contexto para evitar falsos positivos:

```bash
rg -n '<SUI(Button|IconButton|LoadingButton|Chip|TimelineItem|ProgressLinear|Switch|Checkbox|Alert|StatusBadge)\b' . --glob '*.razor'
rg -n '\b(Color|IconColor|Variant|Size|IconSize|ButtonType|Edge|Severity|Tone)=' . --glob '*.razor'
```

## Gates

- [ ] nenhuma ponte da tabela permanece na API pública;
- [ ] `eng/PublicApiBaseline.txt` atualizado com review das diferenças;
- [ ] Chromium, Firefox e WebKit verdes;
- [ ] pacote instalado e executado em raiz e `PathBase`;
- [ ] CSS global e isolation preservam seus URLs públicos;
- [ ] todos os consumers conhecidos compilam sem `CS0618` e sem erro;
- [ ] changelog contém guia de breaking changes e a última versão com pontes
  para rollback.

## Fora de escopo

- renomear o prefixo `SUI`;
- transformar a biblioteca em Material Design;
- incorporar identidade visual de um consumer no tema default;
- remover `sufficit-ui.css` ou exigir scripts globais;
- alterar fields controlados para herdar `InputBase<T>`.
