# Overlays

Tooltip e Dialog gerenciam seu próprio interop ES module. O host não inclui
scripts SUI. Tooltip é a única superfície visual criada no `body`; por isso sua
regra vive em `sui-portals.css`. Dialog permanece na árvore Razor e o host deve
existir uma única vez. Veja [Tooltip](tooltip.md) e [Dialog](dialog.md).
