# Alinhamento reutilizável dos botões consumidos pelo Genius

O Genius tinha controles e estados visuais próprios nos painéis de sessão.
A migração para SUI encontrou um problema compartilhado: o rótulo interno de
SUIButton não acompanhava o alinhamento configurado na API pública Style.

O rótulo agora ocupa a largura disponível, pode encolher e herda justify-content.
A política global existente continua responsável pela quebra de palavras.
Não foi necessário criar API nova: SUICheckbox já associa rótulos externos ricos
por Id, e callbacks nativos continuam no consumidor. Documentação e exemplos
copiáveis mostram alinhamento inicial, indicador lateral e texto longo.

A entrega incorpora a main fb0b046, preservando Soft e alinhamento vertical dos
ícones. O bundle acrescenta 60 bytes brutos à base atual: 54.620 brutos,
10.020 gzip e 8.758 brotli. O teto bruto foi ajustado de 54.600 para 54.700;
os demais limites permanecem iguais, abaixo do teto global de 56 KiB.

## Validação

- `dotnet test tests/Sufficit.Blazor.UI.Tests -c Release`: 581 aprovados.
- `npm run check:css`, catálogo regenerado/conferido e 68 exemplos compilados.
- `dotnet build src/Sufficit.Blazor.UI.csproj -c Release -warnaserror`: sem avisos/erros.
- `ShowcaseBrowserTests.ButtonContentHonorsAlignmentAndWrapsLongLabels`: 390/1280 px.
  Os dois casos falharam com o CSS anterior e passaram com a correção.
- Incluindo `SoftButtonsKeepContrastAlignmentAndKeyboardActions`, quatro casos
  de navegador passaram após incorporar a main; tema, tamanho, paleta, teclado
  e estados continuam funcionais.
- Pacote temporário 2.2.2-preview.sui624r2 inspecionado por `scripts/validate-package.sh`:
  consumidor net10.0, aplicativo executável na raiz e em /app, CSS e módulos JS.
- Consumidor Genius: 53 testes com NuGet 2.2.1; painéis reais em host de teste
  nas larguras 320/390/1280, claro/escuro, e menu nas três larguras.
  O host usa dados simulados e não comprova integração nativa Windows/MAUI.

Não houve publicação NuGet nem alteração do versionamento local preexistente
no checkout canônico. Referências: PR #18 / issue #17 e
[sufficit-ai-genius#625](https://github.com/sufficit/sufficit-ai-genius/pull/625).
