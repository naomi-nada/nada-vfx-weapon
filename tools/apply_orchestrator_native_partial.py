#!/usr/bin/env python3
"""Turn the existing full orchestrator into a partial class without altering behavior.

Run from repository root. Bails out if the expected class declaration has
changed or if another declaration would make the replacement ambiguous.
"""
from pathlib import Path
path = Path('Weapons/Runtime/NadaWeaponRigOrchestrator.cs')
if not path.is_file():
    raise SystemExit(f'Missing source: {path}')
text = path.read_text(encoding='utf-8')
old = 'internal static class NadaWeaponRigOrchestrator'
new = 'internal static partial class NadaWeaponRigOrchestrator'
if new in text:
    print('Orchestrator is already partial; no change required.')
elif text.count(old) != 1:
    raise SystemExit(f'Expected exactly one orchestrator declaration; found {text.count(old)}. Not changed.')
else:
    path.write_text(text.replace(old, new, 1), encoding='utf-8')
    print(f'Updated {path} (one class declaration; all existing methods preserved).')

# This project uses explicit <Compile Include> entries instead of wildcard sources.
project = Path('NADA.VFX.Weapon.csproj')
if not project.is_file():
    raise SystemExit(f'Missing project: {project}; add the new .cs file manually.')
xml = project.read_text(encoding='utf-8')
entry = '<Compile Include="Weapons\\Runtime\\NadaWeaponRigOrchestrator.NativeRemote.cs" />'
# XML on disk contains single path separators; the Python literal has one too.
if entry in xml:
    print('Native remote partial is already registered in project.')
else:
    old_entry = '<Compile Include="Weapons\\Runtime\\NadaWeaponRigOrchestrator.cs" />'
    if xml.count(old_entry) != 1:
        raise SystemExit('Orchestrator compile entry not found exactly once. Add native partial in Rider manually.')
    xml = xml.replace(old_entry, old_entry + '\n        ' + entry, 1)
    project.write_text(xml, encoding='utf-8')
    print('Registered new native remote partial in project.')
