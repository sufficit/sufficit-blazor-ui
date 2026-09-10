# Mouse e teclado configuráveis em campos numéricos

`SUINumericField` separa a precisão de entrada (`Step`) dos incrementos da roda do mouse (`WheelStep`), das setas do teclado (`ArrowKeyStep`) e das setinhas clicáveis (`SpinnerStep`). As permissões `ChangeOnWheel` e `ChangeOnArrowKeys` são independentes e não bloqueiam os botões clicáveis.

```razor
<SUINumericField T="decimal" Step="0.01" Min="50" Max="2500"
                 ChangeOnWheel="@AllowWheel" WheelStep="@Increment"
                 ChangeOnArrowKeys="@AllowArrows" ArrowKeyStep="@Increment"
                 SpinnerStep="@Increment"
                 Immediate @bind-Value="Amount" />
```

- `WheelStep` é opcional. Sem ele, o comportamento existente continua usando `Step`.
- Com `WheelStep`, `ChangeOnWheel=false` bloqueia também o incremento nativo pela roda, sem remover o foco.
- Alterações desses parâmetros atualizam o mesmo campo imediatamente, sem recriá-lo.
- O incremento é simétrico e preserva a parte decimal: `360,37 + 10 = 370,37`.
- `ChangeOnArrowKeys` é true por padrão. False bloqueia somente ArrowUp/ArrowDown, não digitação, Tab ou navegação horizontal.
- Sem `ArrowKeyStep`, as setas habilitadas mantêm o comportamento nativo de `Step`. Com ele, usam o incremento específico. A aplicação de boletos passa o mesmo valor para WheelStep e ArrowKeyStep.
- Digitação continua usando a precisão de `Step`; os incrementos não impõem múltiplos ao valor digitado.
- `SpinnerStep` adiciona setinhas clicáveis SUI no lugar das nativas. Usa o salto
  configurado mesmo com roda/teclado desabilitados. Null preserva o spinner nativo.
  `IncreaseText`/`DecreaseText` permitem localizar os rótulos acessíveis. Cliques
  respeitam Disabled, readonly, fieldset desabilitado e Min/Max; Enter/Espaço
  ativam os botões. A configuração do salto não deve depender dos dois switches.
- Rolagem só altera um campo focado e habilitado. Ctrl/Meta não são interceptados; valores vazios não são preenchidos automaticamente. Min/Max são respeitados.
- Este componente não persiste preferências. A aplicação é responsável por carregar e salvar sua configuração.

Exemplo executável: catálogo `?component=SUINumericField`. Regressões: `NumericWheelSettingsTests` e `ShowcaseBrowserTests.MoneyWheel` (quatro combinações independentes em desktop e mobile, troca de configurações ao vivo, limites e preservação de centavos).
