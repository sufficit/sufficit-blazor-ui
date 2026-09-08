# Autenticação NuGet OIDC validada

Concluída a configuração solicitada para a publicação cotidiana por build.yml.
Criador da política confirmado pelo usuário: hugodeco; variável Actions
NUGET_USER configurada com esse valor e conferida pela API GitHub.

O teste 34277547213 identificou a política ainda vinculada a maintenance.yml.
Depois da alteração pelo usuário, o teste 34278506717 identificou o ambiente
production exigido pela política. Commit 2fdb090 alinhou publish e
publishing-authentication a production, mantendo os gates e permissões de
OIDC limitadas aos dois jobs. Runbook atualizado e YAML validado.

Verificação real aprovada: run 34278698353, em 2fdb090, com mensagem
Successfully exchanged OIDC token for NuGet API key e confirmação da saída
não vazia. A credencial foi mascarada, sem armazenar seu valor. O job de
publicação e os demais jobs foram pulados pela opção verify-publishing-auth.
Nenhuma versão adicional foi publicada para testar autenticação.

https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34278698353

Limite: esta verificação comprova a troca OIDC, não um push com a nova
credencial. A próxima publicação por tag permanece condicionada aos gates.
A pendência anterior de deslistagem em maintenance.yml continua separada:
essa manutenção usa a chave API cuja tentativa de Unlist recebeu HTTP 403.
