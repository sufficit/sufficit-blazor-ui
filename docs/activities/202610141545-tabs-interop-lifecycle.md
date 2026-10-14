# SUITabs: inicialização após navegação

Exceção reportada: initialize recebeu elemento null e tentou addEventListener. O import assíncrono permite a remoção da raiz antes da inicialização.

Correção: módulo ignora raízes null/desconectadas, reveal faz a mesma validação, componente acompanha descarte e libera módulo importado após sua remoção. Inicialização ausente pode tentar novamente no render seguinte. Cache do módulo incrementado para v3. Sem mudança da API pública.

Regressão JavaScript reproduziu TypeError antes da correção e passou depois: 3 cenários (raiz ausente/desconectada, teclado/dispose, reveal horizontal/vertical). Suite existente: 962 testes passaram. Teste de importação pendente e descarte valida que initialize nunca é chamado na raiz removida.

## Entrega

Commit de código UI 0dcd3de e consumidor Blazor f07cdb07 enviados diretamente à main conforme autorização vigente. Instalado pelo deploy.py oficial nos três servidores: eveo-apps, apoint-apps e castrum-apps. Todos responderam Healthy com versão 1.26.1014.1842+f07cdb07. Módulo v3 dos três nós e da URL pública comparado byte a byte com o fonte corrigido: idêntico (SHA-256 db33c4afaf4dddff01a0da5b786e402760171bfc7db6d64e52ae8b00c7f7c276).

CI da biblioteca ainda em verificação de site estático no checkpoint; validação autenticada da tela do usuário não executada. O circuito já afetado precisa ser recriado por recarga da página.
