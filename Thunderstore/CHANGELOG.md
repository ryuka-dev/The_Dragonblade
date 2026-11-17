# Changelog

## 0.5.1

- Fixed the Dash HUD showing up over your gun after you changed levels or died in katana stance.
- Toggle Melee Stance 1.7.0 fixes the same thing on its side, and also fixes an error that kept repeating after dying in melee stance on Hard. Worth updating, but this version doesn't need it.
- Tested on SULFUR 1.0.6.

## 0.5.0

- You can move the Dash HUD now. `DashHudAnchor` puts it in any corner or edge of the screen, and `DashHudOffsetX` / `DashHudOffsetY` nudge it from there. In katana stance the game keeps your gun's ammo counter in the bottom-right corner, right under the HUD; `DashHudOffsetY = 150` lifts the HUD above a pistol's counter.
- The Dash HUD now scales with your resolution and the game's HUD Size setting, the same way the game's own HUD does, and moves in with the ultrawide HUD padding. At 1080p it looks the same as before. At 1440p it's about a third bigger and at 4K twice as big, so if you had raised `DashHudIconSize` to make up for a small icon on a big screen, you may want to lower it again.
- The Dash HUD no longer shows on top of the loading screen.
- Added Traditional Chinese to the SULFUR Config translations.
- Tested on SULFUR 1.0.6.

## 0.4.0

- Works again on SULFUR 1.0 (tested on 1.0.2). Since game version 0.18.3 the mod had stopped loading its features entirely, so dash strike, kill refresh and healing did nothing.
- Fixed dash strike damage on SULFUR 1.0.2.
- Now requires Toggle Melee Stance 1.5.0. That update also lets you switch straight from melee stance to a weapon with the weapon keys, and fixes the fire button getting stuck after an Endless mode level-up.
- Updating by hand: the plugin file is still `The Dragonblade.dll`, so replace the old one instead of keeping two copies, keep `dash_icon.png` and the `lang` folder next to it, and update Toggle Melee Stance to 1.5.0 yourself.

## 0.3.1

- Added SULFUR Config translations for 14 languages. No gameplay changes.

## 0.3.0

- Changed The Dragonblade to require Toggle Melee Stance as a prerequisite.
- Removed duplicated toggle melee input patches from The Dragonblade.
- Dragonblade now reads melee stance state from Toggle Melee Stance public API.
- Moved tap / hold melee key behavior and external weapon switch handling to Toggle Melee Stance.
- Reduced risk of double-patching melee input when both mods are installed.
