# NADA VFX: Weapon

**NADA VFX** is a modular runtime VFX framework for Valheim built around creating, customizing, and controlling visual effects dynamically at runtime.

**NADA VFX: Weapon** is the weapon-focused implementation, allowing VFX rigs to be built, customized, and bound to individual weapons and held items.

> **Development Preview — v0.9.0**
>
> NADA VFX: Weapon is currently in active development. The preview build is functional and has been tested with Valheim 1.0, but bugs and compatibility issues with other mods may still occur.

<img width="3215" height="1385" alt="NADA VFX: Weapon showcase" src="https://github.com/user-attachments/assets/8a68832f-5c11-436c-85ef-8d9c6b1a62e0" />

## About

NADA VFX builds effects at runtime using existing Valheim materials, particle systems, and components loaded in memory as reusable building blocks rather than requiring custom effect assets.

In NADA VFX: Weapon, multiple effects can be combined into a single VFX rig, extensively customized, and bound to individual weapons or held items.

Once a VFX rig is bound, its configuration is stored as persistent per-item state. This allows different instances of the same weapon to have completely different effects while preserving their individual VFX configurations between equips.

NADA VFX is intended to grow into a broader family of focused runtime VFX systems, with separate implementations for weapons, players, armor, buildings, and other world objects.

## Devlog & Showcase

I made a full devlog/showcase covering why I started the project, how it evolved, and what the current system can do.

[![NADA VFX: Weapon — Devlog & Showcase](https://img.youtube.com/vi/X3wuS_aE4wE/maxresdefault.jpg)](https://youtu.be/X3wuS_aE4wE)

## Installation

### Requirements

- Valheim
- BepInEx 5

### Manual Installation

1. Download the latest NADA VFX: Weapon release from the [Releases page](https://github.com/naomi-nada/nada-vfx-weapon/releases/latest).
2. Extract `NADA.VFX.Weapon.dll`.
3. Place the DLL in:

   `Valheim/BepInEx/plugins/`

4. Launch Valheim.

NADA VFX: Weapon will create its configuration entries automatically after the game starts.

## Basic Usage

NADA VFX: Weapon uses three primary item controls:

- **Attach to Weapon** — creates/attaches the NADA VFX rig used to customize the currently held item.
- **Bind to Weapon** — binds the current VFX configuration to the individual held item so its state persists.
- **Unbind Current Weapon** — removes the item's bound NADA VFX state.

VFX configurations can also be saved as reusable **styles** and applied again later.

<img width="610" height="315" alt="NADA VFX controls and styles" src="https://github.com/user-attachments/assets/8204de79-a817-4e47-840f-6a65d9b59d5a" />

## Customization

Each effect exposes its own set of modifiers depending on how that effect behaves. These controls are organized into three main categories:

### Structure

Controls the spatial composition and placement of an effect. Depending on the effect, this can include scale, width, length, count, spacing, radius, offsets, rotation, and coverage.

### Visuals

Controls the appearance and intensity of an effect, including properties such as color, luminance, energy, and effect-specific visual modes.

### Motion

Controls how an effect behaves over time. Depending on the effect, this can include lifetime, simulation speed, drift, drag, movement speed, spin, orbital behavior, and other motion controls.

## Effects

NADA VFX: Weapon currently includes:

- **Inner Flames**
- **Outer Flames**
- **Strands**
- **Sparks**
- **Flare**
- **Aura**
- **Orbitals**
  - Orbs
  - Cores
  - Flames
  - Embers

Effects can be enabled independently and combined to build substantially different VFX rigs.

## Item Binding & Persistence

VFX configurations can be bound to individual items rather than applied globally to every instance of a weapon.

When a configuration is bound, NADA stores the item's VFX state with that individual item. This means two copies of the same weapon can use completely different VFX rigs.

Bound VFX state persists with the item, allowing its configuration to be restored when the item is equipped again.

## Compatibility

NADA VFX: Weapon has been tested with **Valheim 1.0** on:

- **Windows**
- **macOS / Metal**

The system is designed around runtime use of existing Valheim VFX rather than requiring custom shaders or effect assets, helping maintain compatibility across both rendering environments.

Compatibility with other mods has not yet been exhaustively tested.

## Roadmap

NADA VFX is still actively evolving. Current plans include:

- More effects and modifiers
- Continued stability and compatibility work
- Improvements to the style/preset system
- Expanded weapon support and customization
- A dedicated VFX editor/manager
- A public integration API
- Separate runtime VFX systems for **players/armor**, **buildings**, and other world objects

The long-term goal is for NADA VFX to provide a reusable runtime VFX foundation with focused implementations for different kinds of targets.

NADA VFX: Weapon will remain weapon-focused, while player, building, and world-object systems can evolve independently around the same core ideas and public integration layer.

## License

NADA VFX: Weapon is **source-available**, but its implementation is not released under a permissive open-source license.

You're welcome to use NADA VFX: Weapon, read through the source, and build your own mods that interact with it through supported public interfaces. Please don't copy, repackage, redistribute, or distribute modified versions of NADA VFX: Weapon itself without permission.

The planned **NADA VFX public API/SDK** will provide the supported integration layer for third-party mods and will be licensed separately under a permissive license.

See [LICENSE](LICENSE) for the full terms.

The NADA VFX: Weapon license only applies to original code and project material contained in this project. Valheim, game assets, third-party libraries, and other third-party material remain the property of their respective rights holders.

## Feedback & Community

Bug reports, compatibility reports, effect/modifier ideas, and general feedback are welcome.

- Open an [Issue](https://github.com/naomi-nada/nada-vfx-weapon/issues)
- Join the [NADA VFX Discord](https://discord.gg/umsYF5uKeY)

Feedback from other mod developers on architecture, performance, and compatibility is also very welcome. ^-^
