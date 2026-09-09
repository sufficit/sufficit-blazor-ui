# Avaliação da skill (manutenção)

Usar workspace temporário, sem produção nem credenciais, e fornecer apenas a solicitação e os artefatos mínimos ao avaliador. Não lhe revelar a solução esperada. Conferir diffs e comportamento; o validador de frontmatter não mede qualidade de decisão.

| Solicitação realista | Evidência esperada |
| --- | --- |
| “Este grupo de configurações está sem espaço; preserve os painéis expansíveis.” | Diagnóstico pelo código/DOM, correção local, grupos abertos conferidos; sem reset global ou mudança de backend. |
| “Small, Medium e Large parecem iguais no catálogo Blazor.” | Parâmetros, classes e cascata investigados; dimensões finais comparadas; correção no responsável, sem três overrides locais. |
| “Crie uma página de planos usando estas referências; o projeto já tem tema e componentes.” | Composição apropriada, APIs existentes, estados funcionais e conteúdo verdadeiro/fixture identificada; sem instalação automática de React/ReUI. |
| “Analise o dropdown, ainda não altere código.” | Achados e proposta, sem edições/deploy. |
| “Use Open Props num projeto sem design system.” | Decisão explícita de imports/tokens e pipeline compatível; sem copiar reset para outra aplicação existente. |
| “Use esta skill sem Impeccable e sem internet.” | Trabalho a partir dos arquivos locais, sem bloqueio artificial, sem fingir consulta externa. |

Uma rodada independente é útil ao mudar o roteamento ou os contratos centrais. Mudanças pequenas não exigem criar uma equipe. Se adicionar scripts, executar casos que exercitem entradas válidas, ausentes e inválidas; checar ausência de efeitos colaterais.
