# Runbook — release do pacote SUI

## Pré-condições

- worktree limpa e commit alvo presente em `main`;
- `CHANGELOG.md` atualizado e migrações incompatíveis documentadas;
- versão no padrão Sufficit `1.yy.MMdd.HHmm` UTC, como Identity.Core;
- política Trusted Publishing no NuGet para `sufficit/sufficit-blazor-ui`,
  arquivo `build.yml`, environment `production`, autorizando publicar novas versões
  de `Sufficit.Blazor.UI`;
- gates de build, componentes, navegador e pacote aprovados.

## Autenticação do dia a dia

`build.yml` usa `NuGet/login` fixado por commit e autenticação OIDC do GitHub.
A permissão `id-token: write` existe somente nos jobs de publicação e de
verificação explícita de login. O login ocorre depois da validação do pacote,
imediatamente antes do push; a chave temporária não é registrada em logs nem
armazenada como secret. Não há fallback para `secrets.NUGET_API_KEY` na publicação.

Configure a variável Actions `NUGET_USER` com o nome do usuário NuGet que
**criou a política** (não email nem necessariamente o proprietário do pacote).
A variável é obrigatória e não tem fallback: o proprietário público `sufficit`
não foi reconhecido como criador no teste de autenticação. Sem essa variável,
o workflow falha explicitamente antes de solicitar a credencial. A política deve corresponder a:

- Repository Owner: `sufficit`;
- Repository: `sufficit-blazor-ui`;
- Workflow File: `build.yml`;
- Environment: `production` (também configurado nos jobs de login e publicação).

Para validar somente a troca OIDC, sem gerar/publicar pacote adicional:

```bash
gh workflow run build.yml --repo sufficit/sufficit-blazor-ui --ref main \
  -f verify-publishing-auth=true
```

A execução manual com essa opção não roda os demais gates nem publica pacotes.
As publicações por tag continuam exigindo todos os gates. Uma política ausente,
perfil incorreto ou escopo insuficiente deve falhar explicitamente no login.
Referência: https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing

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

O workflow manual `maintenance.yml` (`NuGet legacy version cleanup`) é limitado ao pacote
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
no mesmo minuto devem aguardar o minuto seguinte. Política OIDC inválida bloqueia publicação; a limpeza ainda requer uma chave
API com escopo Unlist. Falha de autenticação não equivale a pacote publicado ou deslistado. Se a falha ocorreu
antes do push, confirme a ausência no NuGet antes de corrigir a tag.
