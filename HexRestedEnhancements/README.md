# HexBaseEnhancements

Adds configurable quality-of-life improvements for your Valheim base, including faster resting, automatic repairs, faster doors, automatic door closing, and an increased comfort radius.

## Screenshots

![HexBaseEnhancements Configuration](https://raw.githubusercontent.com/guillenjgg/valheim-hex-mod-images/main/hexbaseenhancements/hexbaseenhancements_1.png)

## Features

* Configurable Rested status effect delay
* Automatically removes the Wet status effect when sheltered and near a fire
* Rapidly restores health and stamina while resting
* Automatically repairs damaged inventory items when sheltered and near a fire
* Displays a notification and plays the vanilla repair sound when items are repaired
* Greatly increases door opening speed
* Automatically closes opened doors after a configurable delay
* Configurable comfort detection radius from **1 to 50 meters**

## Rested Improvements

HexBaseEnhancements allows the delay before receiving the Rested status effect to be customized.

The default custom delay is:

`2 seconds`

The custom delay can be disabled to restore Valheim's normal behavior.

When rapid health and stamina regeneration is enabled, health and stamina are quickly restored while resting.

## Wet Status Removal

When enabled, the Wet status effect is automatically removed when the player is both:

* Sheltered
* Near a fire

## Automatic Inventory Repair

When enabled, damaged repairable items in the player's inventory are automatically repaired when the player is sheltered and near a fire.

When one or more items are repaired:

* A notification is displayed
* The vanilla repair sound is played

## Door Improvements

Doors can be configured to open significantly faster than normal.

Automatic door closing can also be enabled with a configurable delay.

If a door is manually closed before the auto-close timer expires, it will remain closed.

## Comfort Radius

Valheim normally detects comfort-providing pieces within approximately **10 meters** of the player.

HexBaseEnhancements allows this radius to be configured between:

`1 - 50 meters`

Disabling the increased comfort radius restores the vanilla **10 meter** radius.

## Configuration

Configuration options are available in:

`BepInEx/config/com.hex.baseenhancements.cfg`

Current configuration options include:

* **RemoveWetDebuff** — Automatically remove the Wet status effect when sheltered and near a fire
* **CustomRestedDelay** — Enable or disable the custom Rested status effect delay
* **RestedDelayInSeconds** — Number of seconds before receiving the Rested status effect
* **EnableRapidHealthAndStaminaRegen** — Enable rapid health and stamina recovery while resting
* **EnableAutoRepair** — Automatically repair damaged inventory items when sheltered and near a fire
* **EnableDoorsOpenFaster** — Enable faster door opening
* **EnableDoorsAutoClose** — Automatically close opened doors
* **DoorAutoCloseInSeconds** — Delay before an opened door automatically closes
* **EnableIncreasedComfortRadius** — Enable the custom comfort detection radius
* **ComfortRadiusInMeters** — Radius in meters used to detect comfort-providing pieces

## Multiplayer

> ## ⚠️ Multiplayer / Dedicated Server Disclaimer
> **HexBaseEnhancements has not been tested in multiplayer or on a dedicated server.**
>
> Multiplayer and dedicated server behavior may vary. If you encounter any issues, please report them through GitHub or Discord.

## Installation

### r2modman / Thunderstore Mod Manager

Install **HexBaseEnhancements** through your preferred Thunderstore mod manager.

### Manual Installation

1. Install BepInEx for Valheim.
2. Download HexBaseEnhancements.
3. Copy `HexBaseEnhancements.dll` into:

   `BepInEx/plugins/HexBaseEnhancements/`

4. Start Valheim.

## Source Code

Source code is available on GitHub:

https://github.com/guillenjgg/valheim-hex-base-enhancements

## Issues / Suggestions

For bug reports, suggestions, or feedback, join the Discord:

https://discord.gg/wU2FXD94v4