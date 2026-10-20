# Separação do painel expansível

Objetivo: distinguir cabeçalho e corpo na SUI e aplicar o componente no cancelamento.
Referência: captura do usuário; tokens SUI e direção existente. Sem alterar motivos ou persistência.

1. CONCLUÍDO — ajustar espaçamento, hierarquia e separador no SUIExpansionPanel; migrar o diálogo para o componente.
2. CONCLUÍDO — validar testes e renderização aberta/fechada em desktop e celular.
3. EM ANDAMENTO — integrar, publicar versão da biblioteca e implantar consumidor pelo deploy oficial; verificar saúde e registrar entrega.

Evidência: testes de componentes/cancelamento; capturas reais de fixture; CI/tag NuGet; health do consumidor.

Validação: 970 testes SUI aprovados antes da regressão dedicada; 21 testes de cancelamento aprovados. Chromium: 1280/390px, aberto/fechado, 16px de padding, divisor 1px, sem overflow. Fixture real renderizada via bUnit, sem operação em cliente.
