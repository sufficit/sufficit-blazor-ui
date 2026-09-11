# Texto auxiliar compartilhado: desbloqueio e publicação concluídos

O usuário pediu para resolver os bloqueios da entrega que unifica o texto
auxiliar dos campos SUI. A implementação já estava na main em 900f42d;
os gates falhavam após os novos controles numéricos de 1ebc3ae.

## Ajustes dos gates

Revisão deliberada de tetos para funcionalidades já presentes, com valores
medidos e margem pequena. Os testes seguem ativos; não houve remoção de
recursos, alteração de tolerâncias visuais ou de limites de timing/DOM.

| Verificação | Medido | Teto anterior | Teto revisado |
| --- | ---: | ---: | ---: |
| Módulos JS, Brotli agregado | 9.495 B | 9.216 B | 9.728 B (9,5 KiB) |
| CSS isolado, fontes | 27.372 B | 26.624 B | 27.648 B (27 KiB) |
| CSS agregado do catálogo, transferência sem compressão | 97.014 B | 96.256 B | 97.280 B (95 KiB) |

O incremento decorre do WheelStep/ArrowKeyStep/SpinnerStep e dos controles
visuais numéricos. O módulo numérico continua sendo importado sob demanda.
O helper compartilhado removeu 172 B de CSS isolado. Os tetos do bundle
global não mudaram: resultado 54.738 B bruto / 10.095 gzip / 8.828 Brotli.
Documentação atualizada em `docs/USAGE-SHOWCASE-WORKSPACE.md`.

Dois testes da vitrine falhavam depois de concluir suas asserções funcionais:
salvavam capturas em `/mnt/workspaces/sufficit/tmp`, inexistente e sem permissão
no runner. O destino passou para `TestContext.CurrentContext.WorkDirectory/artifacts`.
Os casos de teclado, readonly, disabled, limites e incrementos foram mantidos.

As referências visuais foram recapturadas no runner canônico pelo workflow
oficial e posteriormente comparadas numa execução normal. A diferença de
renderização local/runner foi tratada por referências do ambiente correto,
sem relaxar a tolerância.

## Validação e publicação

- Build Release completo: sem avisos/erros.
- Componentes: 605 testes aprovados localmente e no GitHub.
- Cinco testes locais de desempenho aprovados.
- Dois testes WASM locais de incremento/capturas aprovados.
- CI final: Chromium 58, Firefox 53 e WebKit 53 aprovados, incluindo a
  regressão de helpers em ambos os temas/larguras e overrides conjuntos.
  Casos exclusivos de outros hosts/engines foram ignorados conforme o contrato.
- Lighthouse, inspeção de pacote e comparação normal de imagens aprovados.
- Vitrine estática: 48 testes aprovados; deploy GitHub Pages concluído.
- Chromium público: TextField e Select com fonte 11,04px, margem superior zero
  e distância final de 4px. Capturas inspecionadas.
- CSS público comparado byte a byte com o bundle local: idêntico.

Evidências locais em `/tmp/sui-budget-release`: logs de CI, medições JSON,
capturas públicas e script de verificação. Os servidores locais temporários
foram encerrados pelos PIDs inspecionados.

## Referências entregues

- Ajuste dos tetos de assets: `da8651f24d9cba884346a7f7650d4c7b8243e2fc`.
- Teto agregado do catálogo: `bd496f21cb7500adea14f183a569e6d10f02eccb`.
- Referências visuais do runner: `09b4fbdccd4c7f4ae244bfe0c54ec7281f42b0cc`.
- Correção das capturas e revisão publicada: `27998c399c45bab0e0f14994088d6c4ce810aa8e`.
- Recaptura aprovada: https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34646689296
- CI final normal aprovado: https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34647041767
- Vitrine/deploy aprovado: https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34647041779
- Dropdown: https://sufficit.github.io/sufficit-blazor-ui/?component=SUISelect
- Textbox: https://sufficit.github.io/sufficit-blazor-ui/?component=SUITextField

O commit intermediário de teste usou skip ci enquanto a execução anterior
terminava; foi validado pelo workflow manual completo e pela execução normal
final. Não foi cancelada execução manual do usuário.

Esta entrega publica a biblioteca na main e a vitrine. Não cria release NuGet
nem executa deploy do aplicativo consumidor sufficit-blazor; os componentes
compartilhados chegarão a ele quando for compilado/publicado com esta revisão.
O bloqueio registrado na atividade anterior está resolvido para a entrega SUI.
