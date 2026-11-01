# The Dragonblade (developer notes)

BepInEx plugin for **SULFUR** (plugin GUID `kumo.sulfur.melee_expansion`). The build output is
`The Dragonblade.dll`.

The player-facing store page is [`Thunderstore/README.md`](Thunderstore/README.md).

## Requirements

The plugin compiles against Toggle Melee Stance's public API, so it needs a built checkout of
[Toggle_Melee_Stance](https://github.com/ryuka-dev/Toggle_Melee_Stance) next to this one:

```text
source/
  The_Dragonblade/
  Toggle_Melee_Stance/      build it first (Debug): bin/Debug/Toggle Melee Stance.dll
```

The reference is not copied into the build output; at runtime the game loads Toggle Melee
Stance as its own plugin.

## Building

Machine-specific paths live only in `Directory.Build.props`: the SULFUR install, the Gale
profile that supplies the BepInEx core DLLs, the Toggle Melee Stance DLL and the
`Thunderstore/` folder. Override any of them on the command line, e.g.
`-p:SulfurGameDir=...`, `-p:SulfurGaleProfileDir=...` or `-p:ToggleMeleeStanceDll=...`.

```powershell
dotnet build "Katana Dash Strike Debug.csproj" -c Release
```

With `-p:DeployToSulfurProfile=true` the build also copies the DLL, `dash_icon.png` and
`lang/*.json` into `BepInEx/plugins/The Dragonblade/` of the Gale profile, and the DLL into
`Thunderstore/`.

## Packaging

`Thunderstore/` holds the package contents and the store files kept in the repository:
`manifest.json`, `README.md`, `CHANGELOG.md`, `icon.png`, `dash_icon.png` and `lang/`. The
built DLL lands there too but is not committed. Before a release, the version in
`manifest.json`, the top entry of `CHANGELOG.md` and `PluginVersion` in `Class1.cs` must match.
