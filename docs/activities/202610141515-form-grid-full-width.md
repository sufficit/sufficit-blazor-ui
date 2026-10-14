# SUIFormGrid com campos em largura inteira

- API nova FullWidthContent ocupa todas as colunas e usa Spacing para o intervalo da linha e entre seus próprios blocos.
- Rótulos em largura inteira não reservam linhas para colunas vizinhas; consumidores sem slot mantêm o DOM existente.
- Spacing=0 corrigido: zero explícito em vez de referência a token inexistente.
- Editor de serviços migrado: observações na mesma grade, CSS compensatório específico removido. AI e AGENTS atualizados.

## Validação

3 testes passaram: contrato sem slot, clamping e encaminhamento de ValueChanged da área nova. Navegador com componentes reais: 16px padrão desktop 1200px/mobile 390px, 32px para token 6, 0px para zero; largura inteira, label sem reserva e sem overflow. Build/publish Release passou.

## Entrega

Script oficial do Blazor nos três nós; mesma versão `1.26.1014.1806+b0a7c28ba9918c5e4037c9089f634dc0cd555622`, serviços ativos/Healthy. DLL UI `7a6347f4774f627e91d4f0fdb06b6a97364601190c7ab34a7f5bd280eb5bb2e7`; CSS `6b2f72d66bed3323f2cdc07c0eae7d0889afbdab6e0321cc78ed69919d12f580`, resposta HTTP conferida em cada host. Arquivo app DLL `78a853b1620ca2587664cf3b2fd931853fed7b3747f5b356995a7ed720cdb05f`. Biblioteca usada por referência de projeto e incluída no artefato do consumidor.

Sem publicação de nova versão NuGet; commits em main continuam pendentes. Mudanças concorrentes de SUIStack e documentação existentes preservadas. Não houve alteração em dados contratuais. Fixture não valida sessão autenticada de Produção.

API documentada em docs/components/form-grid.md. Plano: docs/PLAN-FORM-GRID-FULL-WIDTH.md.

## Consolidação para main

A API FullWidthContent foi incluída no baseline público revisado e no catálogo de parâmetros da biblioteca. CSS gerado conferido por npm run check:css. A compilação multi-framework de Base passou após ajuste de compatibilidade do modelo de composição.

Suite completa da biblioteca: 962 testes passaram; baseline público e catálogo atualizados; CSS gerado aprovado.
