# Vendor — MudBlazor

Código-fonte do [MudBlazor](https://github.com/MudBlazor/MudBlazor) copiado para
dentro deste repositório em vez de referenciado como pacote NuGet.

## Por quê

Para não depender de um pacote externo: nenhuma release de terceiro pode
quebrar as aplicações da Sufficit sem que a mudança passe por aqui primeiro. Em
troca, a manutenção deste código passa a ser nossa — inclusive correções de
segurança e compatibilidade que antes vinham do upstream.

## Licença

MudBlazor é MIT, que **exige** preservar o aviso de copyright. A licença
original está em `LICENSE-MudBlazor.txt` e os cabeçalhos nos arquivos foram
mantidos. A licença MIT-0 na raiz do repositório vale para o código escrito
pela Sufficit, não para este diretório.

## Assets (CSS/JS)

Não foram copiados. No upstream eles são gerados de SCSS e TypeScript por um
pipeline npm, e trazer essa toolchain custaria mais do que resolve. O CSS e o JS
compilados são extraídos do pacote NuGet no CI.

## Origem

Copiado de `MudBlazor/MudBlazor` na branch padrão. Ao aplicar correções do
upstream, registre aqui o commit de referência.

## Pendência aberta: SuffButton e SuffIconButton

Estes dois componentes reportam `RZ10012` ("Found markup element with
unexpected name") para todos os seus componentes filhos. Os outros nove
compilam.

O que já foi descartado por experimento:

- **Não é descoberta de componentes.** Um probe usando `<MudIcon>`
  (definido em `.razor`), `<MudElement>` (definido só em C#) e um componente
  C#-only escrito para o teste compilou sem erro algum.
- **Não é `@inherits MudButton`,** nem `@using MudBlazor.Extensions`, nem
  `@using static MudBlazor.EventUtil` — cada um isolado num probe compila.
- **Não é atributo inexistente:** `Ref`, `RefChanged`, `ClickPropagation`,
  `HtmlTag` e `PreventDefault` existem no `MudElement` vendorizado.
- **Não é o namespace:** qualificar como `<MudBlazor.MudElement>` não muda nada.

Resta investigar a combinação de diretivas, o `@implements IDisposable` e o
bloco `@code`. Um `dotnet build` local mostra a lista completa de erros de uma
vez, o que é muito mais rápido que iterar pelo CI.
