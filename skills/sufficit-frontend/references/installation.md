# Instalação, atualização e versão

Fonte canônica: [sufficit/sufficit-blazor-ui — skills/sufficit-frontend](https://github.com/sufficit/sufficit-blazor-ui/tree/main/skills/sufficit-frontend), ao lado de `sui-design`. A versão SemVer em `release.json` e `metadata.version` no SKILL.md pertence à skill, não ao NuGet SUI.

## Instalar ou conferir

No checkout revisado de `sufficit-blazor-ui`, executar:

```sh
python3 skills/sufficit-frontend/scripts/install.py install --target all
python3 skills/sufficit-frontend/scripts/install.py status --target all
```

Requer Python 3. Copia snapshots completos para Codex (`$CODEX_HOME/skills`, ou `.codex/skills`), Claude (`.claude/skills`), Zcode (`.zcode/skills`) e Symposium (`.symposium/repo/skills` e `.symposium/skills` para compatibilidade). Os caminhos são relativos ao home do usuário; `--home /caminho` permite testar outro perfil sem alterar o ambiente real. Para uma ferramenta, usar `--target codex`, `claude`, `zcode` ou `symposium`.

O Symposium recebe também `version` no topo do frontmatter para seu leitor de metadados; seu `skill-card.md` permite descoberta pelo adapter OpenAI. A diferença é conhecida e entra no hash da instalação. Se o Symposium usa uma raiz customizada, instalar/copiar para a raiz configurada, não presumir que a pasta padrão seja lida.

O recibo `.sufficit-installation.json` contém versão, commit de origem e SHA256 por arquivo. `status` compara o conteúdo instalado com o recibo e com o checkout fornecido: `current`, `missing`, `modified`, `unmanaged`, `legacy-link` ou `update-available`. Um checkout pode estar atrasado em relação ao remoto: `current` significa igual à fonte local, não “última versão do GitHub”. Nenhuma consulta de rede é feita pelo instalador.

## Atualizar de forma verificável

```sh
git fetch origin main
git diff HEAD..origin/main -- skills/sufficit-frontend
git show origin/main:skills/sufficit-frontend/release.json
# Após revisar, numa árvore limpa e sem divergência:
git merge --ff-only origin/main
python3 skills/sufficit-frontend/scripts/install.py install --target all --update
python3 skills/sufficit-frontend/scripts/install.py status --target all
```

Rodar os comandos a partir do repositório fonte, não de uma cópia instalada. `--update` substitui somente snapshots gerenciados sem edição local. A troca usa staging e backup com restauração se falhar. Diretório sem recibo ou conteúdo editado é preservado; revisar e guardar suas alterações antes de reinstalar. Para a migração inicial de um symlink antigo, `--migrate-from /caminho/exato/antigo` autoriza apenas aquele alvo. Não usar esse parâmetro para diretórios desconhecidos.

Depois da instalação, a nova skill fica disponível na próxima descoberta/turno do cliente; sessões que guardam catálogo em memória podem precisar ser reabertas. Instalação em disco não comprova que uma sessão antiga já recarregou instruções.

## Genius

A entrada `sufficit-frontend` no catálogo v2 aponta para `skills/sufficit-frontend` em um SHA Git imutável. Buscar pelo nome, `frontend`, `blazor` ou `design-system`. O cliente carrega o SKILL.md e lê referências sob demanda; o helper Python/JS não é executado automaticamente pelo catálogo.

Para publicar nova versão: atualizar versão/metadados/card e orientações, validar a skill, commitar/publicar no SUI; atualizar o SHA da fonte Sufficit em `sufficit-ai-genius/skills/sources.json`; regenerar com `scripts/update-skill-catalog.sh`; revisar o diff e os testes; publicar pelo fluxo `scripts/publish-skill-catalog.sh`. Não trocar por URL de `main`. Conferir o SHA da entrada pública e o `release.json` nesse mesmo SHA. Skills já instaladas no Genius podem continuar na revisão anterior: conferir origem em Extensões → Skills e reinstalar pela revisão nova conforme o fluxo do cliente.
