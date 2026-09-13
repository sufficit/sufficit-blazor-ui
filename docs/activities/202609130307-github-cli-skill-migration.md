# Migração da skill GitHub CLI

## Motivo

`github-cli` ensina operações gerais do Genius com GitHub, autenticação do
Sufficit Identity e diagnóstico de CI. Ela não depende da biblioteca visual e
não deve ter o Blazor UI como origem canônica.

## Mudança

A pasta `skills/github-cli` foi removida depois que o pacote completo passou
para `sufficit/sufficit-ai-genius`. O catálogo público foi atualizado primeiro
para apontar para a nova revisão imutável, evitando interromper novas
instalações. Naquele momento, `skills/sufficit-frontend` e `skills/sui-design`
permaneceram neste repositório. A migração posterior da skill geral de frontend
para o catálogo público central está registrada em
[20260913-frontend-skill-migration.md](20260913-frontend-skill-migration.md).

Os relatórios históricos das entregas originais foram preservados como registro
do que ocorreu; este documento representa a localização vigente.
