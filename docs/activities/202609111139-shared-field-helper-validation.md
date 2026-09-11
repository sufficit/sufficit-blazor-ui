# Texto auxiliar compartilhado — implementação e publicação bloqueada

Pedido: dropdown e textbox devem ter exatamente o mesmo texto auxiliar,
com o TextField atual como referência; o padrão deve evoluir em conjunto,
permitindo personalização pelas aplicações.

## Implementação entregue

Commit `900f42d27844146a24c83d0e8f38133320773324`, enviado para main.

A exceção de `SUITextField.razor.css` foi removida e levada à regra global
`.sui-field__helper` em `src/styles/sui-foundations.css`. TextField,
Multiline, Select, Autocomplete, NumericField e DateField compartilham
92% de caption (11,04px no padrão), margem superior zero e o gap do campo
(4px no padrão). Cor, itálico, recuo e aria-describedby preservados;
mensagens de erro continuam independentes. CSS de aplicação pode alterar
os helpers em uma única regra, sem competir com isolation do TextField.

Documentação: `docs/DESIGN-VISUAL-HIERARCHY.md`. Demo Select recebeu helper;
catálogo gerado atualizado. A tela da captura foi localizada em
`sufficit-blazor/src/Features/Sales/Pages/RecurringItem.razor`: já usa
Select/TextField SUI e referência ao projeto local, sem CSS específico
necessário para este ajuste.

## Evidências locais

- CSS compartilhado: 54.738 B bruto / 10.095 gzip / 8.828 Brotli,
  dentro dos limites existentes; artefatos legados sincronizados.
- `dotnet build Sufficit.Blazor.UI.slnx -c Release -warnaserror`: sete
  projetos, zero avisos/erros.
- `FieldHelperBrowserTests`: 12 cenários aprovados (Chromium/Firefox/WebKit,
  1280/390px, SUIThemeProvider claro/escuro). Seis tipos de campo por cenário;
  igualdade de fonte e distância, aria-describedby, helper multilinha e
  personalização conjunta para 14px + margem de 6px (distância final 10px).
- Capturas reais inspecionadas em `/tmp/sui-shared-helpers`.
- Referências visuais locais recapturadas após mudança intencional de ritmo;
  comparação normal posterior aprovada, sem mudar tolerâncias.
- Catálogo e exemplos independentes verificados; git diff --check aprovado.

## Bloqueio de publicação confirmado

Suíte completa: 603 aprovados, duas falhas de tamanho. São preexistentes
à tarefa (HEAD anterior 1ebc3ae):

- JavaScript agregado: 9.495 B Brotli > limite 9.216 B. Nenhum JS foi alterado.
- CSS isolado anterior: 27.544 B > limite 26.624 B. Esta correção reduz para
  27.372 B, ainda acima do limite.

O pipeline Pages anterior `34522027493` já falhava no teste de componentes.
O novo pipeline `34611340632` confirmou as mesmas duas falhas e pulou o
job deploy. Não houve publicação da vitrine, release NuGet ou deploy do
consumidor sufficit-blazor. Os orçamentos não foram elevados, e nenhum gate
foi desabilitado. A correção do helper está na main; a liberação da vitrine
permanece pendente até resolver o excesso dos assets anteriores.

Referências:
- https://github.com/sufficit/sufficit-blazor-ui/commit/900f42d27844146a24c83d0e8f38133320773324
- https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34611340632

O plano `PLAN-shared-field-help.md` foi mantido por haver publicação pendente.
Servidor local temporário encerrado pelo PID inspecionado.
