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
  implementado em `sufficit-blazor` e `sufficit-ai-genius`; pendente nas UIs do
  `sufficit-identity` (`Sufficit.Identity.UI` e `Sufficit.Identity.UI.Management`)

## Trabalho concluído (activities/)

- [Remoção completa do MudBlazor](activities/202608091921-completed-mudblazor-removal.md) —
  reescrita dos 7 componentes acoplados, remoção da árvore vendorizada, CSS
  próprio com tokens `--sui-*`
