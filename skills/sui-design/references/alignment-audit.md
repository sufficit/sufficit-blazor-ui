# Auditoria de alinhamento horizontal

Carregue ao construir ou revisar formulários com campos lado a lado.

## Contrato

- Alinhe o topo dos wrappers, labels e controles equivalentes na mesma linha.
- Aceite no máximo `2px` de diferença por padrão; aumente a tolerância somente
  para uma correção óptica documentada.
- Compare a largura desktop e o breakpoint imediatamente anterior ao
  empilhamento. Depois do empilhamento, os campos deixam de ser pares
  horizontais e não devem ser comparados.
- Teste `pt-BR` e `en-US`: um label que quebra linha deve ampliar a linha para
  todos os pares ou provocar empilhamento, nunca deslocar apenas um controle.
- Procure vazamento de seletores adjacentes, por exemplo
  `.field-group + .field-group { margin-top: ... }`, dentro de Grid/Flex.

## Instrumentação

Marque cada container cujos filhos diretos formam uma linha de campos:

```html
<div class="form-grid" data-sui-align-row>
  <label class="field-group">...</label>
  <label class="field-group">...</label>
</div>
```

O auditor reconhece filhos diretos `.field-group`, `.sui-field` ou
`[data-sui-align-field]`. Em cada campo, ele procura label em `label`, `legend`,
`[data-sui-field-label]` ou no primeiro `span`, e controle em `input`, `select`,
`textarea`, `.sui-input`, `.sui-select__trigger` ou
`[data-sui-field-control]`.

## Gate no navegador

Injete `scripts/audit-field-alignment.js` na página renderizada e execute:

```js
const report = SUIAlignmentAudit();
if (!report.pass) throw new Error(JSON.stringify(report.failures, null, 2));
```

Com Playwright/Puppeteer, carregue o arquivo por `addScriptTag({ path })` e rode
a chamada acima em `page.evaluate`. Para uma tela sem marcação, informe
`containerSelector`, `fieldSelector` e, se necessário, os seletores de label e
controle. Exemplo:

```js
SUIAlignmentAudit({
  containerSelector: ".filters-grid",
  fieldSelector: ":scope > .filter",
  tolerance: 2
});
```

O relatório inclui container, pares comparados, dimensão (`field`, `label` ou
`control`) e delta medido. Considere a auditoria aprovada somente quando
`pass === true` em todos os viewports e idiomas suportados.
