# Campos em largura inteira com espaçamento automático

## Objetivo

SUIFormGrid deve agrupar campos em colunas e conteúdo em largura inteira, como observações, usando seu próprio token Spacing. Consumidor não precisa de margem/gap local. Referências: screenshot e instrução do usuário, DESIGN.md e comportamento existente da grade. Preservar consumidores sem o novo slot e alterações concorrentes em SUIStack.

## Etapas

1. **Concluído** — Adicionar FullWidthContent à grade e migrar configurações/observações do editor de serviços, removendo a compensação CSS local. Atualizar AI e documentar API.
2. **Concluído** — Conferir compatibilidade em testes da grade e medir largura/espaçamento no navegador em desktop/mobile, inclusive Spacing personalizado e zero.
3. **Concluído** — Publicar consumidor com biblioteca atualizada nos três nós e conferir artefato/saúde; registrar entrega. Não alterar versão manualmente nem lançar pacote NuGet sem fluxo de release.

## Validação

Três testes da grade passaram, incluindo encaminhamento de ValueChanged no campo do novo slot e compatibilidade dos consumidores sem slot. Navegador com componentes reais mediu: Spacing 4 = 16px em desktop 1200px e celular 390px; Spacing 6 = 32px; Spacing 0 = 0px. Área em largura inteira em todos os casos, rótulo sem reserva para coluna vizinha, sem overflow. Na validação foi corrigida a dependência de um token de espaço zero inexistente: Spacing=0 agora usa zero explícito. Fonte de margem/gap local retirada do consumidor.

Blazor publicado nos três nós com versão `1.26.1014.1806+b0a7c28ba9918c5e4037c9089f634dc0cd555622`; biblioteca UI instalada SHA256 `7a6347f4774f627e91d4f0fdb06b6a97364601190c7ab34a7f5bd280eb5bb2e7`; CSS da biblioteca `6b2f72d66bed3323f2cdc07c0eae7d0889afbdab6e0321cc78ed69919d12f580`, conferido por HTTP em cada nó. Consumidor sem margem/gap compensatório. Sem nova publicação no NuGet e sem consolidação dos commits em main.
