#!/usr/bin/env python3
"""Prepare an already-published standalone WASM directory for a static host."""
import argparse, re, json
from pathlib import Path
parser = argparse.ArgumentParser()
parser.add_argument('directory', type=Path)
parser.add_argument('--base-path', default='/sufficit-blazor-ui/')
args = parser.parse_args()
base = '/' + args.base_path.strip('/') + '/' if args.base_path.strip('/') else '/'
if not re.fullmatch(r'/[A-Za-z0-9_./-]*', base) or '..' in base: parser.error('Invalid base path')
index = args.directory/'index.html'
text = index.read_text()
text, count = re.subn(r'<base href="[^"]*"\s*/?>', f'<base href="{base}" />', text)
if count != 1: raise SystemExit('Expected one base element')
index.write_text(text)
# These precompressed copies were produced before the base path replacement.
for suffix in ['.br','.gz']:
    index.with_name(index.name+suffix).unlink(missing_ok=True)
(args.directory/'.nojekyll').touch()
(args.directory/'404.html').write_text(text)
# Query-string routes work without rewrites on GitHub Pages, including reload.
for asset in ['_framework/blazor.webassembly.js','_content/Sufficit.Blazor.UI/sufficit-ui.css','Sufficit.Blazor.UI.Showcase.styles.css','showcase.css','theme.js']:
    if not (args.directory/asset).is_file(): raise SystemExit(f'Missing static asset: {asset}')
print(f'Pages artifact ready at {args.directory}; base={base}')
