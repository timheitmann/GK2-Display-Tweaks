# GK2 Display Tweaks

GK2 Display Tweaks is a BepInEx plugin for Graveyard Keeper 2
that improves support for ultrawide and high-resolution displays
and adds additional display-related settings.

## Features

- Ultrawide and super-ultrawide resolution support
- Removes the game's 5120 pixel resolution width limit
- Adjustable UI scaling
- Adjustable camera zoom
- Settings integrated directly into the game's settings menu
- Settings are saved between game sessions
- No modification of `Assembly-CSharp.dll`

## Ultrawide Support

Graveyard Keeper 2 normally filters resolutions with an aspect
ratio wider than 2:1.

GK2 Display Tweaks removes this restriction at runtime, allowing
the game to expose ultrawide and super-ultrawide resolutions
reported by the system.

The plugin also removes the game's built-in 5120 pixel maximum
resolution width.

## UI Scale

The game's settings menu receives an additional `UI Scale` option.

Available values:

- 100% to 150%
- 5% increments

The setting is applied immediately and saved between game sessions.

## Camera Zoom

The game's settings menu also receives a `Camera Zoom` option.

Available values:

- 75% to 150%
- 5% increments
- 100% corresponds to the original game camera

The setting is applied immediately and saved between game sessions.

## Legacy Patcher

Earlier versions of the ultrawide fix used a standalone patcher
that directly modified the game's `Assembly-CSharp.dll`.

This implementation has been replaced by the BepInEx plugin and
is no longer required.

The original patcher is preserved in the
[`legacy-patcher`](legacy-patcher/) directory for reference.