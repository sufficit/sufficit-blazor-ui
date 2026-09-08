# Runbook — release do pacote SUI

## Pré-condições

- worktree limpa e commit alvo presente em `main`;
- `CHANGELOG.md` atualizado e migrações incompatíveis documentadas;
- versão no padrão Sufficit `1.yy.MMdd.HHmm` UTC, como Identity.Core;
- secret `NUGET_API_KEY` disponível ao repositório, com permissão de push;
- gates de build, componentes, navegador e pacote aprovados.

## Publicação

1. Gere uma versão UTC normalizada, valide-a e crie a tag do commit aprovado:

   ```bash
   package_version="$(python3 scripts/release_version.py)"
   git tag -a "v$package_version" -m "Sufficit.Blazor.UI $package_version"
   git push origin "v$package_version"
   ```

2. `Build` valida a data real e rejeita SemVer legado/Debug. O empacotamento
   recebe `/p:VersionSuffix="$SUI_PACKAGE_VERSION"`, mantendo versão do pacote,
   assembly e arquivo iguais. Não usar `/p:Version` isoladamente.
3. Após os gates de .NET 10, bUnit, Playwright/axe e Lighthouse, o job
   `Publish tagged package` recria, inspeciona e testa o pacote em consumidores
   mínimos antes do push. Push em `main` não publica NuGet.
4. Confira a versão publicada e atualize os consumidores com uma versão exata
   e seus lockfiles. O prefixo `1` não implica suporte aos TFMs históricos.

Debug permanece `1.99.0.0`. Release/Packing locais usam o relógio UTC; builds
CI sem tag validam um pacote com timestamp sem publicá-lo. O NuGet normaliza
zeros: `1.26.0908.0005` equivale a `1.26.908.5`. Não há sufixo prerelease neste
contrato; validar antes da publicação com pacote local.

## Verificação

- confira versão, TFMs, CSS global/isolation e módulos `.razor.js`;
- execute `python3 -m unittest discover -s scripts -p 'test_*.py'`;
- execute `scripts/validate-package.sh <arquivo.nupkg>`;
- instale a versão exata em um consumidor e registre a evidência da release.

## Corrigir versões antigas

NuGet.org não permite exclusão permanente por numeração incorreta. Deslistar
remove a descoberta normal e preserva restauração por versão exata:
https://learn.microsoft.com/en-us/nuget/nuget-org/policies/deleting-packages

O workflow manual `NuGet legacy version cleanup` é limitado ao pacote
`Sufficit.Blazor.UI` e às versões `1.27.0`, `1.28.0`, `2.0.0`, `2.1.1`, `2.2.1`.
Primeiro publique uma substituta no padrão corporativo. Informe `replacement`
no workflow; `apply=false` apenas consulta, `apply=true` deslista usando
`NUGET_API_KEY` com permissão de unlist. O script rejeita substituta ausente ou
não listada. Depois confira `listed=false` no catálogo NuGet, considerando o
atraso de indexação. Versões corporativas anteriores não entram na operação.

Versões `2.*` históricas ordenam acima de `1.yy...`, mesmo deslistadas em alguns
restores flutuantes. Migre referências `2.*` e pins antigos para a versão exata
corporativa publicada. Não apague caches locais para simular remoção remota.

## Falha e recuperação

Não reutilize versões já publicadas. Corrija e gere outro timestamp; releases
no mesmo minuto devem aguardar o minuto seguinte. Credencial ausente bloqueia
push/unlist; não equivale a pacote publicado ou deslistado. Se a falha ocorreu
antes do push, confirme a ausência no NuGet antes de corrigir a tag.
