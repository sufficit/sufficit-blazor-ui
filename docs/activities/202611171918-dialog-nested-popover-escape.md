# Escape em seletores dentro de diálogos

O host SUI deixa o Escape chegar ao seletor quando há um popover aberto dentro do diálogo. A tecla seguinte pode fechar o diálogo conforme as opções do host. O comportamento evita descartar o rascunho ao fechar um calendário aberto.

A suíte da biblioteca passou com 1.246 testes. O fixture de tokens do consumidor confirmou que Escape fecha o calendário sem encerrar a geração e que o mesmo atalho fecha o diálogo sem seletor aberto. A correção está no código da biblioteca. A publicação NuGet segue o pipeline de release.
