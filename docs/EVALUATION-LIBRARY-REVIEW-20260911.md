# Avaliação da biblioteca Sufficit.Blazor.UI — 2026-09-11

Revisão independente do repositório em `main` (`2a32690`), feita para responder
três perguntas: em que estado a biblioteca está, o que a evidência mostra de
errado e quais atualizações valem o esforço. Tudo que está aqui foi verificado
localmente ou na API pública do GitHub/NuGet na data da avaliação; nada foi
inferido de documentação sem conferência.

## Resumo executivo

**Nota geral: 7,8 / 10.** A engenharia interna é acima da média: biblioteca sem
dependência visual de terceiros, 67 componentes com prefixo consistente, CSS
tokenizado, módulos JavaScript colocalizados, três camadas de teste que falham
o build, release por tag com OIDC e vitrine estática publicada. O problema
principal não está no código: está na **distribuição**. O esquema de versão
por calendário (`1.yy.MMdd.HHmm`) perde para as versões SemVer legadas ainda
listadas no NuGet, então a maioria dos consumidores recebe hoje um pacote de
agosto com 89 commits e 18 mil linhas de atraso. Enquanto isso não for
resolvido, cada correção feita aqui não chega a quem a pediu.

| Área | Nota | Justificativa curta |
| --- | --- | --- |
| Arquitetura e código | 9 | Famílias, enums próprios, `SUIClassBuilder`, descarte de listeners, budgets de tamanho de arquivo com lista de débito que só encolhe |
| Testes e gates | 8 | 605 bUnit + 106 testes de navegador em 3 engines + Lighthouse; perde ponto por 22 componentes sem bUnit e por budgets sem folga que geram falha intermitente |
| Distribuição e versionamento | 4 | Versão calendário é menor que 1.28.0/2.2.1 listadas; `1.*` e `2.*` resolvem pacotes obsoletos; deslistagem bloqueada por credencial |
| Documentação | 7 | Muita documentação e boa convenção de nomes, mas README lista 51 de 67 componentes e `docs/components` não cobre 36 |
| Higiene de repositório | 6 | 16 worktrees e 15 ramos já integrados em `main` ainda vivos (removidos em 2026-09-11); alteração de CSS não commitada; mensagens `[Obsolete]` apontam para uma "v2.0.0" que já foi publicada com outro significado |
| Empacotamento | 6 | Sem XML docs, sem símbolos/SourceLink, `PackageIconUrl` obsoleto; versão exata e determinística são pontos positivos |

## Estado verificado

| Verificação | Resultado |
| --- | --- |
| `dotnet build src -c Release -warnaserror` | 0 avisos, 0 erros |
| `dotnet test tests/Sufficit.Blazor.UI.Tests` | 605 aprovados, 0 falhas, 271 ms |
| `npm run check:css` | bundle sincronizado: raw 54.751, gzip 10.098, brotli 8.840 bytes |
| CI `main` (Build, Component showcase, CodeQL) | verde no último push (`2a32690`) |
| Alertas Dependabot abertos | 0 |
| PRs e issues abertos | 0 e 0 |
| Componentes `.razor` em `src/Components` | 67 (README e `FRONTEND-REVIEW` dizem 68) |
| Módulos JavaScript colocalizados | 10 |
| Linhas autorais (`.cs`, `.razor`, `.css`, `.js`, sem `wwwroot`) | 12.822 |
| Tipos com pontes `[Obsolete]` | 10 componentes |
| Último pacote corporativo publicado | `1.26.908.2020` (2026-09-08) |
| Commits em `main` desde a tag `v2.2.1` | 89 (336 arquivos, +18.337/-3.903) |

### Consumidores encontrados em `/mnt/sufficit`

| Repositório | Referência | O que resolve hoje |
| --- | --- | --- |
| sufficit-identity | `1.26.908.2020` (CPM) | correto |
| sufficit-fleet | `1.26.908.2020` | correto |
| sufficit-cloud-mobile-google-accounts | `[1.26.814.1643,2.0.0)` | **1.28.0** (SemVer legado de 17/08) |
| sufficit-blazor (client, server) | `1.*` | **1.28.0** |
| sufficit-background | `1.*` | **1.28.0** |
| sufficit-network-control | `1.*` | **1.28.0** |
| sufficit-services-run | `1.*` | **1.28.0** |
| sufficit-ai/web | `2.2.1` | **2.2.1** (30/08) |
| sufficit-ai-genius (UI, Desktop, Mobile) | `2.*` | **2.2.1** |
| sufficit-checkout, sufficit-telephony-panel, sufficit-cloud-mobile | `ProjectReference` ou caminho legado | depende do checkout local |

Nove caminhos de produção resolvem um pacote sem as correções de setembro
(helper de campos, alinhamento de botões, variante `Soft`, fix de rolagem do
`SUISelect` no WebKit, contraste do `SUISwitch`). O próprio README ainda ensina
`Version="1.*"`, que hoje entrega o pacote errado.

## Pontos fortes

- **Independência real de terceiros.** Não há pacote nem fonte vendorizada de
  framework visual; um teste de contrato garante isso e o Lighthouse exige
  zero requisições third-party.
- **Gates que mordem.** Orçamentos de linhas por arquivo, de bytes do bundle,
  de transferência em runtime, de API pública (`PublicApiBaseline.txt` com
  1.586 linhas) e de convenção de nomes. As listas de débito só podem encolher,
  e há teste que falha se uma entrada da lista deixa de ser ofensora.
- **Acessibilidade tratada como requisito.** axe em três engines, light/dark,
  mobile/desktop, forced-colors, RTL, zoom 200%, alvos de 44 px, focus trap.
- **Release determinística.** Tag → validação do artefato exato → instalação em
  RCL e app temporárias na raiz e sob `PathBase` → OIDC → publicação. Nada de
  chave estática.
- **Documentação operacional.** Runbooks de release e de Pages, plano de
  migração de consumidores com gates, 36 atividades concluídas arquivadas com
  timestamp.
- **Vitrine publicada** em GitHub Pages com busca, exemplos copiáveis e editor
  de tema.

## Achados

### Críticos

**C1. A versão calendário perde para as SemVer legadas listadas.**
`1.26.908.2020 < 1.27.0 < 1.28.0 < 2.0.0 < 2.1.1 < 2.2.1`. Enquanto
`1.27.0`, `1.28.0`, `2.0.0`, `2.1.1` e `2.2.1` estiverem listadas, todo range
flutuante (`1.*`, `2.*`, `[1.26.814.1643,2.0.0)`) resolve para agosto. O plano
em `.worktrees/sufficit-versioning/PLAN-sufficit-versioning.md` registra que a
deslistagem via `maintenance.yml` recebeu HTTP 403 (chave sem permissão
*Unlist*) e que nenhuma das cinco foi deslistada. Confirmado hoje na API de
registro do NuGet: as cinco continuam `listed=true`.

Consequência: a linha `1.26.*` nunca vence `1.28.0` até 2029, e a linha `2.*`
nunca receberá nada. Isso não é um débito de documentação; é uma quebra
silenciosa da entrega para nove projetos.

**Deslistar não resolve o range flutuante.** Verificado em 2026-09-11 com
`dotnet restore` (SDK 10) contra pacotes públicos cuja versão mais alta de
uma linha está deslistada: `Dapper` `1.*` resolveu `1.60.9` (unlisted),
`MediatR` `8.*` resolveu `8.2.0` (unlisted), `Refit` `6.*` resolveu `6.5.1`
(unlisted). A política do NuGet.org confirma: "unlisted packages may still be
discovered in package restore using floating versions". NuGet.org não apaga
versões a pedido do dono fora de violação de política. Portanto `1.*` só
volta a funcionar quando a versão calendário ultrapassar `1.28.0` (ano 2029),
quando o esquema mudar, ou se o suporte do NuGet aceitar apagar as cinco.

Em 2026-09-11 as tags Git `v1.27.0`, `v1.28.0`, `v2.0.0`, `v2.1.1`, `v2.2.0`
e `v2.2.1` foram removidas do repositório; o CI já rejeita tags SemVer.

### Altos

**A1. Mensagens `[Obsolete]` prometem remoção "in v2.0.0".** As dez pontes
`object`/`string` (`SUIButton.Color`, `SUIAlert.Severity`, etc.) dizem que
serão removidas na `v2.0.0`. Essa versão já foi publicada em 27/08 *com as
pontes*, e o esquema atual não tem major. Consumidor que lê o aviso conclui que
já está seguro ou que a remoção não vai acontecer. `PLAN-SUI-V2.md` também
carrega o nome "v2" que colide com a série SemVer publicada.

**A2. Worktrees e ramos órfãos.** 16 worktrees em `.worktrees/` (mais um
`prunable` em `/tmp`) e 15 ramos locais, todos com `0 ahead` de `main`, ou seja,
já integrados por squash/rebase. Três têm apenas arquivos não rastreados
(`packages.lock.json`, `Directory.*.props` de isolamento e o plano de
versionamento). Cada worktree carrega `bin/`, `obj/` e `node_modules/` próprios.

**A3. Budgets sem folga geram falhas intermitentes no CI.** Os comentários em
`AssetBudgetTests` documentam folgas de 233 B (JS Brotli) e 276 B (CSS
isolado). Em 11/09 às 20:49 o job Chromium falhou em
`Catalog_MatchesCommittedVisualBaselines` e no orçamento de transferência de
CSS (97.014 > 96.256 bytes); um re-run às 20:54 passou sem mudança de código.
Em 10/09 e 11/09 14:38 os testes de componente falharam em
`AllJsModulesTogether_FitTheTransferBudget` e `ScopedComponentCss_StaysSmall`.
O orçamento cumpre seu papel de teto, mas sem margem vira ruído e treina o time
a re-rodar em vez de investigar.

### Médios

**M1. README e `docs/components` desatualizados.** A tabela de famílias do
README omite 16 componentes: `SUIAvatar`, `SUICardActions`, `SUICardContent`,
`SUICardHeader`, `SUICheckbox`, `SUICopyToClipboard`, `SUIDateField`,
`SUIDecisionDialog`, `SUIFilterScope`, `SUIFilterTree`, `SUIPagination`,
`SUIPendingChangesBar`, `SUIProgressCircular`, `SUIProgressSteps`,
`SUISection`, `SUITableSortLabel`. `docs/components` não menciona 36 dos 67
(layout inteiro, cards, timeline, tabs, toasts, dialogs de decisão, filtros,
progress). README e `FRONTEND-REVIEW` falam em 68 componentes; há 67.

**M2. 22 componentes sem nenhum teste bUnit.** `SUISwitchButton`, `SUIGrid`,
`SUIContainer`, `SUISpacer`, `SUIAppBar`, `SUILink`, `SUIList`, `SUIListItem`,
`SUIStat`, `SUITableSortLabel`, `SUITimeline`, `SUITableEmpty`, `SUITd`,
`SUITooltip`, `SUIDecisionDialog`, `SUINavGroup`, `SUIStatusBanner`,
`SUISnackbarHost`, `SUIProgressCircular`, `SUISkeletonLoader`, `SUIToast`,
`SUIEmptyState`. Vários são cobertos indiretamente pelos testes de navegador,
mas o contrato de render, atributos encaminhados e ARIA fica sem guarda rápida.

**M3. Pacote sem metadados de desenvolvedor.** O `.csproj` não gera XML docs
(`GenerateDocumentationFile`), não publica símbolos (`.snupkg`), não configura
SourceLink (`PublishRepositoryUrl`, `EmbedUntrackedSources`) e usa
`PackageIconUrl`, obsoleto desde o NuGet 5.3. `ContinuousIntegrationBuild` só
vem pela linha de comando do CI. Os `/// <summary>` já escritos nos componentes
não chegam ao IntelliSense do consumidor.

**M4. `SUIThemeProvider` emite `<style>` inline.** Qualquer consumidor com
`Content-Security-Policy: style-src 'self'` precisa abrir `'unsafe-inline'` ou
o tema não é aplicado. Não há caminho para nonce nem alternativa por atributo.

### Baixos

**B1. Alteração não commitada** em `src/styles/sui-shared-autocomplete.css`
(`display: flex` no ícone) com bundle já regenerado e sincronizado. Passa no
`check:css`, mas não tem dono nem commit.

**B2. Dependências levemente atrasadas.** `Microsoft.AspNetCore.Components`
está em `10.0.11`; `10.0.12` já está no NuGet. `Microsoft.NET.Test.Sdk` diverge
entre projetos de teste (`17.14.0` e `17.14.1`). Dependabot deve tratar na
segunda-feira, mas a divergência interna é manual.

**B3. `PLAN-SUI-V2.md` com estável "não antes de 2026-11-11"** e checklist do
framework já concluída. O plano ativo mistura itens feitos e pendentes; a
convenção do próprio `docs/README.md` pede "apenas itens pendentes".

**B4. `FileSizeBudgetTests.Debt` congela `CatalogBrowserTests.cs` em 976
linhas** com comentário reconhecendo que não há folga. Próxima asserção obriga
a subir de novo ou dividir a classe.

**B5. `SUICopyToClipboard.razor.js`** usa `document.execCommand("copy")` como
fallback. É o comportamento correto para contextos inseguros, mas a API está
marcada como obsoleta; vale registrar o motivo no próprio módulo.

## Atualizações propostas

Ordenadas por impacto sobre quem consome a biblioteca. Cada item tem gate de
conclusão para virar `PLAN-` ou `activities/` depois.

### P0 — Distribuição (resolver antes de qualquer outra publicação)

1. **Escolher como `1.*` volta a resolver o pacote certo.** Decisão do dono
   do pacote, porque deslistar não basta (ver C1). Alternativas:
   - **(a) range anual** `1.26.*` nos consumidores agora, `1.27.*` em janeiro
     de 2027, `1.28.*` em 2028 e `1.*` de 2029 em diante. Mantém o esquema
     corporativo; custa uma edição por ano em cada consumidor.
   - **(b) pedido ao suporte do NuGet** para apagar `1.27.0`, `1.28.0`,
     `2.0.0`, `2.1.1` e `2.2.1` como publicação acidental. Sem garantia; se
     aceito, `1.*` funciona sem mais nada.
   - **(c) mudar o esquema** deste pacote para algo que ordene acima de
     `1.28.0` dentro do major 1 (por exemplo `1.yyMM.dd.HHmm`). Resolve para
     sempre, mas descola do padrão `1.yy.MMdd.HHmm` compartilhado com
     `Sufficit.Identity.Core`.
   Recomendação: (a) imediatamente e (b) em paralelo; (c) só se o dono aceitar
   dois esquemas na organização.
   **Decisão (2026-09-11, dono do pacote): variante de (c) com major 2,
   `2.yy.MMdd.HHmm`.** Mantém o formato corporativo, ordena acima de `2.2.1` e
   `2.*` resolve sempre o calendário. Aplicado no csproj, `release_version.py`,
   testes, workflows e docs; consumidores migram para `Version="2.*"`.
2. **Deslistar as cinco versões SemVer legadas mesmo assim.** Tira da busca,
   impede adoção nova e deixa claro no catálogo qual linha é a viva. Exige
   chave com escopo *Unlist* em `NUGET_API_KEY` (a chave OIDC do Trusted
   Publishing só tem escopo de push). Gate: `listed=false` nas cinco; os doze
   pacotes calendário continuam listados.
3. **Migrar todos os consumidores para a linha 1.x.** Genius (`2.*`), AI Web
   (`2.2.1`) e Cloud Mobile Google Accounts (`[…,2.0.0)`) passam para o range
   escolhido no item 1; Blazor, Background, Network Control e Services Run
   ajustam de `1.*` para o mesmo range; Identity e Fleet podem manter versão
   exata ou aderir. Gate: nenhum `.csproj` em `/mnt/sufficit` referencia `2.*`
   ou `2.2.1`; cada consumidor compila e passa nos próprios testes com
   `1.26.9xx`.
4. **Corrigir o README.** Trocar o exemplo `Version="1.*"` pelo range
   escolhido e explicar em uma frase por que `1.*` puro não serve até 2029.
   Gate: README, `docs/CONSUMER-ROLLOUT.md` e o runbook de release alinhados.

### P1 — Higiene e sinais confiáveis

5. **Limpar worktrees e ramos integrados.** Feito em 2026-09-11 (commit
   `553e441`): 16 worktrees e 15 ramos removidos, plano de versionamento
   arquivado em `docs/activities/`, `Directory.Build.props` e
   `Directory.Packages.props` vazios na raiz isolam o build de um checkout
   irmão do Identity. Falta o passo "remover worktree ao integrar" no
   `RUNBOOK-RELEASE.md`.
6. **Reescrever as mensagens `[Obsolete]`.** Trocar "will be removed in
   v2.0.0" por uma data-alvo do calendário Sufficit ("removida a partir de
   1.26.11xx") e renomear `PLAN-SUI-V2.md` para `PLAN-API-CLEANUP.md`, com o
   plano contendo só o pendente. Gate: nenhum texto "v2.0.0" em `src/`;
   `PublicApiBaseline.txt` regenerado com review.
7. **Dar folga mínima aos budgets.** Definir regra explícita: teto = valor
   medido × 1,03 arredondado para KiB, ratchet apenas para baixo em commit
   dedicado. Registrar o valor medido no teste junto com o teto. Gate: os
   quatro testes de orçamento com folga ≥ 3 % e nenhuma falha intermitente em
   dez execuções consecutivas do CI.
8. **Commitar ou descartar** a alteração pendente do autocomplete (B1) após
   confirmar visualmente o ícone no Showcase. Gate: `git status` limpo.

### P2 — Documentação e cobertura

9. **Gerar a tabela do README a partir do código.** Estender
   `scripts/generate-catalog.py` (já existe) para emitir a tabela de famílias e
   adicionar teste que compara README com `src/Components`. Gate: teste
   `ReadmeCatalog_MatchesComponents` verde; contagem "67" derivada, não
   digitada.
10. **Cobrir os 36 componentes ausentes em `docs/components`.** Um arquivo por
   família já existe; faltam as seções. Priorizar layout (`SUILayout`,
   `SUIAppBar`, `SUIDrawer`, `SUIGrid`, `SUIContainer`) porque são os que todo
   consumidor novo usa primeiro. Gate: teste de convenção que exige menção de
   cada componente público em `docs/components`.
11. **bUnit mínimo para os 22 componentes sem teste.** Um teste de contrato
    por componente: renderiza, encaminha atributos, aplica classe e, quando há
    ARIA, expõe o papel. Reaproveitar o padrão dos testes existentes. Gate:
    teste de convenção que falha para componente público sem arquivo de teste.

### P3 — Empacotamento e hardening

12. **Metadados de pacote.** Adicionar `GenerateDocumentationFile`,
    `IncludeSymbols` + `SymbolPackageFormat=snupkg`, `PublishRepositoryUrl`,
    `EmbedUntrackedSources`, `ContinuousIntegrationBuild` condicionado a
    `GITHUB_ACTIONS`, e trocar `PackageIconUrl` por apenas `PackageIcon`.
    Publicar o `.snupkg` no mesmo passo do `build.yml`. Gate:
    `validate-package.sh` verifica `lib/net10.0/Sufficit.Blazor.UI.xml` e o
    símbolo; `dotnet build` continua com zero avisos (os `/// <summary>`
    faltantes viram CS1591 e precisam ser escritos ou suprimidos com critério).
13. **Suporte a CSP no `SUIThemeProvider`.** Aceitar parâmetro `Nonce` que é
    emitido no `<style>` e documentar a alternativa de publicar os tokens por
    `style` attribute no wrapper para quem bloqueia `style-src` inline.
    Gate: teste bUnit que verifica o atributo `nonce`; exemplo no Showcase.
14. **Alinhar dependências.** Aceitar o bump para `10.0.12` e unificar
    `Microsoft.NET.Test.Sdk` nos dois projetos de teste. Gate: CI verde.

### P4 — Evolução da API (depois do P0 e P1)

15. **Executar o plano de limpeza de API** (antigo "v2"): remover as 23 pontes
    `object` e 2 `string`, renomear `NavAccordionScope`, `SUIItem.xs..xl` e
    `SUIAlert.CloseIconClicked`, esvaziar `LegacyParameterNames`. Só depois de
    todos os consumidores compilarem sem `CS0618` na versão exata. Gate: os
    nove gates já listados em `PLAN-SUI-V2.md`, com changelog contendo guia de
    migração e versão de rollback.

## Esforço estimado

| Bloco | Esforço | Dependência |
| --- | --- | --- |
| P0 (itens 1–4) | 1 dia após a decisão do item 1; item 2 espera chave com *Unlist* | decisão do dono do pacote |
| P1 (itens 5–8) | meio dia (item 5 já feito) | nenhuma |
| P2 (itens 9–11) | 2 a 3 dias | nenhuma |
| P3 (itens 12–14) | 1 dia | item 12 pode gerar CS1591 em volume |
| P4 (item 15) | 2 dias mais canário nos consumidores | P0 concluído |

## Método

- Leitura de README, PRODUCT, CHANGELOG, `docs/`, `.csproj`, workflows,
  `scripts/`, testes de convenção e amostras de componentes.
- Build Release com `-warnaserror`, suíte bUnit completa e `check:css`.
- Consulta a `gh run list`, logs dos jobs que falharam em 10 e 11/09, PRs,
  issues e alertas Dependabot.
- Consulta à API de registro do NuGet para versões e estado `listed`.
- Varredura de `/mnt/sufficit` por referências a `Sufficit.Blazor.UI` em
  `.csproj` e `.props`.
- Contagem de componentes, módulos, linhas, testes e cruzamento com README,
  `docs/components` e `tests/Sufficit.Blazor.UI.Tests`.
