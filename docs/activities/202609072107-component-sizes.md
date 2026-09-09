# Correção dos tamanhos na SUI

O usuário mostrou o playground de SUIIconButton e relatou que as três opções não mudavam o resultado. Reprodução pública confirmou 40×40px em pequeno/médio/grande, sempre com glifo de 21px.

## Causa e correção

Os componentes geram classes `small`/`medium`/`large`, mas o CSS de botões/ícones usava somente `sm`/`md`/`lg`. Além disso, `.sui-btn--icon` redefinia a altura para o tamanho médio. O CSS agora aceita os dois formatos e o botão de ícone usa a altura selecionada para largura e altura, mantendo o formato quadrado. O reparo compartilhado alcança SUIButton, SUIIconButton e SUILoadingButton, sem alterar API ou renomear classes dos consumidores.

SUIChip também passa a implementar large, anteriormente oferecido pelo playground sem regra própria. Espaçamento, glifo e remoção acompanham o tamanho. O padding interno foi consolidado em propriedade local para preservar o limite de tamanho do CSS.

## Evidências

- Medidas no tema confortável da vitrine, desktop: botões28/40/44px; glifo do IconButton15/21/32px; chips24/32/40px. Nos celulares o alvo dos botões permanece com pelo menos44px, preservando diferenças de glifo/tipografia.
- Seis regressões novas medem bounding boxes do SUIButton, SUIIconButton e SUIChip a1440/390px, verificam código exportado, desabilitado sem alteração de tamanho, ícone escalável, quadratura e ausência de overflow global.
- **567 testes unitários e 24 testes Showcase aprovados**. Publish WebAssembly Release concluído; 68 exemplos independentes compilados; catálogo regenerado e verificado.
- CSS:53.984B bruto,9.916B gzip,8.679B Brotli; budgets mantidos, sem ampliar limites.
- Capturas dos tamanhos desktop e mobile foram abertas e conferidas; arquivos e medições ficam em `tests/Sufficit.Blazor.UI.BrowserTests/bin/Release/net10.0/sizes`.

## Entrega

Commit/push na main conforme autorização persistente. A vitrine é entregue pelo workflow Pages; confirmação pública ocorre depois do deploy, antes da resposta final. URL de teste: https://sufficit.github.io/sufficit-blazor-ui/?component=SUIIconButton.

Nenhuma nova versão NuGet foi publicada e o Cloud Mobile não foi reimplantado nesta correção da vitrine. Consumidores recebem o reparo ao atualizar sua dependência SUI e reconstruir os assets.
