# Vitrine de componentes e GitHub Pages

## Estrutura

- `src`: biblioteca NuGet; não referencia os hosts nem os dados demonstrativos.
- `samples/Sufficit.Blazor.UI.Demos`: RCL com exemplos executáveis, galeria
  compartilhada, metadados e fontes copiáveis.
- `samples/Sufficit.Blazor.UI.Showcase`: host WebAssembly independente, sem
  backend ou credenciais, com navegação documental e editor de temas.
- `samples/Sufficit.Blazor.UI.Catalog`: host Server para a galeria de regressão.

As páginas usam `?component=SUIButton` ou `?view=themes` na raiz do host.
Isso preserva links e recarga direta em hospedagem estática sem regras de
rewrite. Os arquivos servidos são a aplicação WASM, CSS, módulos JavaScript
e recursos estáticos; os componentes executam no navegador após o carregamento.

## Executar localmente

Requer SDK .NET 10; os pacotes ASP.NET Core estão fixados em 10.0.11. O build
do CSS requer Node/npm, e os geradores usam Python 3 sem dependências externas.

```bash
npm ci
npm run check:css
python3 scripts/generate-catalog.py --check
dotnet run --project samples/Sufficit.Blazor.UI.Showcase
```

## Gerar e servir o artefato

```bash
dotnet publish samples/Sufficit.Blazor.UI.Showcase -c Release -o artifacts/showcase -warnaserror
python3 scripts/prepare-pages.py artifacts/showcase/wwwroot --base-path /sufficit-blazor-ui/
mkdir -p artifacts/pages-test
ln -s "$(realpath artifacts/showcase/wwwroot)" artifacts/pages-test/sufficit-blazor-ui
python3 -m http.server 5286 --bind 127.0.0.1 --directory artifacts/pages-test
```

Abra `http://127.0.0.1:5286/sufficit-blazor-ui/`. O link simbólico é necessário
apenas para reproduzir o subpath localmente; reutilize o link caso já exista.
Para domínio próprio ou site na raiz, prepare com `--base-path /` e sirva
`artifacts/showcase/wwwroot` diretamente. Gere novamente antes de alternar
cenários de validação; o script modifica o `base` do arquivo publicado.

O preparador valida os assets necessários, cria `.nojekyll` e `404.html` e
remove as cópias comprimidas antigas de `index.html`. O restante dos assets
publicados pelo SDK permanece intacto.

## Publicar pelo GitHub Actions

1. No repositório, configure **Settings → Pages → Build and deployment →
   Source: GitHub Actions**.
2. Envie as alterações para `main`. O workflow `Component showcase` também
   aceita execução manual. Pull requests executam os gates e geram o artefato,
   sem executar o job de deploy.
3. Acompanhe build e deploy; a URL final aparece no ambiente `github-pages`.
4. Confirme acesso direto a `?component=SUISelect`, recarga, temas e exemplos.

O workflow usa `/sufficit-blazor-ui/` como base. Se o nome do repositório ou o
modo de hospedagem mudar, ajuste o preparador, a URL do teste e o diretório
servido no workflow juntos. Um domínio próprio exige base `/` e configuração
DNS/Pages correspondente; nenhum domínio é provisionado por este repositório.

A implementação local não publica automaticamente arquivos nem altera as
configurações remotas de Pages. Não há segredos no site e nenhuma API de
produção é chamada pelos exemplos.

## Manter a documentação executável

Para adicionar um componente, crie `Examples/<Nome>Example.razor` na RCL de
Demos e execute:

```bash
python3 scripts/generate-catalog.py
python3 scripts/generate-catalog.py --check
python3 scripts/check-catalog-examples.py
```

O gerador cobre todos os componentes Razor de `src`, extrai parâmetros
públicos e exige um exemplo por componente. Registra `catalog.json` e o mapa
de tipos usado por `DynamicComponent`, compatível com trimming da publicação.
Os exemplos copiáveis incluem somente os membros de `DemoBase` usados pelo
markup e suas dependências. O verificador compila esses textos em uma RCL
temporária, fora do host de demos. Há também teste de comparação dos parâmetros
documentados com a API real por reflection.

Os exemplos de elementos filhos podem compor o pai necessário. Exemplos dos
hosts de diálogo/snackbar e do tema exercitam a instância global da aplicação;
não instale providers concorrentes só para demonstrá-los.

## Validação

```bash
npm run check:css
python3 scripts/generate-catalog.py --check
python3 scripts/check-catalog-examples.py
dotnet build Sufficit.Blazor.UI.slnx -c Release -warnaserror
dotnet test tests/Sufficit.Blazor.UI.Tests -c Release --no-build
dotnet build tests/Sufficit.Blazor.UI.BrowserTests -c Release
```

Instale o Chromium usando o Playwright gerado pelo build (Linux):

```bash
tests/Sufficit.Blazor.UI.BrowserTests/bin/Release/net10.0/.playwright/node/linux-x64/node tests/Sufficit.Blazor.UI.BrowserTests/bin/Release/net10.0/.playwright/package/cli.js install --with-deps chromium
```

Com o artefato servido no subpath:

```bash
SUI_SHOWCASE_URL=http://127.0.0.1:5286/sufficit-blazor-ui/ dotnet test tests/Sufficit.Blazor.UI.BrowserTests -c Release --no-build --filter FullyQualifiedName~ShowcaseBrowserTests -- Playwright.BrowserName=chromium
```

Essa suíte abre todos os componentes, verifica recarga, busca, interação,
restauração do formulário, validação, paginação, tema persistente e mudanças de
preferência do sistema. Axe verifica a página de temas com snackbar visível
em desktop claro e mobile escuro. Execute também com base `/` ao mudar URLs.
A suíte Server existente continua necessária para regressão de componentes.

## Recuperação

Para reverter o site, reverta o commit problemático em `main` e deixe o mesmo
workflow validar/publicar o código anterior. Não reutilize um artefato sem
saber com qual `base` ele foi preparado. A publicação da vitrine e a release
NuGet são fluxos independentes.
