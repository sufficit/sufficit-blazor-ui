---
target: como estilizar melhor SUIButton e SUIIconButton
total_score: 13
max_score: 20
na_heuristics: 3,5,7,9,10
p0_count: 0
p1_count: 0
timestamp: 2026-09-08T17-08-18Z
slug: src-styles-sui-buttons-css
---
# Proposta de acabamento dos botões Sufficit

Avaliação independente em duas frentes: A, revisão visual; B, detector e medidas de navegador. Alvo: src/styles/sui-buttons.css e as demonstrações SUIButton/SUIIconButton. Modo Operate; a vitrine é evidência dos componentes, não um produto operacional inteiro. Preferências adotadas do usuário: preservar laranja, evitar visual bruto ou tonal excessivamente pálido, manter proporções e organização automática. Nenhuma mudança de interface executada nesta avaliação.

## Impressão e especificidade

O laranja identifica a Sufficit, mas a família ainda apresenta variações de forma e peso que não correspondem às funções. O maior ganho está em uniformizar o acabamento e dar papéis claros às variantes. A ação principal pode manter o laranja preenchido e o gradiente existente; a suavidade deve vir também da geometria, hierarquia e resposta visual.

## Heurísticas aplicáveis

| Heurística | Nota | Evidência |
| --- | --- | --- |
| 1. Visibilidade de estado | 3/4 | Contador, foco e disabled presentes; active de Outlined/Text igual ao hover |
| 2. Correspondência com mundo real | 3/4 | Ações reconhecíveis; ícones dependem de contexto |
| 3. Controle e liberdade | n/a | Sem fluxo reversível neste alvo |
| 4. Consistência | 2/4 | Raios, pesos e cores diferentes na comparação de botões equivalentes |
| 5. Prevenção de erros | n/a | Sem operação real |
| 6. Reconhecimento | 3/4 | Texto claro; ícone isolado ganha com ajuda contextual |
| 7. Eficiência | n/a | Fora da revisão visual restrita |
| 8. Estética/minimalismo | 2/4 | Salto de intensidade entre Filled/Soft e variação de geometria |
| 9. Recuperação de erros | n/a | Sem cenário observado |
| 10. Ajuda/documentação | n/a | Fora do alvo visual |
| Total | 13/20 | Avaliação heurística limitada aos controles, sem certificação do produto |

## O que preservar

- Proporção: texto 14 px e ícone textual 17,5 px em Medium/Large; área desktop 40/44 px. IconButton usa 21 px para o desenho. Mobile respeita 44 px.
- Espaçamento automático com gap/wrap; foco visível, disabled nativo e feedback no contador.
- Cor principal configurada pelo tema, com contraste verificado no estado observado.

## Prioridades

1. P2 — Unificar forma e peso. Filled/Soft/Outlined/Text do botão textual têm raios 10/14/8/8 px; IconButton fica em 8 px nas quatro variantes. Outlined/Text usam peso600 e Filled/Soft500, fazendo ações secundárias parecerem mais pesadas. Proposta: raio comum equivalente a10px no tema atual e peso500; manter escolhas circulares explícitas. Comando de execução sugerido: impeccable polish.
2. P2 — Definir hierarquia e identidade. O Filled textual usa laranja #c2410c; o equivalente IconButton usa Default quase preto, pois o exemplo não passa Primary. Soft textual começa em12% de tinta, criando salto grande até o preenchido. Proposta: Primary explícito para a ação de marca no exemplo; utilitários neutros; fundo tonal mais presente, inicialmente18%, com borda discreta e texto escuro. Validar claro/escuro antes de fixar os novos valores. Comando: impeccable colorize.
3. P2 — Completar o estado pressionado. Outlined/Text têm o mesmo fundo em hover e active. Proposta: resposta de pressão distinta usando cor/borda/sombra e a transição existente de160ms; preservar foco por teclado e movimento reduzido. Comando: impeccable animate, seguido de polish.

## Receita visual proposta

| Papel | Tratamento |
| --- | --- |
| Principal | Laranja da marca, gradiente discreto existente, sombra curta |
| Secundária | Superfície neutra, contorno fino e texto com peso500 |
| Tonal/Soft | Fundo laranja mais presente, borda discreta, texto laranja escuro |
| Ícone | Mesma geometria e cor da ação equivalente; neutro para ferramenta auxiliar |

A direção usa tokens SUI existentes. Open Props serve de referência para organizar escalas de raio, sombra e movimento (https://open-props.style/), sem introduzir dependência ou trocar a marca.

## Evidência B

Medidas desktop: SUIButton Medium116×40px; IconButton40×40px. Mobile390px: ambos com altura44px. Soft textual: #f8e8e2 em repouso, #f4ddd3 em hover e #f2d5ca pressionado; texto #8c3415. Contrastes aproximados estáveis6,74/6,16/5,79:1. Filled textual: #c2410c com texto #fff7ed, razão aproximada4,88:1 antes do gradiente escurecedor. Esses dados justificam experimentar um tonal mais presente, não dispensam validação da proposta.

Detector CLI em três Razor explícitos retornou zero achados. Limite: não há compreensão de Razor; a varredura de diretório não inclui essa extensão. Overlay executado em duas páginas gerou apenas um aviso genérico de fonte por página, sem evidência para mudar a tipografia. A leitura manual e as medidas sustentam os achados acima.

## Personas e observações

Para uso frequente, estados de pressão iguais reduzem a percepção de resposta. Para iniciante, o ícone isolado depende do contexto: tooltip e nome acessível devem acompanhar ações ambíguas. Para uso móvel, a área mínima já foi confirmada, sem defeito a corrigir neste aspecto. Carga cognitiva baixa no controle individual; não aplicar limites de opções de formulários à lista demonstrativa de variantes.

O rótulo de arquivo longo é fixture de resistência e deve continuar exercitando quebra; não é uma recomendação de copy para ações reais. As capturas B incluem foco: o anel nelas não é borda permanente.

Perguntas dispensadas pela instrução explícita de autonomia e pelas preferências já definidas. A proposta não altera nem publica código.
