# SUITabs: ciclo de vida do JavaScript

Objetivo: impedir falha quando o elemento desaparece durante importação assíncrona.

1. Concluído — examinar exceção, componente e captura.
2. Concluído — proteger inicialização, revelação e descarte; regressão reproduzível.
3. Em andamento — executar testes e publicar correção na main; verificar consumidores.
4. Pendente — verificar entrega em Produção e registrar limitações.

Aceite: null/detached não lançam exceção; teclado e reveal preservados; componente descartado durante importação não inicializa eventos.
