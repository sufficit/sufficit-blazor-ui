# SUIFormGrid: campos e observações com espaçamento automático

Use `ChildContent` para os campos em colunas e `FullWidthContent` para observações ou outros campos que ocupam a largura inteira. A grade usa `Spacing` para separar todas as linhas e também os blocos dentro de `FullWidthContent`. O padrão é 4 (token de 16px); zero é respeitado. Não é preciso adicionar margem no consumidor.

```razor
<SUIFormGrid Columns="2">
    <ChildContent>
        <SUITextField TValue="string" Label="Representante" />
        <SUITextField TValue="decimal" Label="Comissão" />
    </ChildContent>
    <FullWidthContent>
        <SUITextField TValue="string" Label="Informações adicionais" Multiline="true" />
    </FullWidthContent>
</SUIFormGrid>
```

O conteúdo em largura inteira segue a adaptação móvel da grade e não reserva linhas de rótulo para colunas vizinhas. Sem o slot, o DOM e comportamento existentes são preservados. A página consumidora deve usar a grade para os campos relacionados; a biblioteca não acrescenta margens globais a inputs, evitando duplicar espaços em layouts que já possuem gap.
