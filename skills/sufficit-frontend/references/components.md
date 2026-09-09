# Composição e contratos

## Da referência ao componente

Começar pela ação que o usuário precisa completar. Descrever dados, estado, ações principais/secundárias, estados de erro e limite responsivo. Procurar o equivalente no projeto antes de criar outro botão, seletor, diálogo ou sistema de ícones.

Um exemplo de galeria pode servir de referência para um fluxo sem fornecer uma implementação pronta para produção. Confirmar rótulos acessíveis, foco, teclado, ordenação, validação, filtros e ações que o exemplo só desenha. Não inventar depoimentos, métricas ou dados reais para preencher o layout; identificar fixtures como demonstração.

Para listas operacionais, a toolbar precisa alterar os resultados e manter contagem coerente. Em formulários, erro deve ficar associado ao campo, dados devem sobreviver a falha recuperável e submissão repetida deve ter tratamento. Em configuração perigosa, a consequência deve ser compreensível antes da ação e seguir as confirmações já previstas no produto.

## Onde o código pertence

| Sinal | Camada provável |
| --- | --- |
| Disposição de um fluxo ou regra de negócio | Aplicação/feature |
| Erro no contrato de uma primitiva reutilizável em consumidores | Biblioteca compartilhada |
| Valor que representa identidade, densidade ou aparência transversal | Tema/tokens |
| Exemplo que ensina API e variantes da biblioteca | Catálogo/showcase |

Não é uma obrigação criar todas essas camadas. Extração se justifica por contrato estável ou reutilização demonstrável. Um componente compartilhado recebe dados e eventos; serviços, entidades de domínio e permissões específicas ficam no consumidor.

## Estados como contrato

Selecionar os estados que existem para o componente: inicial, hover, foco, selecionado, desabilitado, carregando, vazio, erro, sucesso, conteúdo longo. Conferir a transição e o resultado, não só a classe CSS.

Variantes de tamanho precisam de diferença renderizada coerente com a API: altura, largura, padding, texto e ícone. Não fixar medidas globais para todos os controles. Verificar se um override de ícone/quadrado cancela o tamanho solicitado e se exemplos estão realmente passando o parâmetro. Preservar acessibilidade quando a variante visual é pequena.

## Evolução do design system

Usar quatro lentes de cobertura conforme a tarefa: linguagem de produto, fundamentos visuais, comportamento dos componentes e manutenção. Para cada lacuna relevante registrar evidência, impacto e decisão; usar “não se aplica” com motivo quando necessário, sem percentuais fictícios de maturidade.

Para alterar API pública, documentar comportamento, valores padrão, compatibilidade e migração quando houver quebra. Atualizar um exemplo executável e regressão significativa se a mudança afetar consumidores. Identificar versão/pin consumido e caminho de entrega: corrigir o repositório da biblioteca não atualiza automaticamente um produto já publicado.

Manter documentação próxima do código e com responsáveis/convenções existentes. Medir adoção ou regressões com dados reais quando esse for o objetivo; não exigir processo de governança novo em cada PR.
