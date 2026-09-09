# Campos rotulados e ações na mesma barra

O rótulo faz parte do wrapper do campo. Centralizar um botão em relação a esse
wrapper deixa o botão acima do centro do input. Use a composição existente:

```razor
<SUIStack Row Wrap AlignItems="End" Spacing="3">
    <div style="flex:1 1 16rem;min-width:0">
        <SUITextField T="string" Label="Buscar" @bind-Value="search" />
    </div>
    <SUIButton VariantValue="SUIVariant.Outlined" OnClick="Apply">Aplicar</SUIButton>
</SUIStack>
```

Mantenha campo e ação no mesmo tamanho de controle. `Wrap` permite outra linha
quando não há espaço; `min-width:0` permite que o conteúdo respeite seu contêiner.
O exemplo executável está em `SUIStackExample`; a fixture
`/fixtures/field-actions` testa base alinhada, quebra, filtro imediato e ação.

Esse padrão serve a barras sem mensagens de ajuda/erro sob os campos. Em formulários
com essas mensagens, use `SUIFormGrid` e um grupo de ações após a grade: a base do
wrapper passa a incluir a mensagem e já não representa a base do input.

Aprendizado dos consumidores Identity/Fleet: classes genéricas de toolbar podem
sobrescrever a grade local na cascata; CSS isolado do pai não estiliza automaticamente
a raiz de um componente filho. Prefira a API `AlignItems` a seletores internos e
confira a versão publicada, inclusive atributos que podem ser encaminhados ao HTML
sem que o componente realmente implemente a funcionalidade.
