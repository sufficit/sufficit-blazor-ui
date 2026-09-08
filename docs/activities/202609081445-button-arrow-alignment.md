# Alinhamento do indicador de Conferir resultado

Base: 2dbe520. Objetivo: corrigir o alinhamento visual mostrado na captura do usuário, seguindo sufficit-frontend e o fluxo de software-development.

Causa: o exemplo usava o glifo tipográfico →, cuja posição visual depende da fonte, em vez do ícone da biblioteca. Substituído por SUIIcon arrow-right, SVG18px, preservando texto, aria-hidden, distribuição space-between e estilos do botão. Alteração local à demonstração; não foi necessário compensar posições no CSS global.

Arquivos: samples/Sufficit.Blazor.UI.Demos/Examples/SUIButtonExample.razor e catalog.json gerado, que também atualiza o código copiável da documentação. Commits b391994 e b1d9d84 enviados à main.

Validação:
- Publish WASM Release e compilação dos68 exemplos sem warnings/erros.
- generate-catalog.py --check aprovado após regeneração. A primeira execução Pages detectou o catalog.json antigo; corrigido pelo gerador antes do deploy final.
- Teste existente ButtonContentHonorsAlignmentAndWrapsLongLabels:2/2 aprovados.
- Capturas e medidas locais e públicas em1280/390px: SVG18×18px, diferença entre centros verticais0,0078px, distância à direita15px igual a padding+borda, sem overflow.
- Build34258270677 aprovado: componentes, pacote, CSS, Chromium/Firefox/WebKit, acessibilidade, baselines e Lighthouse.
- Pages34258270696: build/deploy aprovados. Primeiro acesso público teve timeout; diagnóstico subsequente carregou a página sem erros e a nova medição pública passou nas duas larguras.
- Evidências locais em artifacts/arrow-alignment na worktree button-arrow-alignment.

Página: https://sufficit.github.io/sufficit-blazor-ui/?component=SUIButton
CI: https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34258270677
Deploy: https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34258270696

A alteração preexistente em src/Sufficit.Blazor.UI.csproj foi preservada. Não houve atualização de NuGet ou de aplicações consumidoras; o defeito estava no exemplo da vitrine.
