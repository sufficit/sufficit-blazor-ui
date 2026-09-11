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
