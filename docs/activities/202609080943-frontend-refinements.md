# Refinamentos de frontend — biblioteca SUI

Aplicação da skill `sufficit-frontend` 1.0.1 aos consumidores existentes, partindo de fa8a63b. Alteração local alheia em src/Sufficit.Blazor.UI.csproj preservada, usando .worktrees/frontend-refinements.

## Entrega

Commit funcional `ea2d9b5855462139b7fdce4ebb055399241bb17f`, publicado em main.

SUISwitch ganhou HelperText, Id, ErrorText e Invalid. Ajuda e erros descrevem o checkbox; o rótulo mantém o nome acessível. IDs permanecem estáveis durante rerenders e não colidem entre instâncias. Disabled impede emissão de mudança. Binding e integração com EditContext permanecem. Sem ajuda, a estrutura visual anterior é preservada; com ajuda, a descrição fica alinhada ao rótulo e separada por token de 4px. Atributos adicionais continuam na raiz legada.

Catálogo e exemplo executável atualizados com a API. Dois testes bUnit e um caso de navegador verificam relações de acessibilidade, estados e teclado. Sem nova dependência visual e sem release NuGet. Cloud consome a revisão por ProjectReference e pin explícito.

- [CI completo](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34227221142): sucesso.
- [Vitrine publicada](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34227221201): sucesso.
- [Testar SUISwitch](https://sufficit.github.io/sufficit-blazor-ui/?component=SUISwitch): ajuda e Space conferidos no site público.

## Validação e limites

- SUI: 570 testes unitários; 68 exemplos copiáveis compilados; 25 testes de navegador da vitrine; build com warnings como erros. CSS global 53.984 bytes, gzip 9.916, Brotli 8.679; CSS isolado 25.599/25.600 bytes. Nenhum limite ampliado.
- Catálogo Server: 44 casos funcionais passaram localmente. A baseline versionada mostrou 18px extras de altura também na base fa8a63b; a comparação antes/depois com quatro capturas da base no mesmo ambiente passou. Baselines versionadas preservadas. CI remoto completo passou, inclusive baselines, Chromium, Firefox, WebKit e Lighthouse.
- Cloud: 416 testes .NET, 127 Node e 12 casos de navegador. A última execução inclui bdd679f, correção de navegação publicada por outra sessão durante o trabalho. CI local obrigatório no pre-push passou.
- Medidos os intervalos dos controles expandidos e a ajuda dos switches; testados dropdown por teclado, resumo salvo durante edição, resolução automática, busca vazia/limpeza, temas, mobile/desktop, overflow e axe. Capturas em `artifacts/frontend-check/screenshots` do checkout de validação.
- Um teste novo tinha seletor dinâmico invalidado ao abrir accordions; corrigido para percorrer grupos estáveis. Um host de catálogo com manifestos antigos foi recompilado após alterações de CSS. Uma tentativa de iniciar a fixture encontrou TIME_WAIT, sem listeners ativos; aguardado antes de nova execução.
- Fixture sintética sem encaminhamento de mutações. Não houve operação em Android real, instalação de aplicativos, pagamento ou alteração de autenticação. Produção verificada por serviço, HTTP e integridade dos assets; não equivale a sessão autenticada do cliente.
