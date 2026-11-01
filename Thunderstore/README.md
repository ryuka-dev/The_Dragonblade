# The Dragonblade

You found a katana with the words “Ryu-Ichimonji” carved into its blade.

You are not him.  
But for a moment, a fragment of his power answers your hand.

## Overview

The Dragonblade is a katana-focused melee expansion mod for SULFUR.

It adds a cyber-ninja style dash strike, kill-based cooldown refresh, enemy-kill healing, and a bottom-right ability HUD for katana stance gameplay.

Works on SULFUR 1.0 (tested on 1.0.2).

Starting from `0.3.0`, The Dragonblade uses **Toggle Melee Stance** as its prerequisite melee stance module.

## Required Dependency

The Dragonblade requires:

```text
Toggle Melee Stance 1.5.0 or newer
```

Toggle Melee Stance provides:

```text
Tap melee key
→ Toggle melee stance

Hold melee key
→ Use original melee behavior

Fire while toggled
→ Perform melee attack

Aim / Alt Fire while toggled
→ Keep vanilla alternative stance / block behavior
```

The Dragonblade no longer duplicates those toggle melee patches internally.

This reduces conflicts and prevents both mods from fighting over the same melee input state.

## Features

- Uses Toggle Melee Stance as the shared stance foundation.
- While holding a katana, Sprint becomes Dash Strike.
- Dash Strike moves in the direction you are looking.
- Dash Strike uses the current katana as the damage source.
- Killing an enemy refreshes Dash Strike.
- Killing an enemy heals the player.
- Breakable objects do not heal the player.
- Optional config allows non-NPC units, such as breakable obstacles, to refresh cooldown.
- Includes an ability icon HUD with cooldown display, refresh feedback, and healing feedback.
- Includes a small post-dash hang window to make aerial dash movement feel smoother.

## Basic controls

While holding a katana-like melee weapon and Toggle Melee Stance is active:

```text
Tap Melee key
→ Toggle melee stance on / off

Hold Melee key
→ Use the original melee behavior
→ Release to perform the original melee attack
→ Return to the previous weapon

Fire
→ Perform one melee attack while melee stance is active

Aim
→ Keep original aim / block behavior

Sprint
→ Dash Strike

Weapon slot key / scroll wheel / d-pad
→ Leave melee stance and switch to that weapon
```

The Dragonblade does not hardcode `F`, `LeftShift`, mouse buttons, or controller buttons.

It relies on Toggle Melee Stance and the game's own input actions, so it should respect key rebinding and controller input better than raw key checks.

## Installation

With a mod manager such as Gale or r2modman, Toggle Melee Stance is installed for you.

Manual installation:

1. Install BepInEx 5 for SULFUR.
2. Install Toggle Melee Stance 1.5.0 or newer.
3. Create a folder for The Dragonblade in `BepInEx/plugins` and copy `The Dragonblade.dll`, `dash_icon.png` and the `lang` folder into it.
4. Make sure the files are placed like this:

```text
BepInEx/
└─plugins/
  ├─Toggle Melee Stance.dll
  └─The Dragonblade/
    ├─The Dragonblade.dll
    ├─dash_icon.png
    └─lang/
```

Updating by hand: the plugin file is still `The Dragonblade.dll`, so replace the old one and don't keep a second copy anywhere in `BepInEx/plugins`. A manual install doesn't pull dependencies, so update Toggle Melee Stance to 1.5.0 yourself as well. `dash_icon.png` and the `lang` folder go next to the DLL.

## Compatibility

Do not install old standalone melee toggle mods together with The Dragonblade unless they are Toggle Melee Stance 1.5.0 or newer.

The Dragonblade now depends on Toggle Melee Stance for the stance system.

The Dragonblade no longer patches these toggle melee methods directly:

```text
EquipmentManager.HandleMeleeInput(bool)
EquipmentManager.HandleAimInput(bool)
EquipmentManager.PullTrigger()
EquipmentManager.ReleaseTrigger()
Weapon.ReportMeleeDone()
Weapon.ChargeMelee(bool)
```

Those are handled by Toggle Melee Stance.

The Dragonblade still patches systems needed for dash, movement, HUD, and kill rewards.

## Compatibility with Random Weapon Per Level

External weapon-switch handling is now owned by Toggle Melee Stance.

Default behavior:

```text
Kill enemy
→ Another mod switches your weapon
→ You try to fire
→ Toggle Melee Stance exits melee stance
→ The gun fires normally
```

This prevents a broken state where the player appears to be holding a gun, but melee stance still intercepts the fire input.

Recommended Toggle Melee Stance config:

```ini
[Compatibility]
KeepMeleeStanceAfterExternalWeaponSwitch = false
```

## Configuration

The Dragonblade config file is generated after launching the game once:

```text
BepInEx/config/kumo.sulfur.melee_expansion.cfg
```

Each option below is shown with its default value.

### EnableMod

Turns the whole mod on or off.

```ini
[General]
EnableMod = true
```

### EnableKatanaDash

Turns Dash Strike on or off.

```ini
[KatanaDash]
EnableKatanaDash = true
```

### KatanaNameKeywords

Comma-separated keywords used to detect katana-like melee weapons.

```ini
[KatanaDash]
KatanaNameKeywords = Katana,Wakizashi,BiggerKatana
```

### Cooldown

Dash cooldown in seconds (0.1 to 60).

```ini
[KatanaDash]
Cooldown = 5
```

### Distance

Dash distance (0.5 to 50).

```ini
[KatanaDash]
Distance = 8
```

### Duration

Dash duration in seconds (0.03 to 2).

```ini
[KatanaDash]
Duration = 0.22
```

### HitRadius

Radius used to find enemies along the dash path (0.1 to 5).

```ini
[KatanaDash]
HitRadius = 1
```

### DamageMultiplier

Multiplier applied to the current katana's damage (0 to 20).

```ini
[KatanaDash]
DamageMultiplier = 1
```

### HitEachUnitOnce

If true, each enemy can only be hit once per dash.

```ini
[KatanaDash]
HitEachUnitOnce = true
```

### RefreshCooldownOnPlayerKill

Resets the dash cooldown when you kill an enemy.

```ini
[KatanaDash]
RefreshCooldownOnPlayerKill = true
```

### RefreshRequiresKatanaStance

If true, kill refresh and healing only work while melee stance is toggled on with a katana.

```ini
[KatanaDash]
RefreshRequiresKatanaStance = true
```

### RefreshCooldownOnNonNpcUnitKill

If true, breaking non-NPC units such as obstacles also resets the dash cooldown. These never heal you.

```ini
[KatanaDash]
RefreshCooldownOnNonNpcUnitKill = false
```

### HealOnEnemyKill

Heals you when you kill an enemy NPC, under the same conditions as the kill refresh. Breakable obstacles never trigger it.

```ini
[KatanaDash]
HealOnEnemyKill = true
```

### HealAmountOnEnemyKill

Health restored per enemy kill (0 to 999).

```ini
[KatanaDash]
HealAmountOnEnemyKill = 5
```

### PostHangDuration

Short hang time in seconds after a dash ends, so you don't drop at full falling speed right away (0 to 0.5).

```ini
[KatanaDash]
PostHangDuration = 0.12
```

### PostHangMaxDownwardSpeed

Maximum falling speed during the post-dash hang. 0 means no falling at all during the hang (0 to 20).

```ini
[KatanaDash]
PostHangMaxDownwardSpeed = 0.5
```

### SuppressFallingAnimationDuringPostHang

Hides the falling animation during the post-dash hang.

```ini
[KatanaDash]
SuppressFallingAnimationDuringPostHang = true
```

### EnableDashHud

Shows the dash cooldown HUD in the bottom-right corner.

```ini
[UI]
EnableDashHud = true
```

### DashIconFileName

PNG file used as the HUD icon. It is loaded from the same folder as `The Dragonblade.dll`.

```ini
[UI]
DashIconFileName = dash_icon.png
```

### DashHudIconSize

HUD icon size in pixels (48 to 128).

```ini
[UI]
DashHudIconSize = 76
```

### DashHudUseActualSprintBinding

Shows your actual Sprint binding on the HUD when the game reports it.

```ini
[UI]
DashHudUseActualSprintBinding = true
```

### DashHudFallbackKeyLabel

Key label shown on the HUD when the Sprint binding can't be read.

```ini
[UI]
DashHudFallbackKeyLabel = SHIFT
```

### DashKillRefreshFeedbackDuration

Seconds the HUD flashes when a kill resets the cooldown (0 to 3).

```ini
[UI]
DashKillRefreshFeedbackDuration = 0.45
```

### DashHealFeedbackDuration

Seconds the +HP text stays on screen after an enemy kill (0 to 3).

```ini
[UI]
DashHealFeedbackDuration = 0.85
```

### LogDash

Writes dash debug events to the BepInEx log. Keep it off for normal play.

```ini
[Debug]
LogDash = false
```

### LogStateChanges

Writes stance state changes to the BepInEx log. Keep it off for normal play.

```ini
[Debug]
LogStateChanges = false
```

### Toggle Melee Stance settings

Toggle melee behavior is configured in Toggle Melee Stance, not in The Dragonblade.

For example:

```ini
BepInEx/config/kumo.sulfur.toggle_melee_stance.cfg

[Melee]
EnableTapHoldMeleeKey = true
MeleeToggleTapThreshold = 0.13

[Compatibility]
KeepMeleeStanceAfterExternalWeaponSwitch = false
```

## Notes

- The Sprint key is not hardcoded. The mod reads the game's Sprint action, so remapped controls are supported.
- The Melee key behavior is provided by Toggle Melee Stance.
- The Dragonblade does not simulate keyboard input.
- While katana stance is active, the mod keeps the game's own sprint state active through the internal sprint system.
- Dash movement is applied through the game's movement pipeline rather than directly teleporting the player.
- Enemy-kill healing only works on NPC/enemy kills. Obstacles can optionally refresh cooldown, but they never heal the player.

## Files

```text
The Dragonblade.dll
dash_icon.png
lang/
```

## Credits

Created by Ryuka.

This mod is an unofficial fan-made gameplay mod for SULFUR.

## SULFUR Config Localization Support

This mod includes localization files for SULFUR Config.

This localization support is only for the in-game configuration page provided by SULFUR Config. The mod name and config sections are translated in every language below, and setting names in Simplified Chinese and Japanese. Setting descriptions are still in English.

It does not change the game’s own text, item names, dialogue, or gameplay content.

Supported SULFUR Config languages:

- English
- Simplified Chinese
- Swedish
- French
- Italian
- German
- Spanish
- Portuguese
- Russian
- Polish
- Japanese
- Korean
- Turkish
- Arabic
