# SUISelect

`SUISelect<T>` é controlado por `Value`/`ValueChanged`; opções são declaradas
como `SUISelectItem`. `Label`, `HelperText`, `Invalid` e `ErrorText` constroem o
contrato ARIA completo. Defina `Id` quando testes ou links externos exigirem um
identificador previsível.

Teclado: ArrowUp/Down abre e navega, Home/End saltam, Enter/Space selecionam,
Escape fecha e Tab segue para o próximo controle. O menu usa Popover API e não
é cortado por overflow. `MenuWidth`/`MenuMaxWidth` aceitam comprimentos CSS, com
guarda do viewport.

O módulo `SUISelect.razor.js` posiciona o popover, cancela apenas as teclas que
o listbox possui e remove listeners/ResizeObserver no dispose. Import relativo
funciona em raiz e PathBase; não adicione `<script>`.
