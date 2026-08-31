# Hierarquia visual: em que ordem a tela é lida

Uma tela não é lida — ela é **varrida**, e só depois lida em partes. Quem
projeta escolhe a ordem dessa varredura ou deixa que ela aconteça sozinha; não
existe a terceira opção. Quando ninguém escolhe, o olho pega o que for maior,
mais escuro ou mais isolado, e isso quase nunca coincide com o que importa.

Este documento é a regra do sistema visual sobre isso. Vale para todo consumidor
do SUI, não para uma tela específica.

## A pergunta que todo painel precisa responder

> Se a pessoa olhar por dois segundos e for embora, o que ela leva?

Se a resposta for "depende", o painel ainda não tem hierarquia.

## Os papéis de leitura

Cada pedaço de texto de um painel cumpre **um** papel. O papel decide o tamanho,
o peso e a posição — não o gosto de quem escreveu.

| # | Papel | O que responde | Forma |
| --- | --- | --- | --- |
| 1 | **Resposta** | "O que é isto?" | Maior de todos, peso 700. Sozinho no topo |
| 2 | **Título do painel** | "De que assunto é este cartão?" | `h6`, peso 600 |
| 3 | **Título de seção** | "Onde eu estou dentro do cartão?" | **Menor de todos**, 700, caixa alta, espaçado |
| 4 | **Rótulo** | "O que é este campo?" | Pequeno, cor secundária |
| 5 | **Valor** | "Qual é a resposta?" | Maior que o rótulo, cor primária |
| 6 | **Consequência** | "O que acontece se eu clicar?" | **Bloco com moldura e ícone**, colado na ação |
| 7 | **Dica** | "Como isto funciona?" | O menor do corpo, cor secundária |
| 8 | **Limite** | "O que isto NÃO faz?" | Recuado, com barra lateral neutra |

O título de seção ser o **menor** costuma soar errado e não é: ele é placa de
rua. Placa existe para ser **achada** por quem procura, não lida por quem passa.
Título de seção do tamanho do conteúdo compete com o conteúdo.

## As quatro regras

### 1. Cor não cria hierarquia

Cor **distingue** duas coisas que o olho já achou. Ela não decide qual achar
primeiro.

Um aviso importante pintado de laranja, do mesmo tamanho e na mesma posição de
uma dica neutra, não é um aviso: são duas dicas, uma delas colorida. E para quem
não distingue os tons — 1 em cada 12 homens — não é nem isso.

**Promoção é por forma:** moldura, fundo, ícone, isolamento, posição. A cor
entra por último, confirmando o que a forma já disse.

Corolário: dentro de um bloco promovido, o **texto fica na cor normal**. Quem
sinaliza é a moldura. Pintar o texto junto não acrescenta sinal e ainda piora o
contraste de leitura.

### 2. Razão mínima perceptível

13px contra 14px é uma diferença que existe na régua e não na percepção. Para
que dois papéis se separem sem que a pessoa precise compará-los lado a lado, a
razão precisa ser de **cerca de 1,25×**.

Uma escala com quatro tamanhos entre 12 e 15px não é uma escala — é ruído com
casas decimais. Menos degraus, mais distantes.

### 3. Escassez é o que dá significado

Se toda dica virar bloco com moldura, nenhum bloco significa nada. O critério
tem que ser dizível numa frase, e a mesma frase vale para toda a aplicação:

> Ganha moldura o que **interrompe o trabalho** de quem está lendo — reinicia,
> derruba a sessão, apaga, cobra.

O resto continua dica. Um painel com cinco controles e um único bloco promovido
comunica mais que um com cinco blocos.

### 4. Distância significa parentesco

O olho agrupa pelo que está perto. Espaçamento uniforme entre tudo destrói
qualquer agrupamento: a explicação de um controle fica tão perto do controle
seguinte quanto do próprio, e a pessoa não tem como saber a quem ela pertence.

Regra prática, em degraus bem separados:

- dentro de um campo (rótulo, controle, dica dele): **4px**
- entre campos do mesmo assunto: **12px**
- entre assuntos: **24px + uma linha**

O que informa é a **diferença** entre as distâncias, não o valor absoluto.

## Posição: a consequência mora junto da decisão

Um aviso de consequência colocado no topo do grupo é lido **antes de a decisão
existir** — a pessoa ainda está entendendo o campo. Quando ela finalmente vai
clicar, o aviso já saiu da memória de trabalho.

Ordem correta dentro de um grupo de ajuste:

```
título da seção     ← onde estou
rótulo + controle   ← o que estou mudando
dica                ← contexto, se precisar
consequência        ← o que vai acontecer   (colado no botão)
botão               ← a ação
```

## Antipadrões

| Antipadrão | Por que falha |
| --- | --- |
| Aviso só colorido | Invisível na varredura; inexistente para daltônicos |
| Tudo em negrito | Se tudo é promovido, nada é |
| Título de seção grande | Compete com o conteúdo que ele deveria apenas rotular |
| Espaçamento uniforme | Impede agrupamento; a dica parece pertencer ao campo de baixo |
| Consequência no topo do grupo | Lida cedo demais, esquecida na hora do clique |
| Quatro tamanhos entre 12 e 15px | Nenhum degrau é perceptível |

## Verificação

Duas checagens que pegam quase tudo, e nenhuma delas precisa de ferramenta:

1. **Aperte os olhos** (ou desfoque a captura de tela). O que continua legível é
   o que a pessoa vê nos primeiros dois segundos. Se for a coisa errada, a
   hierarquia está invertida.
2. **Converta para escala de cinza.** Tudo o que só se distinguia por cor
   desaparece. O que sumiu precisa de forma, não de tom.

## No SUI

- `SUIAlert` com `ToneValue` e `Icon` é o veículo pronto para consequência de
  peso; para avisos embutidos num grupo de campos, um bloco com barra lateral,
  fundo esmaecido e ícone cumpre o mesmo papel com menos peso visual.
- `SUITone.Warning` sinaliza interrupção; `SUITone.Danger`, perda.
- Os tokens `--sui-fs-*` já formam uma escala. O erro comum não é a falta de
  tokens: é usar quatro deles vizinhos no mesmo bloco.
