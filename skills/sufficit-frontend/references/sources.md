# Fontes e adaptação

Referências consultadas em 2026-09-08. Síntese e critérios próprios da Sufficit; nenhum pacote, template ou catálogo externo é redistribuído aqui. Antes de importar código, conferir licença do artefato e compatibilidade da versão escolhida. Exemplos públicos não tornam toda a coleção livre.

| Fonte | O que observar | Como usar nesta skill |
| --- | --- | --- |
| [Open Props](https://open-props.style/) e [repositório oficial](https://github.com/argyleink/open-props) | Custom properties para espaço, tipografia, cores, sombras e movimento; adoção por conjuntos | Aproveitar uma base de tokens se o projeto precisar; mapear papéis semânticos ao tema local. Conferir valores, não equivalência de sufixos numéricos. |
| [VibePrompts](https://vibeprompts.dev/) | Composições e exemplos de interface; [tabela com toolbar](https://vibeprompts.dev/dashboards/dashboards-data-table-with-toolbar/) inclui busca, ordenação e estado sem resultados | Extrair tarefa, hierarquia e estados. Escrever a implementação na stack do projeto e verificar semântica/teclado; um preview não prova acessibilidade. |
| [ReUI: introdução](https://reui.io/docs) e [estilos](https://reui.io/docs/styling) | Composição de primitivas em exemplos e blocos; papéis semânticos de estado | Conferir API e infraestrutura antes de adotar. Exemplos React/shadcn não são componentes Blazor. Componentes/exemplos abertos e materiais Pro têm condições distintas. |
| [DesignSystems.one: fundamentos](https://www.designsystems.one/foundations) e [espaçamento](https://www.designsystems.one/foundations/spacing) | Relações entre fundamentos, densidade e aplicação consistente | Usar a comparação para justificar escolhas; preservar escala e identidade locais. O catálogo é referência editorial, não norma de conformidade. |
| [Design System Checklist](https://github.com/ardakaracizmeli/design-system-checklist) | Organização em linguagem, fundamentos, componentes e manutenção, disponível em [src/data](https://github.com/ardakaracizmeli/design-system-checklist/tree/master/src/data) | Selecionar critérios aplicáveis e registrar evidência. Não exigir completar um design system inteiro para corrigir uma tela. |

## Escolher sem acumular dependências

1. Partir da lacuna observada no projeto; pesquisar um exemplo diretamente relacionado, não todo o catálogo.
2. Registrar o padrão útil e o que precisa mudar para atender dados, marca, stack e acessibilidade locais.
3. Preferir código/API já existentes. Só adotar dependência quando o ganho justificar build, peso, manutenção e licença.
4. Fixar a versão aprovada pelo projeto e testar o artefato de produção. Links sem versão são referências de consulta, não instruções de CDN para produção.

Open Props declara MIT. Na consulta, a raiz de Design System Checklist não apresentava arquivo de licença explícito; o README se descreve como open-source, o que sozinho não determina condições de redistribuição. ReUI distingue materiais gratuitos e pagos. Para VibePrompts e DesignSystems.one, usamos links e análise própria, sem reproduzir seus catálogos ou presumir uma licença de código.

Se uma fonte estiver indisponível, continuar com o contexto local e declarar a limitação da consulta. Não afirmar que a referência foi examinada se só foi visto um snippet de busca.
