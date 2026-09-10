# NADA VFX: Weapon

A modular runtime VFX system for Valheim that lets you build, customize, and bind visual effects to individual weapons and held items.

> **Development Preview — v0.9.0**
>
> NADA VFX: Weapon is currently in active development. The preview build is functional and has been tested with Valheim 1.0, but bugs and compatibility issues with other mods may still occur.

<img width="3215" height="1385" alt="NADA VFX: Weapon showcase" src="https://github.com/user-attachments/assets/8a68832f-5c11-436c-85ef-8d9c6b1a62e0" />

## What is NADA VFX: Weapon?

NADA VFX: Weapon is a runtime visual-effects system that lets you create custom VFX rigs for weapons and other held items in Valheim.

Rather than requiring custom effect assets, NADA VFX builds its effects at runtime using existing Valheim VFX as building blocks. Multiple effects can be combined into a single VFX rig and extensively customized.

Once a VFX rig is created, it can be bound to an individual item. The item's VFX configuration is stored as persistent per-item state, allowing different instances of the same weapon to have completely different effects.

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

NADA VFX will create its configuration entries automatically after the game starts.

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
- Separate runtime VFX systems for **players/armor** and **building pieces**

The goal is to grow NADA into a family of modular runtime VFX systems while keeping each system independently usable.

## Feedback & Community

Bug reports, compatibility reports, effect/modifier ideas, and general feedback are welcome.

- Open an [Issue](https://github.com/naomi-nada/nada-vfx-weapon/issues)
- Join the [NADA VFX Discord](https://discord.gg/umsYF5uKeY)

Feedback from other mod developers on architecture, performance, and compatibility is also very welcome.

## Credits

**NADA VFX: Weapon** is developed by Naomi Nada B.F.

Built for [Valheim](https://www.valheimgame.com/) using [BepInEx](https://github.com/BepInEx/BepInEx).

Valheim and its original assets are property of Iron Gate Studio and their respective rights holders.
