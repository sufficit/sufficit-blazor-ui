# Publicação da vitrine no GitHub Pages

Status: concluído. Autorização do usuário: ajustar, commit e push diretamente
na main e entregar o link público para teste.

## Resultado

Página: https://sufficit.github.io/sufficit-blazor-ui/

- Commit 02ecf3e: implementação da vitrine, temas, contratos de componentes,
  documentação, workflow Pages e alinhamento do CodeQL.
- Commit 98128e7: quatro referências visuais regeneradas no runner do GitHub.
- Este registro e os links no README/runbook são a atualização final documental.

CodeQL init/analyze usam o mesmo SHA da tag 4.37.9. Dependabot agora agrupa
as ações CodeQL; os PRs antigos 14 e 15 foram encerrados automaticamente após
as alterações entrarem na main. Nenhum force-push foi utilizado.

GitHub Pages foi criado via API com build_type=workflow; HTTPS está ativo.
Não houve publicação NuGet. O deploy da vitrine é independente da release do pacote.

## Verificação

- YAML validado; SHA da tag anotada CodeQL confirmado; 548 testes locais passaram.
- Pages: https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34163712273
  concluiu build, testes estáticos e deploy com sucesso.
- CodeQL: https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34163712446
  concluiu com sucesso.
- A primeira comparação Chromium Server revelou diferença de 18 px na altura
  de uma captura local versus runner. A imagem foi inspecionada e a recaptura
  executada pelo fluxo existente em ambiente CI, preservando a tolerância.
- Recaptura: https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34163967306
  passou em todos os jobs e gravou as referências na main.
- Comparação normal posterior, sem modo de atualização:
  https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34164207611
  passou em CSS, build, testes de componentes, pacote, Chromium, Firefox,
  WebKit e Lighthouse. Publicação NuGet foi corretamente ignorada, sem tag.
- Navegador na URL pública: HTTP 200, 68 links de componentes, página SUIButton
  com recarga direta, tema escuro persistente, mobile 390 px sem overflow e
  nenhum erro JavaScript. Captura local em
  artifacts/catalog-implementation/public-deployment.png.

O plano temporário de publicação foi encerrado e removido após estas validações.
O procedimento de operação está em ../RUNBOOK-SHOWCASE-PAGES.md.
