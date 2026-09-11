# Atividade — versionamento Sufficit e limpeza do catálogo NuGet (arquivada em 2026-09-11)

> Plano recuperado da worktree `sufficit-versioning` ao consolidar as worktrees em 2026-09-11.
> Itens 1–5 concluídos em 2026-09-08. O item pendente (deslistar as cinco versões SemVer
> legadas) segue rastreado em [EVALUATION-LIBRARY-REVIEW-20260911.md](../EVALUATION-LIBRARY-REVIEW-20260911.md), P0.
> Tags Git `v1.27.0`, `v1.28.0`, `v2.0.0`, `v2.1.1`, `v2.2.0`, `v2.2.1` removidas do repositório em 2026-09-11.


Objetivo: padronizar SUI em 1.yy.MMdd.HHmm UTC, Debug 1.99.0.0; corrigir o pipeline e documentação; deslistar exclusivamente as versões SUI fora do padrão.

Terreno: main 1bd4f6d; csproj local já alterado pelo usuário para o padrão corporativo. Trabalhar em worktree isolada, incorporando a alteração autorizada. Preservar critique e demais repositórios. NuGet não permite exclusão permanente; 1.27.0, 1.28.0, 2.0.0, 2.1.1, 2.2.1 são as cinco versões alvo. Publicar substituta antes de atualizar consumidores; não apontar dependências para versão inexistente.

1. [completed] Unificar csproj, validação de versão, pipeline e documentação; identificar acesso autorizado ao NuGet.
2. [completed] Validar Debug/Release/versão fixa, pacote e consumidores mínimos; verificar testes relevantes.
3. [completed] Atender prioridade do usuário: renomear workflow para maintenance.yml, atualizar referências, validar e enviar à main.
4. [completed] OIDC validado: NUGET_USER=hugodeco, build.yml, environment production. Run 34278698353 success, credencial temporária emitida, nenhum pacote publicado.
5. [in_progress — bloqueado parcialmente] Commit/push e publicação concluídos; deslistagem bloqueada por HTTP 403 da chave NuGet. Identity e Fleet atualizados, validados e enviados à main.
6. [pending — depende da deslistagem] Encerrar o plano em relatório de atividade após a credencial permitir a limpeza. Estado remoto já conferido; registrar a verificação final de listed=false antes de concluir.

Critérios: rejeitar SemVer antigo e datas inválidas; preservar timestamp único em pacote e assembly; pacote instalável; nenhuma versão corporativa deslistada; relatar limitações de credenciais sem alegar sucesso.

Validações: testes do resolvedor de versões, MSBuild properties, dotnet pack e scripts/validate-package.sh, contratos bUnit; estado listed via API NuGet; status git remoto.

Checkpoint 1: contrato e documentação corrigidos; seis testes do resolvedor aprovados. Nenhuma chave local/repositorio; consulta de secrets organizacionais requer admin:org (403), não se conclui ausência de secret herdado. CI verificará disponibilidade sem revelar valor.

Checkpoint 2: Debug 1.99.0.0; Release UTC; Packing com VersionSuffix fixa alinha Version/AssemblyVersion/FileVersion. Pacote 1.26.908.5 gerado e validado em RCL e app net10.0 na raiz e /app (CSS global/isolation/JS). 596 testes bUnit aprovados. YAML dos dois workflows parseado. Dry-run NuGet confirma cinco versões listed=true; nenhuma alterada.

Checkpoint 3 em execução: 4f9d9a8 em main e tag v1.26.908.2011; CI 34273328536 confirmou credencial NuGet herdada da organização. Worktree principal SUI integrada com backup do csproj local; critique preservado. Preparadas em worktrees isoladas referências Identity (base ac07750) e Fleet (base 9dc8160); só serão commitadas após restore da versão pública. A varredura também encontrou referências históricas no AI/Genius, fora dos consumidores desta entrega; deslistagem preserva seus restores existentes.

Gate da release: main e tag falharam apenas WebKit ListsEscapeClipping...(1440,false), timeout na visibilidade do último item do Select (linha 107). Publish não executou. Antes de prosseguir no checkpoint 3, reproduzir e corrigir o gate; não rerodar sem diagnóstico. Chromium/Firefox/Lighthouse/pack passaram.

Gate reproduzido no WebKit local (1 falha/1 sucesso): índice 29 ativo, scrollTop=0. Instrumentação mostrou chamadas sobrepostas de interop. Corrigidos Select e Autocomplete para registrar estado despachado antes do await; teste original inalterado passou (2/2). Validando classe de interação nos três engines e bUnit antes de nova tag. Tag 1.26.908.2011 não publicou pacote.

Fix do gate: 25b06e8 enviado para main; nova tag v1.26.908.2020. 18 testes de interação (6 por engine) e 596 bUnit passaram. Referências preparadas ajustadas à nova tag; aguardando NuGet público para restore/lockfiles.

Release confirmada: workflow 34274199136 success, incluindo WebKit e Publish. NuGet PUT respondeu Created às 20:25:26 UTC para 1.26.908.2020. Main CI 34274195537 success. Registration API ainda retornou 404 logo após push; aguardar disponibilidade para validar restore e deslistar.

Bloqueio confirmado: workflow de limpeza 34275484989 em c933db1 recebeu HTTP 403 no primeiro DELETE (1.27.0): “The specified API key is invalid, has expired, or does not have permission to access the specified package.” A mesma chave publicou com sucesso a release; é necessário configurar NUGET_API_KEY com permissão Unlist em Sufficit.Blazor.UI (ou usar a conta proprietária no NuGet). Não repetir enquanto a credencial/permissão não mudar. Nenhuma versão foi deslistada. Não há chave local nem sessão NuGet autenticada disponibilizada pelas ferramentas.

Consumidores: Identity a9f2f25 enviado à main, seis locks na versão pública 1.26.908.2020, restore locked-mode + build 16 projetos + 189 testes UI/documentação aprovados. Fleet: 230 testes na base 9dc8160; push rejeitado por avanço concorrente bfcd95f (steward), rebase aplicado sem conflitos e testes repetidos sobre o resultado final.

Estado final desta execução (bloqueio parcial, não concluir/remover o plano):
- SUI main c933db1b6f522c09ba7622477d695ed082da4c79, pacote público 1.26.908.2020 listado; tag aponta ao código de release 25b06e8. Workflow release 34274199136 success. Workflow main após ajuste do leitor do índice 34275472827 ainda em andamento na última consulta.
- Identity main a9f2f25374ff67fffa6b98072703f54fafdb1c16; checkout principal integrado, limpo. CI 34275572601 ainda em andamento na última consulta.
- Fleet main 738432d11e139814139fdbb1c2600d5a88b51b0f; 240 testes passaram após rebase sobre bfcd95f. Checkout principal sujo e antigo preservado; worktree da tarefa limpa.
- 12 testes Python, 596 bUnit e 18 interações em navegadores passaram localmente. Release teve todos os gates verdes. Identity: build 16 projetos, restore locked e 189 testes UI/docs; Fleet: 240 testes finais.
- Consulta pública após o HTTP 403 confirmou que 1.27.0, 1.28.0, 2.0.0, 2.1.1, 2.2.1 continuam listed=true. Nenhuma chamada de remoção teve sucesso.
- Catálogo Pages 34274195509 publicado com sucesso. Servidor local de teste 5194 encerrado por PID explícito.
- Após configurar NUGET_API_KEY com escopo Unlist para Sufficit.Blazor.UI, executar: gh workflow run maintenance.yml --repo sufficit/sufficit-blazor-ui --ref main -f replacement=1.26.908.2020 -f apply=true. Só repetir quando houver mudança da credencial/permissão. Depois consultar os cinco estados e preservar todas as versões corporativas.
- Relatórios dos consumidores: docs/activities/202609081734-sui-corporate-version.md em Identity e Fleet. Não houve novo deploy de serviços neste pedido.

Nome solicitado entregue em 1722d5a (main remoto e checkout principal): .github/workflows/maintenance.yml, YAML e conteúdo original conferidos. Relatório docs/activities/202609081743-maintenance-workflow-name.md. Deslistagem continua bloqueada por credencial; nenhuma nova tentativa executada.

Nova prioridade: usuário configurando a autorização NuGet para build.yml e autorizou adaptação. NuGet/login v1 resolvido para 8d196754b4036150537f80ac539e15c2f1028841. Proprietário público do pacote: sufficit; usar esse perfil como padrão com variável NUGET_USER opcional. Manter publicação por tag e todos os gates existentes. A limpeza maintenance.yml segue separada, sem repetir o DELETE negado.

OIDC implementado: NuGet/login por SHA; id-token write só em publish e no teste manual; login após validar pacote; sem fallback à API key estática. YAML/estrutura verificados. O perfil padrão sufficit é uma hipótese baseada no perfil público, não confirmação do criador da política; a autenticação real confirmará ou indicará necessidade de NUGET_USER.

OIDC entregue em 70f69ff e 3da8da5, main remota e checkout principal conferidos. Verificação real 34276979022 falhou HTTP 401: No matching trust policy owned by user sufficit was found. GitHub emitiu token; NuGet recusou troca. Removido fallback de usuário: NUGET_USER obrigatório, definido por variável Actions. Não repetir sem a identidade/política correta. Relatório parcial docs/activities/202609081750-nuget-trusted-publishing.md. Nenhuma release extra; maintenance segue bloqueado por Unlist.

Usuário confirmou hugodeco como criador da política NuGet. A nova tentativa é justificada pela correção da identidade usada no login; não reutiliza a hipótese sufficit que recebeu 401.

Validação com usuário confirmado: variável Actions NUGET_USER=hugodeco gravada e lida pela API. Run 34277547213 executou login com hugodeco e retornou 401: Workflow mismatch for policy sufficit-blazor-ui: expected maintenance.yml, actual build.yml. É necessário alterar Workflow File da política NuGet para build.yml e salvar; não renomear/burlar o workflow para corresponder a uma autorização diferente. Não houve publicação nem nova tentativa de DELETE. Consulta pública confirma cinco versões antigas ainda listadas.

Usuário confirmou a atualização da política NuGet; nova execução de verificação OIDC autorizada pela mudança externa, mantendo os demais itens pendentes.

Diagnóstico novo após atualização autorizada: NuGet HTTP 401 Environment mismatch for policy sufficit-blazor-ui: expected production, actual empty. GitHub tem somente github-pages e sua proteção será preservada. Vincular publish e publishing-authentication ao environment production corresponde à política definida pelo usuário, sem alterar segredos ou publicar pacote.

OIDC concluído: 2fdb090 alinhou environment production em ambos os jobs. Run 34278698353 success, log Successfully exchanged OIDC token for NuGet API key; etapa confirmou saída não vazia. Somente autenticação executada, publish skipped. Maintenance permanece com API key estática sem permissão Unlist, sem tentativa nova de DELETE nem alegação de remoção.
