# Blazor e Sufficit.Blazor.UI

Confirmar os nomes, parâmetros e comandos no checkout atual; este guia descreve decisões, não congela uma versão da SUI.

## Antes de alterar

- Inspecionar `.csproj`, arquivos de build e lock/pins: o consumidor usa NuGet, referência de projeto ou checkout fixado? Identificar a versão efetiva.
- Ler componente Razor, code-behind, tema, CSS de origem e arquivos gerados. Descobrir ordem de carregamento, CSS isolation, seletor raiz do tema e módulos JS associados.
- Procurar no catálogo e no código o componente padrão. Um `<select>` paralelo não reproduz automaticamente a API, o tema e os estados do seletor da biblioteca.

## Resolver no nível responsável

Se apenas uma feature mistura classes com significados diferentes, corrigir composição local. Se uma propriedade pública não produz seu efeito em vários consumidores, corrigir a biblioteca, seus exemplos e teste de regressão, dentro do escopo autorizado. Não mascarar um defeito de tamanho global com CSS mais específico em cada aplicação.

Usar tokens e componentes SUI disponíveis. Se um exemplo usa React/Tailwind, portar o comportamento e a estrutura; não introduzir essas dependências em Blazor. Evitar seletores que dependam de detalhes internos quando houver API pública de personalização. Em CSS isolation, verificar onde o atributo de escopo realmente aparece antes de recorrer a `::deep`.

## Renderização e integração

Preservar binding, callbacks, validação, estado e descarte de recursos. Não usar manipulação DOM para substituir estado que o Blazor controla. JS interop deve respeitar a disponibilidade do DOM e o modo de renderização real; um componente prerenderizado não prova que sua interação foi inicializada.

Em seleção/abas/dialogs, exercitar foco e teclado, não apenas clique. Para navegação, conservar URLs profundas e parâmetros de contexto necessários. Em Blazor Server, distinguir um problema visual de circuito desconectado ou módulo JS incompatível.

## Build e entrega

Alterar fonte de CSS e regenerar bundles pelo comando do repositório, se houver; conferir o resultado gerado. Não declarar conclusão após mudar só uma cópia que o app não carrega. Executar build na configuração relevante, verificações da biblioteca e consumidor afetado conforme o contrato.

Se a entrega incluir atualização do consumidor, alinhar pacote/pin e validar o artefato com essa dependência. Se não incluir, informar que a melhoria ainda não chegou ao consumidor. Para publicação autorizada, conferir cache de CSS/JS, base path, static web assets e compatibilidade de módulos com sessões abertas. Usar o mecanismo de fingerprint/versionamento do projeto quando disponível, sem exigir query string manual em todos os assets.

Reinício de serviço, atualização de pacote e deploy são ações distintas; registrar somente as efetivamente realizadas. Usar fixture para testes de ações que alterariam recursos reais e declarar essa limitação.
