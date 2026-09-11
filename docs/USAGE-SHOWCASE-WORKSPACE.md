# Vitrine: composição, tema e custo de carregamento

A vitrine executa componentes reais em WebAssembly, com 68 exemplos independentes. A nova tela de operação em **Composições** combina resumo, busca, filtro, paginação, edição validada, preferências e estados de carregamento/erro/vazio. Os dados são fictícios; o código completo pode ser copiado para uma aplicação que registre os serviços e hosts SUI habituais.

O playground parametrizado cobre `SUIButton`, `SUIIconButton`, `SUIChip` e `SUITabs`. Os demais componentes continuam com exemplos interativos e código completo; não possuem ainda editor genérico de parâmetros. A tabela de API extrai a descrição XML quando disponível e informa quando não há descrição no código-fonte. No celular usa rolagem local, preservando os nomes dos parâmetros.

Os tamanhos dos botões seguem `ControlHSm`, `ControlHMd` e `ControlHLg` do tema. Na vitrine confortável são 28, 40 e 44 px; botões de ícone permanecem quadrados e o glifo cresce junto. Em telas compactas ou ponteiros de toque, o alvo dos botões permanece com pelo menos 44 px, mesmo no tamanho pequeno. Chips têm alturas 24, 32 e 40 px para pequeno, médio e grande. A biblioteca aceita as classes completas emitidas pelos enums e preserva os aliases `sm`/`md`/`lg` existentes de botões e ícones.

O editor de temas permite paleta, aparência, densidade, fonte, tamanho base, espaçamento, raios e cores de sucesso/erro. Preferências são limitadas a intervalos válidos e persistidas por navegador. A comparação clara/escura usa dois iframes interativos isolados; não cria providers globais concorrentes na mesma árvore. A exportação C# reflete os tokens selecionados. `SUIColorContrast.TryGetRatio` calcula contraste sRGB para cores opacas de seis dígitos; não resolve CSS variables, transparência ou o contraste final de uma página inteira.

## Versão e consumo

O rodapé usa a versão informacional da assembly compilada. Uma compilação de desenvolvimento é identificada como tal: a presença de uma API na vitrine não significa que ela já esteja no NuGet. Quando disponível, o link de origem aponta para o commit da compilação. A revisão introduz `SUITabs.Vertical`; consumidores por source reference podem usá-la imediatamente, e consumidores NuGet devem aguardar a publicação da versão que a contém.

`SUITabs` anuncia a orientação e usa setas de acordo com ela. Home/End continuam disponíveis. A seleção é revelada dentro da faixa horizontal sem rolar o documento. Gavetas compactas fechadas ficam inertes e switches têm foco de teclado visível. O tema escuro usa um tom secundário mais claro para texto legível.

## Medição reproduzível de WASM

Publique a vitrine e sirva sua saída estática; veja `RUNBOOK-SHOWCASE-PAGES.md`. Execute:

```sh
SUI_SHOWCASE_URL=http://127.0.0.1:5287/ dotnet test tests/Sufficit.Blazor.UI.BrowserTests -c Release --filter FullyQualifiedName~ShowcaseBrowserTests
```

`ColdWasmLoad_RecordsPayloadAndTimeToInteraction` abre um contexto Chromium novo, aguarda a marca `sui-interactive`, preenche o formulário e confirma seu cancelamento. Registra bytes transferidos/decodificados, tamanho do framework, requisições e tempos no artefato `showcase-wasm-performance.json` junto à saída dos testes.

Referência local desta revisão: aproximadamente 8,65 MB decodificados, 58 requisições e 0,3–0,5 s até a interação verificada, em loopback sem limitação de rede. Esses números não representam uma conexão móvel nem o tempo de produção. Limites de regressão: menos de 11 MiB decodificados e 5 s até a marca de interatividade. Os budgets da biblioteca continuam independentes: CSS global 56 KiB bruto/10 KiB gzip/9 KiB Brotli; JavaScript agregado 9,5 KiB Brotli; CSS isolado 27 KiB. A revisão de 2026-09-11 inclui os incrementos numéricos independentes (roda, teclado e botões), mantendo margem de 233 B para JS e 276 B para CSS sobre a medição desta revisão.

Testes de navegador verificam controles, exportação enviada ao clipboard, persistência, isolamento, validação, alterações pendentes, overflow e axe em desktop/celular. Capturas só ocorrem após os controles reais e o fim de animações transitórias.

O catálogo Server mede separadamente o CSS agregado do host e da biblioteca: 95 KiB sem compressão de transporte (97.014 B medidos no Chromium do CI em 2026-09-11). Os limites de requisições, DOM, transferência total, LCP e CLS permanecem inalterados.
