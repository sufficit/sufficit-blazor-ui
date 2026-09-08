# Nome do workflow de manutenção

Solicitação: usar somente maintenance.yml como nome do arquivo.

Renomeado .github/workflows/nuget-maintenance.yml para
.github/workflows/maintenance.yml. Runbook e plano ativo atualizados.

Validação: YAML parseado; conteúdo byte a byte idêntico ao workflow anterior;
git diff --check. Não houve execução de deslistagem nem alteração de credenciais.
O bloqueio anterior por HTTP 403 permanece registrado no plano ativo.
