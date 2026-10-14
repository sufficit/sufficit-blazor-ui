# SUIStack.Spacing sob CSP — 13/10/2026

O Checkout Sufficit revelou que `SUIStack.Spacing="4"` renderizava `style="gap: var(--sui-space-4)"`, mas a CSP de produção descartava esse estilo. O Chrome calculou `gap: normal` e distância real de 0 px entre filhos.

`SUIStack` agora usa classes CSS para os valores 0–6 e deixa `Style` somente para declarações explícitas do consumidor. O valor padrão 3 continua correspondente a 12 px. Os testes de contrato foram atualizados para exigir classes, ausência de estilo inline gerado e preservação de `Style`; antes da correção, três testes falharam.

Validação: `npm run build:css` e `npm run check:css` passaram; bundle 59.390 bytes bruto, 10.894 gzip e 9.528 Brotli, dentro dos limites atuais. A suíte da biblioteca passou com 960 testes. O Checkout foi compilado e publicado com a nova biblioteca; o Chrome do tablet mediu 16 px de `gap` e de distância efetiva em larguras de 1280 px e 390 px. O limite bruto do script de CSS ficou com 2 bytes de margem para a próxima alteração; qualquer novo seletor requer rever o orçamento ou reduzir o bundle.
