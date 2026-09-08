# Proporção dos botões e refinamento do preenchido

O usuário pediu que Large aumentasse a área do botão sem produzir ícone e texto exagerados. Também esclareceu que desejava melhorar o Filled existente, mantendo Soft como alternativa. Implementado com sufficit-frontend 1.0.1 e software-development.

## Estado inicial e escopo

Base `92350bc`, incluindo a integração da PR #18: alinhamento configurável, rótulos longos e estabilidade do teste de select preservados. Trabalho isolado em `.worktrees/button-balance`. A alteração preexistente de `src/Sufficit.Blazor.UI.csproj` no checkout principal não integra esta entrega.

## Alterações

- SUIButton usa Small como tamanho padrão do ícone independentemente da área do botão. `IconSizeValue` e o override legado continuam funcionando.
- Large mantém o token de fonte de botão; altura e padding continuam maiores. SUIIconButton limita o ícone a 1,5em, sem mudar a escala do SUIIcon independente.
- Filled: peso 500, raio de 1,25 vezes o token, sombra curta, borda derivada da cor da ação e estados de hover/pressionado menos escuros. Soft permanece disponível.
- O âmbar da ação na vitrine passou de `#b7440e` para `#a34f2b`. A alteração de paleta é específica da vitrine; cores de marcas consumidoras não foram substituídas.
- Exemplos voltaram a demonstrar Filled na ação principal, mantendo exemplo Soft. Playground explica a proporção do tamanho Large; catálogo gerado e documentação atualizados.

A tentativa inicial de igualar a borda ao fundo resultou em 2,689:1 sobre uma superfície escura. O teste foi mantido com requisito de 3:1 e a borda corrigida para mistura de 90% da ação com 10% de sua cor de contraste.

## Medições públicas

Valores padrão da vitrine, conferidos em desktop e móvel, temas claro/escuro e variantes Filled/Soft:

| Propriedade | Antes, Large | Depois, Large |
| --- | --- | --- |
| Altura | 44 px | 44 px |
| Fonte | 16 px | 14 px |
| Ícone | 32 px | 17,5 px |

No Filled desktop, Medium mede 116,17 × 40 px e Large 124,17 × 44 px. No móvel, ambos respeitam a altura mínima de interação de 44 px. O conteúdo continua proporcional, enquanto o padding diferencia os tamanhos.

CSS final: 54.670 bytes bruto, 10.044 gzip, 8.774 Brotli. Variação sobre a base: +50/+24/+16 bytes. Regras redundantes removidas; nenhum limite de orçamento aumentado.

## Validação

- `dotnet build Sufficit.Blazor.UI.slnx -c Release`: sem avisos/erros.
- `dotnet test tests/Sufficit.Blazor.UI.Tests -c Release`: 582 testes aprovados, incluindo override explícito do ícone.
- Verificação do catálogo gerado e compilação dos 68 exemplos copiados aprovadas.
- `npm run check:css`: geração e orçamento aprovados.
- Showcase local: 31 casos aprovados. Após o ajuste final da borda, repetidos os quatro casos de aparência/contraste e os oito casos Server pertinentes, todos aprovados.
- Consumidor Cloud Mobile local com referência ao projeto alterado: 12 cenários aprovados.
- CI completo: 582 testes de componentes; Chromium 51, Firefox 46 e WebKit 46 aprovados. Os casos restantes dependem da URL da vitrine ou do mecanismo específico. Pacote e Lighthouse aprovados; primeira rodada Lighthouse com performance, acessibilidade, boas práticas e SEO em 1,0.
- Baselines recapturadas no ambiente CI e comparadas novamente em execução normal, sem atualização automática: todos os jobs aprovados.
- Produção Pages: 16 medições de tamanho/tema/variante confirmadas; quatro casos de contraste, foco, teclado, disabled e estados aprovados com `SUI_SHOWCASE_URL=https://sufficit.github.io/sufficit-blazor-ui/ dotnet test tests/Sufficit.Blazor.UI.BrowserTests -c Release --no-build --filter FullyQualifiedName~ButtonVariantsKeepContrast`. CSS público comparado byte a byte com o gerado; capturas inspecionadas.

## Entrega e limites

- Código: [4937710](https://github.com/sufficit/sufficit-blazor-ui/commit/4937710).
- Referências visuais: [2015913](https://github.com/sufficit/sufficit-blazor-ui/commit/2015913).
- [CI e recaptura](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34242302017).
- [CI com comparação normal](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34242802776).
- [Pages concluído](https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34242041430).
- [Botões publicados](https://sufficit.github.io/sufficit-blazor-ui/?component=SUIButton).

O CI automático inicial foi substituído pela execução de recaptura solicitada por esta tarefa, conforme a concorrência configurada; não foi cancelamento manual do usuário. Não houve publicação de novo pacote NuGet nem deploy do Cloud Mobile nesta entrega. Commit/push da biblioteca e Pages realizados conforme autorização já existente.
