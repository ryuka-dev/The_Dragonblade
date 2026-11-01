# Changelog

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
