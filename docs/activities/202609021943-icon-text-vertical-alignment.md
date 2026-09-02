# Alinhamento vertical entre ícones e textos

Data: 2026-09-02 19:43 -03

## Objetivo

Corrigir no SUI o desalinhamento óptico entre ícones SVG e textos em botões,
preservando dimensões, espaçamento, tipografia e interação dos controles.

## Estado inicial

`SUIButton` e `SUILoadingButton` já agrupavam ícone e texto com flexbox. A
compensação óptica de um pixel, porém, estava aplicada a `.sui-btn__label`,
movendo o grupo inteiro. Dessa forma, ícone e texto continuavam com a mesma
relação interna e o texto deixava o centro geométrico do controle.

## Alterações

- A compensação `translateY(1px)` passou de `.sui-btn__label` para
  `.sui-btn__icon`.
- A regra vale igualmente para ícones iniciais e finais e para o botão em estado
  de carregamento, pois todos compartilham a mesma classe.
- Botões sem ícone e botões exclusivamente de ícone não foram alterados.
- O bundle público `sufficit-ui.css` foi regenerado a partir do CSS fonte.
- A documentação recomenda `StartIcon`/`EndIcon`, mantendo o alinhamento sob
  responsabilidade do componente compartilhado.

## Decisões

- Foi preservado o offset de um pixel já estabelecido pelo sistema de ícones;
  mudou somente o elemento que o recebe.
- Nenhum seletor específico do Genius foi introduzido na biblioteca. Controles
  consumidores com SVG manual precisam adotar `SUIButton` para herdar o contrato.

## Validação

- A regressão de componente falhou antes da correção e passou depois dela.
- `npm run build:css` e `npm run check:css`: bundle sincronizado, com 53.625
  bytes brutos, 9.922 gzip e 8.659 brotli.
- Build Release da solução: 5 projetos, zero erros e zero avisos.
- Testes de componente: 374 aprovados, nenhuma falha.
- Regressão geométrica em Chromium: 1 aprovada sobre o CSS realmente carregado.
- Pacote `2.0.0-ci.icon-alignment`: validado em consumer `net10.0`, tanto na
  raiz quanto sob o path base `/app`.
- Detector mecânico de layout do Impeccable: nenhum achado.

## Resultado

O texto permanece geometricamente centralizado no botão, enquanto o desenho do
ícone recebe isoladamente o ajuste óptico. O comportamento agora é verificável
tanto no CSS fonte quanto no bundle servido a um navegador real.
