# Clipboard sob Permissions-Policy

Objetivo: permitir a cópia explícita pelo usuário quando a API Clipboard é
bloqueada, preservando a política do host e o foco/seleção anteriores.

1. [concluído] Reproduzir a negação real de clipboard-write em Chromium.
2. [concluído] Corrigir SUICopyToClipboard e validar caminhos moderno, legado,
   negação de ambos e limpeza de elementos temporários.
3. [concluído] Validar integração no sufficit-blazor; registrar evidências e
   converter este plano em atividade; revisar, commit e push na main.

Evidência de aceite: teste de navegador com Permissions-Policy: clipboard-write=(),
clique real, texto sintético e leitura do clipboard; testes existentes de cópia.
Sem alteração visual, de contrato público ou de permissões do servidor.

Reprodução: Chromium retornou NotAllowedError com o mesmo texto da captura;
os dois casos de clique/Enter falharam antes da correção.

SUI: 7 testes Chromium e 4 testes bUnit de cópia aprovados após a correção.

## Evidências finais

- Antes: o teste Chromium com `Permissions-Policy: clipboard-write=()` negou
  `writeText` com `NotAllowedError`, reproduzindo a mensagem da captura enviada.
  Os dois testes de cópia sob bloqueio falharam (clique/Enter).
- Depois: 7 testes `ClipboardBrowserTests` aprovados, incluindo API moderna,
  política bloqueada, fallback, falhas e restauração de foco/seleção.
- 4 testes bUnit `CopyToClipboardTests` aprovados.
- Build `validation/endpoint-password-preview/EndpointPasswordPreview.csproj`
  aprovado com o perfil `build/DependencyCompatibility.targets`; houve avisos
  preexistentes de dependências/compatibilidade, nenhum erro.
- `validation/endpoint-password-preview/check.cjs` aprovado nos 8 cenários
  (política permitida/bloqueada × temas claro/escuro × larguras 1440/390).
  Componentes reais copiam senha sintética por clique e usuário por Enter;
  mascaramento, troca de ramal e ausência de overflow/erros JS preservados.
  Capturas locais em `/tmp/endpoint-password-preview/` (dados sintéticos).

- 8 testes `MarkdownCoverageTests` do Blazor aprovados após a conversão
  dos planos em atividades e atualização dos índices.

## Decisões e limites

A correção vive em `sufficit-blazor-ui`, consumido por ProjectReference pelo
Blazor localmente e no CI. Nenhuma política do host foi relaxada. O fallback
só anuncia sucesso quando o navegador confirma a cópia; se ambas as APIs
falharem, orienta seleção/cópia manual. Foco e seleção são restaurados e o
textarea temporário é removido mesmo em erro.

Sem alteração visual, parâmetros públicos ou ferramentas da IA. Os testes de
política são Chromium; o caminho legado depende de suporte do navegador a
execCommand. Produção autenticada não foi exercitada por estes testes.

Entrega: commits na main dos dois repositórios; a publicação do Blazor é
acionada pelo push via server.yml. Commit/push e testes locais não comprovam
produção; o resultado da execução remota deve ser consultado separadamente.
