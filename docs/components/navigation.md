# Navegação

Inclui NavLink, NavGroup e Tabs. Links mantêm anchor/NavLink nativo e expõem
`aria-current`; ListItem acionável renderiza button nativo.

Tabs implementa tablist/tab/tabpanel, IDs associados, roving tabindex,
ArrowLeft/ArrowRight, Home/End e foco. O módulo colocalizado apenas cancela o
scroll nativo dessas teclas e é descartado com o componente. Veja
[NavGroup](nav-group.md) para rail e accordion.

`SUITabPanel` é filho de `SUITabs`: registra-se no pai por cascata e `Text` é o
rótulo da aba. Seu template não renderiza nada; o pai instancia os painéis num
contêiner `hidden` e exibe o `ChildContent` do painel ativo no `tabpanel`.

`SUISlidingTabs` + `SUISlidingTabPanel` é a variante com micro-interações: a
trilha pill flutua num `surface-2` e um indicador desliza até a aba ativa com
bezier de overshoot (mola). Cada painel aceita `Icon` (chave do `SUIIcon` ou
markup de `SUIIcons`); o ícone ativo recebe um "pop" e o painel entra com
fade/slide curto. `ColorValue` muda o realce, `FullWidth` distribui as abas,
`Center` centraliza a trilha e `IconOnly` colapsa os rótulos (telas estreitas
ou toque fazem isso via CSS, mantendo o `aria-label` do botão). O teclado segue
o mesmo contrato do `SUITabs` (roving tabindex, setas, Home/End) e painéis
`Disabled` não são ativáveis. O módulo colocalizado posiciona o indicador por
`offsetLeft/offsetWidth` (ResizeObserver + `document.fonts.ready`) e reposiciona
sem animação no primeiro paint, resize e troca de fonte; `prefers-reduced-motion`
desliga transições e animações.

`SUIProgressSteps` é um `nav` (`AriaLabel`, padrão "Progresso da configuração")
com uma `ol` de botões. `Steps` traz os rótulos, `ActiveIndex` é clampado à
coleção e `ActiveIndexChanged` publica a seleção; sem esse callback nenhuma
etapa é clicável. `MaxReachableIndex` (padrão = etapa atual) mantém as
concluídas revisitáveis e desabilita as futuras. A etapa atual recebe
`aria-current="step"`, e cada botão anuncia "Etapa n de N: rótulo, status" com
`CompletedStatusLabel`, `CurrentStatusLabel` e `PendingStatusLabel`.

`SUIFilterScope` cascateia `FilterText` (nulo ou vazio mostra tudo) para os
`SUIFilterTree` abaixo dele; a cascata não é fixa, então reavalia a cada tecla —
combine com `SUITextField Immediate`. Uma folha `SUIFilterTree` casa quando
alguma de suas `Tags` contém o texto, sem diferenciar maiúsculas; folha sem
tags nunca casa enquanto há filtro. Com `Group`, o nó permanece visível
enquanto qualquer filho casar e se esconde com `display:none` em vez de
desmontar, para os filhos continuarem reportando. `IsMatch` e `IsFiltering`
são públicos para grupos que se expandem durante a busca.
