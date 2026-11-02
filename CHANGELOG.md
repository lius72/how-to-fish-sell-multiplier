# Changelog

## Single-player 1.0.4

- Fixed HarmonyX finalizer installation.
- Added NPC and sell-box sale paths.
- Confirmed `2 -> 2000` sell value and `16 -> 2016` wallet change in a live test.
- Removed diagnostic stack-trace logging.

## Release v1.0.5

- Rebuilt the all-in-one package with the multiplayer host-detection fix.
- Recommended download: `HowToFish.SellMultiplier.AllInOne-1.0.5.zip`.

## Multiplayer 1.0.2

- Fixed host/server detection for the static `MoneyManager.SellItem` method.
- Changed the check to use `MoneyManager.Instance.IsServerInitialized`.
- Verified with a host and two spawned Players in game.

## Multiplayer 1.0.0

- Added a separate host-controlled plugin.
- Applies only with 2 to 4 spawned Player objects.
- Keeps the single-player plugin inactive during multiplayer.
