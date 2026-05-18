# ArenaProject Context

## Project Vision

ArenaProject is a Unity mobile dark fantasy RPG with a city-hub main screen.

The main scene is `MainMenu`. It is not intended to be a static list of menu buttons. The core UX direction is:

- left side: stable `CharacterPanel`
- right side: interactive fantasy city hub
- buildings open location UI windows directly
- mobile-first layout and large touch-friendly controls
- minimal nested menus
- no extra confirmation window for normal building entry

Git and GitHub workflow is active for the project.

## Current Scene State

The current main gameplay scene is:

- `Assets/_Project/Scenes/MainMenu.unity`

Current high-level UI structure:

- `Canvas`
- `CharacterPanel`
- `MainLocationPanel`
- `BarracksWindow`
- `ArenaWindow`
- `StatsWindow`
- `InfoWindow`
- `EquipWindow`

`CharacterPanel` is the stable left-side player panel. It contains the portrait, resource bars, equipped weapon visuals, level/power text, and an integrated stats view.

`MainLocationPanel` is the right-side city hub. It contains clickable/touchable building areas such as Barracks and Arena.

## Core Architecture

### LocationNavigationController

`LocationNavigationController` owns location-window navigation for the city hub.

Responsibilities:

- open location windows by `LocationId`
- close other location windows before opening a new one
- keep only one location window active at a time
- route Barracks open/close through `BarracksWindowUI` so Barracks cleanup logic is preserved
- use serialized window bindings as the primary architecture
- use name lookup only as a fallback safety mechanism

Important rule: normal building taps should directly open the target window. They should not open `InfoWindow` as a mandatory confirmation step.

### BuildingButton

`BuildingButton` is attached to building touch areas.

Responsibilities:

- store `LocationId`
- store optional display name
- call `LocationNavigationController.OpenLocation(locationId)` on tap

The current Barracks and Arena building touch areas use this flow.

### BarracksWindow

`BarracksWindow` is the equipment/inventory location.

Important notes:

- existing Barracks systems are functional and heavily inspector-linked
- do not rewrite or rename Barracks internals unless necessary
- Barracks close now routes through `LocationNavigationController.CloseCurrentLocation`
- `LocationNavigationController.CloseCurrentLocation` still calls `BarracksWindowUI.CloseBarracks()` internally for Barracks cleanup

### ArenaWindow

`ArenaWindow` is the first gameplay location window beyond Barracks.

Current Arena features:

- opens directly from the city hub Arena building
- inactive by default in `MainMenu`
- contains three enemy cards
- each enemy has a portrait placeholder, name, level, combat power, EXP reward, Arena Tokens reward, Info button, and Fight button
- `EnemyInfoPanel` opens inside ArenaWindow for selected enemy details
- `ResultPanel` opens inside ArenaWindow after pseudo fight
- no real animated combat scene exists yet

### PlayerStats

`PlayerStats` is the source of truth for player progression and final combat stats.

Stores:

- player name
- level
- HP / MP
- current EXP
- required EXP (`maxExp`)
- Arena Tokens
- start stats
- growth per level
- manual allocated stat points
- native stats
- equipment bonus stats
- final stats
- combat power

Important methods:

- `RecalculateStats()`
- `ApplyItemStats(ItemData item)`
- `LevelUp()`
- `AddExperience(int amount)`
- `AddArenaTokens(int amount)`
- `TryAllocateStat(PlayerStatType statType)`

Important behavior:

- `RecalculateStats()` recalculates native stats from level, growth, and allocated points
- `RecalculateStats()` resets equipment bonus fields to zero
- equipment bonuses must be reapplied by `EquipmentManager` after a full stat recalculation

### EquipmentManager

`EquipmentManager` is the source of truth for equipped item slots.

Responsibilities:

- equip/unequip items
- track equipped item instances
- update equipment slot visuals
- update weapon visuals on `CharacterPanel`
- recalculate player stats with equipment bonuses

Important method:

- `RefreshPlayerStats()`

Important behavior:

- calls `playerStats.RecalculateStats()`
- reapplies every currently equipped item once with `playerStats.ApplyItemStats(itemData)`
- refreshes stats UI and Barracks inventory UI

Do not bypass this flow when equipment bonuses need to remain valid.

### PlayerProfileUI

`PlayerProfileUI` refreshes top-level identity/progression text on the character panel:

- player name
- level
- combat power

It reads from `PlayerStats`.

### PlayerBarsUI

`PlayerBarsUI` refreshes:

- HP fill/text
- MP fill/text
- EXP fill/text

It reads `currentHp`, `maxHp`, `currentMp`, `maxMp`, `currentExp`, and `maxExp` from `PlayerStats`.

## Completed Systems

Current completed or working systems:

- inventory system
- item database
- item instances
- equipment system
- equip / unequip
- equipment categories
- multiple weapon slots
- multiple ring slots
- character stats
- native stats
- equipment bonus stats
- final stats
- combat power
- HP scaling from endurance
- MP scaling from intelligence
- weapon visuals on character portrait
- Barracks window
- item rarity sorting
- equipped items pinned to top
- city hub access
- modular city navigation
- direct Barracks opening from city hub
- direct Arena opening from city hub
- ArenaWindow v1
- Arena enemy cards
- Arena `EnemyInfoPanel`
- Arena pseudo fight calculation
- Arena `ResultPanel`
- EXP rewards from Arena
- Arena Tokens rewards from Arena
- level up from EXP
- combat power growth from level/stat scaling
- equipment refresh after Arena level-up

## Arena Gameplay v1

Arena enemies currently contain:

- enemy name
- level
- combat power
- HP
- attack
- defense
- crit chance
- base EXP reward
- Arena Tokens reward
- short description

Pseudo fight formula:

- use `playerStats.combatPower` if available and greater than 0
- otherwise use fallback player power
- `finalPlayerPower = playerPower * Random.Range(0.9, 1.1)`
- `finalEnemyPower = enemyPower * Random.Range(0.9, 1.1)`
- victory if `finalPlayerPower >= finalEnemyPower`
- defeat otherwise

Reward rules:

- victory: full calculated EXP and full Arena Tokens
- defeat: 25% calculated EXP, rounded to int, and 0 Arena Tokens

EXP reward multiplier:

- enemy power <= player power * 0.7: x0.5
- enemy power <= player power * 0.9: x0.75
- enemy power <= player power * 1.1: x1.0
- enemy power <= player power * 1.3: x1.25
- enemy power > player power * 1.3: x1.5

## Important Rules

- Do not rename existing scenes, GameObjects, scripts, serialized fields, or inspector references unless absolutely necessary.
- Do not break serialized references.
- Prefer serialized references / inspector bindings.
- Use name lookup only as fallback safety logic.
- Keep changes modular, safe, and reversible.
- Keep mobile-first UX in mind.
- Use large touch-friendly controls.
- Avoid desktop-style UI flows.
- Do not create duplicate canvases.
- Do not create duplicate systems.
- Do not add extra confirmation windows for normal building entry.
- `InfoWindow` should only be used for locked buildings, unavailable features, level requirements, tutorial hints, or coming-soon messages.
- `CharacterPanel` should remain stable.
- `BarracksWindow` internals should not be touched unless required for safe refresh or bugfixing.
- Keep everything inside `MainMenu` for now.
- Do not create separate Unity scenes for locations yet.

## Important Bug Fix

Bug:

After level-up from Arena EXP reward, displayed combat power became incorrect, as if equipment bonuses were not applied. Entering Barracks and unequipping one item caused combat power to recalculate correctly with remaining equipped items.

Root cause:

`PlayerStats.RecalculateStats()` resets equipment bonus stats to zero. This is correct for a clean stat recalculation, but after Arena level-up the equipment bonuses must be reapplied.

The first Arena reward implementation tried to find `EquipmentManager` with `FindFirstObjectByType<EquipmentManager>()`. However, `EquipmentManager` lives on inactive `BarracksWindow`, so it could be missed. The fallback called `playerStats.RecalculateStats()` directly, leaving the player with base/native stats only.

Fix:

`ArenaWindowUI` now finds `EquipmentManager` including inactive scene objects via:

```csharp
Resources.FindObjectsOfTypeAll<EquipmentManager>()
```

It selects the manager whose `manager.playerStats == playerStats`, then calls:

```csharp
equipmentManager.RefreshPlayerStats()
```

This safely recalculates native stats, resets equipment bonuses, and reapplies equipped item bonuses exactly once through the existing equipment flow.

Double-apply is avoided because Arena does not call `ApplyItemStats` directly.

## Current Recommended Next Steps

Recommended next development direction:

1. Save System v1
2. Persist `PlayerStats`
3. Persist Arena Tokens
4. Persist current EXP / level / required EXP
5. Persist manual stat allocations
6. Persist equipment state
7. Persist inventory item instances
8. Then improve Arena balance and enemy generation

After persistence is stable, good follow-up systems:

- Arena enemy generation by player power
- Arena rank progression
- Arena token shop
- first real combat prototype
- combat log or lightweight auto-battle visualization
