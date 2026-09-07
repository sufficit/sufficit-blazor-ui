# Módulo de abas durante atualização de servidor

A implantação do Cloud Mobile revelou `reveal is not a function` em um circuito: clientes podiam conservar o módulo JavaScript anterior à API nova. O Cloud foi revertido antes da correção.

O import de SUITabs agora usa `SUITabs.razor.js?v=2`, separando o módulo atual do URI legado no cache de módulos do navegador. Sem mudança de API pública, layout ou comportamento de teclado.

Validação: 566 testes unitários aprovados e teste de navegador `TabsBypassALegacyModuleAlreadyLoadedInTheBrowser` com módulo legado carregado na mesma página, confirmando função nova e navegação de abas. Publish WebAssembly Release aprovado. A correção será consumida pelo Web recompilado; rollback e ativação final são registrados no Cloud.

O CI também identificou uma expectativa antiga no teste de subpath: o filtro `endsWith(.razor.js)` aplicado à URL completa descartava o módulo versionado. O filtro agora examina `URL.pathname`, mantendo os asserts de navegação real e do prefixo `/app/_content/`. A regressão foi executada em Chromium contra o host Server configurado em `/app/` e passou. Nenhuma alteração adicional de runtime.
