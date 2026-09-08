# Revisão sistemática de frontend da Blazor UI

## Objetivo e estado inicial

Aplicar sufficit-frontend 1.0.1 à biblioteca, ampliando a rodada anterior de Switch. Base `b2d9f69`, catálogo com 68 componentes. Trabalho isolado em `.worktrees/library-review`; a alteração preexistente de `src/Sufficit.Blazor.UI.csproj` no checkout principal foi preservada. O Cloud foi usado como consumidor de regressão local.

## Entrega por área

- **SUITextField / SUINumericField / SUICheckbox:** foco no campo após limpeza por teclado; referências ARIA somente para mensagens existentes; erro de EditContext associado ao checkbox; bloqueio de alteração quando desabilitado.
- **SUISelect / SUIAutocomplete:** item ativo de listas longas permanece visível sem mover a página; lista e opções fora da sequência de Tab; autocomplete em popover escapa do recorte de contêineres, evita submit/scroll nativos ao selecionar pelo teclado e cancela busca pendente ao desabilitar ou selecionar. Interop extraído em partials, com posicionamento compartilhado e descarte de listeners.
- **SUIButton / SUILoadingButton:** ícones herdam contraste do botão; tokens de contraste semântico respeitados; textos longos cabem na caixa; largura preservada durante carregamento com ícones; estado busy exposto; link habilitado navega corretamente.
- **SUIFormGrid:** adaptação ao espaço do contêiner, com parâmetro aditivo `MinColumnWidth` (14rem); `StackOnMobile=false` preserva colunas fixas; corrigida especificidade que reservava duas linhas de rótulo no celular.
- **Catálogo e vitrine:** composição executável e copiável com painel estreito, listas de 30 opções, alternância de tema, carregamento e link. A matriz em `docs/FRONTEND-REVIEW.md` registra também famílias preservadas e suas regressões existentes.

Não foram adicionados frameworks visuais externos nem alteradas regras de negócio do Cloud. A revisão não afirma que todos os 68 componentes receberam redesenho individual: foram corrigidos oito componentes e verificados contratos compartilhados das demais famílias.

## Validação

- `dotnet build Sufficit.Blazor.UI.slnx -c Release -warnaserror`: solução completa, sem avisos.
- `dotnet test tests/Sufficit.Blazor.UI.Tests -c Release -warnaserror`: **579 aprovados**.
- `python3 scripts/generate-catalog.py --check`: catálogo de 68 componentes consistente.
- `python3 scripts/check-catalog-examples.py`: **68 exemplos copiados compilados**.
- `npm run check:css`: CSS bruto 54.339 B, gzip 9.969 B, Brotli 8.730 B. Crescimento bruto de 355 B; limites comprimidos originais preservados. CSS isolado: 25.993 B. Ajustes de orçamento justificados na matriz permanente.
- Browser Server: **51 Chromium**, **46 Firefox** e **46 WebKit** aprovados. Os 25 casos exclusivos de Showcase são ignorados nessa execução; cinco métricas exclusivas de Chromium são ignoradas nos outros motores.
- Vitrine estática publicada sob `/sufficit-blazor-ui/`: **25 testes aprovados**, incluindo cópia dos dois exemplos completos.
- Cloud: `SUI_PROJECT_PATH=<worktree>/src/Sufficit.Blazor.UI.csproj tools/check-frontend.sh`, no checkout `2f36ad5`: **12 cenários aprovados**, desktop/celular, tema escuro e movimento reduzido. Sem deploy do Cloud nesta rodada.
- Pacote e consumidores mínimos: aprovados no CI; Lighthouse aprovado.
- Smoke na URL pública em 390 px: limpeza devolve foco, End/Enter seleciona destino 30, CSS coincide byte a byte com o repositório, sem erros JavaScript. Captura e JSON locais em `/tmp/library-review-public-mobile.png` e `/tmp/library-review-public.json`.

Os testes novos reproduziram defeitos na base antes das correções. As referências visuais precisaram de recaptura no ambiente CI; a altura desktop permaneceu 5.286 px e a mobile passou de 5.645 para 5.571 px com a correção intencional de rótulos. A execução seguinte fez comparação normal e passou, sem modo de atualização.

## Referências entregues

- Implementação: [436f785](https://github.com/sufficit/sufficit-blazor-ui/commit/436f7857aa2947f5b4ceadafb669cbb5602f768c).
- Referências visuais do CI: [d40dc3a](https://github.com/sufficit/sufficit-blazor-ui/commit/d40dc3aecc9cb82f28e23d27df862ad549a010e1).
- [CI de recaptura aprovado](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34232558060).
- [CI de comparação normal aprovado](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34233101531).
- [Publicação Pages aprovada](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34232544553).
- [Exemplos públicos para testar](https://sufficit.github.io/sufficit-blazor-ui/?view=patterns#interaction-patterns).

## Limites

Sem nova versão NuGet/tag nesta rodada. Cloud validado por referência local ao código novo; a versão de produção não foi atualizada. As verificações de acessibilidade cobrem os cenários exercitados e não constituem certificação integral. Capturas locais temporárias não são artefatos permanentes; as quatro referências de comparação do catálogo estão versionadas.
