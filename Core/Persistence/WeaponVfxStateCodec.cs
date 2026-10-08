using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NADA.VFX.Weapon.Core.State.Blocks;
using NADA.VFX.Weapon.Core.State.Blocks.Effects;

namespace NADA.VFX.Weapon.Core.Persistence
{
    /// <summary>
    /// Versioned, deterministic, block-native persistence envelope.
    /// No ItemData, legacy state, config, Unity objects, or runtime behavior.
    /// NextInstanceId belongs to the persistence envelope, not effect state.
    /// This is NOT the ZDO network protocol.
    /// </summary>
    internal static class WeaponVfxStateCodec
    {
        private const uint Magic = 0x3244564E; // "NVD2" little-endian
        private const ushort FormatVersion = 1;
        private const int MaxPayloadBytes = 65536;
        private const int MaxEffects = 128;
        private const int MaxNameCharacters = 64;
        private const int MaxNameBytes = 256;
        private static readonly UTF8Encoding StrictUtf8 =
            new UTF8Encoding(false, true);

        internal static bool TryEncode(
            WeaponVfxState state,
            uint nextInstanceId,
            out byte[] bytes,
            out string reason)
        {
            bytes = null;
            reason = null;
            try
            {
                ValidateState(state, nextInstanceId);
                using (var stream = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(
                               stream, Encoding.UTF8, leaveOpen: true))
                    {
                        writer.Write(Magic);
                        writer.Write(FormatVersion);
                        writer.Write(state.SchemaVersion);
                        writer.Write(nextInstanceId);
                        WriteTransform(writer, state.RigTransform);
                        writer.Write((ushort)state.Effects.Count);
                        foreach (VfxEffectBlock block in state.Effects)
                        {
                            writer.Write(block.InstanceId);
                            writer.Write(GetTypeCode(block.TypeId));
                            WriteBool(writer, block.Enabled);
                            WriteBool(writer, block.DisplayName != null);
                            if (block.DisplayName != null)
                                WriteName(writer, block.DisplayName);
                            WriteTransform(writer, block.Transform);
                            WriteSettings(writer, block);
                        }
                    }
                    if (stream.Length > MaxPayloadBytes)
                        throw new InvalidDataException("Payload exceeds size limit.");
                    bytes = stream.ToArray();
                }
                return true;
            }
            catch (Exception ex) when (IsCodecFailure(ex))
            {
                reason = ex.Message;
                return false;
            }
        }

        internal static bool TryDecode(
            byte[] bytes,
            out WeaponVfxState state,
            out uint nextInstanceId,
            out string reason)
        {
            state = null;
            nextInstanceId = 0;
            reason = null;
            try
            {
                if (bytes == null || bytes.Length == 0 ||
                    bytes.Length > MaxPayloadBytes)
                    throw new InvalidDataException("Empty or oversized payload.");

                WeaponVfxState decoded;
                uint decodedCursor;
                using (var stream = new MemoryStream(bytes, writable: false))
                using (var reader = new BinaryReader(
                           stream, Encoding.UTF8, leaveOpen: true))
                {
                    if (reader.ReadUInt32() != Magic)
                        throw new InvalidDataException("Invalid native-state magic.");
                    if (reader.ReadUInt16() != FormatVersion)
                        throw new InvalidDataException("Unsupported native-state format.");
                    int schemaVersion = reader.ReadInt32();
                    if (schemaVersion != WeaponVfxState.CurrentSchemaVersion)
                        throw new InvalidDataException("Unsupported block schema version.");

                    decodedCursor = reader.ReadUInt32();
                    decoded = new WeaponVfxState
                    {
                        SchemaVersion = schemaVersion,
                        RigTransform = ReadTransform(reader),
                        Effects = new List<VfxEffectBlock>()
                    };

                    int count = reader.ReadUInt16();
                    if (count > MaxEffects)
                        throw new InvalidDataException("Too many effect blocks.");
                    for (int index = 0; index < count; index++)
                    {
                        uint instanceId = reader.ReadUInt32();
                        string typeId = GetTypeId(reader.ReadByte());
                        bool enabled = ReadBool(reader);
                        string displayName = ReadBool(reader)
                            ? ReadName(reader)
                            : null;
                        VfxTransformState transform = ReadTransform(reader);
                        VfxEffectSettings settings = ReadSettings(reader, typeId);
                        decoded.Effects.Add(new VfxEffectBlock
                        {
                            InstanceId = instanceId,
                            TypeId = typeId,
                            Enabled = enabled,
                            DisplayName = displayName,
                            Transform = transform,
                            Settings = settings
                        });
                    }
                    if (stream.Position != stream.Length)
                        throw new InvalidDataException("Trailing payload data.");
                }
                ValidateState(decoded, decodedCursor);
                state = decoded;
                nextInstanceId = decodedCursor;
                return true;
            }
            catch (Exception ex) when (IsCodecFailure(ex))
            {
                reason = ex.Message;
                return false;
            }
        }

        private static bool IsCodecFailure(Exception error)
        {
            return error is InvalidDataException ||
                   error is IOException ||
                   error is ArgumentException ||
                   error is OverflowException ||
                   error is DecoderFallbackException;
        }

        private static void ValidateState(WeaponVfxState state, uint nextInstanceId)
        {
            if (state == null || state.SchemaVersion != WeaponVfxState.CurrentSchemaVersion ||
                state.Effects == null || state.RigTransform == null)
                throw new InvalidDataException("Missing state or unsupported schema.");
            if (state.Effects.Count > MaxEffects)
                throw new InvalidDataException("Too many effect blocks.");
            ValidateTransform(state.RigTransform);
            var ids = new HashSet<uint>();
            uint maxId = 0;
            foreach (VfxEffectBlock block in state.Effects)
            {
                if (block == null || block.InstanceId == 0 ||
                    !ids.Add(block.InstanceId))
                    throw new InvalidDataException("Null block, zero ID, or duplicate instance ID.");
                GetTypeCode(block.TypeId);
                if (block.Transform == null || block.Settings == null)
                    throw new InvalidDataException("Missing block transform or settings.");
                if ((block.DisplayName ?? "").Length > MaxNameCharacters)
                    throw new InvalidDataException("Display name exceeds length limit.");
                ValidateTransform(block.Transform);
                ValidateSettings(block);
                if (block.InstanceId > maxId)
                    maxId = block.InstanceId;
            }
            if (maxId == uint.MaxValue)
            {
                if (nextInstanceId != 0)
                    throw new InvalidDataException("Exhausted ID cursor must be zero.");
            }
            else if (nextInstanceId == 0 || nextInstanceId <= maxId)
            {
                throw new InvalidDataException("NextInstanceId must exceed all allocated IDs.");
            }

            // These are referential checks, not an alternative orbital resolver.
            // The existing OrbitalsFormationResolver still owns formation semantics.
            foreach (VfxEffectBlock block in state.Effects)
            {
                OrbitalsFormationVfxSettings formation = GetFormation(block.Settings);
                if (formation == null)
                    continue;
                var localTargets = new HashSet<uint>();
                foreach (uint target in formation.GlueTargetInstanceIds)
                {
                    if (target == 0 || target == block.InstanceId ||
                        !ids.Contains(target) || !localTargets.Add(target))
                        throw new InvalidDataException("Invalid Glue target instance ID.");
                    VfxEffectBlock targetBlock = state.Effects.Find(b => b.InstanceId == target);
                    if (GetFormation(targetBlock.Settings) == null)
                        throw new InvalidDataException("Glue target is not orbital.");
                }
            }
        }

        private static void WriteBool(BinaryWriter writer, bool value) =>
            writer.Write((byte)(value ? 1 : 0));

        private static bool ReadBool(BinaryReader reader)
        {
            byte value = reader.ReadByte();
            if (value > 1)
                throw new InvalidDataException("Invalid Boolean value.");
            return value == 1;
        }

        private static void WriteFinite(BinaryWriter writer, float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new InvalidDataException("Non-finite setting.");
            writer.Write(value);
        }

        private static float ReadFinite(BinaryReader reader)
        {
            float value = reader.ReadSingle();
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new InvalidDataException("Non-finite setting.");
            return value;
        }

        private static void WriteTransform(BinaryWriter writer, VfxTransformState transform)
        {
            WriteFinite(writer, transform.XOffset);
            WriteFinite(writer, transform.YOffset);
            WriteFinite(writer, transform.ZOffset);
            WriteFinite(writer, transform.XRotation);
            WriteFinite(writer, transform.YRotation);
            WriteFinite(writer, transform.ZRotation);
        }

        private static VfxTransformState ReadTransform(BinaryReader reader) =>
            new VfxTransformState
            {
                XOffset = ReadFinite(reader),
                YOffset = ReadFinite(reader),
                ZOffset = ReadFinite(reader),
                XRotation = ReadFinite(reader),
                YRotation = ReadFinite(reader),
                ZRotation = ReadFinite(reader)
            };

        private static void ValidateTransform(VfxTransformState value)
        {
            RequireFinite(value.XOffset);
            RequireFinite(value.YOffset);
            RequireFinite(value.ZOffset);
            RequireFinite(value.XRotation);
            RequireFinite(value.YRotation);
            RequireFinite(value.ZRotation);
        }

        private static void RequireFinite(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new InvalidDataException("Non-finite setting.");
        }

        private static void WriteName(BinaryWriter writer, string name)
        {
            name = name ?? string.Empty;
            if (name.Length > MaxNameCharacters)
                throw new InvalidDataException("Display name exceeds length limit.");
            byte[] encoded = StrictUtf8.GetBytes(name);
            if (encoded.Length > MaxNameBytes)
                throw new InvalidDataException("Display name exceeds byte limit.");
            writer.Write((ushort)encoded.Length);
            writer.Write(encoded);
        }

        private static string ReadName(BinaryReader reader)
        {
            int byteCount = reader.ReadUInt16();
            if (byteCount > MaxNameBytes)
                throw new InvalidDataException("Display name exceeds byte limit.");
            byte[] value = reader.ReadBytes(byteCount);
            if (value.Length != byteCount)
                throw new EndOfStreamException("Truncated display name.");
            string decoded = StrictUtf8.GetString(value);
            if (decoded.Length > MaxNameCharacters)
                throw new InvalidDataException("Display name exceeds length limit.");
            return decoded;
        }

        private static OrbitalsFormationVfxSettings GetFormation(VfxEffectSettings settings)
        {
            switch (settings)
            {
                case OrbitalsOrbsVfxSettings orbs: return orbs.Formation;
                case OrbitalsCoresVfxSettings cores: return cores.Formation;
                case OrbitalsFlamesVfxSettings flames: return flames.Formation;
                case OrbitalsEmbersVfxSettings embers: return embers.Formation;
                default: return null;
            }
        }

        private static void WriteFormation(BinaryWriter writer, OrbitalsFormationVfxSettings formation)
        {
            WriteBool(writer, formation.GlueLeaderEnabled);
            writer.Write((ushort)formation.GlueTargetInstanceIds.Count);
            foreach (uint id in formation.GlueTargetInstanceIds)
                writer.Write(id);
            OrbitalsPathVfxSettings p = formation.Path;
            WriteBool(writer, p.SnakeEnabled);
            WriteFinite(writer, p.Count);
            WriteFinite(writer, p.Speed);
            WriteFinite(writer, p.Spacing);
            WriteFinite(writer, p.Length);
            WriteFinite(writer, p.Radius);
            WriteFinite(writer, p.Cycles);
            WriteFinite(writer, p.Drift);
        }

        private static OrbitalsFormationVfxSettings ReadFormation(BinaryReader reader)
        {
            bool leaderEnabled = ReadBool(reader);
            int targetCount = reader.ReadUInt16();
            if (targetCount > MaxEffects)
                throw new InvalidDataException("Too many Glue targets.");
            var targets = new List<uint>(targetCount);
            for (int i = 0; i < targetCount; i++)
                targets.Add(reader.ReadUInt32());
            return new OrbitalsFormationVfxSettings
            {
                GlueLeaderEnabled = leaderEnabled,
                GlueTargetInstanceIds = targets,
                Path = new OrbitalsPathVfxSettings
                {
                    SnakeEnabled = ReadBool(reader),
                    Count = ReadFinite(reader),
                    Speed = ReadFinite(reader),
                    Spacing = ReadFinite(reader),
                    Length = ReadFinite(reader),
                    Radius = ReadFinite(reader),
                    Cycles = ReadFinite(reader),
                    Drift = ReadFinite(reader)
                }
            };
        }

        private static void ValidateFormation(OrbitalsFormationVfxSettings formation)
        {
            if (formation?.Path == null || formation.GlueTargetInstanceIds == null ||
                formation.GlueTargetInstanceIds.Count > MaxEffects)
                throw new InvalidDataException("Invalid orbital formation.");
            OrbitalsPathVfxSettings p = formation.Path;
            RequireFinite(p.Count);
            RequireFinite(p.Speed);
            RequireFinite(p.Spacing);
            RequireFinite(p.Length);
            RequireFinite(p.Radius);
            RequireFinite(p.Cycles);
            RequireFinite(p.Drift);
        }

        // Wire IDs are stable protocol values, independent of C# class names.
        private static byte GetTypeCode(string typeId)
        {
            switch (typeId)
            {
                case VfxEffectTypeIds.InnerFlames: return 1;
                case VfxEffectTypeIds.OuterFlames: return 2;
                case VfxEffectTypeIds.Strands: return 3;
                case VfxEffectTypeIds.Sparks: return 4;
                case VfxEffectTypeIds.Flare: return 5;
                case VfxEffectTypeIds.Aura: return 6;
                case VfxEffectTypeIds.OrbitalsOrbs: return 7;
                case VfxEffectTypeIds.OrbitalsCores: return 8;
                case VfxEffectTypeIds.OrbitalsFlames: return 9;
                case VfxEffectTypeIds.OrbitalsEmbers: return 10;
                default: throw new InvalidDataException("Unknown effect type.");
            }
        }

        private static string GetTypeId(byte code)
        {
            switch (code)
            {
                case 1: return VfxEffectTypeIds.InnerFlames;
                case 2: return VfxEffectTypeIds.OuterFlames;
                case 3: return VfxEffectTypeIds.Strands;
                case 4: return VfxEffectTypeIds.Sparks;
                case 5: return VfxEffectTypeIds.Flare;
                case 6: return VfxEffectTypeIds.Aura;
                case 7: return VfxEffectTypeIds.OrbitalsOrbs;
                case 8: return VfxEffectTypeIds.OrbitalsCores;
                case 9: return VfxEffectTypeIds.OrbitalsFlames;
                case 10: return VfxEffectTypeIds.OrbitalsEmbers;
                default: throw new InvalidDataException("Unknown effect wire ID.");
            }
        }

        private static void WriteSettings(BinaryWriter writer, VfxEffectBlock block)
        {
            switch (block.TypeId)
            {
                case VfxEffectTypeIds.InnerFlames:
                {
                    var s = ( InnerFlamesVfxSettings )block.Settings;
                    WriteBool(writer, s.WorldEnabled);
                    WriteBool(writer, s.BlackEnabled);
                    WriteBool(writer, s.WhiteEnabled);
                    WriteFinite(writer, s.Energy);
                    WriteFinite(writer, s.Scale);
                    WriteFinite(writer, s.Luminance);
                    WriteFinite(writer, s.Hue);
                    WriteFinite(writer, s.Lifetime);
                    WriteFinite(writer, s.SimulationSpeed);
                    WriteFinite(writer, s.Length);
                    WriteFinite(writer, s.Width);
                    return;
                }
                case VfxEffectTypeIds.OuterFlames:
                {
                    var s = ( OuterFlamesVfxSettings )block.Settings;
                    WriteBool(writer, s.WorldEnabled);
                    WriteBool(writer, s.BlackEnabled);
                    WriteBool(writer, s.WhiteEnabled);
                    WriteBool(writer, s.DragEnabled);
                    WriteFinite(writer, s.Energy);
                    WriteFinite(writer, s.Scale);
                    WriteFinite(writer, s.Luminance);
                    WriteFinite(writer, s.Hue);
                    WriteFinite(writer, s.Lifetime);
                    WriteFinite(writer, s.SimulationSpeed);
                    WriteFinite(writer, s.Length);
                    WriteFinite(writer, s.Width);
                    return;
                }
                case VfxEffectTypeIds.Strands:
                {
                    var s = ( StrandsVfxSettings )block.Settings;
                    WriteBool(writer, s.SpectrumEnabled);
                    WriteFinite(writer, s.Energy);
                    WriteFinite(writer, s.ScaleWhole);
                    WriteFinite(writer, s.ScaleParts);
                    WriteFinite(writer, s.Luminance);
                    WriteFinite(writer, s.Hue);
                    WriteFinite(writer, s.Lifetime);
                    WriteFinite(writer, s.Length);
                    WriteFinite(writer, s.SpectrumSpeed);
                    WriteFinite(writer, s.Speed);
                    WriteFinite(writer, s.Radius);
                    WriteFinite(writer, s.Drift);
                    return;
                }
                case VfxEffectTypeIds.Sparks:
                {
                    var s = ( SparksVfxSettings )block.Settings;
                    WriteFinite(writer, s.Energy);
                    WriteFinite(writer, s.Scale);
                    WriteFinite(writer, s.Luminance);
                    WriteFinite(writer, s.Hue);
                    WriteFinite(writer, s.Lifetime);
                    WriteFinite(writer, s.SimulationSpeed);
                    WriteFinite(writer, s.Length);
                    WriteFinite(writer, s.Width);
                    return;
                }
                case VfxEffectTypeIds.Flare:
                {
                    var s = ( FlareVfxSettings )block.Settings;
                    WriteFinite(writer, s.Scale);
                    WriteFinite(writer, s.Luminance);
                    WriteFinite(writer, s.Hue);
                    return;
                }
                case VfxEffectTypeIds.Aura:
                {
                    var s = ( AuraVfxSettings )block.Settings;
                    WriteFinite(writer, s.Scale);
                    WriteFinite(writer, s.Luminance);
                    WriteFinite(writer, s.Hue);
                    return;
                }
                case VfxEffectTypeIds.OrbitalsOrbs:
                {
                    var s = ( OrbitalsOrbsVfxSettings )block.Settings;
                    WriteFinite(writer, s.Scale);
                    WriteFinite(writer, s.Luminance);
                    WriteFinite(writer, s.Hue);
                    WriteFormation(writer, s.Formation);
                    return;
                }
                case VfxEffectTypeIds.OrbitalsCores:
                {
                    var s = ( OrbitalsCoresVfxSettings )block.Settings;
                    WriteFinite(writer, s.Scale);
                    WriteFinite(writer, s.Luminance);
                    WriteFinite(writer, s.Hue);
                    WriteBool(writer, s.SpinEnabled);
                    WriteFinite(writer, s.SpinSpeed);
                    WriteFormation(writer, s.Formation);
                    return;
                }
                case VfxEffectTypeIds.OrbitalsFlames:
                {
                    var s = ( OrbitalsFlamesVfxSettings )block.Settings;
                    WriteFinite(writer, s.Energy);
                    WriteFinite(writer, s.Scale);
                    WriteFinite(writer, s.Luminance);
                    WriteFinite(writer, s.Hue);
                    WriteFinite(writer, s.Lifetime);
                    WriteFinite(writer, s.SimulationSpeed);
                    WriteFormation(writer, s.Formation);
                    return;
                }
                case VfxEffectTypeIds.OrbitalsEmbers:
                {
                    var s = ( OrbitalsEmbersVfxSettings )block.Settings;
                    WriteFinite(writer, s.Energy);
                    WriteFinite(writer, s.Scale);
                    WriteFinite(writer, s.Luminance);
                    WriteFinite(writer, s.Hue);
                    WriteFinite(writer, s.Lifetime);
                    WriteFinite(writer, s.SimulationSpeed);
                    WriteFormation(writer, s.Formation);
                    return;
                }
                default: throw new InvalidDataException("Unknown settings type.");
            }
        }

        private static VfxEffectSettings ReadSettings(BinaryReader reader, string typeId)
        {
            switch (typeId)
            {
                case VfxEffectTypeIds.InnerFlames:
                    return new InnerFlamesVfxSettings
                    {
                        WorldEnabled = ReadBool(reader),
                        BlackEnabled = ReadBool(reader),
                        WhiteEnabled = ReadBool(reader),
                        Energy = ReadFinite(reader),
                        Scale = ReadFinite(reader),
                        Luminance = ReadFinite(reader),
                        Hue = ReadFinite(reader),
                        Lifetime = ReadFinite(reader),
                        SimulationSpeed = ReadFinite(reader),
                        Length = ReadFinite(reader),
                        Width = ReadFinite(reader),
                    };
                case VfxEffectTypeIds.OuterFlames:
                    return new OuterFlamesVfxSettings
                    {
                        WorldEnabled = ReadBool(reader),
                        BlackEnabled = ReadBool(reader),
                        WhiteEnabled = ReadBool(reader),
                        DragEnabled = ReadBool(reader),
                        Energy = ReadFinite(reader),
                        Scale = ReadFinite(reader),
                        Luminance = ReadFinite(reader),
                        Hue = ReadFinite(reader),
                        Lifetime = ReadFinite(reader),
                        SimulationSpeed = ReadFinite(reader),
                        Length = ReadFinite(reader),
                        Width = ReadFinite(reader),
                    };
                case VfxEffectTypeIds.Strands:
                    return new StrandsVfxSettings
                    {
                        SpectrumEnabled = ReadBool(reader),
                        Energy = ReadFinite(reader),
                        ScaleWhole = ReadFinite(reader),
                        ScaleParts = ReadFinite(reader),
                        Luminance = ReadFinite(reader),
                        Hue = ReadFinite(reader),
                        Lifetime = ReadFinite(reader),
                        Length = ReadFinite(reader),
                        SpectrumSpeed = ReadFinite(reader),
                        Speed = ReadFinite(reader),
                        Radius = ReadFinite(reader),
                        Drift = ReadFinite(reader),
                    };
                case VfxEffectTypeIds.Sparks:
                    return new SparksVfxSettings
                    {
                        Energy = ReadFinite(reader),
                        Scale = ReadFinite(reader),
                        Luminance = ReadFinite(reader),
                        Hue = ReadFinite(reader),
                        Lifetime = ReadFinite(reader),
                        SimulationSpeed = ReadFinite(reader),
                        Length = ReadFinite(reader),
                        Width = ReadFinite(reader),
                    };
                case VfxEffectTypeIds.Flare:
                    return new FlareVfxSettings
                    {
                        Scale = ReadFinite(reader),
                        Luminance = ReadFinite(reader),
                        Hue = ReadFinite(reader),
                    };
                case VfxEffectTypeIds.Aura:
                    return new AuraVfxSettings
                    {
                        Scale = ReadFinite(reader),
                        Luminance = ReadFinite(reader),
                        Hue = ReadFinite(reader),
                    };
                case VfxEffectTypeIds.OrbitalsOrbs:
                    return new OrbitalsOrbsVfxSettings
                    {
                        Scale = ReadFinite(reader),
                        Luminance = ReadFinite(reader),
                        Hue = ReadFinite(reader),
                        Formation = ReadFormation(reader),
                    };
                case VfxEffectTypeIds.OrbitalsCores:
                    return new OrbitalsCoresVfxSettings
                    {
                        Scale = ReadFinite(reader),
                        Luminance = ReadFinite(reader),
                        Hue = ReadFinite(reader),
                        SpinEnabled = ReadBool(reader),
                        SpinSpeed = ReadFinite(reader),
                        Formation = ReadFormation(reader),
                    };
                case VfxEffectTypeIds.OrbitalsFlames:
                    return new OrbitalsFlamesVfxSettings
                    {
                        Energy = ReadFinite(reader),
                        Scale = ReadFinite(reader),
                        Luminance = ReadFinite(reader),
                        Hue = ReadFinite(reader),
                        Lifetime = ReadFinite(reader),
                        SimulationSpeed = ReadFinite(reader),
                        Formation = ReadFormation(reader),
                    };
                case VfxEffectTypeIds.OrbitalsEmbers:
                    return new OrbitalsEmbersVfxSettings
                    {
                        Energy = ReadFinite(reader),
                        Scale = ReadFinite(reader),
                        Luminance = ReadFinite(reader),
                        Hue = ReadFinite(reader),
                        Lifetime = ReadFinite(reader),
                        SimulationSpeed = ReadFinite(reader),
                        Formation = ReadFormation(reader),
                    };
                default: throw new InvalidDataException("Unknown settings type.");
            }
        }

        private static void ValidateSettings(VfxEffectBlock block)
        {
            switch (block.TypeId)
            {
                case VfxEffectTypeIds.InnerFlames:
                {
                    if (!(block.Settings is InnerFlamesVfxSettings s))
                        throw new InvalidDataException("Settings/type mismatch.");
                    RequireFinite(s.Energy);
                    RequireFinite(s.Scale);
                    RequireFinite(s.Luminance);
                    RequireFinite(s.Hue);
                    RequireFinite(s.Lifetime);
                    RequireFinite(s.SimulationSpeed);
                    RequireFinite(s.Length);
                    RequireFinite(s.Width);
                    return;
                }
                case VfxEffectTypeIds.OuterFlames:
                {
                    if (!(block.Settings is OuterFlamesVfxSettings s))
                        throw new InvalidDataException("Settings/type mismatch.");
                    RequireFinite(s.Energy);
                    RequireFinite(s.Scale);
                    RequireFinite(s.Luminance);
                    RequireFinite(s.Hue);
                    RequireFinite(s.Lifetime);
                    RequireFinite(s.SimulationSpeed);
                    RequireFinite(s.Length);
                    RequireFinite(s.Width);
                    return;
                }
                case VfxEffectTypeIds.Strands:
                {
                    if (!(block.Settings is StrandsVfxSettings s))
                        throw new InvalidDataException("Settings/type mismatch.");
                    RequireFinite(s.Energy);
                    RequireFinite(s.ScaleWhole);
                    RequireFinite(s.ScaleParts);
                    RequireFinite(s.Luminance);
                    RequireFinite(s.Hue);
                    RequireFinite(s.Lifetime);
                    RequireFinite(s.Length);
                    RequireFinite(s.SpectrumSpeed);
                    RequireFinite(s.Speed);
                    RequireFinite(s.Radius);
                    RequireFinite(s.Drift);
                    return;
                }
                case VfxEffectTypeIds.Sparks:
                {
                    if (!(block.Settings is SparksVfxSettings s))
                        throw new InvalidDataException("Settings/type mismatch.");
                    RequireFinite(s.Energy);
                    RequireFinite(s.Scale);
                    RequireFinite(s.Luminance);
                    RequireFinite(s.Hue);
                    RequireFinite(s.Lifetime);
                    RequireFinite(s.SimulationSpeed);
                    RequireFinite(s.Length);
                    RequireFinite(s.Width);
                    return;
                }
                case VfxEffectTypeIds.Flare:
                {
                    if (!(block.Settings is FlareVfxSettings s))
                        throw new InvalidDataException("Settings/type mismatch.");
                    RequireFinite(s.Scale);
                    RequireFinite(s.Luminance);
                    RequireFinite(s.Hue);
                    return;
                }
                case VfxEffectTypeIds.Aura:
                {
                    if (!(block.Settings is AuraVfxSettings s))
                        throw new InvalidDataException("Settings/type mismatch.");
                    RequireFinite(s.Scale);
                    RequireFinite(s.Luminance);
                    RequireFinite(s.Hue);
                    return;
                }
                case VfxEffectTypeIds.OrbitalsOrbs:
                {
                    if (!(block.Settings is OrbitalsOrbsVfxSettings s))
                        throw new InvalidDataException("Settings/type mismatch.");
                    RequireFinite(s.Scale);
                    RequireFinite(s.Luminance);
                    RequireFinite(s.Hue);
                    ValidateFormation(s.Formation);
                    return;
                }
                case VfxEffectTypeIds.OrbitalsCores:
                {
                    if (!(block.Settings is OrbitalsCoresVfxSettings s))
                        throw new InvalidDataException("Settings/type mismatch.");
                    RequireFinite(s.Scale);
                    RequireFinite(s.Luminance);
                    RequireFinite(s.Hue);
                    RequireFinite(s.SpinSpeed);
                    ValidateFormation(s.Formation);
                    return;
                }
                case VfxEffectTypeIds.OrbitalsFlames:
                {
                    if (!(block.Settings is OrbitalsFlamesVfxSettings s))
                        throw new InvalidDataException("Settings/type mismatch.");
                    RequireFinite(s.Energy);
                    RequireFinite(s.Scale);
                    RequireFinite(s.Luminance);
                    RequireFinite(s.Hue);
                    RequireFinite(s.Lifetime);
                    RequireFinite(s.SimulationSpeed);
                    ValidateFormation(s.Formation);
                    return;
                }
                case VfxEffectTypeIds.OrbitalsEmbers:
                {
                    if (!(block.Settings is OrbitalsEmbersVfxSettings s))
                        throw new InvalidDataException("Settings/type mismatch.");
                    RequireFinite(s.Energy);
                    RequireFinite(s.Scale);
                    RequireFinite(s.Luminance);
                    RequireFinite(s.Hue);
                    RequireFinite(s.Lifetime);
                    RequireFinite(s.SimulationSpeed);
                    ValidateFormation(s.Formation);
                    return;
                }
                default: throw new InvalidDataException("Unknown settings type.");
            }
        }
    }
}
