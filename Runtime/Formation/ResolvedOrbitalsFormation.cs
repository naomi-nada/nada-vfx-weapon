using System.Collections.Generic;

namespace NADA.VFX.Weapon.Runtime.Formation
{
    /// <summary>
    /// Effective formation data for one orbital block.
    ///
    /// Count always belongs to this block.
    /// The remaining trajectory values may come from a Glue leader.
    /// </summary>
    internal sealed class ResolvedOrbitalsFormation
    {
        internal uint InstanceId { get; }
        internal string TypeId { get; }

        internal bool Enabled { get; }

        internal bool IsLeader { get; }
        internal bool IsFollower { get; }

        internal uint? LeaderInstanceId { get; }

        internal uint TrajectorySourceInstanceId { get; }

        internal float OwnCount { get; }

        internal bool EffectiveSnakeEnabled { get; }
        internal float EffectiveSpeed { get; }
        internal float EffectiveSpacing { get; }
        internal float EffectiveLength { get; }
        internal float EffectiveRadius { get; }
        internal float EffectiveCycles { get; }
        internal float EffectiveDrift { get; }

        internal float EffectiveXOffset { get; }
        internal float EffectiveYOffset { get; }
        internal float EffectiveZOffset { get; }

        internal float EffectiveXRotation { get; }
        internal float EffectiveYRotation { get; }
        internal float EffectiveZRotation { get; }

        internal ResolvedOrbitalsFormation(
            uint instanceId,
            string typeId,
            bool enabled,
            bool isLeader,
            bool isFollower,
            uint? leaderInstanceId,
            uint trajectorySourceInstanceId,
            float ownCount,
            bool effectiveSnakeEnabled,
            float effectiveSpeed,
            float effectiveSpacing,
            float effectiveLength,
            float effectiveRadius,
            float effectiveCycles,
            float effectiveDrift,
            float effectiveXOffset,
            float effectiveYOffset,
            float effectiveZOffset,
            float effectiveXRotation,
            float effectiveYRotation,
            float effectiveZRotation)
        {
            InstanceId = instanceId;
            TypeId = typeId;
            Enabled = enabled;

            IsLeader = isLeader;
            IsFollower = isFollower;

            LeaderInstanceId = leaderInstanceId;
            TrajectorySourceInstanceId = trajectorySourceInstanceId;

            OwnCount = ownCount;

            EffectiveSnakeEnabled = effectiveSnakeEnabled;
            EffectiveSpeed = effectiveSpeed;
            EffectiveSpacing = effectiveSpacing;
            EffectiveLength = effectiveLength;
            EffectiveRadius = effectiveRadius;
            EffectiveCycles = effectiveCycles;
            EffectiveDrift = effectiveDrift;

            EffectiveXOffset = effectiveXOffset;
            EffectiveYOffset = effectiveYOffset;
            EffectiveZOffset = effectiveZOffset;

            EffectiveXRotation = effectiveXRotation;
            EffectiveYRotation = effectiveYRotation;
            EffectiveZRotation = effectiveZRotation;
        }
    }

    /// <summary>
    /// One full-state orbital formation resolution.
    ///
    /// Invalid relationship graphs produce no partial Glue solution.
    /// Runtime callers can therefore fail closed instead of choosing
    /// arbitrary winners.
    /// </summary>
    internal sealed class OrbitalsFormationResolution
    {
        private readonly List<ResolvedOrbitalsFormation> _entries;

        private readonly Dictionary<uint, ResolvedOrbitalsFormation>
            _byInstanceId;

        internal bool IsValid { get; }

        internal string FailureReason { get; }

        internal int LeaderCount { get; }

        internal int FollowerCount { get; }

        internal int OrbitalsCount =>
            _entries.Count;

        internal IEnumerable<ResolvedOrbitalsFormation> Entries =>
            _entries;

        private OrbitalsFormationResolution(
            bool isValid,
            string failureReason,
            List<ResolvedOrbitalsFormation> entries,
            int leaderCount,
            int followerCount)
        {
            IsValid = isValid;
            FailureReason = failureReason;

            LeaderCount = leaderCount;
            FollowerCount = followerCount;

            _entries =
                entries ??
                new List<ResolvedOrbitalsFormation>();

            _byInstanceId =
                new Dictionary<uint, ResolvedOrbitalsFormation>();

            foreach (ResolvedOrbitalsFormation entry in _entries)
            {
                if (entry == null)
                    continue;

                _byInstanceId[entry.InstanceId] =
                    entry;
            }
        }

        internal bool TryGet(
            uint instanceId,
            out ResolvedOrbitalsFormation formation)
        {
            return _byInstanceId.TryGetValue(
                instanceId,
                out formation);
        }

        internal static OrbitalsFormationResolution Valid(
            List<ResolvedOrbitalsFormation> entries,
            int leaderCount,
            int followerCount)
        {
            return new OrbitalsFormationResolution(
                true,
                null,
                entries,
                leaderCount,
                followerCount);
        }

        internal static OrbitalsFormationResolution Invalid(
            string failureReason)
        {
            return new OrbitalsFormationResolution(
                false,
                failureReason,
                new List<ResolvedOrbitalsFormation>(),
                0,
                0);
        }
    }
}