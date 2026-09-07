# AzzyAI - Kimi Edition

Fork of AzzyAI optimized exclusively for **Kimi puppet** (Illusionist class) in Ragnarok Online's "Echoes of Morroc" server.

**Latest Features:**
- ✅ **Blueprint Combo System**: Visual node-based AI combos with triggers (OnIdle/OnChase/OnAttack)
- ✅ **Advanced Logic Gates**: AND, OR, XOR, NAND, NOR, NOT for complex condition chains
- ✅ **Dynamic Conditions**: KimiHP, OwnerHP, MobCount, Distance, SP, OwnerSP
- ✅ **Kimi-Only Optimization**: Removed legacy homunculus/mercenary code
- ✅ **Full C# GUI**: AzzyAIConfig for visual configuration and combo building

## Quick Start

### Installation
1. Copy `USER_AI/` folder contents to your RO client: `RagnarokOnline/AI_sakray/USER_AI/`
2. Restart RO client or use `/reloadai` command
3. AI automatically loads from `AI.lua`

### Configuration
**GUI Method (Recommended)**
1. Open `GUI/AzzyAIConfig/bin/Release/AzzyAIConfig.exe`
2. Configure Kimi behavior, skills, and tactics
3. Save - automatically updates `H_Config.lua`

**Manual Method**
- Edit `USER_AI/H_Config.lua` for behavior thresholds
- Edit `USER_AI/H_Tactics.lua` for per-monster tactics
- Edit `USER_AI/H_Extra.lua` for custom logic extensions

## Project Structure

```
├── USER_AI/                    # Runtime AI scripts (copy to RO client)
│   ├── AI.lua                  # Entry point
│   ├── AI_main.lua             # Core state machine & blueprint engine
│   ├── H_Config.lua            # Configuration (HP thresholds, behaviors)
│   ├── H_Tactics.lua           # Monster-specific tactics
│   ├── H_Extra.lua             # Custom extensions & skill levels
│   ├── H_SkillList.lua         # Kimi skill definitions
│   └── ...
├── GUI/AzzyAIConfig/           # C# GUI source code
│   ├── MainForm.cs             # Main config window
│   ├── KimiConf.cs             # Kimi config properties
│   ├── ComboTactControl.cs     # Blueprint combo editor
│   ├── AzzyAIConfig.sln        # Visual Studio solution
│   └── bin/Release/AzzyAIConfig.exe  # Compiled executable
├── DEFAULT/                    # Original default AI (legacy)
└── .gitignore                  # Git exclusions
```

## Building GUI from Source

**Requirements:**
- Visual Studio 2022 or later
- .NET Framework 4.7.2+

**Steps:**
1. Open `GUI/AzzyAIConfig/AzzyAIConfig.sln` in Visual Studio
2. Build → Rebuild Solution
3. Output: `GUI/AzzyAIConfig/bin/Release/AzzyAIConfig.exe`

## Supported Kimi Types

All 4 Kimi variations supported with customizable tactics:
- **Ward Kimi** (Type 1): Tank/Support - High DEF/HP
- **Occult Kimi** (Type 2): Magic DPS - High SP/Magic
- **Agile Kimi** (Type 3): Physical DPS - High ATK Speed
- **Raging Kimi** (Type 4): Balanced DPS - Versatile stats

See `USER_AI/H_Config.lua` for type-specific defaults and customization.

## Key Kimi Skills

All 4 types have access to 7 core skills:
- `Illusion of Claws` (8009) - Physical ASPD-based
- `Illusion of Breath` (8024) - Magic single-target  
- `Illusion Crusher` (8031) - Dash + stun
- `Illusion of Light` (8034) - AoE Holy magic
- `Chaotic Heal` (8014) - Random healing
- `Warm Def` (8006) - Defense buff + hide
- `Body Double` (8022) - Death prevention

Configure skill levels in `USER_AI/H_Extra.lua`

## Development

- **Lua Version**: 5.1 (client-injected game API)
- **Blueprint Engine**: Full visual combo system with 15+ node types
- **Git**: Use `.gitignore` to exclude build artifacts and runtime data

## Debugging

Enable logging in `USER_AI/H_Extra.lua`:
```lua
LogEnable["AAI_SKILLFAIL"] = 1  -- Skill execution failures
LogEnable["AAI_CostSP"] = 1     -- SP consumption
LogEnable["AAI_MOBCOUNT"] = 1   -- Mob counting
```

Logs write to RO folder as `AAI_*.log`

## Based On

- **Original**: AzzyAI by Dr. Azzy
- **Kimi Fork**: Refactored for Illusionist puppet only
- **Server**: Echoes of Morroc (eom.world)
