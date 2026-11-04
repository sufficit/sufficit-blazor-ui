# Largura intrínseca da barra de alterações

A captura do usuário mostrava o resumo quebrado apesar de espaço disponível na viewport. A página já utiliza o componente global SUIPendingChangesBar. O limite de 48rem foi substituído por largura intrínseca limitada à viewport. A disposição responsiva das ações abaixo de 600px permanece.

Referências: captura do usuário e componente existente; nenhuma alteração de tokens, cores ou ações.

Validação: teste de navegador do consumidor reproduziu duas linhas em 1849px antes da mudança e passou depois, incluindo 1440/768/390px, salvar e descartar. Build do fixture passou (avisos existentes); quatro testes PendingChangesBarTests passaram. Commit da biblioteca: 7c223ef; regressão no consumidor: bd3d59ae. Consumidor publicado pelo deploy.py em eveo-apps, apoint-apps e castrum-apps: versão 1.26.1104.1947+bd3d59ae083d26423a72c12b9bd780f5d3e372c9. Todos Healthy; hashes de DLL, WASM e CSS compartilhado conferidos contra o artefato. Endpoint público Healthy. Fluxos funcionais testados na fixture isolada, sem edição de dados reais.
