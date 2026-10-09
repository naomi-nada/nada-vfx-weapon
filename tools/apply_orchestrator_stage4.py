#!/usr/bin/env python3
"""Surgically apply Stage 4 to the user's current orchestrator.

Run from anywhere inside the NADA.VFX.Weapon repository:
    python3 tools/apply_orchestrator_stage4.py

All anchors are verified before writing, so a changed upstream file is not
silently rewritten. No runtime source is bundled because the orchestrator was
pasted in chat rather than uploaded as an actual .cs attachment.
"""
from pathlib import Path
import os
import sys


def main():
    repo = Path.cwd()
    while not (repo / 'NADA.VFX.Weapon.csproj').exists():
        if repo.parent == repo:
            raise SystemExit('Could not locate the NADA.VFX.Weapon.csproj root. Run in the project repository.')
        repo = repo.parent

    candidates = list(repo.rglob('NadaWeaponRigOrchestrator.cs'))
    if len(candidates) != 1:
        raise SystemExit(f'Expected exactly one orchestrator .cs in repo, found {len(candidates)}.')
    path = candidates[0]
    original = path.read_text(encoding='utf-8-sig')
    newline = '\r\n' if '\r\n' in original else '\n'
    normalized = original.replace('\r\n', '\n')
    marker = '        internal static bool RunRemote('
    if normalized.count(marker) != 1:
        raise SystemExit('Could not uniquely identify RunRemote; no changes made.')
    work, remote = normalized.split(marker, 1)

    if 'NadaWeaponLocalSourceSelection localSource =' in work:
        raise SystemExit('Stage 4 source selection already appears in this file. No changes made.')

    replacements = [
        (
            'Source selection and fail-closed native guard',
            '''            Transform weaponVisualRootTransform =
                context.WeaponVisualRoot;

            if (!NadaRigCache.CacheReady)''',
            '''            Transform weaponVisualRootTransform =
                context.WeaponVisualRoot;

            NadaWeaponLocalSourceSelection localSource =
                NadaWeaponLocalSourceResolver.Resolve(itemData);

            bool hasPersistentBinding =
                localSource.Kind == NadaWeaponLocalSourceKind.NativeBound ||
                localSource.Kind == NadaWeaponLocalSourceKind.LegacyBound;

            bool invalidNative =
                localSource.Kind == NadaWeaponLocalSourceKind.InvalidNative;

            string nativeFailureReason = localSource.FailureReason;

            if (localSource.Kind == NadaWeaponLocalSourceKind.NativeBound)
            {
                // The binary codec validates structure; the existing formation
                // resolver validates cross-block Glue relationships.
                if (localSource.State == null)
                {
                    invalidNative = true;
                    nativeFailureReason = "Native state is missing.";
                }
                else
                {
                    bool formationAccepted =
                        OrbitalsFormationResolver.TryResolve(
                            localSource.State,
                            out OrbitalsFormationResolution formation);

                    if (!formationAccepted ||
                        formation == null ||
                        !formation.IsValid)
                    {
                        invalidNative = true;
                        nativeFailureReason =
                            formation?.FailureReason ??
                            "Native orbital relationships are invalid.";
                    }
                }
            }

            if (invalidNative)
            {
                // Fail closed before any structure assembly. Destroy is
                // deferred: deactivate first so stale effects cannot render.
                Transform staleRig = weaponVisualRootTransform != null
                    ? NadaRigPaths.FindDirectChild(
                        weaponVisualRootTransform,
                        Plugin.LocalWeaponRootName)
                    : null;

                if (staleRig != null)
                {
                    staleRig.gameObject.SetActive(false);
                    NadaWeaponRigRemoval.RemoveTrackedRig(staleRig);
                }

                NadaLogControl.Info(
                    $"native-local-invalid:{rootObject.GetInstanceID()}:{nativeFailureReason}",
                    $"{Plugin.ModName}: [NativeLocalStateRejected] " +
                    $"root='{rootObject.name}' " +
                    $"reason='{nativeFailureReason ?? "invalid-native-payload"}'");

                return;
            }

            if (!NadaRigCache.CacheReady)'''
        ),
        (
            'Native binding eligibility for equipped/dropped visuals',
            '''            bool isBoundDroppedItem =
                itemData != null &&
                VfxStateIO.IsBound(itemData) &&
                rootObject.GetComponent<global::ItemDrop>() != null;

            bool isBoundPreviewOrEquippedVisual =
                itemData != null &&
                VfxStateIO.IsBound(itemData) &&
                weaponVisualRootTransform != null;''',
            '''            bool isBoundDroppedItem =
                itemData != null &&
                hasPersistentBinding &&
                rootObject.GetComponent<global::ItemDrop>() != null;

            bool isBoundPreviewOrEquippedVisual =
                itemData != null &&
                hasPersistentBinding &&
                weaponVisualRootTransform != null;'''
        ),
        (
            'Remove redundant preview/legacy ownership checks',
            '''            bool hasBoundBlockState =
                itemData != null &&
                VfxStateIO.IsBound(itemData);

            bool hasEditorPreview =
                NadaWeaponEditorPreviewState.TryGet(
                    itemData,
                    out WeaponVfxState editorPreviewState);

            bool useBlockRuntime =
                hasEditorPreview ||
                hasBoundBlockState;''',
            '''            bool hasEditorPreview =
                localSource.Kind == NadaWeaponLocalSourceKind.EditorPreview;

            bool useBlockRuntime =
                localSource.Kind == NadaWeaponLocalSourceKind.EditorPreview ||
                hasPersistentBinding;'''
        ),
        (
            'Use native state directly instead of migrating a legacy placeholder',
            '''                blockState =
                    hasEditorPreview
                        ? editorPreviewState
                        : CreateMigratedPrototypeState(
                            context,
                            "local");''',
            '''                blockState =
                    localSource.Kind == NadaWeaponLocalSourceKind.NativeBound ||
                    localSource.Kind == NadaWeaponLocalSourceKind.EditorPreview
                        ? localSource.State
                        : CreateMigratedPrototypeState(
                            context,
                            "local");

                if (localSource.Kind == NadaWeaponLocalSourceKind.NativeBound)
                {
                    NadaLogControl.Info(
                        $"native-local-runtime:{rootObject.GetInstanceID()}",
                        $"{Plugin.ModName}: [NativeLocalRuntime] " +
                        $"root='{rootObject.name}' " +
                        $"effects={blockState.Effects.Count} " +
                        $"source='item-data'");
                }'''
        ),
        (
            'Apply rig transform from the winning pure block state',
            '''            NadaRigTransformApplier.Apply(
                localWeaponRootTransform,
                context.State);

            NadaMotionBinder.BindOrbitalsRigFollow(''',
            '''            if (localSource.Kind == NadaWeaponLocalSourceKind.NativeBound ||
                localSource.Kind == NadaWeaponLocalSourceKind.EditorPreview)
            {
                NadaRigTransformApplier.Apply(
                    localWeaponRootTransform,
                    blockState.RigTransform);
            }
            else
            {
                NadaRigTransformApplier.Apply(
                    localWeaponRootTransform,
                    context.State);
            }

            NadaMotionBinder.BindOrbitalsRigFollow('''
        ),
    ]

    for label, old, new in replacements:
        n = work.count(old)
        if n != 1:
            raise SystemExit(f'{label}: expected 1 matching anchor, found {n}. No changes made. Please send the actual orchestrator file.')
        work = work.replace(old, new, 1)

    # Native-only bindings must not log `bound=False` just because the old
    # legacy marker is absent. Both local skip diagnostics use this value.
    old_diagnostic = 'bound={VfxStateIO.IsBound(itemData)}'
    if work.count(old_diagnostic) != 2:
        raise SystemExit('Local bound log anchors changed; no changes made.')
    work = work.replace(old_diagnostic, 'bound={hasPersistentBinding}')

    # Rejoin the untouched remote method and all downstream helpers.
    data = (work + marker + remote).replace('\n', newline)
    temp = path.with_name(path.name + '.nada-stage4.tmp')
    with temp.open('w', encoding='utf-8', newline='') as stream:
        stream.write(data)
    os.replace(temp, path)
    print(f'Patched: {path.relative_to(repo)}')
    for label, _, _ in replacements:
        print(f'  OK: {label}')
    print('Verified: RunRemote() and all subsequent code unchanged.')
    print('Review the diff: git diff -- <path-to-NadaWeaponRigOrchestrator.cs>')


if __name__ == '__main__':
    main()
