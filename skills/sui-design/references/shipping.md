# Publicar (ship) — runbook eveo-apps

Runbook de release dos apps Blazor Sufficit no host `eveo-apps`. **Não há CD** — o CI só builda/testa; o release é manual. Carregue ao publicar.

## Índice
- Topologia e caminhos
- Pré-condições
- Fluxo (build → rsync → swap → restart → health)
- Exemplo real (sufficit-cloud-mobile Web)
- DataProtection (atenção ao mover path)
- Health checks
- Rollback
- Atenções por projeto

## Topologia e caminhos

- Host: `eveo-apps.sufficit.com.br` (`177.104.185.118`), SSH **`root@…:26492`**, chave `~/.ssh/id_ed25519`.
- Raiz de deploy (cloud-mobile): **`/opt/sufficit-cloud-mobile`** (padronizado em 2026-08-12; antes era `/opt/sufficit/cloud-mobile`).
- `api`/`web`/`worker` são **symlinks** → `releases/<slug>/<component>/`. Ativar = reposicionar symlink + restart.
- Services systemd: `sufficit-cloud-mobile-{api,web,worker}.service` (user `cloudmobile`, `ProtectSystem=strict`, `ReadWritePaths=…/data`).
- Front: NGINX local (Unix sockets) + HAProxy entre nós (afinidade de sessão Blazor). Web na porta compatibilidade `26515`; API `/health/ready`.
- **SDK net10 no servidor é só runtime** (9.0.119 SDK + runtimes 6/8/9/10). **Publique fora do servidor** (net10 SDK) e copie o output framework-dependent.

## Pré-condições

- Change commitado e pushed em `main`; CI verde (`dotnet build -c Release -warnaserror` + `dotnet test`).
- Decida o escopo: publique **só o componente alterado** (ex.: só `Web`). API/Worker só se mexeram.

## Fluxo

```bash
SLUG="$(date -u +%Y%m%dT%H%M%SZ)-<motivo>"
REL="/opt/sufficit-cloud-mobile/releases/$SLUG/web"
KEY=~/.ssh/id_ed25519; HOST=root@eveo-apps.sufficit.com.br
SSHOPTS="-4 -i $KEY -p 26492 -o BatchMode=yes -o ConnectTimeout=15"

# 1. publish fora do servidor (net10 SDK)
dotnet publish src/Sufficit.Cloud.Mobile.Web/Sufficit.Cloud.Mobile.Web.csproj \
  -c Release -o /tmp/cm-web-publish --nologo

# 2. rsync para a nova release (não afeta o serviço em execução)
ssh $SSHOPTS $HOST "mkdir -p $REL"
rsync -az --delete -e "ssh $SSHOPTS" /tmp/cm-web-publish/ "$HOST:$REL/"
ssh $SSHOPTS $HOST "chown -R root:root /opt/sufficit-cloud-mobile/releases/$SLUG"

# 3. swap atômico + restart (com rollback automático)
ssh $SSHOPTS $HOST bash -s "$SLUG" <<'REMOTE'
set -euo pipefail; SLUG="$1"
REL="/opt/sufficit-cloud-mobile/releases/$SLUG"; LINK="/opt/sufficit-cloud-mobile/web"
OLD="$(readlink "$LINK")"; ln -sfn "$REL/web" "$LINK"
systemctl restart sufficit-cloud-mobile-web.service
for i in $(seq 1 10); do [ "$(systemctl is-active sufficit-cloud-mobile-web.service)" = active ] && break; sleep 1; done
STATE="$(systemctl is-active sufficit-cloud-mobile-web.service || true)"
if [ "$STATE" != active ]; then ln -sfn "$OLD" "$LINK"; systemctl restart sufficit-cloud-mobile-web.service; echo "ROLLBACK para $OLD"; exit 11; fi
echo "OK ativo; new=$REL/web old=$OLD"
REMOTE
```

## Exemplo real

`sufficit-cloud-mobile` Web, fix de detail 404 para manager escopado (2026-08-12):
- release: `/opt/sufficit-cloud-mobile/releases/20260812T170739Z-scoped-manager-detail-fix/web`;
- publish local (net10, framework-dependent) + rsync + swap + restart;
- health: `active`; `/` → 302 Identity (`client_id=sufficit_cloud_mobile`); CSS com as regras novas; `nginx -t` ok; API `/health/ready` → 200.

Em seguida o time padronizou o path (`/opt/sufficit/cloud-mobile` → `/opt/sufficit-cloud-mobile`) e fixou DataProtection no novo path — ver abaixo.

## DataProtection (atenção ao mover path)

ASP.NET DataProtection protege tickets/estado OIDC. As keys persistem em disco; **mover a raiz de deploy sem atualizar o KeyPath invalida sessões e gera `CryptographicException` / read-only fs**. Configurar:

```json
// appsettings.json
"DataProtection": { "KeyPath": "/opt/sufficit-cloud-mobile/data/dataprotection" }
```

```csharp
// Program.cs
builder.Services.AddDataProtection()
    .SetApplicationName("Sufficit.Cloud.Mobile.Web")
    .PersistKeysToFileSystem(new DirectoryInfo(
        builder.Configuration["DataProtection:KeyPath"]
        ?? "/opt/sufficit-cloud-mobile/data/dataprotection"));
```

O unit tem `ReadWritePaths=…/data` (senão o `ProtectSystem=strict` bloqueia escrita). Keys em `/opt/sufficit-cloud-mobile/data/dataprotection/` (owner `cloudmobile`, mode 600).

## Health checks

```bash
systemctl is-active sufficit-cloud-mobile-web.service                                           # active
curl -s -o /dev/null -w "%{http_code}\n" --max-time 8 http://127.0.0.1:26515/                    # 302 -> Identity
curl -s --max-time 8 http://127.0.0.1:26515/cloud-mobile.css | grep -c "min-width: 44px"         # >0 (CSS novo)
nginx -t                                                                                         # syntax ok
curl -s -o /dev/null -w "%{http_code}\n" --max-time 8 http://127.0.0.1:26516/health/ready        # 200 (API intacta)
journalctl -u sufficit-cloud-mobile-web --since "15 min ago" | grep -iE "error|exception|crit"   # vazio
```

## Rollback

```bash
ssh -p 26492 root@eveo-apps.sufficit.com.br \
  'ln -sfn /opt/sufficit-cloud-mobile/releases/<SLUG-ANTERIOR>/web /opt/sufficit-cloud-mobile/web && systemctl restart sufficit-cloud-mobile-web.service'
```

Releases antigos permanecem em `releases/` (retenção do time) — rollback é só reposicionar o symlink.

## Atenções por projeto

- **Componente alterado dita o escopo:** só `Web` se a change for BFF/Razor/CSS; `Api`/`Worker` só se mexeram.
- **Não há checkout git no servidor** — sempre publish fora + rsync.
- **Retrocompatibilidade de circuito:** restart do Blazor Server derruba circuitos SignalR ativos (reconnect/restauração de sessão via ticket server-side). Aceitável; janela de segundos.
