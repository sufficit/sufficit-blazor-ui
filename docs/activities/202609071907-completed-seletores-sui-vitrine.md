# Seletores SUI na vitrine

Status: concluído e publicado. O usuário identificou na captura que o dropdown
de tema era nativo e não seguia a aparência da biblioteca.

## Alterações

- Cabeçalho e editor de temas usam SUISelect/SUISelectItem nos quatro seletores:
  tema, paleta, aparência e densidade. CSS local cuida apenas da disposição.
- Preservados modos sistema/claro/escuro, preferência persistente, formulário
  preenchido, presets, densidade, rótulos acessíveis e navegação por teclado.
- Opção selecionada do SUISelect usa texto do tema sobre fundo suave. A cor de
  ação preenchida era inadequada sobre a superfície escura da paleta âmbar.
- Blur antigo não fecha mais um menu reaberto após o foco voltar ao controle.
  Uma versão de foco invalida o fechamento assíncrono anterior.
- Documentação visual e changelog atualizados. Sem remoção de API pública.

## Evidências

Commit de implementação: 9646e62, enviado diretamente à main conforme a
permissão persistente desta sessão.

- Build/publish Release sem avisos; 548 testes de componentes passaram.
- 9 testes Chromium do artefato estático passaram, incluindo menus abertos em
  desktop/mobile, teclado, persistência e preservação do formulário.
- Medição de contraste compõe o fundo color-mix em canvas e exige 4,5:1;
  cobre um estado que a análise axe isolada não apontou. Axe também passou.
- CSS verificado: 53.960 bytes bruto, 9.884 gzip, 8.655 Brotli; budgets mantidos.
- CI completo passou: Chromium, Firefox, WebKit, Lighthouse, pacote, build e CSS.
  https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34165255992
- CodeQL passou:
  https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34165256002
- Publicação Pages passou:
  https://github.com/sufficit/sufficit-blazor-ui/actions/runs/34165256042
- 4 testes dos seletores/acessibilidade executados novamente na URL pública
  passaram. Capturas de menus abertos claro/escuro inspecionadas; artefatos
  locais em artifacts/selects-light.png e artifacts/selects-dark-mobile.png.

Página: https://sufficit.github.io/sufficit-blazor-ui/

Plano temporário encerrado e removido; servidor local de verificação encerrado.
