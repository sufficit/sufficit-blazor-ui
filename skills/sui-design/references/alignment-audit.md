# Auditoria de alinhamento horizontal

Carregue ao construir ou revisar formulários com campos lado a lado.

## Contrato

- Audite apenas campos equivalentes na mesma linha visual; botão de ação,
  switch-group e texto auxiliar não são pares de um input só porque ocupam a
  mesma Grid.
- Alinhe o topo dos wrappers e labels, o topo dos controles e a altura dos
  controles. Input e select com o mesmo topo, mas alturas diferentes, falham.
- Aceite no máximo `2px` de diferença por padrão; aumente a tolerância somente
  para uma correção óptica documentada.
- Compare a largura desktop e o breakpoint imediatamente anterior ao
  empilhamento. Depois do empilhamento, os campos deixam de ser pares
  horizontais e não devem ser comparados.
- Teste `pt-BR` e `en-US`: um label que quebra linha deve ampliar a linha para
  todos os pares ou provocar empilhamento, nunca deslocar apenas um controle.
- Repita com helper, erro de validação e disabled. Conteúdo abaixo do controle
  pode aumentar o wrapper, mas não deve deslocar o topo do controle vizinho.
- Procure vazamento de seletores adjacentes, por exemplo
  `.field-group + .field-group { margin-top: ... }`, dentro de Grid/Flex.
- Trate zero containers ou zero pares horizontais como falha de instrumentação.
  Um gate que não mediu nada não pode aprovar a tela.

## Correção estrutural preferida

Corrija o contexto da linha, não cada campo com offsets. Para fields SUI, use a
primitive pública — ela já aplica este contrato e instrumenta o gate:

```razor
<SUIFormGrid Columns="2" LabelLines="2">
  <SUITextField T="string" Label="Nome" @bind-Value="Model.Name" />
  <SUISelect T="string" Label="Região" @bind-Value="Model.Region">...</SUISelect>
</SUIFormGrid>
```

Em layouts mistos, legados ou fora de uma RCL SUI, reproduza a estrutura:

```css
.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  align-items: start;
  gap: 1rem;
}

.form-grid > :is(.sui-field, [data-sui-align-field]) {
  min-width: 0;
  align-self: start;
  margin-block: 0;
}

/* Reserve duas linhas quando labels traduzidos puderem quebrar. */
.form-grid .sui-field__label { min-block-size: 2.4em; }

/* Neutralize uma regra válida somente no fluxo vertical. */
.form-grid > .field-group + .field-group { margin-block-start: 0; }

@media (max-width: 44rem) {
  .form-grid { grid-template-columns: minmax(0, 1fr); }
  .form-grid .sui-field__label { min-block-size: auto; }
}
```

Para MudBlazor, aplique a mesma ideia ao wrapper `.mud-input-control` e à
`.mud-input-label`. Não use `transform`, margem negativa ou ajuste por `id`:
essas correções quebram com tradução, zoom, helper e erro.

## Instrumentação

Marque cada container cujos filhos diretos formam uma linha de campos. O
`SUIFormGrid` já emite essa marca; não a duplique:

```html
<div class="form-grid" data-sui-align-row>
  <label class="field-group">...</label>
  <label class="field-group">...</label>
</div>
```

O auditor reconhece filhos diretos `.field-group`, `.sui-field`,
`.mud-input-control` ou `[data-sui-align-field]`. Em cada campo, ele procura
labels SUI/Mud/HTML e controles SUI/Mud/HTML. Quando um componente cria um
wrapper adicional, marque o wrapper com `data-sui-align-field`; para markup
fora desses padrões, use `data-sui-field-label` e `data-sui-field-control`.
O auditor identifica o campo pelo `id` do wrapper/controle; quando nenhum
existir, use `data-sui-align-name="nome-estável"` para produzir evidência útil.

`data-sui-align-*` é instrumentação, não deve ser usado para compensar o CSS.

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
  labelSelector: ".filter__label",
  controlSelector: ".filter__control",
  tolerance: 2
});
```

Por padrão, `requireContainers`, `requireComparisons` e `requireControls` são
`true`; `requireLabels` é `false` para permitir linhas mistas deliberadas, mas
labels ausentes aparecem em `diagnostics`. Ative `requireLabels: true` quando
todos os pares devam ter label visível.

O relatório inclui containers, campos medidos e `pairs`: cada par aprovado ou
reprovado identifica os dois campos e seus deltas por dimensão (`field`,
`label` ou `control`) e métrica (`top`/`height`). Não aceite somente o booleano;
registre `comparisons` e os pares medidos na evidência do gate. Considere a
auditoria aprovada somente quando `pass === true`, `comparisons > 0` e os
diagnósticos esperados forem explicados em:

1. viewport desktop;
2. último viewport ainda horizontal;
3. idiomas suportados com o maior label realista;
4. estado normal e estado com helper/erro de validação.
