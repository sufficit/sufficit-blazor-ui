# Distribuição da skill Sufficit Frontend

## Objetivo e origem

A pedido do usuário, mover a fonte para junto de `skills/sui-design`, instalar no Codex, Claude, Zcode e Symposium e publicar no catálogo Genius com orientações de versão e atualização. Fonte inicial: sufficit-ai/15f238d.

## Entrega

- Fonte canônica: `skills/sufficit-frontend` neste repositório. Versão final **1.0.1**, commit `d74d8c0156987cb55e6d1bb249731fa1b8adbd09`, publicado em main. Skill independente de versão do NuGet SUI.
- Pacote inclui licença MIT-0, release.json, metadata.version, card Symposium e guia `references/installation.md`.
- Instalador `scripts/install.py` distribui snapshots completos; recibos guardam versão, revisão e SHA256 por arquivo. Status detecta ausência, alteração local, cópia sem gerenciamento e atualização disponível. Atualização preserva alterações locais e restaura o backup se a troca falhar.
- Destinos verificados: `/home/hugodeco/.codex/skills/sufficit-frontend`, `/home/hugodeco/.claude/skills/sufficit-frontend`, `/home/hugodeco/.zcode/skills/sufficit-frontend`, `/home/hugodeco/.symposium/repo/skills/sufficit-frontend` e pasta `.symposium/skills` de compatibilidade.
- A cópia antiga foi removida de sufficit-ai e seu README aponta para a fonte nova. Commits de migração `37fa269` e `f1cfbb0` publicados; o histórico original foi preservado.
- Genius: issue [#612](https://github.com/sufficit/sufficit-ai-genius/issues/612), PR [#613](https://github.com/sufficit/sufficit-ai-genius/pull/613) mergeada em `8308830`. Catálogo v2 com 150 entradas; 149 Android/Google preservadas, v1 sem mudança de conteúdo.
- Catálogo publicado e comparado: https://storage.googleapis.com/suff-public/sufficit-ai-genius/skills/v2/catalog.json . Entrada `sufficit-frontend` contém versão 1.0.1 e SHA d74d8c0, com recursos carregados sob demanda. Não houve release de binário Genius.

## Verificação e correções

- Validador de skill e referências locais aprovados.
- Instalador exercitado em home temporário: cinco destinos, execução repetida, preservação de edição local, migração de symlink legado, atualização e rollback com falha de rename simulada.
- Cinco recibos reais em estado `current`, versão 1.0.1, revisão d74d8c0. A fonte antiga só foi removida depois de migrar a instalação Codex.
- Leitor real de metadados e matcher do card Symposium conferidos. A primeira versão tinha uma quebra de linha insuficiente após o título do card; correção 1.0.1 adiciona o separador exigido pelo adapter. Manifesto instalado no Symposium recebe version no topo, adaptação incluída no hash.
- CI local Genius aprovado: 2108 testes, dois skips de plataforma, zero erros de build; três warnings em testes não alterados. Packaging final aprovado após atualizar o pin para 1.0.1. Primeira asserção jq tinha precedência incorreta; corrigida e suíte completa aprovada.
- Regeneração do catálogo byte-estável e comparação dos 149 itens anteriores sem diferenças. Todos os 14 arquivos da skill no SHA público baixados e comparados à fonte.
- `gh pr edit` encontrou falha da API de Projects Classic; atualização do corpo feita via REST, sem reexecutar o comando incompatível. PR mergeada e label wip removida.

## Operação

Na raiz deste checkout:

```sh
python3 skills/sufficit-frontend/scripts/install.py status --target all
python3 skills/sufficit-frontend/scripts/install.py install --target all --update
```

O guia de instalação explica `git fetch`, comparação de versão remota, revisão do diff e avanço fast-forward antes da atualização. `current` compara com a fonte local, não promete ser a última versão remota. Sessões já abertas podem guardar catálogo em memória; instalação foi verificada em disco e pelos leitores, sem reiniciar ferramentas do usuário. A nova descoberta ocorre no próximo turno/carregamento do cliente.
