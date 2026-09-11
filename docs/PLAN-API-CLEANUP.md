# Plano — limpeza da API pública (antigo "v2")

> Este plano se chamava `PLAN-SUI-V2.md`. O nome mudou em 2026-09-11 porque a
> série SemVer `2.0.0`–`2.2.1` foi publicada por engano em agosto com as pontes
> ainda presentes, e a linha corporativa agora é `2.yy.MMdd.HHmm`. Só itens
> pendentes ficam aqui; o histórico está em `docs/activities/` e no CHANGELOG.

**Feito em 2026-09-11:** as 26 pontes `object`/`string` foram removidas
(ver [api-migration.md](components/api-migration.md)); `NavAccordionScope`
virou `SUINavAccordionScope` (nenhum consumidor a referenciava);
`SUIAlert.OnClose` existe e `CloseIconClicked` ficou como encaminhador
obsoleto. Consumidores Background e Network Control migrados no mesmo dia;
os demais já não usavam pontes.

## Pendente

| Item | Bloqueio | Como fechar |
| --- | --- | --- |
| `SUIItem.xs/sm/md/lg/xl` → `Xs/Sm/Md/Lg/Xl` | Razor casa atributos de componente sem distinguir maiúsculas, então não dá para ter `xs` obsoleto ao lado de `Xs`: é rename duro. 78 call sites: sufficit-blazor (29), sufficit-cloud-mobile (33), sufficit-ai (8), samples (8) | um commit por consumidor no mesmo dia da release, com `rg -n '<SUIItem\b[^>]*\s(xs|sm|md|lg|xl)='`; remover as cinco entradas de `NamingConventionTests.LegacyParameterNames` no commit da SUI |
| remover `SUIAlert.CloseIconClicked` | 11 call sites: sufficit-blazor (5), sufficit-ai (3), services-run (1), fleet (1), cloud-mobile (1) | trocar por `OnClose` nos consumidores (não quebra, só tira o aviso); depois remover o encaminhador e a entrada da lista de débito |

## Gates de qualquer remoção

- [ ] `eng/PublicApiBaseline.txt` atualizado com review das diferenças;
- [ ] Chromium, Firefox e WebKit verdes;
- [ ] pacote instalado e executado em raiz e `PathBase`;
- [ ] todos os consumers conhecidos compilam sem `CS0618` e sem erro;
- [ ] changelog contém a lista de quebras e a última versão anterior para rollback.

## Fora de escopo

- renomear o prefixo `SUI`;
- transformar a biblioteca em Material Design;
- incorporar identidade visual de um consumer no tema default;
- remover `sufficit-ui.css` ou exigir scripts globais;
- alterar fields controlados para herdar `InputBase<T>`.
