# Plano — acompanhamento da avaliação de 2026-09-11

Itens do roadmap de [EVALUATION-LIBRARY-REVIEW-20260911.md](EVALUATION-LIBRARY-REVIEW-20260911.md)
ainda abertos. Os itens P1.6, P1.7, P2.9, P2.10, P2.11, P3.12, P3.13 e P3.14
foram entregues em 2026-09-11; P4.15 (limpeza da API) e P3.12b (summaries
XML em todos os membros públicos, `CS1591` ativo) em 2026-09-12. Saíram desta
lista.

| # | Item | Estado |
| --- | --- | --- |
| P0.2 | Deslistar `1.27.0`, `1.28.0`, `2.0.0`, `2.1.1`, `2.2.1` no NuGet | bloqueado na política do NuGet, não mais no repositório — ver abaixo |

## P0.2 — estado em 2026-09-19

A operação deixou de depender de `NUGET_API_KEY` de vida longa: o job
`unlist-legacy` em `publish.yml` usa a mesma credencial OIDC de curta duração
da publicação (Trusted Publishing).

O que já está verificado:

- a troca de token OIDC é aceita pela política (`verify-auth=true`,
  run [`35476395243`](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/35476395243));
- a inspeção `apply=false` roda e confirma as cinco versões ainda em
  `listed=true`, sem emitir `DELETE`
  (run [`35476436871`](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/35476436871));
- com `apply=true` o `DELETE` é recusado com `HTTP 403 ... does not have
  permission to access the specified package`
  (run [`35476476714`](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/35476476714));
  nenhuma versão foi alterada.

Causa: na política de Trusted Publishing, o campo *Glob Patterns and Packages*
está preenchido com `sufficit-blazor-ui`, que é o nome do **repositório**. O
`PackageId` publicado é `Sufficit.Blazor.UI` (`src/Sufficit.Blazor.UI.csproj`).
O glob não casa nenhum pacote, então a credencial é emitida e depois recusada.

Ação pendente, na interface do nuget.org (não há como fazer pelo repositório):
ajustar *Glob Patterns and Packages* para `Sufficit.Blazor.UI` (ou `Sufficit.*`),
mantendo *Workflow File* `publish.yml` e os escopos *Push new packages and
package versions* e *Unlist or relist package versions*.

O mesmo glob governa o escopo de push: enquanto não for corrigido, **a próxima
tag de release também falha** com `403` no `dotnet nuget push`.

Depois de corrigir, executar e conferir `listed=false` no catálogo:

```bash
gh workflow run publish.yml --repo sufficit/sufficit-blazor-ui --ref main \
  -f unlist-legacy=true -f replacement=2.26.919.1826 -f apply=true
```
