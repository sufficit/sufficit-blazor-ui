# SUIIcon: Name nulo durante renderização

## Problema e correção
O stack trace recebido apontava `SUIIcon.get_IsMarkup()`. O getter chamava
`Name.TrimStart()` diretamente; um consumidor pode fornecer null em runtime,
mesmo com o parâmetro declarado como string não anulável.

A detecção agora reutiliza `SUIIconMarkup.IsMarkup(Name)`, que já aceita null.
O switch mantém o círculo de fallback para nomes ausentes/desconhecidos.
A API pública, os nomes de ícones e a validação de SVG permanecem preservados.
CHANGELOG atualizado e teste bUnit cobre null explícito, vazio e espaços.

## Validação
- Antes da correção: teste `MissingName_RendersTheFallbackCircle` reproduziu
  NullReferenceException em `SUIIcon.razor:156` no caso null; os outros 2 passaram.
- Após a correção: 87 testes de `IconRenderingTests` e `IconInjectionTests`
  passaram, incluindo a regressão e a rejeição de SVG inseguro.
- `dotnet build src/Sufficit.Blazor.UI.csproj -c Release`: aprovado,
  zero erros e zero avisos.
- Suíte bUnit completa: 1163 passaram e 3 falharam por budgets existentes:
  `SUITooltip.razor.js` tem 362 linhas (limite 320) e 13337 bytes
  (limite 12288); total JS Brotli 14582 bytes (limite 14336).
  O JS e os testes de budget estão idênticos ao HEAD anterior à correção;
  nenhuma alteração nesses arquivos foi incluída nesta entrega.
- Diff revisado e `git diff --check` aprovado.

## Entrega e limites
Commit na main e push para origin conforme instrução do mantenedor.
A referência da entrega é o commit que adiciona este registro.
Nenhuma tag de publicação, pacote NuGet ou deploy executado; consumidores
precisam receber uma versão contendo esta correção para resolver a falha no site.
Plano temporário encerrado após implementação e validação; evidências preservadas aqui.
