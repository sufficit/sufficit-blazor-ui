# Painel expansível: hierarquia e espaçamento

Referência: captura do usuário no cancelamento e tokens existentes da SUI.
Cabeçalho aberto usa surface-2, peso 600 e alvo mínimo44px. Corpo tem divisor,
padding16px e line-height de leitura. Margens externas de filhos são normalizadas.
API pública preservada; aria-expanded agora usa strings explícitas true/false.
Consumidor migra de details nativo para SUIExpansionPanel.

Validação: suíte SUI e regressão de estado acessível; 21 testes de cancelamento.
Chromium em1280/390px, aberto/fechado: padding16px, divisor1px, sem overflow.
Capturas em /mnt/workspaces/sufficit/tmp/expansion-spacing-preview.
Dados sintéticos; nenhuma operação real em contrato.

Publicação: tag e evidência de produção serão registradas após os gates.
