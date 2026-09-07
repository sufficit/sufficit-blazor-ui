#!/usr/bin/env python3
"""Compile the exact Razor text offered by the catalog's Copy button."""
from pathlib import Path
from tempfile import TemporaryDirectory
import json, subprocess
root = Path(__file__).resolve().parent.parent
entries = json.loads((root/'samples/Sufficit.Blazor.UI.Demos/catalog.json').read_text())
with TemporaryDirectory(prefix='sui-examples-') as directory:
    target = Path(directory)
    (target/'Examples.csproj').write_text(f'''<Project Sdk="Microsoft.NET.Sdk.Razor">
<PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings></PropertyGroup>
<ItemGroup><ProjectReference Include="{root}/src/Sufficit.Blazor.UI.csproj" /></ItemGroup>
</Project>''')
    (target/'_Imports.razor').write_text('@using Microsoft.AspNetCore.Components\n@using Microsoft.AspNetCore.Components.Forms\n@using Microsoft.AspNetCore.Components.Web\n')
    for entry in entries: (target/(entry['name']+'Example.razor')).write_text(entry['source'])
    subprocess.run(['dotnet','build',str(target/'Examples.csproj'),'-c','Release','-warnaserror','--verbosity','quiet'],check=True)
print(f'Compiled {len(entries)} standalone copied examples.')
