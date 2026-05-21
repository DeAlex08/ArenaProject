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
- `MarketWindow`
- `TrialTowerWindow`
- `StatsWindow`
- `InfoWindow`
- `EquipWindow`

`CharacterPanel` is the stable left-side player panel. It contains the portrait, resource bars, equipped weapon visuals, level/power text, and an integrated stats view.

`MainLocationPanel` is the right-side city hub. It contains clickable/touchable building areas such as Barracks, Arena, Market, and Tower of Trials.

## Current Project State After Combat Balance v1 And Tier 1 Market Gear

The project currently compiles at the script level and the main gameplay loop is:

- player starts from saved or default progression data
- player inventory is restored from local JSON if saved inventory data exists
- equipped slots are restored from saved item instance IDs
- equipment bonuses are reapplied through `EquipmentManager.RefreshPlayerStats()`
- CharacterPanel progression UI refreshes from `PlayerStats`
- Barracks inventory/equipment UI continues to use existing item instances and equipment state
- MarketWindow opens from the city hub and provides a category-based Arena Token shop
- MarketWindow supports Buy and Sell modes
- MarketWindow sells static Tier 1 Level 1-10 gear items and adds purchases to `PlayerInventory`
- MarketWindow can sell unequipped player-owned items back for partial Arena Tokens
- purchased Market items persist through the existing inventory/equipment save-load flow
- ArenaWindow opens from the city hub and shows three generated selectable enemies
- Arena opponents are regenerated from current player combat power when ArenaWindow opens
- Arena has a free `Refresh Opponents` button that regenerates the enemy list
- EnemyInfoPanel shows the generated combat details for the selected opponent
- selecting Fight runs Combat Balance v1 through the shared `CombatSimulator`
- after Fight, ArenaWindow opens `CombatPlaybackPanel` before showing ResultPanel
- Combat Playback visually plays already-calculated combat events and does not recalculate the fight
- Combat Playback v1 now uses pose-based fighter sprites through `CombatFighterPuppetUI`
- the player fighter uses the imported knight pose sheet
- the enemy fighter uses the imported orc pose sheet
- `CombatFighterPuppetUI` keeps a hidden modular puppet hierarchy for future body-part sprites, but visible v1 playback uses pose states
- Combat Playback now uses a daytime fantasy arena battlefield background with a dark readability overlay
- Combat Playback fighters are scaled up and positioned lower so they stand closer to the arena floor
- basic slash VFX plays on attacks
- basic impact VFX plays on hits, with stronger impact VFX for crits
- player can choose Arena combat stance in ArenaWindow before fighting
- selected player stance is persisted in the local JSON save
- Arena ResultPanel shows Victory, Defeat, or Draw
- Arena ResultPanel shows stance, remaining HP, damage dealt, crits, dodges, blocks, EXP, and Arena Tokens
- Battle Log opens inside ArenaWindow and shows the full round-by-round combat log
- Arena rewards still apply EXP and Arena Tokens through `PlayerStats`
- Arena rewards still save through the existing progression/save system
- Tower of Trials v1 opens from the city hub Tower building through `TrialTowerWindow`
- Tower of Trials is the first PvE progression system
- Tower uses 10 floor cards with locked, available, and cleared states
- clearing Tower floors unlocks the next floor and persists through the existing local JSON save
- Tower fights reuse the shared Combat Balance v1 `CombatSimulator`, `CombatPlaybackUI`, ResultPanel-style flow, Battle Log, and the stable left `CharacterPanel`
- Tower enemies scale by floor power/stats instead of current player power
- Tower rewards EXP and Arena Tokens, with full first-clear rewards and reduced repeat-clear rewards

Current live progression loop:

- Arena -> earn Arena Tokens and EXP through generated PvP-style duels
- Market -> spend Arena Tokens on Tier 1 gear upgrades or sell unequipped items
- Barracks -> equip and compare gear, refresh player combat power
- Tower of Trials -> push PvE floors for progression, EXP, and additional Arena Tokens

Current known limitations:

- only one local save profile exists
- there is no explicit New Game / Reset UI button yet
- save format has no version/migration field yet
- item persistence depends on every real item having a stable `ItemData.itemId`
- inventory/equipment persistence covers owned items and equipped slots, but not generated loot tables, shops, or item affixes yet
- MarketWindow v2 is functional and now uses the static Tier 1 Level 1-10 gear pool, but still needs visual polish against BarracksWindow
- no loot drop system exists yet
- no procedural item generation exists yet
- Tower currently rewards only EXP and Arena Tokens
- Tower presentation still needs stronger visual identity, boss floors, floor themes, and PvE-specific enemy art
- Combat Playback v1 exists as a side-view Arena layout with pose-based fighter sprites, a daytime arena battlefield background, lightweight UI motion, and first-pass slash/impact VFX
- EnemyPanel uses a custom fantasy frame and orc portrait, but HP/MP alignment still needs later polish
- player combat stance selection exists and persists, but stance choice is still a simple ArenaWindow control with no deeper character build integration yet
- shield blocking is supported in code but no shield item type/data exists yet
- Defense still exists in current combat code and enemy data, but the long-term design direction is to remove Defense and make body-zone Armor the main mitigation stat
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

The current Barracks, Arena, Market, and Tower of Trials building touch areas use this flow.

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
- `ResultPanel` opens inside ArenaWindow after Combat Balance v1
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

### MarketWindow

`MarketWindow` is the first shop/economy location window.

Current Market v2 features:

- lives inside the existing `MainMenu` scene
- opens directly from the city hub Market building through `LocationNavigationController` / `BuildingButton`
- inactive by default until opened
- uses Arena Tokens as the purchase currency
- shows current Arena Tokens
- uses a Barracks-inspired category layout
- has Buy and Sell modes
- has category buttons for:
  - Helmets
  - Weapons
  - Armor
  - Gloves
  - Belts
  - Legs
  - Boots
  - Rings
  - Amulets
  - Artifacts
- Buy mode shows shop items filtered by selected category
- Sell mode shows player-owned inventory items filtered by selected category
- item cards show item name, type/category, rarity/level when available, stats, price, and the relevant Buy/Sell button
- current Buy mode offers a static Tier 1 Level 1-10 gear pool
- old placeholder/test items are no longer part of the Market buy pool
- Tier 1 gear assets live under `Assets/_Project/Items/Tier1`
- 66 Tier 1 `ItemData` assets currently exist
- Tier 1 archetypes:
  - Berserker: attack-related stats plus Rage
  - Gambler: attack-related stats plus Luck
  - Duelist: Agility plus Reaction, with lighter weapon attack
- Tier 1 rarities:
  - Common
  - Rare
  - Epic
- Uncommon, Legendary, and Mythic Tier 1 market items are intentionally not implemented yet
- Tier 1 Market slots:
  - Weapon
  - Helmet
  - Armor
  - Gloves
  - Boots
  - Ring
  - Amulet
  - Belt
- weapons represent attack through the existing item/stat fields and display as Attack in Market/Barracks item text
- armor pieces represent body-zone armor in display:
  - Helmet -> ArmorHead
  - Armor -> ArmorBody
  - Gloves -> ArmorArms
  - Boots -> ArmorLegs
- current underlying player stat architecture still stores equipment armor as the existing shared `armor` stat
- accessories do not give Attack or Armor
- accessories compensate with stronger or additional combat stats
- percent modifiers are planned for later, but are not implemented yet because `ItemData` does not currently support them safely
- purchases subtract Arena Tokens from `PlayerStats`
- purchases add a new item instance through `PlayerInventory.AddItem`
- purchases persist through the existing progression/inventory save-load flow
- selling removes the unequipped item from `PlayerInventory`
- selling adds Arena Tokens back to `PlayerStats`
- sell price is 50% of the configured shop/base price, rounded down
- currently equipped items are protected from accidental sale
- insufficient currency shows a simple MarketWindow message instead of opening another confirmation window

Important Market notes:

- existing saved players can acquire Tier 1 gear through Market without resetting saves or injecting items into save data
- the current Market is functional and should not be removed while polishing visuals
- future work should make Market cards visually closer to Barracks item cards

### Gear Progression Design

Long-term player max level target:

- Level 80

Planned static/progression gear bands:

- Level 1-10
- Level 11-20
- Level 21-30
- Level 31-40
- Level 41-50
- Level 51-60
- Level 61-70
- Level 71-80

Current implemented gear band:

- Level 1-10 only

Important gear direction:

- do not add Legendary or Mythic item effects until explicitly requested
- do not add procedural loot yet
- do not add percent modifiers until explicitly requested
- future percent modifier candidates include CritDamageBonusPercent, LuckyDodgeBypassBonusPercent, ReactionDamageBonusPercent, and DodgeBypassResistPercent
- future higher gear bands should build on the same static item pool approach first, before procedural item generation

### TowerWindow

`TrialTowerWindow` is the first PvE progression location.

Current Tower of Trials v1 features:

- lives inside the existing `MainMenu` scene
- opens directly from the city hub Tower building through `LocationNavigationController` / `BuildingButton`
- inactive by default until opened
- visually follows the current dark fantasy UI direction
- contains 10 floors
- floor 1 is unlocked by default
- each floor card shows:
  - floor number
  - enemy name
  - portrait placeholder
  - recommended level
  - enemy combat power
  - EXP reward preview
  - Arena Token reward preview
  - status: Locked, Available, or Cleared
  - Enter button
- current available floor is visually highlighted
- locked floors are greyed out and cannot be entered
- cleared floors remain marked after restart
- clearing a floor unlocks the next floor
- Tower progress is saved through `PlayerSaveManager`

Tower combat architecture:

- Tower does not create a second combat system
- Tower reuses `CombatSimulator`
- Tower reuses `CombatPlaybackUI`
- Tower reuses the existing side-view combat playback architecture
- Tower reuses the stable left-side `CharacterPanel`
- Tower uses a ResultPanel-style result flow and Battle Log flow inside `TrialTowerWindow`
- Tower player stance currently reuses the saved Arena stance fallback through `PlayerSaveManager`
- Tower enemies are generated from floor data and scale by floor:
  - HP
  - attack
  - defense
  - armor
  - agility
  - reaction
  - crit chance
  - combat power

Tower reward behavior:

- first clear Victory gives full floor EXP and full Arena Tokens
- repeat clear Victory gives reduced rewards
- repeat clear currently gives 35% EXP and 25% Arena Tokens
- Defeat and Draw do not unlock the next floor
- Tower currently rewards only EXP and Arena Tokens
- Tower loot drops are not implemented yet

### CombatSimulator

`CombatSimulator` owns Combat Balance v1 and remains the shared combat core for Arena and Tower.

Responsibilities:

- simulate fights outside location window UI classes
- run simultaneous round-based combat
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
- calculate dodge, block, crit, lucky hit, armor mitigation, and terminal reaction retaliation
- produce a full readable combat log

Important behavior:

- max fight duration is 20 rounds
- both fighters can die in the same round, causing Draw
- if nobody dies after 20 rounds, higher remaining HP percentage wins
- close remaining HP percentages produce Draw
- body-zone hit weights and damage modifiers are applied inside the simulator
- current attack resolution order is:
  1. body zone
  2. base damage
  3. crit and lucky rolls
  4. dodge
  5. block
  6. armor
  7. HP damage
  8. reaction retaliation
- Crit uses attacker Rage against defender Rage
- Lucky Hit uses attacker Luck against defender Luck
- Dodge uses defender Agility against attacker Agility
- Reaction uses defender Reaction against attacker Reaction
- Lucky Hit bypasses Block completely
- Lucky Hit has a 25% chance to bypass Dodge
- Lucky Crit is supported
- Block fully negates damage when it succeeds
- Reaction is terminal retaliation damage and cannot trigger dodge, block, another reaction, or an infinite chain
- current armor mitigation is implemented in code and still includes Defense in the effective armor threshold
- current design direction is to remove Defense later and make Armor the only mitigation stat
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

### Planned Combat Cleanup

Current combat design decision:

- remove Defense as a long-term combat stat
- keep Armor by body zone
- Armor should become the main mitigation stat
- planned future mitigation formula:
  - `FinalDamage = Damage * (1 - Armor / (Armor + 100))`

Important:

- do not implement this formula until explicitly requested
- for now, this is documented as the next combat cleanup direction only
- avoid adding more features on top of Defense unless a short-term compatibility bridge is required

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
- Tower unlocked floor
- Tower cleared floor list

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
- buying items in MarketWindow through `PlayerInventory.AddItem`
- selling items in MarketWindow through `PlayerInventory.RemoveItem`
- clearing Tower floors through `TowerWindowUI`

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
- `TowerWindowUI` loads unlocked floor and cleared floor list from `PlayerSaveManager`
- missing/old Tower save data falls back safely to floor 1 unlocked and no cleared floors

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
- direct Market opening from city hub
- direct Tower of Trials opening from city hub
- MarketWindow v2
- Market Buy mode
- Market Sell mode
- Market category filtering
- Arena Token shop purchases
- Tier 1 Level 1-10 static Market gear pool
- 66 Tier 1 `ItemData` assets under `Assets/_Project/Items/Tier1`
- Berserker, Gambler, and Duelist starter archetype gear
- Common, Rare, and Epic starter gear rarities
- old placeholder/test items removed from Market buy pool
- Tier 1 equipment purchase flow through Market
- unequipped item sell flow through Market
- equipped item sale protection
- ArenaWindow v1
- generated Arena enemy cards
- Arena enemy generation from player combat power
- free Arena opponent refresh
- Arena `EnemyInfoPanel`
- Combat Balance v1 in shared `CombatSimulator`
- simultaneous round combat shared by Arena and Tower
- Crit, Lucky Hit, Lucky Crit, Dodge, full Block, Armor mitigation, and terminal Reaction retaliation
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
- Tower of Trials v1
- Tower 10-floor PvE progression
- Tower locked / available / cleared floor states
- Tower first-clear rewards
- Tower repeat-clear reduced rewards
- persistent Tower unlocked floor and cleared floor list
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
- purchased Market items persist through existing inventory save data
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
- Tower unlocked/cleared floor persistence
- inventory item instance persistence
- equipped item slot persistence
- stable item IDs through `ItemData.itemId`
- inventory/equipment load during `PlayerInventory.Start()`
- equipment restore through `EquipmentManager.RestoreEquipmentFromSave`
- save after equip / unequip
- save after inventory add / remove
- save after Tower floor clear / unlock
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

Combat Balance v1:

- fights are simultaneous and round-based
- maximum duration is 20 rounds
- both fighters act in the same round
- attacks target weighted body zones
- supported body zones: Head, Body, Left Arm, Right Arm, Legs
- attack order is body zone -> damage -> crit/lucky -> dodge -> block -> armor -> HP -> reaction
- Crit uses Rage against Rage
- Lucky Hit uses Luck against Luck
- Lucky Hit bypasses Block and has a 25% chance to bypass Dodge
- Lucky Crit is supported
- Dodge uses Agility against Agility
- Block fully negates damage
- Reaction uses Reaction against Reaction
- Reaction damage is terminal and cannot chain
- current mitigation still uses Defense plus Armor in code
- long-term direction is to remove Defense and make body-zone Armor the only mitigation stat
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
- the battlefield uses a daytime fantasy arena background image
- a dark transparent overlay sits over the arena background to preserve fighter and floating-text readability
- shows a mirrored `EnemyPanel` on the right side
- EnemyPanel uses a custom fantasy frame and currently shows an orc portrait, enemy name, weapon slots, HP bar, and MP bar
- EnemyPanel does not show EXP
- EnemyPanel HP/MP alignment and bar polish are still known follow-up items
- shows player and enemy pose-based fighters on the central battlefield facing each other
- uses `CombatFighterPuppetUI` for visible pose states instead of rotating placeholder limbs
- player fighter visuals are loaded from the knight pose sheet under `Resources/Combat/FighterPoses`
- enemy fighter visuals are loaded from the orc pose sheet under `Resources/Combat/EnemyOrcPoses`
- enemy portrait visuals currently use the imported orc portrait under `Resources/Combat/UI`
- enemy fighter is mirrored/faced toward the player as needed by the playback layer
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
- visible pose presentation currently uses imported pose sprites, not rotating limb placeholders
- fighter sprites are currently scaled up from the first pose-sprite pass and positioned lower so they sit closer to the arena floor
- future pose sprites can replace the current knight/orc pose Images without changing combat calculation
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
- plays combat as simultaneous round exchanges: both fighters act during the same visual exchange, then HP bars update after the exchange
- animates enemy HP bar and lightweight battlefield HP text as damage happens
- temporarily updates visible player HP through `PlayerStats.currentHp` during playback so the left `CharacterPanel` HP bar decreases live
- restores the original player HP value before ResultPanel/reward flow continues
- uses simple UI movement for center-stage attacks
- plays a short bright slash flash on attack
- plays a small impact flash near the target on hit
- crit impact VFX uses a stronger flash than normal hit impact
- slash and impact VFX placeholders are procedural soft UI sprites rather than hard rectangular flashes
- normal hits trigger lightweight camera shake and a very short hit stop
- critical hits trigger stronger camera shake, stronger hit stop, and a temporary crit screen flash
- blocked hits trigger a small camera shake
- flashes the hit/dodging battlefield fighter, and flashes the enemy portrait when the enemy is hit
- shows floating combat text near the fighter receiving the hit or action result, instead of overlapping in the battlefield center:
  - damage numbers
  - BLOCK
  - DODGE
  - CRIT
  - COUNTER
- simultaneous player/enemy combat text uses separate target positions and small offsets so both read clearly
- floating combat text now animates upward and fades; crit text is slightly larger
- Combat Playback has audio hooks on `CombatPlaybackUI`:
  - slash
  - hit
  - crit
  - block
  - dodge
- if real audio clips are not assigned in the inspector, `CombatPlaybackUI` generates runtime placeholder SFX:
  - `Generated_SlashWhoosh`
  - `Generated_HitThud`
  - `Generated_CritImpact`
  - `Generated_BlockClang`
  - `Generated_DodgeWhoosh`
- generated SFX are temporary and should be replaced later with real authored slash, hit, crit, block, and dodge audio assets
- has a `Skip` button
- Skip appears after 2 seconds or after the first hit/dodge exchange
- pressing Skip ends playback immediately and shows ResultPanel
- Skip does not recalculate combat and stops playback-only animation/audio coroutines before ResultPanel opens
- ResultPanel and Battle Log remain available after playback
- current Combat Playback direction is focused on visual polish, VFX, readability, pacing, and eventual replacement with stronger authored combat art
- current recommended next step for Combat Playback is replacing procedural placeholder SFX with real audio assets and continuing hit-feel tuning
- future Combat Playback polish items:
  - real slash sounds
  - real block clang
  - real dodge whoosh
  - real crit sound
  - smoother HP animation
  - enemy HUD alignment

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

## Tower of Trials v1

Tower of Trials is the first PvE progression system.

Current Tower flow:

1. Player opens `TrialTowerWindow` from the city hub Tower building.
2. Player chooses an available floor from the scrollable floor list.
3. Enter launches combat through the existing `CombatSimulator`.
4. `CombatPlaybackUI` plays the already-calculated fight using the same side-view playback architecture.
5. ResultPanel-style summary appears after playback.
6. Victory on first clear marks the floor as cleared and unlocks the next floor.
7. Tower progress is saved through `PlayerSaveManager`.
8. Continue returns to the Tower floor list.

Tower floor progression:

- total floors: 10
- floor 1 is unlocked by default
- each floor can be Locked, Available, or Cleared
- locked floors are greyed out and cannot be entered
- current available floor is visually highlighted
- cleared floors remain cleared after restart
- clearing floor N unlocks floor N + 1 until floor 10

Tower enemy scaling:

- Tower enemies are currently generated from floor number rather than current player power
- each floor has authored/generated lightweight floor data:
  - enemy name
  - recommended level
  - combat power
  - HP
  - attack
  - defense
  - armor
  - agility
  - reaction
  - crit chance
  - EXP reward
  - Arena Token reward
- Tower enemies are intended to feel PvE-oriented and should later get stronger floor identity, boss floors, and themed enemy art

Tower rewards:

- first-clear Victory gives full floor EXP and full Arena Tokens
- repeat-clear Victory gives reduced rewards
- repeat-clear rewards currently use 35% EXP and 25% Arena Tokens
- Defeat does not unlock the next floor
- Draw does not unlock the next floor
- Tower currently rewards only EXP and Arena Tokens
- Tower does not yet drop items

Tower reuse rules:

- do not create a second combat system for Tower
- continue reusing `CombatSimulator`
- continue reusing `CombatPlaybackUI`
- continue reusing the stable `CharacterPanel`
- continue using the existing progression/save flow for EXP, Arena Tokens, level-up, and equipment stat refresh
- Tower-specific work should mostly live in `TowerWindowUI` and save-data extensions unless a shared abstraction becomes clearly useful

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
- Do not create duplicate combat systems.
- Do not replace `CombatSimulator`.
- Preserve the shared Arena/Tower combat flow.
- Do not redesign `MarketWindow` architecture unless explicitly requested.
- Do not add procedural loot yet.
- Do not add Legendary/Mythic item effects yet.
- Do not add percent modifiers until explicitly requested.
- Preserve save/load compatibility.

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

1. Add Tower Loot v1
2. Add a post-battle loot popup after Tower victories
3. Start with curated/static Tower drops before procedural loot
4. Save dropped items through the existing `PlayerInventory.AddItem` and item persistence flow
5. Add simple floor-based drop tables for early Tower floors
6. Keep future procedural loot as a later system after curated drops feel good
7. Continue polishing the core gameplay loop: Arena -> Tokens, Market -> Gear, Tower -> PvE progression
8. Replace procedural Combat Playback placeholder SFX with real authored audio assets
9. Test pose-based Combat Playback timing/readability on mobile aspect ratios
10. Polish enemy HUD alignment, especially HP/MP bar placement and mirrored frame spacing
11. Tune hit feel further: camera shake intensity, hit stop duration, crit flash strength, and smoother HP animation
12. Give Tower stronger visual identity: floor themes, boss floors, PvE enemy portraits, and floor-specific mood
13. Replace temporary pose placeholders with real authored fighter pose sprites when art direction is ready
14. Add explicit New Game / Reset Progress debug UI
15. Add save versioning / migration guard before save data grows further
16. Test and tune generated Arena and Tower combat balance against real saved characters
17. Add Arena rank progression and token economy later
18. Add Arena combat balance pass after testing real outcomes

Good follow-up systems:

- Tower Loot v1
- post-battle loot popup
- curated/static item drops saved through `PlayerInventory.AddItem`
- procedural item generation later, after curated drops and the PvE loop are proven
- combat balance pass for dodge, block, crit, and damage formulas
- improve Combat Playback with audio, hit feel, camera shake, modular puppet parts, VFX, pacing controls, or replay support
- shield item type / shield equipment slot if the design needs shield blocking

Current product focus:

- prioritize gameplay feel, progression loop clarity, and Tower PvE identity
- avoid adding many disconnected systems before Arena, Market, Barracks, and Tower form a satisfying loop
