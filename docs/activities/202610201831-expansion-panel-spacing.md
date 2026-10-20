# Painel expansível: hierarquia e espaçamento

Referência: captura do usuário no cancelamento e tokens existentes da SUI.
Cabeçalho aberto usa surface-2, peso 600 e alvo mínimo44px. Corpo tem divisor,
padding16px e line-height de leitura. Margens externas de filhos são normalizadas.
API pública preservada; aria-expanded agora usa strings explícitas true/false.
Consumidor migra de details nativo para SUIExpansionPanel.

Validação: 972 testes SUI, incluindo regressão de estado acessível; 21 testes de cancelamento.
Chromium em1280/390px, aberto/fechado: padding16px, divisor1px, sem overflow.
Capturas em /mnt/workspaces/sufficit/tmp/expansion-spacing-preview.
Dados sintéticos; nenhuma operação real em contrato.

Publicado: `v2.26.1020.2129`, commit `c56c3f4`.
Workflow https://github.com/sufficit/sufficit-blazor-ui/actions/runs/39500809056
concluído com sucesso: CSS, build, componentes, pacote, Chromium/Firefox/WebKit,
acessibilidade, Lighthouse e publicação OIDC no NuGet.

Consumidor Blazor `093480633465e97dc36800b744ec6823a35923ba`, deploy oficial
`python3 deploy.py eveo-apps --publish`, backup0600 em
`/root/sufficit-deploy-audit/20261020-before-expansion-spacing.tar.gz`.
Health público Healthy em2026-10-20T21:35:35Z:
`1.26.1020.2129+093480633465e97dc36800b744ec6823a35923ba`.
SHA256 assemblies UI/Server e CSS público conferem com o artefato.
Evidência /mnt/workspaces/sufficit/tmp/expansion-spacing-delivery.
Validação visual usa fixture; sem sessão autenticada de cliente em produção.
