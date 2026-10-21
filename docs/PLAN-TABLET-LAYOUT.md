# Ajustes no tablet físico

Objetivo: cabeçalho legível em retrato e diálogo utilizável com teclado no Lenovo TB-J706F.
Referência travada: capturas reais em tmp/tablet-real; DESIGN.md e tokens SUI, sem redesenho.

1. CONCLUÍDO — corrigir a composição responsiva do SUIPageHeader e o viewport do diálogo/consumidor para teclado; manter seleção e persistência.
2. CONCLUÍDO — testes de regressão e validação no Lenovo real: retrato/paisagem, painel aberto, escrita, teclado e descarte sem mutação.
3. EM ANDAMENTO — integrar, publicar SUI e implantar Blazor pelo script oficial; conferir health/assets e registrar evidências.

Critérios: título/descrição com largura legível; ações quebram em linha própria antes de comprimir texto; com teclado, campo e ações alcançáveis por rolagem; nenhum cancelamento real.

Pré-validação real com os arquivos candidatos: descrição passa de87px para439px; teclado aberto reduz viewport de1112px para728px; rodapé termina650px, acima do teclado. Suíte SUI:972 testes aprovados.

Validação candidata:972 testes SUI e36 serviços/cancelamento; header390/800/1024/1440px; Lenovo retrato com IME728px e rodapé650px; paisagem com IME312px e botão279px após rolagem.

CI da tag150 encontrou assert antigo de display:grid no teste de carregamento CSS. Atualizar contrato flex e acrescentar regressão geométrica com ações largas antes de nova tag; sem publicar tag150.

Regressões reais do catálogo executadas localmente:2 aprovadas (carregamento CSS e ações600px em390/800px). Publicar nova tag com o teste corrigido.
