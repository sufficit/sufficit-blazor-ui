# Ícones nos seletores SUI

Objetivo: restaurar os ícones da caixa postal através de API reutilizável da biblioteca.
Referência: captura original e pedido do usuário; manter ícone fixo verde/âmbar no campo e ícone de padrão/silêncio no valor/menu. SUIIcon e SUIColor são autoridades de renderização/tokens. Sem CSS na feature.

1. CONCLUÍDO — Adicionar StartIcon/IconColorValue ao SUISelect e Icon/IconColorValue ao SUISelectItem, incluindo atualização dinâmica, estilo e testes.
2. CONCLUÍDO — Restaurar ícones na caixa postal e demonstrar API no catálogo.
3. CONCLUÍDO — Verificar testes, CSS, build consumidor e navegador (ícones, rolagem, teclado, responsividade); registrar entrega.

Deploy do consumidor continua pendente no plano docs/PLAN-VOICEMAIL-DROPDOWNS.md do sufficit-blazor, sem nova autorização.

Evidências: 1135 testes passaram; build consumidor/fixture sem erros; CSS gerado e validado dentro do orçamento; navegador 1440/768/390 com ícones, seleção, teclado e uma área rolável. Capturas /tmp/voicemail-select-evidence. Sem publicação.

Consumidor publicado por autorização posterior: eveo-apps, versão 1.26.1029.0121, biblioteca compilada pela ProjectReference; hashes verificados. Sem release NuGet/commit da biblioteca nesta etapa.
