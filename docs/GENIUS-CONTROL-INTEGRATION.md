# Controles SUI no Genius

O Genius consome a SUI por ProjectReference quando há checkout irmão e por
NuGet 2.x em builds isolados. Alterações de consumo devem preservar os dois caminhos.

- Botões comuns usam SUIButton; ações somente com ícone usam SUIIconButton.
- SUICheckbox permite um rótulo externo composto por ícone, título e descrição:
  fornecer Id estável ao componente e usar label[for] no consumidor. A seleção
  permanece em Value/ValueChanged; o rótulo externo não substitui o controle.
- Callbacks de cópia/abertura nativa continuam pertencendo ao consumidor.
- Paleta, tipografia e dimensões de controles podem ser conectadas pelos tokens
  públicos --sui-*. O app preserva sua identidade e usa os estados da biblioteca.

Acompanhamento: sufficit/sufficit-blazor-ui#17 e sufficit/sufficit-ai-genius#624.

SUIButton respeita `Style="justify-content:flex-start;text-align:start"` para
rótulos ao início e `justify-content:space-between` para texto com indicador
lateral. O consumidor não precisa conhecer o elemento interno do rótulo.
A quebra de palavras continua usando a política global da biblioteca.

Os projetos UI, Desktop e Mobile do Genius aceitam a mesma propriedade MSBuild
`SufficitBlazorUIProjectPath`. Apontar para um checkout existente escolhe fonte;
um caminho inexistente exercita o fallback NuGet. O consumo foi verificado com
NuGet 2.2.1 e com o fonte da biblioteca. A correção visual do alinhamento está
no fonte atualizado e será incluída numa publicação posterior do pacote.
