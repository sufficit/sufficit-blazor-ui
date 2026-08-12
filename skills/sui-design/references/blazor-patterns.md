# Engenharia Blazor — padrões Sufficit

Carregue ao construir lógica de página/componente em Blazor Server/interactive.

## Índice
- Lifecycle async (regra dura)
- Estado e render
- Dispose
- Headings semânticos
- Naming e dados
- Render mode por rota
- Touch targets
- Contexto multi-tenant (sufficit-cloud-mobile)
- Lição: detail-by-id sempre agrega

## Lifecycle async (regra dura)

**Nunca bloqueie a UI em `OnInitializedAsync`/`OnParametersSetAsync`** — eles travam o render. Padrão Sufficit:

```csharp
protected override async Task OnInitializedAsync()
{
    Selection.Changed += OnSelectionChanged;
    await LoadAsync();
}

private async Task LoadAsync()
{
    _loading = true; _error = null;
    try
    {
        _items = await Api.ListAsync();
    }
    catch (ApiException ex) { _error = ex.Message; }
    catch (HttpRequestException) { _error = "Sem conexão com o serviço. Tente novamente."; }
    finally { _loading = false; }
}
```

Para operações pesadas que não devem travar o primeiro render, dispare a partir de `OnAfterRender(firstRender)` com `_ = Task.Run(...)`; trackee `IsLoading`; mostre `<SUISkeletonLoader Type="SUISkeletonType.Table" />`.

## Estado e render

- De thread de background, sempre `await InvokeAsync(StateHasChanged)` — nunca `StateHasChanged()` direto.
- `_loading`/`_error`/`_notFound` controlam branches de render (`<SUISkeletonLoader>` / `<SUIAlert>` / `<SUIEmptyState>`). Nunca pop-in abrupto de conteúdo.

## Dispose

Componentes que assinam eventos/serviços devem `@implements IDisposable` e descadastrar:

```csharp
public void Dispose() => Selection.Changed -= OnSelectionChanged;
```

Senão vaza o circuito (Blazor Server) ou cresce o heap.

## Headings semânticos

Título de página = `<h1>` real (ver `components.md` → "Headings"). Um `<h1>` por rota. `FocusOnNavigate Selector="h1"` no `Routes.razor` move o foco/annonuncia a troca de página ao leitor de tela. Subseções `<h2>`.

## Naming e dados

- **"Manager", não "Admin"** (`User.IsManager()`).
- **Guid PKs** em entidades novas (não int identity).
- **Comentários e commits em inglês** (convenção Sufficit).
- Constantes nomeadas no lugar de magic numbers.

## Render mode por rota

Apps híbridos (`sufficit-cloud-mobile`): default `InteractiveWebAssembly`, mas rotas com segredo do servidor/auth circuit forçam `InteractiveServer`. Não assuma que toda página roda no mesmo modo; rotas auth-gated que chamam APIs server-only devem forçar server.

## Touch targets

≥44px em mobile. O SUI defaulta botão de ícone md para ~36px — em barras de app toque, sobrescreva no `@media (max-width: 899px)`:

```css
.cloud-mobile-shell .sui-icon-button { min-width: 44px; min-height: 44px; }
```

## Contexto multi-tenant (sufficit-cloud-mobile)

Padrão de autorização por contexto/tenant em apps Sufficit multi-contexto. Reuse ao construir apps similares.

**Lado Web (BFF):**
- `ContextSelectionState` (por circuito): começa em "todos os contextos" (`IsAllContexts=true`); `Select(tenantId)` escopa; `SelectAll()` volta ao agregado.
- `ContextSelectionHandler` (DelegatingHandler): aplica o filtro nas chamadas à API:
  - **detail-by-id GET** (`/api/v1/instances/{id}`, `/api/v1/operations/{id}`) → sempre `?scope=all` (o id é o escopo; a API autoriza);
  - **coleções GET** → `?scope=all` só no modo agregado;
  - **caso contrário** → header `X-Sufficit-Context-Id: <tenantId>`.

**Lado API:**
- `ICallerContext` (construído de claims validadas): `TenantId` (default = `sub` do usuário), `AccessibleTenantIds` (próprio + diretivas `mobile:{contextId}`), `CanSelectAnyTenant` (role manager/administrator).
- `ApplyContextScope`: `includeAllContexts` ? (manager vê tudo; usuário vê `AccessibleTenantIds`) : `TenantId == caller.TenantId`.
- `OperationNotFound`/`InstanceNotFound` (`AppProblemException.NotFound`) quando o recurso não está no escopo autorizado.

## Lição: detail-by-id sempre agrega

**Bug real que corrigimos no `sufficit-cloud-mobile`:** manager com um contexto selecionado abria detalhe de outro tenant → 404 ("Detalhe indisponível neste contexto"). Causa: o branch escopado de `ApplyContextScope` filtra `TenantId == selecionado` **sem bypass de manager**, e o handler só mandava `scope=all` no circuito agregado.

**Regra:** detail-by-id busca sempre em escopo agregado — o id é o escopo, o server autoriza via `AccessibleTenantIds`/`CanSelectAnyTenant`. A página então re-escopa o circuito para o tenant do recurso (`Selection.Select(recurso.TenantId)`) para as chamadas seguintes. Coleções e mutações continuam respeitando o contexto selecionado.

Quando construir um endpoint de detalhe por id: aceite `?scope=all` e autorize pelo conjunto acessível do caller; nunca exija header de contexto único para resolver um id.
