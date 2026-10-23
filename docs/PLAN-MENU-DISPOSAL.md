# Menu removido durante inicialização

Objetivo: impedir erro/circuito desconectado quando SUIMenu sai da árvore enquanto
import/connect JavaScript está em andamento, liberando referências e listeners.

1. Concluído — reproduzir a corrida com teste determinístico e examinar ciclo de vida.
2. Concluído — corrigir componente/JS e validar inicialização, descarte e reentrada.
3. Em andamento — publicar biblioteca/consumidor e testar navegação rápida no tablet.
4. Pendente — registrar resultados e limites.

Reprodução conhecida: anúncios → Novo imediatamente após lista aparecer;
SUIMenu.razor.js connect recebe panel null (addEventListener). Sem mutações de dados
para este teste. Preservar menu normal, teclado, foco e limpeza de resize.

Regressão reproduzida: ambos os testes MenuLifecycleTests falharam antes da correção.

Validação: 1.016 testes unitários e um teste Chromium de elementos nulos/desconectados, reconexão, teclado, foco e desconexão passaram.
