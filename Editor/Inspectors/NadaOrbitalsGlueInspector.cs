using System;
using System.Collections.Generic;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;
using NADA.VFX.Weapon.Runtime.Formation;

namespace NADA.VFX.Weapon.Editor.Inspectors
{
    // Glue relationships are authored here; the pure formation resolver
    // decides whether a proposed relationship graph is valid.
    internal static class NadaOrbitalsGlueInspector
    {
        private static readonly Dictionary<uint, string> RejectionByInstanceId =
            new();

        internal static bool IsActiveFollower(VfxEffectBlock block)
        {
            WeaponVfxState state = NadaVfxEditorWorkingState.State;

            return block?.Enabled == true &&
                   state?.Effects != null &&
                   FindActiveClaimingLeader(state, block.InstanceId) != null;
        }

        internal static bool Draw(
            VfxEffectBlock block,
            OrbitalsFormationVfxSettings formation)
        {
            WeaponVfxState state = NadaVfxEditorWorkingState.State;

            if (block == null ||
                formation?.GlueTargetInstanceIds == null ||
                state?.Effects == null)
            {
                NadaVfxEditorControls.Placeholder(
                    "Glue settings or editor state are missing.");
                return false;
            }

            bool changed = false;
            string prefix = $"glue:{block.InstanceId}";

            VfxEffectBlock claimingLeader = FindActiveClaimingLeader(
                state, block.InstanceId);

            if (claimingLeader != null)
            {
                string leaderName = NadaVfxEditorWorkingState.GetDisplayName(
                    claimingLeader);

                NadaVfxEditorControls.Placeholder(
                    block.Enabled
                        ? $"Following {leaderName} (#{claimingLeader.InstanceId}). Change this on the leader."
                        : $"Saved as a target of {leaderName} (#{claimingLeader.InstanceId}); inactive while this block is off.");
            }

            bool mayChangeLeader =
                claimingLeader == null || formation.GlueLeaderEnabled;

            if (NadaVfxEditorControls.Toggle(
                    $"{prefix}:leader",
                    "Glue",
                    formation.GlueLeaderEnabled,
                    out bool nextLeaderEnabled,
                    enabled: mayChangeLeader))
            {
                if (!nextLeaderEnabled)
                {
                    formation.GlueLeaderEnabled = false;
                    ClearRejection(block.InstanceId);
                    changed = true;
                }
                else
                {
                    formation.GlueLeaderEnabled = true;

                    if (ValidateWhenActive(state, block, out string reason))
                    {
                        ClearRejection(block.InstanceId);
                        changed = true;
                    }
                    else
                    {
                        formation.GlueLeaderEnabled = false;
                        Reject(block.InstanceId, reason);
                    }
                }
            }

            if (!mayChangeLeader)
            {
                NadaVfxEditorControls.Placeholder(
                    "Remove this block from its current leader before making it a leader.");
            }

            if (formation.GlueLeaderEnabled)
            {
                NadaVfxEditorControls.Placeholder(
                    "Targets you select follow this orbital's trajectory and phase. Each target keeps its own Count and Visuals.");
            }
            else if (formation.GlueTargetInstanceIds.Count > 0)
            {
                NadaVfxEditorControls.Placeholder(
                    "Glue is off. Saved targets are dormant; you can remove them below.");
            }

            if (formation.GlueLeaderEnabled ||
                formation.GlueTargetInstanceIds.Count > 0)
            {
                var displayedIds = new HashSet<uint>();

                foreach (VfxEffectBlock candidate in state.Effects)
                {
                    if (candidate == null ||
                        candidate.InstanceId == block.InstanceId ||
                        !IsOrbitalType(candidate.TypeId))
                    {
                        continue;
                    }

                    displayedIds.Add(candidate.InstanceId);

                    bool selected = formation.GlueTargetInstanceIds.Contains(
                        candidate.InstanceId);

                    OrbitalsFormationVfxSettings candidateFormation =
                        GetFormation(candidate);

                    VfxEffectBlock otherLeader = FindActiveClaimingLeader(
                        state, candidate.InstanceId, block.InstanceId);

                    bool canAdd =
                        formation.GlueLeaderEnabled &&
                        candidate.Transform != null &&
                        candidateFormation?.Path != null &&
                        candidateFormation.GlueTargetInstanceIds != null &&
                        !candidateFormation.GlueLeaderEnabled &&
                        otherLeader == null;

                    string label =
                        $"{NadaVfxEditorWorkingState.GetDisplayName(candidate)} (#{candidate.InstanceId})";

                    if (!selected && candidateFormation?.GlueLeaderEnabled == true)
                        label += " - leader";
                    else if (!selected && otherLeader != null)
                        label += $" - follows #{otherLeader.InstanceId}";
                    else if (!selected && (candidate.Transform == null ||
                                           candidateFormation?.Path == null ||
                                           candidateFormation.GlueTargetInstanceIds == null))
                        label += " - invalid block";
                    else if (!candidate.Enabled)
                        label += " - off";

                    if (!NadaVfxEditorControls.Toggle(
                            $"{prefix}:target:{candidate.InstanceId}",
                            label,
                            selected,
                            out bool nextSelected,
                            enabled: selected || canAdd))
                    {
                        continue;
                    }

                    if (!nextSelected)
                    {
                        // Allow removing a claim even while repairing
                        // an otherwise-invalid persisted relationship.
                        formation.GlueTargetInstanceIds.RemoveAll(
                            id => id == candidate.InstanceId);
                        ClearRejection(block.InstanceId);
                        changed = true;
                        continue;
                    }

                    formation.GlueTargetInstanceIds.Add(candidate.InstanceId);

                    if (ValidateWhenActive(state, block, out string targetReason))
                    {
                        ClearRejection(block.InstanceId);
                        changed = true;
                    }
                    else
                    {
                        formation.GlueTargetInstanceIds.RemoveAt(
                            formation.GlueTargetInstanceIds.Count - 1);
                        Reject(block.InstanceId, targetReason);
                    }
                }

                // Imported dangling IDs must remain removable in the editor.
                var leftoverIds = new HashSet<uint>();
                foreach (uint storedId in formation.GlueTargetInstanceIds.ToArray())
                {
                    if (displayedIds.Contains(storedId) ||
                        !leftoverIds.Add(storedId))
                        continue;

                    if (NadaVfxEditorControls.Toggle(
                            $"{prefix}:stale:{storedId}",
                            $"Invalid target #{storedId} (remove)",
                            true,
                            out bool keepTarget) &&
                        !keepTarget)
                    {
                        formation.GlueTargetInstanceIds.RemoveAll(
                            id => id == storedId);
                        ClearRejection(block.InstanceId);
                        changed = true;
                    }
                }
            }

            if (RejectionByInstanceId.TryGetValue(
                    block.InstanceId, out string issue))
            {
                NadaVfxEditorControls.Placeholder(
                    $"Glue change rejected: {issue}");
            }

            return changed;
        }

        // Check dormant leader relationships as though the leader were on.
        // Never modify the editor's real block Enabled flag to validate.
        private static bool ValidateWhenActive(
            WeaponVfxState state,
            VfxEffectBlock block,
            out string reason)
        {
            WeaponVfxState candidateState = state;

            if (!block.Enabled)
            {
                candidateState = new WeaponVfxState
                {
                    SchemaVersion = state.SchemaVersion,
                    RigTransform = state.RigTransform,
                    Effects = new List<VfxEffectBlock>(state.Effects)
                };

                for (int i = 0; i < candidateState.Effects.Count; i++)
                {
                    if (!ReferenceEquals(candidateState.Effects[i], block))
                        continue;

                    candidateState.Effects[i] = new VfxEffectBlock
                    {
                        InstanceId = block.InstanceId,
                        TypeId = block.TypeId,
                        DisplayName = block.DisplayName,
                        Enabled = true,
                        Transform = block.Transform,
                        Settings = block.Settings
                    };
                    break;
                }
            }

            bool accepted = OrbitalsFormationResolver.TryResolve(
                candidateState, out OrbitalsFormationResolution resolution);

            reason = accepted && resolution?.IsValid == true
                ? null
                : resolution?.FailureReason ?? "invalid-formation";

            return reason == null;
        }

        private static VfxEffectBlock FindActiveClaimingLeader(
            WeaponVfxState state,
            uint targetId,
            uint excludeLeaderId = 0)
        {
            foreach (VfxEffectBlock candidate in state.Effects)
            {
                if (candidate == null ||
                    !candidate.Enabled ||
                    candidate.InstanceId == excludeLeaderId)
                {
                    continue;
                }

                OrbitalsFormationVfxSettings formation = GetFormation(candidate);
                if (formation?.GlueLeaderEnabled == true &&
                    formation.GlueTargetInstanceIds != null &&
                    formation.GlueTargetInstanceIds.Contains(targetId))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static OrbitalsFormationVfxSettings GetFormation(
            VfxEffectBlock block)
        {
            if (block == null)
                return null;

            return block.Settings switch
            {
                OrbitalsOrbsVfxSettings orbs when block.TypeId == VfxEffectTypeIds.OrbitalsOrbs => orbs.Formation,
                OrbitalsCoresVfxSettings cores when block.TypeId == VfxEffectTypeIds.OrbitalsCores => cores.Formation,
                OrbitalsFlamesVfxSettings flames when block.TypeId == VfxEffectTypeIds.OrbitalsFlames => flames.Formation,
                OrbitalsEmbersVfxSettings embers when block.TypeId == VfxEffectTypeIds.OrbitalsEmbers => embers.Formation,
                _ => null
            };
        }

        private static bool IsOrbitalType(string typeId)
        {
            return typeId == VfxEffectTypeIds.OrbitalsOrbs ||
                   typeId == VfxEffectTypeIds.OrbitalsCores ||
                   typeId == VfxEffectTypeIds.OrbitalsFlames ||
                   typeId == VfxEffectTypeIds.OrbitalsEmbers;
        }

        private static void Reject(uint instanceId, string reason)
        {
            RejectionByInstanceId[instanceId] = reason;
            Plugin.Log?.LogWarning(
                $"{Plugin.ModName}: [EditorGlueRejected] id={instanceId} reason='{reason}'.");
        }

        private static void ClearRejection(uint instanceId)
        {
            RejectionByInstanceId.Remove(instanceId);
        }
    }
}
