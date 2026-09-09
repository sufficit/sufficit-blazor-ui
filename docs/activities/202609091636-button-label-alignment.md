# Alinhamento vertical dos rótulos de botão

## Objetivo

Corrigir no componente compartilhado `SUIButton` o desalinhamento vertical
percebido nos botões “Regenerar”, “Anterior” e “Próxima” da tela de tokens do
Sufficit Blazor, preservando tamanhos, ícones, estados e rótulos multilinha.

## Diagnóstico

O contêiner flexível e a caixa do rótulo já estavam geometricamente
centralizados. A diferença visual vinha da caixa tipográfica produzida pelo
`line-height: 1.35` herdado por todos os botões SUI. No Chromium, o texto ficava
aproximadamente 0,45 px acima do centro do controle, embora a caixa do rótulo
estivesse centralizada.

## Alteração

- O `line-height` compartilhado de `.sui-btn` passou de `1.35` para `1.3`.
- Os artefatos CSS públicos foram regenerados a partir de `src/styles`.
- Foi acrescentado um teste de navegador que mede a caixa real do texto e exige
  desvio máximo de 0,25 px em relação ao centro do botão.
- A solução não usa transformações ou deslocamentos específicos por ícone.

Após a alteração, o desvio medido no catálogo caiu para aproximadamente
0,094 px, mantendo a leitura e a quebra de rótulos mais longos.

## Validação

- `npm run build:css`: concluído; bundle regenerado.
- `npm run check:css`: concluído; fontes e bundle sincronizados.
- Testes bUnit da biblioteca: 596 aprovados, 0 falhas.
- Testes Chromium de alinhamento: 2 aprovados, 0 falhas.
- Build Debug do consumidor `Sufficit.Blazor`: 0 avisos e 0 erros.
- Testes da tela de tokens no consumidor: 17 aprovados, 0 falhas.
- Detector Impeccable para layout: nenhuma ocorrência.
- `git diff --check` nos repositórios da biblioteca e do consumidor: sem erros.

## Entrega

A correção permanece local, sem commit, push ou deploy, pois essas operações
não foram solicitadas nesta etapa.
