# Plano concluído — polimento da cor de ações primárias

**Status:** Concluído  
**Criado:** 2026-08-14  
**Concluído:** 2026-08-14  
**Escopo:** palette SUI, botões primários preenchidos, catálogo e skill interna

## Problema confirmado

O catálogo usava o mesmo âmbar vivo como acento e superfície preenchida, com
texto quase preto. A combinação alcançava contraste de 7,84:1, mas produzia um
bloco visual pesado e confundia dois papéis: destaque de marca e ação principal.

## Decisão implementada

- `Primary` permanece como acento de links, foco, tabs e contornos;
- `PrimaryAction`/`PrimaryActionContrast` são papéis opcionais da palette;
- o catálogo usa ember `#b7440e` com branco quente `#fff7ed` nas ações
  preenchidas, com contraste resting de 5,11:1;
- temas existentes recuam para `Primary`/`PrimaryContrast` e não exigem
  migração;
- hover e pressed escurecem somente a superfície de ação, sem filtros que
  também alterem texto e borda.

## Implementação

- [x] auditar contraste, estados e papéis de cor nos temas claro e escuro;
- [x] implementar tokens públicos opcionais e fallback compatível;
- [x] aplicar o novo papel ao botão primário preenchido e ao catálogo;
- [x] cobrir emissão, fallback, contraste, foco, hover e pressed em testes;
- [x] atualizar bundle CSS, skill interna, documentação e changelog;
- [x] atualizar e inspecionar os quatro baselines visuais;
- [x] executar build estrito, bUnit e Playwright em Chromium/Firefox/WebKit;
- [x] validar o pacote consumido em net9/net10, na raiz e sob `PathBase`.

## Evidência final

- solução Release: cinco projetos, zero warnings/erros;
- bUnit: 28/28;
- Chromium: 24/24;
- Firefox e WebKit: 22/22 em cada engine, além dos dois skips intencionais de
  baseline e forced-colors exclusivos do Chromium;
- pacote `1.27.0-test.11`: RCLs e aplicações net9/net10 aprovadas na raiz e em
  `/app`;
- bundle reproduzível: 46.478 bytes bruto, 8.884 gzip e 7.757 Brotli;
- quatro baselines atualizados; inspeção visual desktop/claro e mobile/escuro
  aprovada sem perda de hierarquia, legibilidade ou estado;
- skill `sui-design` validada e detector final `impeccable` sem achados;
- diff whitespace e contratos de acessibilidade aprovados.

## Rollback

Remover os dois campos opcionais e os dois tokens CSS restaura o vínculo direto
com `Primary`/`PrimaryContrast`. Como o fallback já preserva esse comportamento
para consumers que não adotarem a extensão, não há migração obrigatória nem
quebra de API.
