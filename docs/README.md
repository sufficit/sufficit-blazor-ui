# Índice de documentação

Este diretório é organizado pelo propósito de cada documento. `README.md` é
reservado para índices; todo outro arquivo Markdown usa `TYPE-SUBJECT.md`,
com prefixo de tipo em maiúsculas e sujeito em kebab-case maiúsculo. Trabalho
concluído é arquivado em `activities/` com prefixo de timestamp
`YYYYMMDDHHmm-`.

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

- [Adoção dos componentes `SUI*`](PLAN-CONSUMER-MIGRATION.md) —
  implementado em `sufficit-blazor`, `sufficit-ai-genius`,
  `Sufficit.Identity.UI.Management`, `sufficit-cloud-mobile` e
  `Sufficit.Identity.UI.Vault`; pendente apenas em `Sufficit.Identity.UI`

## Trabalho concluído (activities/)

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
