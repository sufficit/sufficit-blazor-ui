# Sistema visual aplicado

## Tokens e temas

Inventariar os valores realmente carregados antes de criar tokens. Separar, quando útil ao sistema, valor primitivo (cor/medida), papel semântico (superfície/texto/ação) e necessidade específica do componente. Não introduzir três camadas de aliases para resolver um único gap.

Um mesmo sufixo não significa a mesma medida entre bibliotecas: `space-5` pode ser 20px num sistema e 24px em outro. Conferir valor computado, escala do tema e consumidor. Mapear por papel e medida pretendida.

Open Props é uma opção de fundação, não um tema obrigatório. Se adotado, importar apenas os conjuntos úteis pelo pipeline do projeto. Normalização e estilos de elementos podem afetar controles existentes: conferir a cascata antes de incluí-los. Não adicionar um reset global em uma correção local. Validar o suporte a recursos como custom media no build/navegadores-alvo; uma variável CSS não substitui uma condição de media query.

Temas compartilham estrutura, mas podem precisar de pares diferentes de fundo/texto/borda/foco. Usar os papéis existentes também em hover, seleção, avisos e sobreposições; não inverter cores cegamente. Não copiar uma paleta de galeria por parecer semelhante à marca.

## Espaçamento e composição

Distinguir três relações: interior de um controle; itens do mesmo assunto; separação entre assuntos. Escolher os intervalos na escala local. Espaço antes de um novo subtítulo deve comunicar a mudança de assunto, não colá-lo à última linha anterior.

Usar `gap` quando o contêiner define a relação entre irmãos; padding delimita área interna e margem separa blocos independentes. Observar colapso de margens, `min-width`, alinhamento e quebra. Classes para uma pilha simples não devem herdar acidentalmente o contrato de um accordion, grade ou card.

Manter densidade adequada à tarefa: ferramentas operacionais podem ser compactas sem amontoar controles; páginas de apresentação podem ter mais espaço sem esconder a ação. Não impor o mesmo layout de cards a todos os assuntos. Tipografia e proximidade devem explicar a organização antes da decoração.

## Adaptação

Escolher breakpoints pelo conteúdo e convenções do projeto. Em grades, permitir que a coluna caiba na largura disponível; em flex, conferir tamanho mínimo dos filhos. Textos técnicos longos, traduções e zoom podem exigir quebra ou rolagem local deliberada.

Ao transformar tabela em lista compacta, conservar campos necessários, ações, filtros e paginação. Não esconder uma função para fazer o layout caber. Um controle deve continuar operável por teclado e toque; a área clicável pode superar o tamanho visual do ícone. Escala visual e tamanho do alvo são contratos distintos.

## Movimento e desempenho

Usar movimento para explicar mudança de estado, entrada de conteúdo ou relação espacial. Preferir duração e easing do projeto, evitando animar todos os blocos por padrão. `prefers-reduced-motion` deve deixar conteúdo imediatamente utilizável e preservar feedback funcional.

Antes de usar animação contínua, blur, sombras extensas ou transições de layout, avaliar custo de pintura/composição em dispositivo representativo. Reservar espaço para mídia/fontes e estados de carregamento para evitar saltos. Medir antes/depois quando a tarefa envolver desempenho; limites de bundle e renderização vêm do projeto, não de um número universal desta skill.
