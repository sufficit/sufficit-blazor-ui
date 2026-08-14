# Rollout da arquitetura SUI nos consumidores

## Referência reproduzível

O rollout inicial usou o pacote local
`artifacts/packages/Sufficit.Blazor.UI.1.26.814.1539.nupkg`, SHA-256
`95f1d796e0443857a28e9ed7615f58a9ab757ae86420080b96c20704e2f20834`,
e a referência de projeto irmã durante a validação integrada.

O release candidate final, já com o alias `.theme-dark`, é
`artifacts/packages/Sufficit.Blazor.UI.1.26.814.1618.nupkg`, SHA-256
`b5bea65eb1d75ddf3f76b7da10b652f0dbc741ffc242a721e353674765ddbf3a`.
O pacote foi instalado e compilado, com warnings tratados como erro, em RCLs
mínimas limpas `net9.0` e `net10.0`.

Nenhum pacote foi publicado externamente. O pacote e os consumidores foram
construídos a partir das cópias de trabalho locais em 2026-08-14.

Na publicação do catálogo, o entrypoint de 47.030 bytes resultou em 8.764
bytes gzip e 7.370 bytes Brotli; foundations em 12.190/3.166/2.661 bytes e
portals em 2.872/982/800 bytes, respectivamente. O bundle isolado
`Sufficit.Blazor.UI.2tmqdx6zu0.bundle.scp.css` foi publicado com fingerprint e
respondeu junto dos CSS e módulos em paths de raiz e `/app`.

## Contrato do host

Cada aplicação deve carregar:

1. `_content/Sufficit.Blazor.UI/sufficit-ui.css`, entrypoint global estável;
2. `{Host}.styles.css`, bundle do host que importa o CSS isolation da SUI;
3. seus próprios estilos de produto depois das fundações, quando precisar
   especializar a identidade.

Não se inclui `sufficit-ui.js`. Select, NavGroup, Tabs, Dialog e Tooltip
importam e descartam seus próprios módulos ES pelo ciclo de vida Blazor.

## Matriz validada

| Consumidor | CSS isolation | Identidade/tema | Gate executado |
|---|---|---|---|
| `sufficit-cloud-mobile` (canário) | `Sufficit.Cloud.Mobile.Web.styles.css` importa o bundle SUI | âmbar/laranja `#ee6321`, light | build estrito sem warnings, 100 testes, smoke HTTP e screenshots |
| Identity Management/Vault/Public | `Sufficit.Identity.Server.styles.css` importa o bundle SUI | Management vermelho `#cc0000`; Vault usa fallback âmbar | build estrito sem warnings, 712 testes e contratos de assets |
| `sufficit-blazor` Server/Client | referências diretas fazem ambos os bundles do host importarem o bundle SUI | âmbar `#ee6321`; `.theme-dark` reconhecida pela SUI | build normal, 280 testes e contrato sem script global |
| `sufficit-ai-genius` Desktop/Mobile | os dois bundles do host importam o bundle SUI | shell claro/escuro próprio; controles SUI âmbar e alias `.theme-dark` | solução estrita sem warnings, Android normal e 383 testes |

No `sufficit-blazor`, `-warnaserror` permanece bloqueado por débitos anteriores
ao rollout: constraints net9 de dependências de telefonia resolvidas com
pacotes net10, PackageReferences redundantes, nulabilidade/analyzers em projetos
irmãos e o advisory já existente do AngleSharp. O build normal chegou a zero
erros e não emitiu CS0618 da SUI. No head Android do AI Genius, o primeiro gate
estrito promoveu avisos preexistentes de nulabilidade e disponibilidade de APIs
Android; o build normal subsequente passou e gerou o pacote com o bundle SUI.

## Canário e cenários

O `sufficit-cloud-mobile` foi escolhido por ser um host SUI puro. A migração
removeu 88 usos das pontes depreciadas e passou a usar `ColorValue`,
`VariantValue`, `SizeValue` e `ToneValue`. O smoke real em Development validou:

- HTTP 200 para o CSS global, o bundle isolado, o CSS do produto e os cinco
  módulos colocalizados;
- tokens do tema com `--sui-color-primary: #ee6321`;
- shell, navegação/NavGroup, Select, Dialog, Tabs, tabelas, autocomplete e
  fields presentes no build integrado;
- layouts desktop 1440×1000 e mobile 390×844 sem perda de estilo.

Evidências visuais:

- `artifacts/phase8-consumers/cloud-mobile-desktop.png`;
- `artifacts/phase8-consumers/cloud-mobile-mobile.png`.

Falhas de consulta vistas no screenshot são o estado de erro esperado porque a
API de domínio não foi iniciada; não afetaram o carregamento/interação do shell.

## Janela de compatibilidade

- `sufficit-ui.css` permanece como entrypoint estável por toda a série v1 e na
  primeira v2; `styles/sui-foundations.css` e `styles/sui-portals.css` são
  detalhes internos e não devem ser usados diretamente por consumidores.
- Parâmetros `object`/textuais depreciados permanecem até `v2.0.0`; consumidores
  migrados usam os parâmetros tipados. O adapter textual `IdentitySuiTone` fica
  no consumer, separado dos componentes-base.
- A classe histórica `.theme-dark` é aceita como alias; novos hosts devem usar
  `SUIThemeProvider`/`data-sui-theme`.

## Rollback

Para rollback antes de commit/publicação:

1. restaurar no consumer a referência/versão anterior e o markup legado;
2. remover o link `{Host}.styles.css` somente se a versão anterior não possuir
   CSS isolation;
3. restaurar o script global somente para versões anteriores que ainda o
   exijam;
4. executar restore, build e o smoke de assets antes do redeploy.

O rollback foi exercitado sem tocar na cópia de trabalho: um `git archive HEAD`
do canário foi extraído em diretório temporário e compilado contra
`Sufficit.Blazor.UI 1.26.814.1358` da fonte local. O build dos dois projetos
terminou com zero erros e zero warnings. Em seguida a cópia ativa permaneceu na
referência nova e voltou a passar todos os gates.
