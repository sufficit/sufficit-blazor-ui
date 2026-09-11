# Arquitetura — versionamento, compatibilidade e TFMs

## Contrato de versão

A SUI usa o formato de `Sufficit.Identity.Core` com major 2: **`2.yy.MMdd.HHmm`**,
com data/hora UTC. Debug usa `2.99.0.0`; Release e Packing capturam o horário
uma vez. `Version`, `AssemblyVersion` e `FileVersion` permanecem alinhados.

A tag de publicação é `v2.yy.MMdd.HHmm`. O NuGet remove zeros à esquerda:
`2.26.0908.0005` aparece como `2.26.908.5`; é a mesma versão. O script
`python3 scripts/release_version.py` gera o formato normalizado; com argumento,
valida a data real, o horário e a numeração. `VersionSuffix` fixa o valor no
MSBuild durante empacotamento/CI, sem reconsultar o relógio entre artefatos.

CI sem tag também valida um pacote no formato corporativo, mas não publica.
`2.99.0.0`, a linha `1.26.*` de 2026 e o SemVer legado (`1.27.0`, `1.28.0`, `2.0.0`, `2.1.1`, `2.2.1`) não são versões de release. O major 2 existe porque o NuGet.org não apaga as SemVer legadas e um range `1.*` nunca as venceria; `2.*` é o range recomendado aos consumidores. Não usar `/p:Version`
isoladamente, pois isso pode divergir da identidade do assembly; usar
`/p:VersionSuffix=2.yy.MMdd.HHmm`.

Pacotes publicados são imutáveis. Uma correção recebe um novo timestamp UTC;
`--skip-duplicate` não faz parte do caminho normal. Duas releases no mesmo
minuto precisam de minutos distintos. A data identifica o build, sem prometer
compatibilidade pela progressão de major/minor. Mudanças incompatíveis exigem
changelog, plano de migração e validação nos consumidores.

## Compatibilidade da linha atual

- TFM: `net10.0`;
- `sufficit-ui.css` permanece o único entrypoint global público;
- CSS isolation continua sendo carregado pelo `{Consumer}.styles.css`;
- os 25 parâmetros visuais legados continuam presentes com `ObsoleteAttribute`;
- `SUISelectItem.Value` permanece `object` por desenho e não faz parte da
  remoção v2;
- APIs aditivas como `SUIFormGrid` são registradas no changelog.

## Política de framework

Em 2026-08-14, a varredura dos projetos que referenciam diretamente a SUI
encontrou todos os consumers de produção em `net10.0` ou `net10.0-android`.
Em 2026-08-21, o contrato temporário `net9.0` foi retirado do projeto, CI e
validador de pacote para eliminar uma matriz duplicada sem consumer de
produção correspondente.

Essa retirada é incompatível com os pacotes históricos que ofereciam net9.0.
O consumidor deve conferir TFMs e migração antes de atualizar, mesmo com o
prefixo corporativo `1`. O nome histórico “v2” no plano de migração identifica
o trabalho de API, não a numeração NuGet.

Adicionar ou retirar TFMs exige validação dos consumidores afetados.

## API compatibility

O baseline em `eng/PublicApiBaseline.txt` impede remoções acidentais e o package
validation verifica a forma do `.nupkg`. Alterar o baseline exige revisão
intencional; ele não deve ser regenerado automaticamente no CI.

Uma release candidate v2 deve comparar assemblies v1/v2, classificar cada
diferença e provar que toda remoção aparece no plano v2. Mudanças sem item de
migração bloqueiam a release.
