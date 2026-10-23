# Menu removido durante inicialização

Objetivo: impedir erro/circuito desconectado quando SUIMenu sai da árvore enquanto
import/connect JavaScript está em andamento, liberando referências e listeners.

1. Concluído — reproduzir a corrida com teste determinístico e examinar ciclo de vida.
2. Concluído — corrigir componente/JS e validar inicialização, descarte e reentrada.
3. Concluído — integrar alterações autorizadas do Blazor, validar, publicar e testar no tablet.
4. Concluído — registrar resultados e limites.

Reprodução conhecida: anúncios → Novo imediatamente após lista aparecer;
SUIMenu.razor.js connect recebe panel null (addEventListener). Sem mutações de dados
para este teste. Preservar menu normal, teclado, foco e limpeza de resize.

Regressão reproduzida: ambos os testes MenuLifecycleTests falharam antes da correção.

Validação: 1.016 testes unitários e um teste Chromium de elementos nulos/desconectados, reconexão, teclado, foco e desconexão passaram.

Bloqueio: CI 39986690442 publicou corretamente, mas publicação posterior 1.26.1023.1639+871e56df substituiu UI pelo código antigo. Tablet reproduziu a falha antiga às 13:43:36. Checkout Blazor contém alterações alheias; AGENTS exige decisão antes de publicar essa árvore. UI original foi atualizado por fast-forward até 3537974.

2026-10-23: usuário autorizou publicar e integrar tudo na main, incluindo árvore atual do Blazor. Ordem: revisar e testar alterações atuais; integrar por worktree; publicação oficial; validação tablet; registro final. Bloqueio anterior resolvido pela autorização explícita.

CI 40004375828 concluído com sucesso. Três hosts Healthy em 1.26.1023.1848+942f17a9. Tablet passou importação atrasada, 20 ciclos Novo/Cancelar e menu móvel 390px, sem gravações/erros JS/overflow. Journal do eveo-apps sem SUIMenu ou quedas de circuito desde o início do teste (18:56:47 UTC).
