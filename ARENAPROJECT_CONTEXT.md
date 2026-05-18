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

## Current Project State After Equipment Persistence

The project currently compiles at the script level and the main gameplay loop is:

- player starts from saved or default progression data
- player inventory is restored from local JSON if saved inventory data exists
- equipped slots are restored from saved item instance IDs
- equipment bonuses are reapplied through `EquipmentManager.RefreshPlayerStats()`
- CharacterPanel progression UI refreshes from `PlayerStats`
- Barracks inventory/equipment UI continues to use existing item instances and equipment state
- Arena rewards still apply EXP and Arena Tokens through `PlayerStats`

Current known limitations:

- only one local save profile exists
- there is no explicit New Game / Reset UI button yet
- save format has no version/migration field yet
- item persistence depends on every real item having a stable `ItemData.itemId`
- inventory/equipment persistence covers owned items and equipped slots, but not generated loot tables, shops, or item affixes yet

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

### PlayerSaveManager

`PlayerSaveManager` is Save System v1 for local player progression, inventory, and equipment.

Current persistence approach:

- JSON file
- stored in `Application.persistentDataPath`
- file name: `arena_player_progression.json`

Saved progression fields:

- level
- current EXP
- required EXP (`maxExp`)
- available stat points
- Arena Tokens
- allocated stat points

The allocated stat fields are saved together with available stat points so spent points do not disappear after loading.

Saved inventory fields:

- owned item `instanceId`
- owned item stable `itemId`

Saved equipment fields:

- equipped item `instanceId` by equipment slot
- slots include helmet, weapon1, weapon2, armor, gloves, belt, legs, boots, ring1-ring4, amulet, and artifact

New player defaults:

- level starts at 1
- current EXP starts at 0
- Arena Tokens start at 0
- starting required EXP (`maxExp`) is 100

Save triggers:

- gaining EXP through `PlayerStats.AddExperience(int amount)`
- gaining Arena Tokens through `PlayerStats.AddArenaTokens(int amount)`
- level-up through `PlayerStats.LevelUp()`
- manual stat allocation through `PlayerStats.TryAllocateStat(PlayerStatType statType)`
- equipping an item through `EquipmentManager`
- unequipping an item through `EquipmentManager`
- inventory add/remove through `PlayerInventory`

Load flow:

- `PlayerStats.Start()` attempts to load progression from JSON
- loaded values are validated for safe minimums
- stats are recalculated
- `PlayerStats` then searches inactive scene objects for a matching `EquipmentManager`
- if a matching manager is found, `EquipmentManager.RefreshPlayerStats()` reapplies equipment bonuses through the existing equipment flow
- `PlayerInventory.Start()` normalizes missing runtime item instance IDs
- if saved inventory data exists, `PlayerInventory` rebuilds owned item instances from `ItemDatabase`
- `EquipmentManager.RestoreEquipmentFromSave()` restores equipped slots from saved item instance IDs
- restored equipment is refreshed once through the existing equipment stat flow

Development reset:

- `PlayerStats` has a context menu method: `Clear Saved Progression`
- this deletes the local JSON save file

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
- Save System v1 for player progression
- new player progression defaults: level 1, EXP 0, Arena Tokens 0
- persistent level/current EXP/required EXP
- persistent Arena Tokens
- persistent available and allocated stat points
- persistent inventory item instances
- persistent equipped item slots
- stable item IDs for saved inventory
- context menu reset for local progression save

## Progression And Save System v1

Save System v1 is intentionally small and local-only. It now stores player progression plus the first version of inventory/equipment state.

Implemented:

- `PlayerSaveManager`
- JSON save file in `Application.persistentDataPath`
- progression load during `PlayerStats.Start()`
- progression save after EXP gain, token gain, level-up, and stat allocation
- inventory item instance persistence
- equipped item slot persistence
- stable item IDs through `ItemData.itemId`
- inventory/equipment load during `PlayerInventory.Start()`
- equipment restore through `EquipmentManager.RestoreEquipmentFromSave`
- save after equip / unequip
- save after inventory add / remove
- development reset through `PlayerStats` context menu

Not implemented yet:

- save slots
- cloud save
- explicit new-game UI
- save migration/versioning
- encryption or anti-cheat protection

Current limitation:

The save system is still one local player profile only. It has no explicit new-game UI, save slots, migration/version field, cloud save, encryption, or anti-cheat protection.

Important safety note:

After loading progression, equipment bonuses must still be applied through `EquipmentManager.RefreshPlayerStats()`. Do not apply equipment item bonuses directly from save code.

## Equipment And Inventory Persistence v1

Inventory source of truth:

- `PlayerInventory.ownedItems`
- each owned item is an `ItemInstance`
- each `ItemInstance` stores `instanceId` and `itemData`

Item ID strategy:

- `ItemData.itemId` is the stable save identifier for item database entries.
- Existing item assets now have explicit IDs:
  - `Helmet_Long`
  - `Helmet_Lord`
  - `Sword_Pain`
  - `Sword_Rage`
- `ItemDatabase.GetStableItemId(item)` uses `item.itemId` first and falls back to the asset name.
- `ItemDatabase.GetItemById(itemId)` resolves saved item IDs back to `ItemData`.

Saved inventory format:

```json
"inventoryItems": [
  {
    "instanceId": "runtime-item-instance-guid",
    "itemId": "Sword_Pain"
  }
]
```

Equipment source of truth:

- `EquipmentManager` private equipped slot fields:
  - `equippedHelmet`
  - `equippedWeapon1`
  - `equippedWeapon2`
  - `equippedArmor`
  - `equippedGloves`
  - `equippedBelt`
  - `equippedLegs`
  - `equippedBoots`
  - `equippedRing1` through `equippedRing4`
  - `equippedAmulet`
  - `equippedArtifact`

Saved equipment format:

```json
"equipment": {
  "helmet": "saved-item-instance-id",
  "weapon1": "saved-item-instance-id",
  "weapon2": "saved-item-instance-id",
  "armor": "",
  "gloves": "",
  "belt": "",
  "legs": "",
  "boots": "",
  "ring1": "",
  "ring2": "",
  "ring3": "",
  "ring4": "",
  "amulet": "",
  "artifact": ""
}
```

Load order:

1. `PlayerStats.Start()` loads progression.
2. `PlayerStats` recalculates base/native stats.
3. `PlayerInventory.Start()` normalizes missing `ItemInstance.instanceId` values.
4. `PlayerInventory` loads saved inventory item instances if save data exists.
5. `EquipmentManager.RestoreEquipmentFromSave()` restores equipped slots by saved `instanceId`.
6. `EquipmentManager.RefreshPlayerStats()` recalculates native stats and reapplies equipped item bonuses once.
7. UI refresh is handled through the existing equipment refresh path.

Duplicate and double-apply protection:

- Inventory load clears `ownedItems` only when saved inventory data exists.
- Duplicate saved `instanceId` entries are skipped.
- Equipped slots are restored only by looking up items already loaded in `PlayerInventory`.
- Saved equipment entries with missing items or wrong item type are ignored with `Debug.LogWarning`.
- Equipment restore assigns equipped fields and refreshes stats once.
- Save/load code does not call `PlayerStats.ApplyItemStats` directly.

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

1. Add explicit New Game / Reset Progress debug UI
2. Add save versioning / migration guard before save data grows further
3. Add Arena enemy generation based on player combat power and rank
4. Add Arena rank progression and token economy
5. Add item rewards / loot drops and save them through `PlayerInventory.AddItem`

After persistence is stable, good follow-up systems:

- Arena token shop
- first real combat prototype
- combat log or lightweight auto-battle visualization
