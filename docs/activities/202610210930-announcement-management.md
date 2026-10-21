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
executados em fixtures isoladas. Entrega e teste real concluídos; evidências abaixo.


## Entrega e validação real — 21/10/2026

Integração na main: EFData PR23 (35c7b153), Standard PR28 (6dcc8d5e),
Endpoints PR49 (abdaa8ca), SUI PR58 (1b7cf91a), Blazor PR207 (bcf33070).
A interface final inclui ações SUI Text, com distribuição igual no desktop e
menu ao tocar no registro móvel. Sem estilos específicos da página.

Validação:18 testes EF SQLite,12 API,34 anúncio/cliente,999 SUI;
195 testes conjuntos IA/anúncios; Standard488 aprovados,25 ignorados;
mais35 testes ServiceCollections. SUI PR58 passou Chromium/Firefox/WebKit,
Lighthouse, build/pack e CodeQL. Orçamento CSS/JS ajustado conforme política de
3%, com medida e justificativa inline. Regressão de descoberta IA corrigida
na descrição da própria ferramenta, sem modificar o algoritmo global.
Fixture SQLite do diretório de clientes alinhada à tabela de reconciliação
já utilizada pelo leitor EFData. Dependência mínima EFData1.26.1021.1228.

Migração management_revision aplicada após backup privado de esquema/registros
em eveo-apps:/root/announcement-management-before-20261021.json (0600).
Coluna confirmada nos três destinos. Rollout API e Blazor concluído em
eveo-apps,apoint-apps,castrum-apps:health Healthy, DLLs iguais por aplicação e
hashes de appsettings preservados. Blazor final1.26.1021.1258+3dbbd367.
Backups preservados em /opt/sufficit-endpoints.before-ann-management-20261021
 e /opt/sufficit-blazor-server.before-ann-management-20261021 em cada host.

Na primeira preparação do Blazor em Eveo faltaram configurações no staging;
restauradas imediatamente do backup. O wrapper do fluxo oficial passou a
preservar appsettings da instalação antes da troca e verificar hashes/health.
Rollout final passou essa verificação em todos os hosts.

E2E pelo tablet no cliente de testes9acb8479-321a-4ba0-b95c-9505e3057492:
registro isolado338db7e2-278a-4183-983d-2e99b7a3a68b criado12:50:52UTC,
desativado12:52,reativado12:53,excluído12:57. Cancelamento da primeira
confirmação não enviou escrita. Exclusão confirmada nas duas tabelas;
áudio a5034c66-c621-448f-acb1-488375164184 permaneceu cadastrado.
Oito eventos administrativos (solicitação/conclusão de cada operação), com
ator autenticado correto, persistiram após a exclusão. Versões mudaram após
cada transição, inclusive reativação. Auditoria administrativa existente
preservada; retenção de execuções continua separada (5 dias).

Filtros reais:Todos sem parâmetro/chip;Desativados gerou status=inactive,
chip e três registros;remover chip retornou cinco registros e retirou o
parâmetro. Piloto legado, DID e IVR existentes preservados. Aliases legados
não reconciliados continuam protegidos; esta entrega não os migra.

Link:https://blazor.sufficit.com.br/pages/telephony/announcements?contextid=9acb8479-321a-4ba0-b95c-9505e3057492

CI da main ainda em execução no fechamento inicial deste registro; validações
locais e instalação acima são independentes e foram observadas diretamente.


Conferência final no tablet com a versão1258:viewport390px e scrollWidth390px;
cada registro contém apenas título e áudio na segunda linha. Menu Editar/
Desativar/Excluir abriu dentro do viewport, de x165 a357px e y430 a580px.
Capturas reais:tmp/ann-management-tablet-mobile.png e
 tmp/ann-management-tablet-mobile-menu.png no workspace. Sem filtro na URL
nem chip no estado inicial. Backups e configurações preservados nos três hosts.
