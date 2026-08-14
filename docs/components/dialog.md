# Dialog

Registre `AddSufficitUI()` e coloque um único `SUIDialogHost` na raiz interativa.
Abra conteúdo pelo `ISUIDialogService`; `ConfirmAsync` retorna o resultado sem
bloquear o renderer.

O host define `role="dialog"`, `aria-modal`, título associado, move o foco para
dentro, prende Tab/Shift+Tab, fecha por Escape e restaura o trigger. Substituir
um dialog ativo atualiza a referência sem acumular listeners. O módulo e o
tracker global de foco são ref-counted e descartados quando o host sai.
