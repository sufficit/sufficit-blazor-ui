# Plano — acompanhamento da avaliação de 2026-09-11

Itens do roadmap de [EVALUATION-LIBRARY-REVIEW-20260911.md](EVALUATION-LIBRARY-REVIEW-20260911.md)
ainda abertos. Os itens P1.6, P1.7, P2.9, P2.10, P2.11, P3.12, P3.13 e P3.14
foram entregues em 2026-09-11; P4.15 (limpeza da API) e P3.12b (summaries
XML em todos os membros públicos, `CS1591` ativo) em 2026-09-12. Saíram desta
lista.

| # | Item | Estado |
| --- | --- | --- |
| P0.2 | Deslistar `1.27.0`, `1.28.0`, `2.0.0`, `2.1.1`, `2.2.1` no NuGet | bloqueado na política do NuGet, não mais no repositório — ver abaixo |

## P0.2 — estado em 2026-09-20

A operação deixou de depender de `NUGET_API_KEY` de vida longa: o job
`unlist-legacy` em `publish.yml` usa a mesma credencial OIDC de curta duração
da publicação (Trusted Publishing).

O que já está verificado:

- a troca de token OIDC é aceita pela política (`verify-auth=true`,
  run [`35476395243`](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/35476395243));
- a inspeção `apply=false` roda e confirma as cinco versões ainda em
  `listed=true`, sem emitir `DELETE`
  (run [`35476436871`](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/35476436871));
- após a correção do glob em 2026-09-20, todo `apply=true` continua recusado
  com o mesmo `HTTP 403` genérico
  (runs [`35478999337`](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/35478999337),
  [`35479099495`](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/35479099495),
  [`35479658809`](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/35479658809));
  nenhuma versão foi alterada.

Diagnóstico fechado com sondas não-mutantes embutidas no job `unlist-legacy`
(run [`35479658809`](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/35479658809)),
validado contra o código-fonte do NuGet Gallery:

- `DELETE /api/v2/package/Sufficit.Blazor.UI/0.0.1` — versão que não existe no
  catálogo (28 versões, `0.0.1` ausente) — respondeu **403** com a mensagem do
  filtro de autorização (`ApiKeyNotAuthorized`). No controller, o filtro
  `ApiScopeRequired(PackageUnlist)` roda **antes** da busca do pacote: se a
  credencial tivesse qualquer escopo com a ação de unlist, a resposta seria
  **404** (pacote inexistente). Logo, a credencial mintada **não carrega a ação
  de unlist em escopo algum**.
- `GET /api/v2/verifykey/Sufficit.Blazor.UI/2.26.919.1826` respondeu **400**
  (rejeição anterior à avaliação de escopos), não 403: o filtro
  `ApiScopeRequired(PackageVerify, PackagePush, PackagePushVersion)` passou,
  ou seja, a credencial **tem** escopo de push.

Conclusão: a política está salva com o escopo *Push*, mas **sem** o escopo
*Unlist or relist package versions* marcado. O NuGet responde os dois problemas
com a mesma frase genérica, o que só foi separável com as sondas acima.

Ação pendente, apenas na interface do nuget.org: editar a política `publish.yml`
→ seção *Select Scopes* → marcar **Unlist or relist package versions** →
salvar. O glob corrigido para `Sufficit.Blazor.UI` permanece como está.

Depois de marcado, executar e conferir `listed=false` no catálogo:

```bash
gh workflow run publish.yml --repo sufficit/sufficit-blazor-ui --ref main \
  -f unlist-legacy=true -f replacement=2.26.919.1826 -f apply=true
```
