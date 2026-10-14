# SUITabs: inicialização após navegação

Exceção reportada: initialize recebeu elemento null e tentou addEventListener. O import assíncrono permite a remoção da raiz antes da inicialização.

Correção: módulo ignora raízes null/desconectadas, reveal faz a mesma validação, componente acompanha descarte e libera módulo importado após sua remoção. Inicialização ausente pode tentar novamente no render seguinte. Cache do módulo incrementado para v3. Sem mudança da API pública.

Regressão JavaScript reproduziu TypeError antes da correção e passou depois: 3 cenários (raiz ausente/desconectada, teclado/dispose, reveal horizontal/vertical). Suite existente: 962 testes passaram. Teste de importação pendente e descarte valida que initialize nunca é chamado na raiz removida.
