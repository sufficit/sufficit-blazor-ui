# SUITooltip

Envolve o conteúdo em um anchor e publica `aria-describedby` somente enquanto
o tooltip está ativo. `Text`, `Placement`, `ShowDelay`, `HideDelay`, `Arrow` e
`Disabled` controlam a experiência; conteúdo essencial nunca deve existir só
no tooltip.

O módulo cria um único portal compartilhado no `body`, posiciona Left/Right/
Top/Bottom e limpa timers, listeners, atributos ARIA e portal quando a última
instância é descartada. Os estilos globais ficam em `sui-portals.css` porque o
portal está fora do scope Razor.
