# SUINavGroup

Em navegação expandida, NavGroup renderiza um button associado por
`aria-controls` ao collapse e coordena accordion entre irmãos. Em rail raiz,
renderiza trigger de 48 px e flyout com menu, fechamento atrasado e exclusão
mútua entre flyouts.

O módulo colocalizado mede e reposiciona apenas flyouts conectados, usa
ResizeObserver e listeners de viewport por instância e libera todos no dispose.
`Title` fornece nome acessível; `TitleContent` muda apresentação, não elimina a
obrigação do nome.
