# Runbook — release do pacote SUI

## Pré-condições

- worktree limpa e commit alvo presente em `main`;
- `CHANGELOG.md` atualizado;
- versão escolhida conforme SemVer;
- nenhuma ponte pública removida fora de uma nova major;
- secret `NUGET_API_KEY` configurado no repositório.

## Publicação

1. Execute localmente os gates relevantes ou confirme o workflow verde do
   commit que será marcado.
2. Crie uma tag anotada, por exemplo:

   ```bash
   git tag -a v1.27.0 -m "Sufficit.Blazor.UI 1.27.0"
   git push origin v1.27.0
   ```

3. O workflow `Build` valida o formato da tag, compila `net9.0` e `net10.0`,
   executa bUnit, Playwright/axe, gera e inspeciona o pacote e somente então
   habilita o job `Publish tagged package`.
4. O job final recria e valida o artefato exato com a versão da tag antes de
   enviar ao NuGet.org. Não existe publicação automática por `push` em `main`.

## Prerelease

Use tags como `v2.0.0-preview.1`. O sufixo é preservado no pacote e não é
selecionado por consumers que aceitam apenas versões estáveis.

## Verificação

- confira a versão e os TFMs na página do pacote;
- instale a versão exata em um consumer canário;
- valide o CSS global, o bundle de CSS isolation e os módulos `.razor.js`;
- registre o resultado em `docs/CONSUMER-ROLLOUT.md` ou na atividade da release.

## Falha e recuperação

Pacotes NuGet publicados são imutáveis. Não reutilize uma versão e não dependa
de `--skip-duplicate`: corrija a causa, incremente o patch/prerelease e publique
uma nova tag. Se o job falhar antes do push, remova/corrija a tag somente após
confirmar que a versão não apareceu no NuGet.org.
