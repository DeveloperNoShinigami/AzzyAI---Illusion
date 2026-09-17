# KimiAI for Return to Morroc Refuge

**Testing release â€” 13 September 2026**

KimiAIConfig provides a Windows interface for configuring Kimi skills, movement, monster tactics, regular combos, and visual Blueprint graphs. This package is built for the Refuge client's `AI_sakray/USER_AI` directory.

## Install

1. Close the game and any open KimiAIConfig window.
2. Back up the existing `AI_sakray/USER_AI` folder if you already use a custom AI.
3. Open the ZIP and extract its **contents directly into `AI_sakray/USER_AI`**. Do not add another `USER_AI` or package-name folder inside it.
4. Run `KimiAIConfig.exe` from that folder. This build requires Windows with **.NET Framework 4.8**.
5. Select your Kimi type in **Kimi Settings**. Open its named tab, configure **Skills & Behavior**, **Automatic Skills**, and optionally **Combo**, then click **Apply**.
6. Start the game using the client's custom/user AI mode, then summon Kimi. Restart the client after replacing runtime files to ensure it loads the updated scripts.

Correct placement:

```text
Refuge/
  AI_sakray/
    USER_AI/
      KimiAIConfig.exe
      AI.lua
      AI_main.lua
      H_Config.lua
      ...other Lua files and guides...
      data/
      ErrorLog/
```

The server folder is **AI_sakray**. Do not substitute `AI_salray` or the separate `AI/USER_AI` location. The supplied runtime uses the `AI_sakray/USER_AI` path.

## First configuration

The package has a clean starting profile. Both combo systems, automatic skill selection, and automatic healing/buff toggles start off. All active skill levels display 1 and their Enabled switches are off. Normal movement and ordinary attacks still depend on the supplied behavior and monster tactics. Occult is the initial type selection; change it if needed.

Enable only learned skills for the selected Kimi and set their available levels. Level selection does not unlock skills or satisfy loyalty requirements. For automatic attacks with skills, also enable UseAttackSkill. For a regular combo, enable ComboEnabled and ComboRunDuringAttack and fill consecutive slots. For a Blueprint graph, follow BLUEPRINT_GUIDE.md and enable BlueprintComboEnabled. Start with one system at a time.

Automatic healing requires the skill's Enabled switch, UseChaoticHeal, and an appropriate UseAutoHeal mode. It checks the configured HP thresholds, SP, and timing. At or above a threshold, that HP condition does not trigger a heal. A Heal explicitly placed in a combo is a separate action.

## Updating an existing installation

This full ZIP includes clean settings and tactics for a new installation. **Extracting it over existing files replaces files with the same names**, including your settings. Keep your backup.

To update while retaining a configuration already made with this GUI, replace only `KimiAIConfig.exe` and the guides, then open the new EXE. Its installer updates core runtime files and preserves existing user configuration files. Apply then saves your selected settings. Do not run an older EXE afterward: it can restore its older embedded runtime.

The installer creates one-time `.bak` files for replaced core scripts. These are not a complete settings backup. Custom Lua changes in core files are overwritten by the embedded version.

## Guides

Use the GUI's **Documentation** menu or open **Documentation.html** for a browser-readable manual. The Markdown guides below contain the same material.

- [USER_GUIDE.md](USER_GUIDE.md): every tab, settings reference, tactics, healing, targeting, and regular combos.
- [BLUEPRINT_GUIDE.md](BLUEPRINT_GUIDE.md): visual programming, node properties, connections, and examples.
- [KIMI_SKILLS.txt](KIMI_SKILLS.txt): per-Kimi skills, IDs, maximum levels, categories, and loyalty requirements.
- [CHANGELOG.md](CHANGELOG.md): implemented changes, reversions, and outstanding issues.
- [CREDITS.md](CREDITS.md): original project acknowledgments and notices.

## Release status

Regular combos are reported working in the latest user test. Exact one-hit auto-attack counting is not supported: an attack command can continue through a skill wait. Owner protection remains unresolved; the build includes additional rescue diagnostics. Some skill metadata and Blueprint paths remain unverified in-game. This is a testing release, not a claim that every exposed option works on Refuge.

## Troubleshooting

**Only ordinary attacks:** confirm the correct EXE/folder, Kimi type, skill Enabled controls, UseAttackSkill or the chosen combo enable flag, and learned levels. Apply and restart the client after updating scripts.

**Pauses between combo actions:** check SP and cooldowns. Chaotic Heal costs 50% of maximum SP; the regular sequence waits for an unaffordable skill. A skill's cast time can exceed the configured minimum delay.

**Owner not protected:** reproduce with a known monster while Kimi is fighting another target. Record the monster name and time. Include the recent `[RESCUE CHECK]` lines rather than assuming the nearest monster is the attacker.

**Report a problem:** provide the Kimi type, skill levels, expected behavior, actual behavior, and timestamp. Include `H_Config.lua`, relevant monster tactics, and recent entries from `ErrorLog/AAI_ERROR.log` and `ErrorLog/AAI_SKILLFAIL.log`. For Blueprint issues, include `combo_tactics.cbp`. These logs contain ordinary diagnostics as well as failures and can grow during testing.

## Saving another configuration

Use File > Export > Kimi Settings to save a named Lua file. Use File > Import > Kimi Settings to load one, review the values, then Apply. This includes all four automatic and standard combo profiles. Monster tactics and Blueprint graphs use their own controls.

Close every KimiAIConfig instance before replacing the EXE. The new EXE embeds the corrected runtime; reopening an older EXE can restore its older core Lua files.

**Before testing a skill-only combo:** check the target monster in Kimi Tactics. A specific row overrides Default. Skill usage = Never blocks offensive combo skills, even when owner rescue selects that monster. Change the specific row to Always if those skills should be allowed.

For Ward automatic healing and a 30-cell AI boundary, see the Ward setup section in USER_GUIDE.md. Import Ward-30-cell-settings.lua through the GUI to review that setup. Wall routes and actor visibility still depend on the client.

## Map navigation

In **Kimi Settings**, click the **Navigation Map** value and type any part of a map filename. Matching maps appear automatically below the field. Click the desired result, then click **Apply**. Clear the value to disable map navigation. Reload the AI after applying. Use `/where` in the game to identify the current map filename, for example `moc_pryd05`.

The selector includes **1,276 maps** from the original Morroc renewal server cache, with imported map overrides applied. Only the selected map's terrain is loaded into AI memory. **Disabled / Unknown map** is the default and retains normal movement. Change the selection whenever you change maps; the AI cannot automatically identify your map. A wrong selection can prevent movement or produce incorrect routes. Missing map data falls back to normal movement.

With a map selected, movement uses clear path segments of up to eight cells around known walls. Repeated movement commands are suppressed while Kimi is progressing along the same segment. Chase routes stay within the configured owner movement boundary, capped at 100 cells. The total walking route can be longer than 100 steps while staying within that boundary. Targets still follow existing tactics, chase permissions, and combo rules.

Kiting rejects blocked tiles and diagonal wall corners. Ranged retreat also checks terrain line of sight. When no valid retreat exists, Kimi can continue an in-range cast instead of repeatedly moving into a known wall. Terrain does not include temporary server obstacles; visible occupied tiles and stalled waypoints are handled separately. This does not guarantee a route to every target or allow attacks through walls.

The EXE includes and deploys `KimiNavigation.lua`, `NavigationMaps.txt`, and `NavigationData.txt`. Keep these files with the distributed runtime. Navigation has passed local route and syntax checks; in-game testing remains necessary.


### Returning to the owner and 100-cell limits

Detection distances, movement bounds, and kite bounds support up to 100 cells. These limits do not increase skill casting range or the client's actor visibility. Off-screen combat is possible only while the client continues to report the target; terrain data cannot locate unseen monsters.

With the correct map selected, normal movement uses terrain routing. Following and emergency return choose a reachable tile near the owner instead of a potentially blocked geometric destination. Return recovery can include Kimi's position outside the combat leash when the owner has moved away. No teleport command is added.

With OpportunisticTargeting enabled, nearest eligible target selection runs before terrain routing, including when a skill is selected. It does not reset movement when the same target remains nearest. Nearest means distance from Kimi among eligible candidates; known unreachable targets are deprioritized. It is not a comparison of complete walking-route lengths. Active casts and rescue priorities remain protected. Long terrain searches are continued across AI updates to limit per-update work.
