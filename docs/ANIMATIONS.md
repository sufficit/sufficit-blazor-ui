# Animações (`SUIAnimation`)

Um único enum padroniza as animações da biblioteca. É `[Flags]`: combine tipos com `|`, ou use `None` para desligar tudo.

| Tipo | O que faz |
|---|---|
| `None` | Nenhuma animação. |
| `Enter` | Aparece com fade e leve subida. |
| `Loading` | Esqueleto com brilho enquanto carrega (sem o flag, o esqueleto fica parado). |
| `ValueChange` | Destaque curto quando um valor exibido muda. |
| `CountUp` | Números inteiros contam do valor anterior ao novo (~0,5 s). |
| `Hover` | Leve elevação e sombra ao passar o ponteiro. Só quando pedido explicitamente; fora do `Default`. |
| `Default` | `Enter`, `Loading`, `ValueChange` e `CountUp` (sem `Hover`: o card não sobe ao passar o mouse). |

## Padrão da aplicação

```csharp
builder.Services.AddSufficitUI(options => options.Animations = SUIAnimation.Enter | SUIAnimation.Loading);
// ou: options.Animations = SUIAnimation.None;
```

Fica registrado como `SUIAnimationPolicy` (singleton). Sem registro, vale `Default`.

## Por componente

```razor
<SUIStat Value="12" Label="Chamadas" Animation="SUIAnimation.None" />
<SUIStat Value="@Total" Label="Recebidas" Animation="SUIAnimation.CountUp | SUIAnimation.ValueChange" />
```

O parâmetro do componente vence o padrão da aplicação.

## Movimento reduzido

`prefers-reduced-motion: reduce` continua desligando toda animação e transição CSS (política central em
`sui-foundations.css`), mesmo com flags ligados. `CountUp` troca texto no servidor e não é afetado pela media query;
para usuários que pedem menos movimento, a aplicação pode registrar um padrão sem `CountUp`.

## Componentes que já respeitam

- `SUIStat` (todos os tipos).

Novos componentes animados devem expor `SUIAnimation? Animation` e resolver com `SUIAnimationPolicy.Resolve(...)`.
