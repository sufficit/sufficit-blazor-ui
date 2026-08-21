# Arquitetura — versionamento, compatibilidade e TFMs

## Contrato de versão

O pacote segue SemVer. A tag Git `vMAJOR.MINOR.PATCH[-prerelease]` é a fonte
única da versão publicada; builds locais usam `0.0.0-local` e builds de CI sem
tag usam `0.0.0-ci.<run>`.

- **Patch:** correção compatível, sem remoção de API ou mudança deliberada de
  comportamento público.
- **Minor:** API aditiva e deprecações com substituição executável.
- **Major:** remoção/renomeação pública, mudança de comportamento incompatível
  ou retirada de TFM ainda oferecido pela major anterior.

Pacotes publicados são imutáveis. Uma falha pós-release gera uma nova versão;
`--skip-duplicate` não faz parte do caminho normal.

## Compatibilidade da linha atual

- TFM: `net10.0`;
- `sufficit-ui.css` permanece o único entrypoint global público;
- CSS isolation continua sendo carregado pelo `{Consumer}.styles.css`;
- os 25 parâmetros visuais legados continuam presentes com `ObsoleteAttribute`;
- `SUISelectItem.Value` permanece `object` por desenho e não faz parte da
  remoção v2;
- APIs aditivas como `SUIFormGrid` podem entrar em minor.

## Política de framework

Em 2026-08-14, a varredura dos projetos que referenciam diretamente a SUI
encontrou todos os consumers de produção em `net10.0` ou `net10.0-android`.
Em 2026-08-21, o contrato temporário `net9.0` foi retirado do projeto, CI e
validador de pacote para eliminar uma matriz duplicada sem consumer de
produção correspondente.

Essa retirada é incompatível com a série v1 já publicada. Portanto, qualquer
pacote produzido a partir desta linha deve usar versão `v2.0.0` ou superior; o
runbook e o changelog impedem uma tag v1 acidental.

Adicionar um novo TFM segue o caminho aditivo. Retirar um TFM exige major,
mesmo quando o runtime já saiu de suporte.

## API compatibility

O baseline em `eng/PublicApiBaseline.txt` impede remoções acidentais e o package
validation verifica a forma do `.nupkg`. Alterar o baseline exige revisão
intencional; ele não deve ser regenerado automaticamente no CI.

Uma release candidate v2 deve comparar assemblies v1/v2, classificar cada
diferença e provar que toda remoção aparece no plano v2. Mudanças sem item de
migração bloqueiam a release.
