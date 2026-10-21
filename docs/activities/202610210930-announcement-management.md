# Gestão reversível e exclusão de anúncios

Implementação autorizada após prévia desktop/celular aprovada pelo usuário.
Ativar/desativar preserva cadastro; excluir remove as duas linhas de cadastro,
mantendo áudio e auditoria. Propriedade, versão, consumidores e aliases legados
não reconciliados bloqueiam operações indevidas. management_revision impede
aceitar uma versão anterior após desativar/reativar. Migração aditiva documentada
no EFData, aplicada após backup privado antes do rollout, sem alterar cadastros.

Interface usa somente SUI, sem CSS da página. Desktop ações iguais; celular duas
linhas e cores semânticas, nome acessível informa situação, toque abre menu.
Filtros no cabeçalho/chips/URL: active/inactive; Todos é ausência, sem chip/parâmetro.
Editor substitui lista e preserva filtros. IA só consulta a seleção visível.

Validação conjunta:18 testes EF (SQLite),12 API,34 tela/cliente,999 biblioteca;
build SUI Release sem warnings. Navegador390/768/1366px: sem overflow horizontal,
menu nativo por toque, Escape/clique externo, ações159.28/159.30/159.30px.
Capturas de componentes reais no workspace tmp/ann-management-*.png.
A validação visual inicial usa dados sintéticos; testes de escrita/auditoria foram
executados em fixtures isoladas. Deploy e teste real ainda pendentes neste registro.
