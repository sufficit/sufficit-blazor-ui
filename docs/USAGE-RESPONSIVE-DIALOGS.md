# Cabeçalhos e diálogos no tablet

SUIPageHeader reserva uma base legível para a identidade e permite que ações
largas ocupem outra linha. Até760px, a identidade ocupa a linha inteira; não
fixar altura da identidade quando trocar a direção de flex.

SUIDialogHost usa max-height90dvh e rolagem interna. No documento HTML do
consumidor Android/Chrome, declarar:

```html
<meta name="viewport" content="width=device-width, initial-scale=1.0, interactive-widget=resizes-content">
```

Isso faz o teclado reduzir também o layout viewport, em vez de cobrir o rodapé.
Em retrato o diálogo pode caber inteiro; em paisagem, usar a rolagem interna para
alcançar ações. Não usar viewport fixo nem impor limite com altura da tela física.
O suporte à diretiva depende do navegador; validar no dispositivo consumidor.
