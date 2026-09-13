# Actions, releases e API

## Actions e workflows

Use `run list` para descobrir execuções e `run view` para identificar o job e a etapa que
falharam. Busque logs somente quando forem necessários; resuma a causa e o trecho relevante.
Nunca peça ao usuário para copiar o log enquanto a conta conectada puder obtê-lo pelo GitHub.

```json
["run", "list", "--repo", "owner/repo", "--limit", "10", "--json", "databaseId,workflowName,status,conclusion,headBranch,headSha,url,createdAt"]
```

```json
["run", "view", "123456", "--repo", "owner/repo", "--json", "status,conclusion,jobs,url"]
```

```json
["run", "view", "123456", "--repo", "owner/repo", "--log-failed"]
```

Se o resumo não trouxer o erro, liste os jobs para obter o `databaseId` e consulte o job:

```json
["run", "view", "123456", "--repo", "owner/repo", "--json", "jobs", "--jq", ".jobs[] | {id: .databaseId, name, status, conclusion, steps}"]
```

```json
["run", "view", "123456", "--repo", "owner/repo", "--job", "987654", "--log-failed"]
```

Quando o workflow publicar resultados, descubra e baixe somente o artefato relevante com
`run download`. Se logs continuarem indisponíveis, informe qual chamada falhou e qual permissão
está ausente; solicitar uma cópia manual é o último recurso.

Antes de `run rerun`, verifique se a falha parece transitória ou se o código/configuração já
mudou. Não repita indefinidamente. Disparos com `workflow run` e cancelamentos são alterações
reais e precisam estar dentro do pedido atual.

## REST e GraphQL

Use `gh api` quando um subcomando de alto nível não oferecer os campos ou a operação. Consulte
o endpoint correto e passe parâmetros como itens separados. Para leitura REST paginada, use
`--paginate`; combine com `--slurp` apenas quando precisar de uma coleção única.

```json
["api", "repos/owner/repo/issues/480/comments", "--paginate", "--jq", ".[] | {id,body,user:.user.login,created_at}"]
```

Para payload complexo, crie JSON em arquivo e use `--input`. Não coloque conteúdo multilinha
em uma expressão de shell e não inclua cabeçalho Authorization: o host já autentica o `gh`.

Use GraphQL para relações que exigiriam muitas chamadas REST. Limite campos e paginação; uma
consulta ampla pode gerar uma resposta grande demais para a sessão.

## Releases e artefatos de Actions

Descubra releases antes de baixar e use `release download` com diretório explícito. Para
artefatos de workflow, use `run download`. Não execute automaticamente binários baixados;
inspecione tipo, origem e integridade conforme a tarefa.

```json
["release", "download", "v1.2.3", "--repo", "owner/repo", "--dir", "/destino", "--pattern", "*.zip"]
```

```json
["run", "download", "123456", "--repo", "owner/repo", "--dir", "/destino"]
```
