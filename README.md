# The Queen

The Queen is a *Slay the Spire 2* character mod built with Godot 4 .NET and STS2.RitsuLib.

The mod adds The Queen as a playable character, with custom cards, relics, potions, visuals, localization, and mechanics around Soul Lamp, Bound cards, Amalgam intents, and monster capture rewards.

## Features

- Playable Queen character with custom combat, map, selection, rest-site, merchant, and energy UI visuals.
- Large custom card pool, including Queen cards, token cards, Amalgam/monster cards, and capture reward cards.
- Custom relics, potions, powers, enchantments, hover tips, VFX, and gameplay patches.
- Localization for English, Simplified Chinese, Japanese, Korean, Russian, and mod settings text.
- Development helpers for finding a local *Slay the Spire 2* install and copying build outputs into the game's `mods/TheQueen` folder.

## Install

For players, use a packaged release if one is available. A release package should contain:

- `TheQueen.dll`
- `TheQueen.pck`
- `TheQueen.json`

Place those files together in:

```text
<Slay the Spire 2 install>/mods/TheQueen/
```

The mod depends on `STS2-RitsuLib`. Install a compatible version before launching the game. The current manifest requires at least `0.4.20`.

## Development

Requirements:

- *Slay the Spire 2* installed locally.
- Godot `4.5.1` with .NET support.
- .NET SDK `9.0`.
- STS2.RitsuLib.

Build the C# project:

```powershell
dotnet build
```

The project tries to auto-detect the local *Slay the Spire 2* install. If that fails, copy `local.props.example` to `local.props` and set `Sts2Dir` or `Sts2DataDir` for your machine.

After a successful build, MSBuild copies the mod DLL and manifest into:

```text
<Slay the Spire 2 install>/mods/TheQueen/
```

Export `TheQueen.pck` from Godot using the existing export preset, then place it next to the DLL and manifest before testing in game.

## Repository Layout

- `Scripts/` - C# mod code.
- `TheQueen/` - Godot resources, scenes, images, shaders, fonts, animations, and localization.
- `tools/` - local development utilities.
- `TheQueen.csproj` - Godot .NET project and post-build copy target.
- `TheQueen.json` - active mod manifest.
- `mod_manifest.json` - legacy/compatibility manifest kept in sync with `TheQueen.json`.

## License

The original source code in this repository is released under the MIT License. See `LICENSE`.

Important asset exception: some included assets are derived from or include original *Slay the Spire 2* resources, including the original Queen and Amalgam creature appearances. Those assets are not covered by the MIT license. See `THIRD_PARTY_NOTICES.md` before reusing or redistributing assets from this repository.
