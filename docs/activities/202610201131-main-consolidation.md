# Consolidação de dependências e checks da main

Pedido explícito: integrar trabalhos pendentes Sufficit e sincronizar com o remoto.

Integrados sete PRs Dependabot de Actions/CodeQL, Microsoft.AspNetCore.Components e Web10.0.13, adaptador NUnit e runner xUnit. A sobreposição dos dois PRs Microsoft foi conciliada mantendo ambos os pacotes10.0.13, sem downgrade.

964 testes de componentes passaram com warnings como erros. Showcase publicado localmente e preparado com base /sufficit-blazor-ui/. A falha do CI foi reproduzida: TabsBypassALegacyModuleAlreadyLoadedInTheBrowser esperava requisitar módulo v2, mas SUITabs já importa v3. O teste foi alinhado ao módulo vigente, mantendo o teste de cache antigo sem reveal e a navegação por teclado. Regressão falhou antes e passou depois; suíte completa ShowcaseBrowserTests:53 passaram em Chromium. Nenhuma verificação foi desativada. Logs em /tmp/sufficit-consolidation-ui-*.

Sem corte de tag, publicação manual de pacote ou deploy manual.
