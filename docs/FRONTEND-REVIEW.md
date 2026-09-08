# Revisão de frontend da SUI

A revisão de setembro/2026 usa sufficit-frontend e o catálogo de 68 componentes como ponto de partida. A matriz distingue correções novas de contratos existentes que precisam permanecer estáveis. Os testes exercitam comportamentos e geometria; não representam certificação integral de acessibilidade.

| Área | Evidência e decisão | Validação |
| --- | --- | --- |
| Campos de texto/número/checkbox | Limpeza por teclado deve devolver foco; erros devem referenciar elementos existentes; preservar parsing e EditContext | bUnit e teclado no navegador |
| Select/autocomplete | Item ativo precisa permanecer visível; Tab deve sair da lista; Enter do autocomplete não deve submeter formulário; lista deve escapar do recorte do contêiner | lista longa, teclado, formulário e overflow |
| Botões e variantes | Link de LoadingButton bloqueado; retirada de ícones altera largura; texto longo não deve sair da caixa; estado busy precisa ser exposto | navegação, dimensões idle/busy e texto longo |
| Tema e ícones | Cores info/success/warning/error possuem contrastes próprios, ignorados pelo CSS do botão; ícone preenchido não deve desaparecer no fundo | estilos computados em tema customizado/claro/escuro |
| Composição e espaçamento | Grade em painel estreito de desktop conserva duas colunas pelo viewport; adaptação deve considerar espaço disponível | painel de 340 px com viewport de 1440 px e mobile de 390 px |
| Abas/drawer/navegação | Manter orientação, foco, PathBase, seleção visível e fechamento responsivo existentes | suíte de regressão Server |
| Diálogos/tooltips/portais | Preservar camada, foco, contraste herdado e descarte de listeners | suíte de regressão Server |
| Tabela/paginação/filtros | Preservar teclas, foco, estado vazio, paginação e identidade de linhas | testes existentes e vitrine |
| Feedback/animações | Preservar preferência reduced-motion em skeleton, progresso, toast, drawer e abas; evitar animação decorativa obrigatória | testes de movimento e acessibilidade |
| Distribuição | Exemplos executáveis, código copiável, estilos e módulos no pacote e sob subpath | catálogo gerado, pack/consumer, Pages |

## Compatibilidade

As melhorias preservam callbacks, nomes e variantes existentes. Não há dependência de framework visual externo. Alterações de geometria limitam-se a estados com defeito: carregamento com ícones, conteúdo que exige quebra e campos em contêineres estreitos. A validação cobre o Cloud como consumidor de referência, sem publicar mudanças de regra de negócio.

## Custo dos recursos

O CSS global passou de 53.984 para 54.339 bytes brutos (+355). As versões gzip (9.969 bytes) e Brotli (8.730 bytes) continuam dentro dos limites originais. O limite bruto foi ajustado para 54.500 bytes, e o CSS isolado para 26 KiB, acomodando a grade adaptativa. O orçamento agregado de JavaScript Brotli passou de 8 KiB para 8,5 KiB para o teclado do autocomplete, que reutiliza o posicionamento do select. O teste de transferência de CSS do catálogo considera 94 KiB para os estilos combinados da biblioteca e do host.

## Como conferir

Na vitrine, abra **Padrões → Formulários e ações em espaços pequenos**. Os exemplos permitem conferir foco ao limpar, formulário em painel estreito, listas com 30 opções, carregamento com ícones, link e contraste customizado. O código completo está disponível para copiar na mesma página. A rota `/interaction-review` do catálogo Server exercita a mesma composição nos testes.

As imagens de referência do catálogo precisam ser recapturadas no CI: a remoção da reserva de duas linhas de rótulo no celular altera intencionalmente a altura dos formulários. A atualização não substitui os testes funcionais nem a comparação posterior com as novas referências.
