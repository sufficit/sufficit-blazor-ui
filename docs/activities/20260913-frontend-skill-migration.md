# Migração da skill Sufficit Frontend

## Motivo

`sufficit-frontend` é uma orientação geral para aplicações Sufficit e não um
artefato da biblioteca Blazor UI. Mantê-la completa neste repositório duplicava
o pacote já publicado no catálogo do Genius e confundia sua origem canônica.

## Mudança

- O pacote completo permanece em
  `sufficit/sufficit-ai-skills/skills/sufficit/sufficit-frontend`.
- O catálogo v2 publicado pelo Genius já aponta ao commit público imutável
  `52b322ed03ffba8be1630238442746e1345b739a` antes desta remoção.
- A cópia `skills/sufficit-frontend` foi removida deste repositório.
- O README direciona usuários à origem pública central.
- `skills/sui-design` permanece aqui porque documenta especificamente a
  biblioteca e seus componentes.

## Validação

O catálogo público foi consultado após a publicação: contém 151 entradas, uma
única origem física `sufficit/sufficit-ai-skills` e a entrada
`sufficit-frontend` no caminho central esperado. A suíte do repositório confirma
que a remoção não afeta a biblioteca: os 668 testes de unidade passaram. A
execução genérica também tentou os testes Playwright sem iniciar a vitrine e
falhou somente com `ERR_CONNECTION_REFUSED` em `127.0.0.1:5180`; essa execução
não indicou falha funcional do pacote.
