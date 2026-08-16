# Plano — próxima etapa de engenharia da SUI

**Status:** Concluído  
**Criado:** 2026-08-14  
**Escopo:** `sufficit-blazor-ui`, sua skill interna e o consumer ainda pendente  
**Objetivo:** transformar os resultados do hardening arquitetural em contratos
repetíveis de release, formulário, distribuição, compatibilidade e adoção.

## Contexto e decisões

O hardening anterior concluiu a separação por famílias, o CSS híbrido, os
módulos JavaScript colocalizados, o catálogo e os testes de acessibilidade. Esta
etapa não refaz essa arquitetura; fecha os riscos operacionais e torna práticas
que hoje dependem de documentação/skill parte verificável da biblioteca.

Decisões desta implementação:

- manter componentes agrupados por família; não criar uma pasta ou README para
  cada componente sem necessidade real;
- manter CSS autoral modular e CSS isolation; não introduzir Sass;
- preservar compatibilidade binária/source na série v1;
- concentrar remoções e adapters `InputBase<T>` numa v2 explicitamente
  planejada;
- manter `sufficit-ui.css` como entrypoint público estável;
- publicar somente versões SemVer provenientes de tag e somente depois de todos
  os gates obrigatórios;
- manter a skill de design independente de infraestrutura de um consumer.

## Fases e gates

| Fase | Entrega | Gate | Estado |
| --- | --- | --- | --- |
| 0 | Plano ativo e baseline registrado | escopo e decisões revisáveis | **Concluída** |
| 1 | Release determinístico | tag SemVer; build, testes, browser e pacote verdes antes do push NuGet | **Concluída** |
| 2 | Layout oficial de formulários | primitive SUI, catálogo, bUnit, Playwright, docs e skill alinhados | **Concluída** |
| 3 | Qualidade visual/browser | comparação de baseline, Chromium/Firefox/WebKit e forced-colors | **Concluída** |
| 4 | Smoke do pacote executável | app instalada do `.nupkg` responde em `/` e sob `PathBase` | **Concluída** |
| 5 | Distribuição CSS | entrypoint sem `@import` em runtime, minificado e com budget | **Concluída** |
| 6 | Contrato v2 e TFMs | escopo de quebra, migração e calendário net9/net10 documentados | **Concluída** |
| 7 | Skill desacoplada | design skill validada; runbook operacional movido ao owner correto | **Concluída** |
| 8 | Último consumer | `Sufficit.Identity.UI` avaliado/migrado com gates próprios | **Concluída** |
| 9 | Fechamento | solução, pacote, browsers e consumidores verdes; plano arquivado | **Concluída** |

## Implementação detalhada

### Fase 1 — versão e publicação

- [x] remover versionamento baseado no relógio;
- [x] derivar a versão publicada de tags `vMAJOR.MINOR.PATCH[-prerelease]`;
- [x] produzir pacote de CI com versão local determinística e não publicável;
- [x] fazer o job NuGet depender de build multialvo, bUnit, browser/axe e
  validação do pacote;
- [x] impedir publicação automática por mero `push` em `main`;
- [x] registrar política de release e changelog.

### Fase 2 — formulários

- [x] criar primitive oficial de grid responsivo com
  `data-sui-align-row` embutido;
- [x] garantir `min-width: 0`, alinhamento pelo topo, reserva equivalente de
  label e empilhamento mobile;
- [x] migrar o catálogo para consumir a primitive, removendo a correção local;
- [x] adicionar contratos bUnit e geométricos Playwright;
- [x] atualizar documentação e a skill `sui-design`.

### Fase 3 — visual e navegadores

- [x] comparar screenshots com baselines versionados e tolerância explícita;
- [x] executar os contratos funcionais em Chromium, Firefox e WebKit;
- [x] cobrir light/dark, desktop/mobile e `forced-colors: active`;
- [x] conservar artefatos de falha para diagnóstico.

### Fase 4 — pacote instalado

- [x] criar Blazor Web Apps temporárias net9/net10 a partir do `.nupkg`;
- [x] renderizar ao menos um componente SUI;
- [x] iniciar os hosts e verificar markup, CSS global, CSS isolation e módulos;
- [x] repetir sob `PathBase` sem matar processos por padrão textual.

### Fase 5 — CSS distribuído

- [x] preservar arquivos fonte separados;
- [x] resolver imports no build/pack e entregar um único entrypoint global;
- [x] minificar somente o artefato distribuído;
- [x] aplicar budget de bytes bruto/gzip/Brotli no CI;
- [x] manter sourcemap ou caminho claro de diagnóstico quando suportado.

### Fase 6 — v2 e frameworks

- [x] inventariar pontes obsoletas e produzir checklist/codemod de migração;
- [x] especificar adapters opcionais `InputBase<T>` sem alterar os fields
  controlados;
- [x] definir data de retirada do `net9.0`, cujo suporte termina em 2026-11-10;
- [x] validar a decisão contra a matriz real de consumers.

### Fase 7 — skill interna

- [x] manter em `sui-design` apenas regras reutilizáveis de frontend Blazor/SUI;
- [x] mover o runbook `eveo-apps` ao repositório/skill operacional proprietário;
- [x] eliminar topologia específica do consumer da distribuição desta skill;
- [x] validar metadados, referências e script de alinhamento.

### Fase 8 — consumer pendente

- [x] reavaliar `Sufficit.Identity.UI` e seus limites de framework/tema;
- [x] migrar somente se não houver decisão de produto bloqueante;
- [x] executar build, testes e smoke de assets do consumer;
- [x] atualizar `PLAN-CONSUMER-MIGRATION.md` com evidência real.

## Evidências e diário

### 2026-08-14 — baseline

- branch `main`, commit `c5e302e`;
- worktree limpa antes desta implementação;
- entrypoint global atual: 47.030 bytes e 1.433 linhas, com dois `@import`;
- sete folhas CSS isoladas: 13.541 bytes de fonte;
- screenshots atuais são capturados, mas ainda não comparados ao baseline;
- browser CI atual instala somente Chromium;
- validação do `.nupkg` atual compila RCLs, não uma aplicação executável;
- publicação atual ocorre em workflow independente a cada `push` em `main`;
- único consumer registrado como pendente: `Sufficit.Identity.UI` pública.

### 2026-08-14 — fase 1 concluída

- `Version` local fixada em `0.0.0-local`; identidade de assembly v1 estável;
- tag `vMAJOR.MINOR.PATCH[-prerelease]` é a única fonte de versão publicável;
- workflow separado que publicava todo `push` em `main` foi removido;
- job NuGet agora depende de `pack`, bUnit e browser/axe e revalida o artefato
  exato antes do push;
- `CHANGELOG.md` e `docs/RUNBOOK-RELEASE.md` adicionados;
- YAML parseado e pacote `1.27.0-test.1` validado em consumers mínimos net9 e
  net10, com zero warnings e zero erros.

### 2026-08-14 — fase 2 concluída

- `SUIFormGrid` adicionado com 1–4 colunas, spacing tokenizado, reserva de 1–3
  linhas de label, empilhamento a `44rem` e atributos HTML encaminhados;
- CSS isolation gera seletores profundos a partir da raiz escopada, alcançando
  os fields filhos sem exigir CSS do consumer;
- catálogo migrou da correção `.catalog__form-grid` local para a primitive;
- documentação de forms e skill passaram a recomendar a API pública antes do
  CSS manual;
- 25 testes bUnit passaram; build de cinco projetos terminou sem warnings; o
  gate geométrico Playwright passou; a skill e o JavaScript do auditor foram
  validados.

### 2026-08-14 — fase 3 concluída

- teste visual agora compara pixels com limiar de canal 24 e tolerância máxima
  de 0,5%, falhando também em qualquer mudança de dimensões;
- quatro baselines comprometidos: light/dark em 1440×1000 e 390×844;
- `forced-colors: active` recebeu bordas e foco explícitos com system colors;
- CI usa matriz Chromium, Firefox e WebKit e preserva screenshots/logs quando
  houver falha;
- execução local: Chromium 22/22; Firefox 20/20 + 2 skips intencionais;
  WebKit 20/20 + 2 skips intencionais; zero warnings.

### 2026-08-14 — fase 4 concluída

- `validate-package.sh` mantém os RCL consumers e adiciona Blazor Web Apps
  interativas instaladas exclusivamente do `.nupkg` local;
- net9 e net10 compilam com warnings como erro e iniciam tanto na raiz quanto
  em `/app`;
- smoke confirma HTML SSR de `SUIFormGrid`/fields/button, CSS global, link
  fingerprintado do host, import e conteúdo do bundle CSS isolation da RCL e
  módulo colocalizado do Select;
- processos são registrados por PID e encerrados explicitamente no cleanup;
- pacote `1.27.0-test.7` passou nos oito cenários: duas RCLs e quatro execuções
  de apps (dois TFMs × raiz/PathBase), sem warnings ou erros.

### 2026-08-14 — fase 5 concluída

- foundations, portals e regras globais permanecem separados em `src/styles`;
- Lightning CSS resolve os imports somente no build e gera o único asset
  público `src/wwwroot/sufficit-ui.css`, sem folhas autorais duplicadas no
  pacote;
- artefato minificado: 46.069 bytes bruto, 8.830 gzip e 7.719 Brotli, abaixo
  dos budgets 52.000/9.500/8.000;
- `npm ci` é reproduzível pelo lockfile; Dependabot cobre a ferramenta; CI
  falha se o entrypoint committed estiver stale ou acima do budget;
- pacote `1.27.0-test.8`, build dos cinco projetos, smoke raiz/PathBase, contrato
  de resources e visual regression passaram sem warnings.

### 2026-08-14 — fase 6 concluída

- `ARCHITECTURE-VERSIONING-AND-TFM.md` fixa SemVer, garantias da v1, política de
  framework e review do API baseline;
- `PLAN-SUI-V2.md` inventaria 23 pontes `object` + duas `string`, preserva
  `SUISelectItem.Value` e especifica adapters `InputBase<T>` separados;
- v2 stable foi condicionada a data não anterior a 2026-11-11, nova varredura,
  prerelease no canário e gates de todos os consumers;
- varredura local confirmou consumers de produção em net10/net10-android; o
  cloud-mobile herda net10 de `Directory.Build.props`; net9 ficou apenas no
  contrato v1 e nos smokes temporários.

### 2026-08-14 — fase 7 concluída

- `skills/sui-design` agora descreve apenas decisões reutilizáveis de UI,
  componentes, formulários, acessibilidade e validação visual;
- o procedimento de publicação `eveo-apps` foi movido para
  `sufficit-cloud-mobile/docs/RUNBOOK-PRODUCTION-RELEASE.md` e referenciado pelo
  README do repositório operacional;
- referências a topologia, deploy e publicação do consumer foram removidas da
  skill distribuída com a biblioteca;
- `quick_validate.py` e a verificação sintática do auditor de alinhamento
  passaram sem erros.

### 2026-08-14 — fase 8 concluída

- a UI pública já estava em `net10.0`, com referência direta à SUI, stylesheet
  global e `SUISelect` no seletor de cultura; o inventário anterior estava
  desatualizado;
- o bundle `Sufficit.Identity.Server.styles.css` passou a ser carregado pelas
  três superfícies e importa o CSS isolation da SUI no artefato compilado;
- os formulários públicos preservam o design próprio de autenticação, evitando
  uma reescrita sem sobreposição funcional; novas composições alinhadas passam
  a usar o contrato `SUIFormGrid` documentado na biblioteca e na skill;
- `dotnet build Sufficit.Identity.sln -c Release -warnaserror` concluiu com
  zero warnings/erros e os 712 testes do consumer passaram.

### 2026-08-14 — fase 9 concluída

- bundle CSS reproduzível em 46.069 bytes bruto, 8.830 gzip e 7.719 Brotli;
- solução com cinco projetos compilada em Release e warnings como erro;
- 25 testes bUnit passaram;
- pacote `1.27.0-test.9` foi instalado em RCLs e aplicações executáveis net9 e
  net10, na raiz e sob `/app`, com markup, CSS global/isolation e módulo JS
  respondendo;
- Chromium passou 22/22; Firefox e WebKit passaram 20/20 cada, com dois skips
  intencionais por engine para baseline e forced-colors exclusivos do Chromium;
- inspeção das capturas desktop/light e mobile/dark não encontrou corte,
  overflow ou desalinhamento; o detector final de anti-padrões retornou vazio;
- skill, auditor JavaScript, workflows YAML e diff whitespace foram validados;
- nenhuma publicação externa foi executada.

## Riscos e rollback

- Mudanças de workflow serão verificadas por inspeção YAML e execução local dos
  comandos que não dependem de secrets; a publicação real não será disparada.
- A primitive de formulário é aditiva. O markup antigo continua suportado.
- A distribuição CSS manterá o mesmo URL público; em caso de regressão, o
  artefato anterior pode ser restaurado sem mudar consumers.
- Nenhuma ponte obsoleta será removida antes da major planejada.
- Alterações em consumer serão isoladas e validadas no próprio repositório.
