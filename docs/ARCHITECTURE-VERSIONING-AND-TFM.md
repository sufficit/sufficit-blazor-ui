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

## Compatibilidade da série v1

- TFMs: `net9.0` e `net10.0`;
- `sufficit-ui.css` permanece o único entrypoint global público;
- CSS isolation continua sendo carregado pelo `{Consumer}.styles.css`;
- os 25 parâmetros visuais legados continuam presentes com `ObsoleteAttribute`;
- `SUISelectItem.Value` permanece `object` por desenho e não faz parte da
  remoção v2;
- APIs aditivas como `SUIFormGrid` podem entrar em minor.

## Política de framework

Em 2026-08-14, a varredura dos projetos que referenciam diretamente a SUI
encontrou todos os consumers de produção em `net10.0` ou `net10.0-android`.
`net9.0` permanece apenas como contrato de compatibilidade do pacote e nos
consumers temporários de validação.

O .NET 9 encerra suporte em 2026-11-10. A série v1 preserva `net9.0` até essa
data. A v2 poderá ser `net10.0`-only, mas apenas depois de:

1. nova varredura de references no commit candidato;
2. build/teste de todos os consumers conhecidos;
3. prerelease consumida pelo menos pelo canário `sufficit-cloud-mobile`;
4. documentação explícita da retirada no changelog e release notes.

Adicionar um novo TFM segue o caminho aditivo. Retirar um TFM exige major,
mesmo quando o runtime já saiu de suporte.

## API compatibility

O baseline em `eng/PublicApiBaseline.txt` impede remoções acidentais e o package
validation verifica a forma do `.nupkg`. Alterar o baseline exige revisão
intencional; ele não deve ser regenerado automaticamente no CI.

Uma release candidate v2 deve comparar assemblies v1/v2, classificar cada
diferença e provar que toda remoção aparece no plano v2. Mudanças sem item de
migração bloqueiam a release.
