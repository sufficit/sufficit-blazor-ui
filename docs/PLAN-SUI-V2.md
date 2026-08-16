# Plano — Sufficit.Blazor.UI v2

**Status:** Planejada; sem quebra aplicada na série v1  
**Janela alvo:** preview após validação dos consumers; estável não antes de
2026-11-11  
**Objetivo:** retirar pontes legadas, simplificar o TFM e adicionar integração
opcional com `EditContext` sem mudar o contrato dos fields controlados.

## Escopo obrigatório

### Remover pontes visuais obsoletas

| Componentes | Remover | Usar antes da migração |
| --- | --- | --- |
| `SUIButton` | `Color`, `IconColor`, `Variant`, `Size`, `IconSize`, `ButtonType` | `ColorValue`, `IconColorValue`, `VariantValue`, `SizeValue`, `IconSizeValue`, `ButtonTypeValue` |
| `SUIIconButton` | `Color`, `Size`, `Variant`, `Edge`, `ButtonType` | equivalentes terminados em `Value` |
| `SUILoadingButton` | `Color`, `Size`, `Variant`, `ButtonType` | equivalentes terminados em `Value` |
| `SUIChip`, `SUITimelineItem` | `Color`, `Size`, `Variant` | equivalentes terminados em `Value` |
| `SUIProgressLinear`, `SUISwitch` | `Color` | `ColorValue` |
| `SUIAlert` | `Severity` string | `ToneValue` |
| `SUIStatusBadge` | `Tone` string | `ToneValue` |

Total: 23 pontes `object` e duas pontes `string`. Não remover
`SUISelectItem.Value`: o item não genérico registra o valor no `SUISelect<T>`
pai, que faz a comparação tipada.

### Adapters de formulário

Manter `SUITextField<T>`, `SUINumericField<T>`, `SUISelect<T>` e
`SUIAutocomplete<T>` como componentes controlados. Adicionar tipos separados
derivados de `InputBase<T>` — nomes finais sujeitos a API review — que:

- consumam `ValueExpression`/`FieldIdentifier` e notifiquem o `EditContext`;
- façam parsing somente onde o tipo exigir;
- projetem mensagens de validação em `Invalid`/`ErrorText` dos fields atuais;
- mantenham IDs, `aria-describedby`, `aria-invalid` e `aria-errormessage`;
- funcionem dentro de `SUIFormGrid` sem CSS especial.

Não incorporar leitura implícita de `EditContext` nos componentes controlados:
isso mudaria timing, parsing e comportamento de binding de aplicações v1.

### Renomes bloqueados pela compatibilidade v1

Cada item abaixo é uma violação real de convenção, hoje congelada em lista de
débito nos testes (`NamingConventionTests.PrefixExemptTypes` e
`LegacyParameterNames`). Renomear quebra call sites Razor, então só entra na v2.

| Atual | Alvo | Motivo |
| --- | --- | --- |
| `NavAccordionScope` | `SUINavAccordionScope` | tipo público sem o prefixo da biblioteca |
| `SUIItem.xs/sm/md/lg/xl` | `Xs/Sm/Md/Lg/Xl` | parâmetros Blazor são PascalCase |
| `SUIAlert.CloseIconClicked` | `OnClose` | callbacks são `OnXxx` ou `XxxChanged` |

Ao aplicar, remover a entrada correspondente da lista de débito no mesmo commit:
a lista só pode encolher.

### Framework

- [ ] confirmar novamente que não existe consumer de produção net9;
- [ ] lançar prerelease v2 net10-only;
- [ ] validar canário e os consumers Identity, Blazor, AI Genius e Background;
- [ ] retirar net9 somente na major e após 2026-11-10;
- [ ] remover do CI/validador os cenários net9 no mesmo commit da mudança.

## Checklist de migração de consumer

1. Fixar a versão v1 atual; não usar range aberto durante a migração.
2. Compilar com warnings como erro e substituir todos os `CS0618` SUI.
3. Procurar no Razor os parâmetros da tabela e converter valores legados/string
   para enums SUI.
4. Executar testes, smoke de assets e auditoria geométrica de forms.
5. Instalar `v2.0.0-preview.N`, repetir os gates e registrar diferenças.
6. Atualizar para a stable somente depois do canário aprovado.

Pesquisas de apoio, sempre revisadas em contexto para evitar falsos positivos:

```bash
rg -n '<SUI(Button|IconButton|LoadingButton|Chip|TimelineItem|ProgressLinear|Switch|Alert|StatusBadge)\\b' . --glob '*.razor'
rg -n '\b(Color|IconColor|Variant|Size|IconSize|ButtonType|Edge|Severity|Tone)=' . --glob '*.razor'
```

## Gates da v2

- [ ] nenhuma ponte da tabela permanece na API pública;
- [ ] baseline público atualizado com review das diferenças;
- [ ] adapters `InputBase<T>` têm bUnit para válido/inválido/parsing;
- [ ] catálogo cobre validação automática e fields controlados lado a lado;
- [ ] Chromium, Firefox e WebKit verdes;
- [ ] pacote net10 instalado e executado em raiz e `PathBase`;
- [ ] CSS global e isolation preservam seus URLs públicos;
- [ ] todos os consumers conhecidos compilam sem `CS0618` e sem erro;
- [ ] changelog contém guia de breaking changes e rollback para última v1.

## Fora de escopo

- renomear o prefixo `SUI`;
- transformar a biblioteca em Material Design;
- incorporar identidade visual de um consumer no tema default;
- remover `sufficit-ui.css` ou exigir scripts globais;
- alterar fields controlados para herdar `InputBase<T>`.
