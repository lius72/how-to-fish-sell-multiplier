# How to Fish - Sell Multiplier

A BepInEx 5 plugin that multiplies the value received when selling fish in
[How to Fish](https://store.steampowered.com/app/4001890/).

The recommended player download is
`HowToFish.SellMultiplier.AllInOne-1.0.5.zip` from the repository's Releases
page. It includes BepInEx, both plugin modes, configuration files, and
installation instructions.

## Features

- Single-player Mode: applies a 1000x sell multiplier when exactly one spawned
  Player is present.
- Private Co-op Mode: applies a host-controlled multiplier when 2 to 4 spawned
  Players are present.
- Separate plugin DLLs prevent both multipliers from stacking.
- The multiplier is calculated on the host/server.
- Includes capped integer arithmetic to avoid overflow.

## Requirements

- How to Fish (Steam AppID 4001890)
- BepInEx 5.4.23.6 x64

## Installation

1. Download the desired ZIP from the repository's Releases page.
2. Extract its contents into the `How to Fish` directory containing
   `How to Fish.exe`.
3. Start the game through Steam.

Typical install location:

```text
SteamLibrary\steamapps\common\How to Fish\How to Fish
```

## Configuration

Single-player:

```text
BepInEx\config\local.howtofish.sellmultiplier.cfg
```

```ini
[Economy]
SellMultiplier = 1000

[Safety]
MaxPlayers = 1
```

Private co-op:

```text
BepInEx\config\local.howtofish.sellmultiplier.multiplayer.cfg
```

```ini
[General]
Enabled = true

[Economy]
SellMultiplier = 1000

[Multiplayer]
MinPlayers = 2
MaxPlayers = 4
```

Set `SellMultiplier = 1` or `Enabled = false` to disable a mode.

## Multiplayer Notice

The co-op plugin only calculates money on the host/server. It is intended only
for a private lobby where every participant knows about and agrees to the mod.
Do not use it in public matchmaking or public servers.

## Building

The build requires the game's managed assemblies and BepInEx core assemblies.
No game files are distributed in this repository.

```powershell
.\build.ps1 `
  -GameDir "E:\SteamLibrary\steamapps\common\How to Fish\How to Fish" `
  -BepInExDir "E:\SteamLibrary\steamapps\common\How to Fish\How to Fish"
```

## License

The plugin source is licensed under the MIT License. BepInEx and HarmonyX are
third-party projects with their own licenses. See `THIRD_PARTY_NOTICES.md`.

This project is not affiliated with Dazed Games.
