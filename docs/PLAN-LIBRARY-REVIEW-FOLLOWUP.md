# Plano — acompanhamento da avaliação de 2026-09-11

Itens do roadmap de [EVALUATION-LIBRARY-REVIEW-20260911.md](EVALUATION-LIBRARY-REVIEW-20260911.md)
ainda abertos. Os itens P1.6, P1.7, P2.9, P2.10, P2.11, P3.12, P3.13 e P3.14
foram entregues em 2026-09-11; P4.15 (limpeza da API) e P3.12b (summaries
XML em todos os membros públicos, `CS1591` ativo) em 2026-09-12; P0.2 (unlist
das versões legadas) em 2026-09-20. Saíram desta lista — nenhum item resta
aberto.

## P0.2 — encerrado em 2026-09-20

Bloqueio e diagnóstico (histórico): a política de Trusted Publishing chegou a
recusar o `apply=true` com `HTTP 403` genérico. Sondas não-mutantes embutidas
no job `unlist-legacy`
(run [`35479658809`](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/35479658809),
validadas contra o código-fonte do NuGet Gallery) separaram o que o NuGet
responde com uma frase única: `DELETE` de versão inexistente respondeu 403 do
filtro de autorização (uma credencial com escopo de unlist chegaria à busca do
pacote e responderia 404), enquanto `verifykey` passou pelo filtro de push. A
política tinha o escopo *Push* e o glob `Sufficit.Blazor.UI` corretos, mas
*faltava* o escopo *Unlist or relist package versions*.

Desfecho:

- o escopo foi marcado na interface do nuget.org em 2026-09-20;
- `apply=true` com `replacement=2.26.920.117` executou com todos os `DELETE`
  aceitos (`HTTP 200`)
  (run [`35481779299`](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/35481779299));
- após a propagação do índice, o registration confirmou `listed=false` para
  `1.27.0`, `1.28.0`, `2.0.0`, `2.1.1` e `2.2.1`.

As versões seguem baixáveis (unlist não é exclusão); relist exato permanece
disponível se algum consumidor legado precisar ser localizado primeiro.
