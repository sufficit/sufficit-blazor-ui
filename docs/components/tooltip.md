# SUITooltip

Envolve o conteúdo em um anchor e publica `aria-describedby` somente enquanto
o tooltip está ativo. `Text`, `Placement`, `ShowDelay`, `HideDelay`, `Arrow` e
`Disabled` controlam a experiência; conteúdo essencial nunca deve existir só
no tooltip.

O módulo cria um único portal compartilhado no `body`, posiciona Left/Right/
Top/Bottom e limpa timers, listeners, atributos ARIA e portal quando a última
instância é descartada. Os estilos globais ficam em `sui-portals.css` porque o
portal está fora do scope Razor.

## Título, corpo rico e descrição permanente

`Title` e `Content` (markup) montam um tooltip com título e corpo; o conteúdo é renderizado num elemento
oculto e clonado no portal ao abrir. `PersistentDescription="true"` mantém esse texto no DOM (oculto
visualmente) e o liga ao gatilho por `aria-describedby` desde a conexão — ícones de ajuda ficam descritos
mesmo fechados, sem spans escondidos no consumidor.

```razor
<SUITooltip Title="Escopos" PersistentDescription="true">
    <Content>Separe por espaço, ex.: <code>openid profile</code>.</Content>
    <ChildContent><SUIIconButton Icon="info" Title="Ajuda sobre escopos" /></ChildContent>
</SUITooltip>
```
