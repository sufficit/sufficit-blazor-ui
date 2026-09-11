# Índice de documentação

Este diretório é organizado pelo propósito de cada documento. `README.md` é
reservado para índices; todo outro arquivo Markdown usa `TYPE-SUBJECT.md`,
com prefixo de tipo em maiúsculas e sujeito em kebab-case maiúsculo. Trabalho
concluído é arquivado em `activities/` com prefixo de timestamp
`YYYYMMDDHHmm-`.

- [Vitrine: composições, temas e desempenho](USAGE-SHOWCASE-WORKSPACE.md)

## Convenção de nomes

| Prefixo | Uso |
| --- | --- |
| `ARCHITECTURE-` | Fronteiras duráveis, propriedade e decisões técnicas |
| `DESIGN-` | Intenção de produto, interação e regras do sistema visual |
| `PLAN-` | Trabalho ativo com gates explícitos — apenas itens pendentes |
| `RUNBOOK-` | Procedimento operacional ordenado, validação e rollback |
| `USAGE-` | Configuração e uso voltado ao consumidor de um recurso implementado |
| `EVALUATION-` | Instruções de avaliação ou avaliação datada |
| `INVESTIGATION-` | Evidência com limite de tempo, diagnóstico e conclusões |

## Planos ativos (trabalho pendente)

- [Sufficit.Blazor.UI v2](PLAN-SUI-V2.md) — remoção das pontes obsoletas,
  integração com `EditContext`, net10-only e gates dos consumers
- [Adoção dos componentes `SUI*`](PLAN-CONSUMER-MIGRATION.md) —
  adoção básica concluída nos consumers; permanecem verificações visuais,
  runtime WASM e navegação Management explicitamente listadas no plano

## Avaliações

- [Revisão da biblioteca — 2026-09-11](EVALUATION-LIBRARY-REVIEW-20260911.md) —
  nota por área, achados verificados (versão calendário perde para SemVer
  legado listado, worktrees órfãos, budgets sem folga, docs e bUnit
  incompletos) e roadmap P0–P4 de atualizações

## Runbooks

- [Vitrine estática e GitHub Pages](RUNBOOK-SHOWCASE-PAGES.md) — estrutura, exemplos,
  publicação, validação em raiz/subpasta e recuperação

- [Release do pacote SUI](RUNBOOK-RELEASE.md) — tag UTC Sufficit, gates, publicação,
  verificação e recuperação

## Design

- [Hierarquia visual: em que ordem a tela é lida](DESIGN-VISUAL-HIERARCHY.md) —
  papéis de leitura e a escala de cada um, por que cor sozinha não cria
  hierarquia, razão mínima perceptível, e onde a consequência de uma ação mora

## Arquitetura

- [Versionamento, compatibilidade e TFMs](ARCHITECTURE-VERSIONING-AND-TFM.md) —
  calendário Sufficit, compatibilidade, política de retirada de framework e API baseline

## Trabalho concluído (activities/)

- [Seletores SUI na vitrine](activities/202609071907-completed-seletores-sui-vitrine.md) — dropdowns
  do cabeçalho/editor, contraste de seleção e retorno de foco

- [Publicação da vitrine no GitHub Pages](activities/202609071848-completed-publicacao-vitrine-pages.md) —
  deploy público, CodeQL alinhado e CI validado nos três navegadores

- [Catálogo estático e temas](activities/202609071829-completed-catalogo-estatico-temas.md) — vitrine WASM,
  GitHub Pages, 68 exemplos executáveis e contratos de componentes

- [Polimento da cor de ações primárias](activities/202608141506-completed-primary-action-color-polish.md) —
  acento âmbar separado da superfície ember, texto branco quente e contratos
  de contraste/estados nos dois temas
- [Alinhamento e ritmo do catálogo](activities/202608141448-completed-catalog-alignment-polish.md) —
  `SUIChoiceCard` sem tracks vazios, radios alinhados, headings com ritmo
  consistente e contratos geométricos desktop/mobile
- [Próxima etapa de engenharia da SUI](activities/202608141432-completed-sui-next-stage.md) —
  release determinístico, `SUIFormGrid`, regressão visual multi-browser,
  pacote executável, bundle CSS, skill desacoplada e último consumer validados
- [Independência de terceiros e hardening de testes](activities/202608141603-completed-frontend-test-hardening.md) —
  tokens CSS de terceiros removidos, guardas de convenção/tamanho/estilo,
  a11y ampliada, budgets de runtime e gate Lighthouse no CI
- [Arquitetura e hardening da biblioteca SUI](activities/202608141316-completed-sui-architecture-hardening.md) —
  famílias de componentes, CSS híbrido, módulos colocalizados, acessibilidade,
  catálogo, pacote multialvo e rollout nos consumidores concluídos
- [Adoção SUI no Vault + remoção dos componentes antigos](activities/202608092140-completed-vault-adoption.md) —
  Vault migrado, `AppIcon`/`EmptyState`/`PageHeader`/`StatusBadge` extintos do identity
- [Adoção SUI no cloud-mobile — completa](activities/202608092113-completed-cloud-mobile.md) —
  biblioteca expandida para 44 componentes + serviços, MudBlazor removido, 9 páginas migradas
- [Adoção SUI no Identity Management + contrato de temas](activities/202608092045-completed-identity-management-adoption.md) —
  `ISUITheme`/`SUIThemeProvider`, 4 componentes promovidos, Management migrado
- [Remoção completa do MudBlazor](activities/202608091921-completed-mudblazor-removal.md) —
  reescrita dos 7 componentes acoplados, remoção da árvore vendorizada, CSS
  próprio com tokens `--sui-*`
