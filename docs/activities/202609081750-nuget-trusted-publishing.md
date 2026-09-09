# Migração do build para NuGet Trusted Publishing

Solicitação: adaptar build.yml à autorização NuGet para publicação cotidiana.

Implementado NuGet/login fixado em 8d196754b4036150537f80ac539e15c2f1028841;
id-token write limitado a publish e verificação manual; login imediatamente
antes do push, após validar o artefato. Mantidos publicação por tag, calendário
Sufficit e todos os gates. A publicação não usa mais secrets.NUGET_API_KEY.

Adicionada opção workflow_dispatch verify-publishing-auth: testa apenas a
troca OIDC, sem gerar/publicar pacote nem rodar a matriz de testes.

Validação: YAML/estrutura/permissões verificados. Execução real 34276979022
recebeu OIDC do GitHub, mas o NuGet recusou a troca com HTTP 401:
No matching trust policy owned by user 'sufficit' was found.
https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34276979022

Autenticação pendente: é necessário configurar a variável Actions NUGET_USER
com o usuário NuGet que criou a autorização e confirmar a correspondência de
sufficit/sufficit-blazor-ui, build.yml e environment vazio. O fallback baseado
no proprietário do pacote foi removido após esse diagnóstico. Nenhum pacote
adicional foi publicado nesta migração. Maintenance continua com a chave API
e bloqueio de Unlist já registrado no plano ativo.
