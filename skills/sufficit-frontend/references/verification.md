# Evidência no navegador

Escolher a matriz pelo risco e pelo que mudou. Para uma correção de layout comum: estado reportado, largura ampla e estreita, tema afetado e um estado vizinho que compartilha estilos. Para primitiva compartilhada: incluir variantes e consumidor representativo. Não confundir isso com obrigação de auditar todo o produto.

## Preparar

Usar o navegador/ferramenta disponível e os comandos do projeto. Aguardar renderização interativa, fontes e transições relevantes antes de medir. Exercitar dados realistas sem executar mutações reais desnecessárias. Se usar fixture, deixar claro o que ela não valida.

Reproduzir a reclamação antes da correção quando possível; guardar evidência comparável. Para spacing, abrir os painéis; para dropdown, abrir o menu e navegar por teclado; para tamanhos, renderizar as variantes lado a lado. Não se satisfazer com screenshot de estados fechados.

## Conferir

- **Geometria:** distância real entre irmãos, padding, tamanho dos controles/ícones, alinhamento e overflow. Ler o valor computado e a regra vencedora quando houver divergência.
- **Uso:** foco visível, ordem de tabulação, nome acessível, controles desabilitados, submissão e recuperação nos estados relevantes.
- **Adaptação:** textos longos, zoom e ponto de quebra representativo quando atingirem a mudança. Rolagem local deliberada é diferente de overflow acidental da página.
- **Aparência:** hierarquia, tema, contraste e estados; análise automática de acessibilidade não comprova conformidade completa nem substitui a revisão visual.
- **Entrega:** mesmo CSS e versão de componentes no artefato testado e no destino, caso publicação faça parte da tarefa. Sem acesso autenticado, health e assets não provam o fluxo autenticado.

Não introduzir limiares universais a partir de uma tela. Para contrastes, tamanhos de alvo e conformidade formal, aplicar o padrão exigido pelo projeto e consultar sua fonte normativa atual quando necessário.

## Medição reutilizável

[scripts/measure-layout.mjs](../scripts/measure-layout.mjs) é uma função para avaliação **dentro de uma página já aberta**. Não inicia browser, não navega, não altera DOM e não classifica aprovação. Retorna valores de elementos encontrados e a distância vertical entre seletores consecutivos; comparar esses valores com o contrato da tela.

Exemplo com Playwright já instalado no projeto:

```js
import path from 'node:path';
import { pathToFileURL } from 'node:url';
const moduleUrl = pathToFileURL(path.join(skillDir, 'scripts/measure-layout.mjs'));
const { default: measureLayout } = await import(moduleUrl.href);
const evidence = await page.evaluate(measureLayout, {
  selectors: ['.settings-control', '.settings-warning', '.settings-next-task']
});
```

`skillDir` e `page` são fornecidos pelo harness existente; não instalar um segundo harness só para esta função. Seletores ausentes/inválidos e conteúdo sem caixa visível aparecem explicitamente. Uma distância negativa pode significar sobreposição ou layout em colunas; interpretar no contexto. Para listas, o helper usa o primeiro elemento e registra a contagem: seletores ambíguos exigem refino.

Guardar screenshots, medidas e resultado dos checks na convenção do projeto. Uma regressão automatizada deve detectar o defeito real (ex.: altura final indiferente ao parâmetro), não espelhar nomes de classes ou aceitar qualquer arquivo de captura existente.
