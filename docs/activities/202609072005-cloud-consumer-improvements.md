# Evolução SUI a partir do Cloud Mobile

Pedido: implementar melhorias da vitrine e devolver à biblioteca central padrões úteis descobertos na reformulação do Cloud Mobile, consumidor já existente. Base: `34db6f0`.

## Entrega

- API aditiva `SUITabs.Vertical`, teclado orientado e aba ativa revelada sem rolar o documento; drawer fechado inerte, foco de switch e contraste secundário escuro.
- Utilitário `SUIColorContrast`, playground de quatro componentes e 68 exemplos executáveis preservados. Composição operacional completa com estados, busca, filtro, paginação, edição e preferências.
- Editor de tema com tipografia, espaços, raios e cores semânticas, comparação por iframes isolados, exportação fiel e versão de assembly explícita no catálogo.
- Descrições da API extraídas dos comentários XML; rolagem local da tabela mobile. Correções de bindings de clipboard, validação de cores e estado de alterações pendentes.
- DESIGN.md/sidecar reconciliados por documentador independente; uso e medições em `docs/USAGE-SHOWCASE-WORKSPACE.md`.

## Evidências

- `dotnet test tests/Sufficit.Blazor.UI.Tests -c Release -warnaserror`: **566 aprovados**; API pública apenas adições.
- `python3 scripts/generate-catalog.py --check`: 68 componentes; `python3 scripts/check-catalog-examples.py`: 68 exemplos copiados compilados.
- `npm run check:css`: CSS global 53.960 B bruto, 9.884 B gzip, 8.655 B Brotli. Budgets originais de JS/CSS mantidos; comentários de interop redundantes reduzidos sem alterar comportamento.
- `dotnet publish samples/Sufficit.Blazor.UI.Showcase -c Release`: sucesso; artifact preparado e servido estaticamente em loopback.
- **17 testes Showcase Chromium aprovados**, incluindo controles reais, clipboard, estados, tema persistente, isolamento, acesso móvel e axe nas superfícies novas. Capturas esperam interatividade e fim de animações.
- **25 testes de regressão do catálogo Server aprovados**: filtro CatalogBrowserTests, excluindo comparação de baseline visual e teste de base path (não foram reexecutados nesta rodada local).
- Medição fria local: 8.650.741 B decodificados, 8.668.141 B transferidos, 58 recursos, 346 ms até marca interativa e 467 ms até ação verificada. Sem limitação de rede; não é benchmark de produção.
- Revisor independente: `disposition: ship` após julgar os quatro achados materiais **resolved** (barra pendente, validação, prévia desktop e API mobile). Aprovação limitada à lista reavaliada.

## Entrega e limites

Commit e push na main autorizados na conversa; Pages publica pelo workflow `Component showcase`. URL: https://sufficit.github.io/sufficit-blazor-ui/. A confirmação do destino é feita após push e registrada na resposta de entrega, sem tratar o deploy como concluído antecipadamente.

Nenhuma versão NuGet foi publicada por esta revisão. Cloud referencia o source em commit fixado; consumidores NuGet devem esperar uma versão que contenha as APIs novas. Dados das composições são fictícios. A verificação de contraste opaco não substitui avaliação completa de acessibilidade.
