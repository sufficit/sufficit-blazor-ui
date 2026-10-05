# scripts/

Dois runtimes, papéis distintos. Nada aqui roda em produção — tudo é build/CI.

- **Node (`*.mjs`)** — pipeline de CSS e verificação de qualidade da página
  renderizada. Exigem `lightningcss`/`playwright` (via `npm install` na raiz).
  - `build-css.mjs` — monta e minifica `src/styles/sui-entry.css` em
    `src/wwwroot/sufficit-ui.css`, com orçamentos de bytes (raw/gzip/brotli)
    que precisam ser reajustados junto com `AssetBudgetTests`.
  - `check-lighthouse.mjs` — roda Lighthouse contra a vitrine publicada.

- **Python (`*.py`)** — geração de código/versão e tarefas de release que
  já dependiam de Python na esteira. Interpretador 3.x padrão, sem venv.
  - `generate-catalog.py` — regenera o catálogo de demos a partir dos fontes.
  - `release_version.py` / `prepare-pages.py` / `check-catalog-examples.py` /
    `unlist_legacy_packages.py` + `test_*.py` — versionamento, publicação e
    seus testes.

- **`validate-package.sh`** — validação bash do pacote NuGet empacotado.

Regra de bolso: CSS/página → Node; catálogo/release → Python.
