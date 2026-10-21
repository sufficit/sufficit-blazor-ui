# Gestão responsiva de registros

SUIRecordTable<TItem> mostra Nome/Descrição/Situação/Ações no desktop e uma lista
de duas linhas no celular. TitleSelector, SecondarySelector, ActiveSelector e
KeySelector definem o conteúdo. StatusTemplate e Actions recebem o item.
SUIActionGroup divide toda a largura disponível em colunas iguais.
SUIResponsive oferece slots Desktop/Mobile pelo mesmo breakpoint.
SUIMenu usa popover nativo: clique/toque abre, Escape/clique externo fecha;
teclas direcionais/Home/End navegam pelas ações. Seu trigger pode ser título+
descrição. Situação consta no nome acessível para não depender só da cor.
Listas usam tokens success/neutral, sem cores privadas do consumidor.

Todos significa ausência de critério: não deve produzir chip ou parâmetro.
Menus de filtros ficam no cabeçalho; critérios ativos são SUIChip removíveis.
O consumidor mantém estado/URL/autorização/confirmação e auditoria.
O menu é uma camada de apresentação; callbacks recebem a intenção, não permissão.

A tela de anúncios no sufficit-blazor é o consumidor desta entrega. Validar
SUIMenu em Chrome touch, Escape/clique externo, viewport e ações reais.
