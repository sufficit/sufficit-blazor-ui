# Plano — alinhamento e ritmo do catálogo

**Status:** Concluído  
**Criado:** 2026-08-14  
**Escopo:** `SUIChoiceCard`, `SUIPageHeader` e composição do catálogo

## Problemas confirmados

- `SUIChoiceCard` sem `IconContent` ainda reservava uma coluna inicial de
  40 px. Título e descrição eram comprimidos nessa coluna e o indicador de
  radio aparecia na coluna intermediária.
- Cards detalhados centralizavam o indicador contra todo o bloco de texto, em
  vez de alinhá-lo ao início do título.
- o bloco tipográfico começava com zero pixels após os badges;
- o cabeçalho principal acumulava 49,6 px entre o componente e a navegação;
- `FocusOnNavigate` deixava o outline nativo apertado no `<h1>` não interativo.

## Implementação

- [x] criar tracks apenas para leading/trailing realmente renderizados;
- [x] permitir quebra de título e descrição sem truncamento;
- [x] alinhar o indicador ao título em cards detalhados e centralizá-lo em
  opções simples;
- [x] normalizar o ritmo do catálogo em 24 px entre grupos e 4–8 px dentro de
  um grupo;
- [x] remover somente o outline do heading programaticamente focado com
  `tabindex="-1"`;
- [x] adicionar contratos bUnit e Playwright para classes, largura útil,
  overflow, alinhamento vertical e gaps;
- [x] atualizar documentação, changelog e quatro baselines visuais;
- [x] executar todos os gates e arquivar esta atividade.

## Evidência intermediária

- antes: conteúdo dos três cards media 40 px; o título longo tinha 390 px de
  overflow; o gap do bloco tipográfico era 0 px;
- depois: o contrato geométrico passa em 1440 px e 390 px, exigindo conteúdo
  acima de 78% da largura do card, overflow máximo de 1 px, deslocamento máximo
  de 1,1 px e gaps tokenizados;
- build da solução: cinco projetos, zero warnings/erros;
- bUnit: 27 testes aprovados;
- inspeção visual light/desktop e dark/mobile aprovada após duas passagens
  limitadas de refinamento.

## Rollback

As mudanças são aditivas em classes internas e CSS isolation. O rollback
consiste em restaurar o grid fixo anterior, remover o wrapper tipográfico e
restaurar os quatro baselines; nenhuma API pública foi removida ou renomeada.

## Evidência final

- solução Release: cinco projetos, zero warnings/erros;
- bUnit: 27/27;
- Chromium: 23/23;
- Firefox e WebKit: 21/21 em cada engine, mais os dois skips intencionais de
  baseline e forced-colors exclusivos do Chromium;
- pacote `1.27.0-test.10`: RCLs e aplicações net9/net10 aprovadas na raiz e em
  `/app`, incluindo o CSS isolation atualizado;
- entrypoint CSS global permanece reproduzível em 46.069 bytes bruto, 8.830
  gzip e 7.719 Brotli;
- comparação dos quatro baselines aprovada; inspeção final desktop/light e
  mobile/dark sem truncamento, deslocamento ou ritmo inconsistente;
- detector `impeccable --scope layout`, diff whitespace e verificação dos PIDs
  terminaram sem achados.
