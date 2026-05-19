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

## Current Project State After Combat Fighter Puppet v1

The project currently compiles at the script level and the main gameplay loop is:

- player starts from saved or default progression data
- player inventory is restored from local JSON if saved inventory data exists
- equipped slots are restored from saved item instance IDs
- equipment bonuses are reapplied through `EquipmentManager.RefreshPlayerStats()`
- CharacterPanel progression UI refreshes from `PlayerStats`
- Barracks inventory/equipment UI continues to use existing item instances and equipment state
- ArenaWindow opens from the city hub and shows three generated selectable enemies
- Arena opponents are regenerated from current player combat power when ArenaWindow opens
- Arena has a free `Refresh Opponents` button that regenerates the enemy list
- EnemyInfoPanel shows the generated combat details for the selected opponent
- selecting Fight runs Combat Simulation v1 through `CombatSimulator`
- after Fight, ArenaWindow opens `CombatPlaybackPanel` before showing ResultPanel
- Combat Playback visually plays already-calculated combat events and does not recalculate the fight
- Combat Playback v1 now uses pose-based fighter placeholders through `CombatFighterPuppetUI`
- `CombatFighterPuppetUI` keeps a hidden modular puppet hierarchy for future body-part sprites, but visible v1 playback uses pose states
- player can choose Arena combat stance in ArenaWindow before fighting
- selected player stance is persisted in the local JSON save
- Arena ResultPanel shows Victory, Defeat, or Draw
- Arena ResultPanel shows stance, remaining HP, damage dealt, crits, dodges, blocks, EXP, and Arena Tokens
- Battle Log opens inside ArenaWindow and shows the full round-by-round combat log
- Arena rewards still apply EXP and Arena Tokens through `PlayerStats`
- Arena rewards still save through the existing progression/save system

Current known limitations:

- only one local save profile exists
- there is no explicit New Game / Reset UI button yet
- save format has no version/migration field yet
- item persistence depends on every real item having a stable `ItemData.itemId`
- inventory/equipment persistence covers owned items and equipped slots, but not generated loot tables, shops, or item affixes yet
- Combat Playback v1 exists as a side-view Arena layout with pose-based fighter placeholders and lightweight UI motion only
- player combat stance selection exists and persists, but stance choice is still a simple ArenaWindow control with no deeper character build integration yet
- shield blocking is supported in code but no shield item type/data exists yet
- Arena enemies are generated from current player combat power, but are not saved yet
- Arena rank progression is intentionally not implemented yet

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
- contains three generated enemy cards
- each enemy has a portrait placeholder, name, level, combat power, EXP reward, Arena Tokens reward, Info button, and Fight button
- enemy data includes HP, attack, defense, armor, agility, reaction, crit chance, base EXP, token reward, and description
- `Refresh Opponents` regenerates the current easy/balanced/hard opponent set for free
- `EnemyInfoPanel` opens inside ArenaWindow for selected enemy details and displays generated combat stats
- compact player stance selector lives inside ArenaWindow
- `CombatPlaybackPanel` opens after pressing Fight and before ResultPanel
- `ResultPanel` opens inside ArenaWindow after Combat Simulation v1
- `BattleLogPanel` opens inside ArenaWindow from the ResultPanel Battle Log button
- no real animated combat scene exists yet

Important ArenaWindow responsibilities:

- build and refresh Arena UI panels
- generate Arena enemy data from current player combat power
- convert generated Arena enemy data into `CombatSimulator.FighterData`
- convert current `PlayerStats` into `CombatSimulator.FighterData`
- call `CombatSimulator.Simulate(...)`
- pass calculated combat events to `CombatPlaybackUI`
- calculate and apply rewards based on combat outcome
- show and save player-selected combat stance
- visually highlight the selected stance button
- load saved stance on ArenaWindow initialization / enable
- keep ResultPanel, EnemyInfoPanel, and BattleLogPanel inside ArenaWindow
- keep CombatPlaybackPanel inside ArenaWindow
- keep Continue behavior scoped to closing only ResultPanel
- keep Battle Log behavior scoped to opening/closing only BattleLogPanel
- do not apply stat/equipment/save logic directly outside existing `PlayerStats` and save flows

### CombatSimulator

`CombatSimulator` owns Arena Combat Simulation v1.

Responsibilities:

- simulate Arena fights outside `ArenaWindowUI`
- run turn-based rounds
- resolve simultaneous attacks
- support body zones:
  - Head
  - Body
  - Left Arm
  - Right Arm
  - Legs
- support combat stances:
  - Aggressive
  - Standard
  - Defensive
- support weapon block now and shield block later
- calculate dodge, block, crit, damage mitigation, and simple counterattacks
- produce a full readable combat log

Important behavior:

- max fight duration is 20 rounds
- both fighters can die in the same round, causing Draw
- if nobody dies after 20 rounds, higher remaining HP percentage wins
- close remaining HP percentages produce Draw
- body-zone damage modifiers are applied inside the simulator
- dodge is based mainly on agility
- counterattack chance is based mainly on reaction
- crit chance uses explicit crit chance when available, otherwise safe luck-based fallback
- armor/defense mitigation is applied before block mitigation
- weapon block reduces roughly 50% damage
- shield block reduces most damage and is prepared for future shield items
- CombatSimulator does not apply rewards or save data directly
- CombatSimulator returns structured playback events as calculation output, but does not play visuals
- `ArenaWindowUI` remains responsible for UI flow and reward application
- `CombatResult` returns combat statistics for ResultPanel:
  - damage dealt
  - crits
  - dodges
  - blocks
  - remaining HP
  - player/enemy stances

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
- selected Arena combat stance

The allocated stat fields are saved together with available stat points so spent points do not disappear after loading.

Selected Arena stance persistence:

- stored as string field `selectedArenaStance`
- valid values currently match `CombatStance` enum names:
  - `Aggressive`
  - `Standard`
  - `Defensive`
- missing or unknown saved stance safely falls back to `Standard`
- saving stance preserves existing progression, inventory, and equipment save data.

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
- selecting Arena combat stance through `ArenaWindowUI`

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
- `ArenaWindowUI` loads selected Arena combat stance from `PlayerSaveManager`
- missing/old stance save data falls back safely to `Standard`

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
- generated Arena enemy cards
- Arena enemy generation from player combat power
- free Arena opponent refresh
- Arena `EnemyInfoPanel`
- Arena Combat Simulation v1
- Arena Combat Playback v1
- Arena combat stance selector UI
- Arena `ResultPanel`
- Arena `BattleLogPanel`
- Arena `CombatPlaybackPanel`
- round-by-round combat log
- combat result stats: remaining HP, damage dealt, crits, dodges, blocks
- Victory / Defeat / Draw Arena outcomes
- Draw reward handling
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
- selected Arena combat stance persistence
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
- armor
- agility
- reaction
- crit chance
- base EXP reward
- Arena Tokens reward
- short description

Arena Enemy Generation v1:

- enemies are generated in `ArenaWindowUI`
- generation happens at runtime; generated enemies are not authored as static scene/prefab data
- three enemies are generated at a time:
  - Easy: roughly 70-85% of current player combat power
  - Balanced: roughly 95-110% of current player combat power
  - Hard: roughly 120-145% of current player combat power
- generated enemy levels stay near player level:
  - Easy: player level -1 to player level
  - Balanced: player level to player level +1
  - Hard: player level +1 to player level +3
  - generated level never goes below 1
- generated combat stats are derived from generated combat power:
  - HP: about 32-46% of combat power
  - Attack: about 4.0-6.2% of combat power
  - Defense: about 1.4-2.5% of combat power
  - Armor: about 90-125% of generated defense
  - Agility and Reaction scale from enemy level plus combat power
  - Crit chance scales by difficulty
- generated enemies are not saved in Save System v1
- enemies can regenerate when ArenaWindow opens or when the player presses `Refresh Opponents`
- `Refresh Opponents` is free for now
- refreshing opponents closes temporary Arena overlays such as EnemyInfoPanel, ResultPanel, and BattleLogPanel, then rebuilds the three enemy cards
- EnemyInfoPanel now shows generated details:
  - level
  - combat power
  - HP
  - attack
  - defense / armor
  - agility
  - reaction
  - crit chance
  - calculated EXP reward
  - Arena Tokens reward
  - generated difficulty description
- rank progression is not involved in enemy generation yet

Combat Simulation v1:

- fights are turn-based by rounds
- both fighters act simultaneously each round
- maximum duration is 20 rounds
- attacks target one random body zone
- supported body zones: Head, Body, Left Arm, Right Arm, Legs
- dodge uses agility
- counterattack chance uses reaction after dodge or block
- crit chance uses configured enemy crit chance or player luck/rage fallback
- mitigation uses defense, armor, body zone modifier, and block modifier
- weapon block reduces roughly half of incoming damage
- shield block support exists in `CombatSimulator`, but no shield item type exists yet
- minimum landed damage is always at least 1
- if nobody dies after 20 rounds, remaining HP percentage decides the winner
- if remaining HP percentages are close enough, the result is Draw
- if both fighters die in the same round, the result is Draw

Combat Playback v1:

- lives inside `ArenaWindow`
- uses `CombatPlaybackUI`
- opens after pressing Fight and before ResultPanel
- uses already-calculated `CombatSimulator.CombatPlaybackEvent` data
- does not recalculate combat
- is not intended to be a popup/card modal
- keeps the existing player `CharacterPanel` visible on the left side of the screen
- transforms the right Arena area into a side-view battle layout
- shows a central battlefield/stage between the left CharacterPanel area and the right EnemyPanel
- shows a mirrored `EnemyPanel` on the right side
- EnemyPanel shows enemy portrait placeholder, enemy name, weapon slots, HP bar, and MP bar
- EnemyPanel does not show EXP
- shows player and enemy pose-based fighter placeholders on the central battlefield facing each other
- uses `CombatFighterPuppetUI` for visible pose states instead of rotating placeholder limbs
- current visible pose states are:
  - Idle
  - AttackLeft
  - AttackRight
  - Block
  - Dodge
  - Hit
  - CritHit
  - Death
- real pose sprites can later be assigned per state:
  - idleSprite
  - attackLeftSprite
  - attackRightSprite
  - blockSprite
  - dodgeSprite
  - hitSprite
  - critHitSprite
  - deathSprite
- each runtime fighter root is built as:
  - PlayerFighterRoot / EnemyFighterRoot
  - PoseRoot
  - ModularPuppetRoot
  - Body
  - Head
  - LeftArm
  - RightArm
  - LeftWeapon
  - RightWeapon
  - Legs
- `PoseRoot` is the visible layer for Combat Playback v1
- `ModularPuppetRoot` is hidden for now and kept as future architecture only
- visible pose placeholders are temporary UI `Image` placeholders only
- real pose sprites can later replace the placeholder pose Images without changing combat calculation
- real body-part sprites can later replace the hidden modular child Images if the project returns to full 2D puppet animation
- fighter structure should continue evolving toward modular 2D puppet characters rather than single static PNG characters
- fighter puppet structure supports separate visual parts:
  - Body
  - Head
  - LeftArm
  - RightArm
  - LeftWeapon
  - RightWeapon
  - Legs
- attack playback switches to AttackLeft/AttackRight pose and lunges FighterRoot toward the target
- block playback switches to Block pose with a small guard pulse
- dodge playback switches to Dodge pose and moves FighterRoot backward
- hit playback switches to Hit pose and shakes/flashes the FighterRoot
- crit playback switches to CritHit pose and uses a stronger flash/shake
- death playback switches to Death pose at the end of the round if HP reaches 0
- counter playback reuses the attack pose flow and is identified by `COUNTER` floating text
- future animation may expand either toward authored pose sprites or full independent body/arm/weapon motion
- shows player and enemy names plus stances near the battlefield fighters
- plays events in sequence with short delays
- animates enemy HP bar and lightweight battlefield HP text as damage happens
- uses simple UI movement for center-stage attacks
- flashes the hit/dodging battlefield fighter, and flashes the enemy portrait when the enemy is hit
- shows floating combat text near the hit/dodging fighter:
  - damage numbers
  - BLOCK
  - DODGE
  - CRIT
  - COUNTER
- has a `Skip` button
- Skip appears after 2 seconds or after the first hit/dodge exchange
- pressing Skip ends playback immediately and shows ResultPanel
- ResultPanel and Battle Log remain available after playback

Combat stances:

- Aggressive: attacks with both hands and does not block
- Standard: attacks once and blocks 2 random body zones
- Defensive: 75% chance to full block 4 random zones, 25% chance to attack once

Player stance selection UI:

- lives inside `ArenaWindow`
- defaults to `Standard`
- uses three compact segmented buttons:
  - Aggressive
  - Standard
  - Defensive
- selected stance is visually highlighted
- selected stance is saved through `PlayerSaveManager.selectedArenaStance`
- selected stance is restored when ArenaWindow loads
- old save files without stance data fall back to `Standard`
- selecting a stance does not start combat by itself
- Fight buttons use whichever stance is currently selected

ResultPanel combat summary:

- no longer emphasizes old final power values
- shows player stance and enemy stance
- shows remaining HP for both fighters
- shows total damage dealt by both fighters
- shows compact `C/D/B` stats:
  - crits
  - dodges
  - blocks
- still shows EXP gained and Arena Tokens gained
- Continue still closes only ResultPanel and returns to the Arena enemy list

Reward rules:

- victory: full calculated EXP and full Arena Tokens
- defeat: 25% calculated EXP, rounded to int, and 0 Arena Tokens
- draw: 50% calculated EXP and 25% Arena Tokens, rounded to int

Combat log behavior:

- each fight generates a readable round-by-round log
- the log includes stances, attack/block plans, target zones, dodges, blocks, crits, counters, damage, round-end HP, and final result
- rounds are visually separated in text
- log entries use lightweight tags:
  - `[STANCE]`
  - `[DODGE]`
  - `[BLOCK]`
  - `[CRIT]`
  - `[COUNTER]`
- ResultPanel has a `Battle Log` button
- BattleLogPanel is opened inside ArenaWindow
- BattleLogPanel closes independently and does not close ResultPanel or ArenaWindow
- Battle Log still uses the full text log generated by `CombatSimulator`
- Combat Playback uses structured playback events from the same calculated fight

EXP reward multiplier:

- enemy power <= player power * 0.7: x0.5
- enemy power <= player power * 0.9: x0.75
- enemy power <= player power * 1.1: x1.0
- enemy power <= player power * 1.3: x1.25
- enemy power > player power * 1.3: x1.5

Generated reward scaling:

- generated base EXP scales from enemy level, enemy combat power, and difficulty
- Easy enemies use lower EXP/token scaling
- Balanced enemies use normal EXP/token scaling
- Hard enemies use higher EXP/token scaling
- the existing EXP reward multiplier still applies after base EXP generation

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

1. Test pose-based Combat Playback v1 timing/readability on mobile aspect ratios
2. Replace temporary pose placeholders with real authored fighter pose sprites when art direction is ready
3. Add explicit New Game / Reset Progress debug UI
4. Add save versioning / migration guard before save data grows further
5. Test and tune generated Arena enemy balance against real saved characters
6. Add Arena rank progression and token economy later
7. Add Arena combat balance pass after testing real outcomes

Good follow-up systems:

- Arena token shop
- item rewards / loot drops saved through `PlayerInventory.AddItem`
- combat balance pass for dodge, block, crit, and damage formulas
- improve Combat Playback with modular puppet parts, VFX, pacing controls, or replay support
- shield item type / shield equipment slot if the design needs shield blocking
