# KimiAI for Return to Morroc Refuge

A Windows configuration tool and Lua AI for Ward, Occult, Agile, and Raging Kimi. This fork adapts AzzyAI and the existing Kimi customization for **Return to Morroc Refuge**.

**Status: testing build.** Build, configuration persistence, and focused runtime checks have passed. Recent skill timing and Tank – Gather behavior still require in-game confirmation. The client and server control skill availability, loyalty requirements, and whether cast requests succeed.

## Current features

- **Four colored Kimi tabs** with artwork, type-specific skills, automatic settings, and eight-slot combos. Shared movement and general options remain in Kimi Settings.
- **Save and load all four profiles.** Kimi Settings export includes every Kimi's automatic settings and regular combos. Apply also saves tactics and Blueprint settings to their separate files.
- **Visual Blueprint programming** with conditions, actions, and logic gates. Monster tactics also govern regular combo skill use.
- **Terrain-aware movement** with a searchable selector for 1,276 maps, wall routing, following, and kiting. Map selection is manual. Distance settings support up to 100 cells; they do not extend client visibility or skill range.
- **Tank – Pull and Tank – Gather.** Pull preserves the original tank behavior. Gather collects toward AutoMobCount, capped by TankMonsterLimit (maximum 30), then starts combat. It also starts with a smaller group when no nearby eligible mobs remain.
- **Body Double Cast Interval** on all four Kimi. With UseAutoBD enabled, HP threshold 0 and a positive interval enable timed casting without an HP check. Positive thresholds retain HP-based casting; both values 0 disable automatic casting. Buff duration does not impose a cooldown.
- **Bastion Renewal and other support controls.** Ward's automatic healing respects its HP threshold. A positive Bastion Renewal Cooldown sets the recast-attempt interval directly; 0 uses the stored 15-second default. Master Swap automation is available for Ward, Occult, and Agile. Chaotic Heal uses its skill Enabled switch and automatic-healing settings.
- **Optional diagnostic logging** through the GUI. Routine logging is disabled by default.
- **Embedded runtime updates.** The EXE carries the core Lua files and map data, so applying settings installs the matching runtime.

## Install a prepared package

1. Close the game and KimiAIConfig. Back up your existing custom AI folder.
2. Extract the package contents directly into `Refuge/AI_sakray/USER_AI/`—do not create another nested `USER_AI` folder.
3. Run **KimiAIConfig.exe** there on Windows with **.NET Framework 4.8**.
4. Select the Kimi type, enable learned skills, set their levels, and configure automatic behavior or combos. Click **Apply**.
5. Enable the client's custom/user AI mode and restart the client to load updated scripts.

GitHub's **Download ZIP** is the source checkout, not the prepared installation package. Compiled executables and release ZIPs are not tracked in this repository.

## Build from source

Use Windows with MSBuild or the .NET SDK and the .NET Framework 4.8 targeting pack. From the repository root:

```powershell
dotnet msbuild GUI/AzzyAIConfig/AzzyAIConfig.csproj /p:Configuration=Release /p:Platform=x86 /p:TargetFrameworkVersion=v4.8
```

The output is `GUI/AzzyAIConfig/bin/Release/KimiAIConfig.exe`. The project file retains an older framework default; the command above selects the framework used for the current testing build.

To install a source build, copy the contents of `USER_AI/` into the client's `AI_sakray/USER_AI/` folder, then put the compiled `KimiAIConfig.exe` in that same folder. Run it there and Apply. Keep the terrain files alongside the runtime. Close the GUI before replacing its EXE.

## Documentation

- [Installation and first configuration](release-docs/README.md)
- [All tabs and settings](release-docs/USER_GUIDE.md)
- [Blueprint programming](release-docs/BLUEPRINT_GUIDE.md)
- [Tactics and extra options](release-docs/TACTICS_AND_EXTRAS.md)
- [Changelog](release-docs/CHANGELOG.md)
- [Skill reference](KIMI_SKILLS.txt)
- [Credits and source notices](release-docs/CREDITS.md)

## Reporting a problem

Include the Kimi type, expected and actual behavior, relevant settings, and the time of the problem. Temporarily enable **Debug Logging**, reproduce once, and include the relevant log entries with `H_Config.lua` and applicable tactics. Include the Blueprint file for Blueprint problems. Turn logging off after collecting the report.

## Repository layout

| Folder | Contents |
| --- | --- |
| `GUI/AzzyAIConfig/` | Windows configuration source and embedded-runtime installer |
| `USER_AI/` | Runtime scripts, starting configuration, and navigation data |
| `release-docs/` | Installation guide, settings reference, Blueprint guide, and changelog |
| `Kimi Info/` | Supplied skill screenshots and Kimi references |
| `design/`, `navigation-source/` | UI previews and terrain reference material |
| `DEFAULT/` | Original default AI reference files |

Based on AzzyAI by Dr. Azzy, the configuration GUI credited to Machiavellian, and the existing Kimi customization credited to Nathan. Original source notices remain in place; see [Credits](release-docs/CREDITS.md).
