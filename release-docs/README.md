# KimiAI for Return to Morroc Refuge

**Testing build.** This package includes KimiAIConfig.exe, the matching Lua runtime, terrain data, and documentation. Recent gameplay changes still need in-game confirmation.

## Installation

1. Close the game and KimiAIConfig. Back up your existing `AI_sakray/USER_AI` folder.
2. Extract the ZIP contents directly into `Refuge/AI_sakray/USER_AI/`. Do not add another nested folder.
3. Run `KimiAIConfig.exe` from that folder. Windows with .NET Framework 4.8 is required.
4. Select the correct Kimi type, open its named tab, enable learned skills, and set their levels. Configure automatic behavior or a combo, then click Apply.
5. Use the client's custom/user AI mode and restart the client after replacing runtime files.

The required directory is `AI_sakray/USER_AI`, not `AI_salray` or the separate `AI/USER_AI` folder. The EXE installs its matching core scripts when settings are applied. Reopening an older EXE can restore older scripts.

GitHub's source ZIP does not contain the compiled EXE. Source-build instructions are in the repository's top-level README.

## First configuration

Review all settings before testing; do not assume a supplied or imported profile matches your Kimi. Skill levels do not unlock skills or satisfy loyalty requirements.

- Enable the desired skill and set its learned level. UseAttackSkill controls automatic attack skills. For a regular combo, enable ComboEnabled and ComboRunDuringAttack and configure consecutive slots.
- Choose UseAutoHeal and HP thresholds for Ward's Bastion Renewal or Occult's Chaotic Heal. Chaotic Heal needs its skill Enabled switch; there is no separate UseChaoticHeal requirement.
- Body Double uses its skill Enabled switch and UseAutoBD. HP threshold 0 plus a positive Body Double Cast Interval enables timed casting; positive thresholds use owner HP. Both 0 disables automatic casting. Buff duration does not delay recasting.
- Positive Bastion Renewal Cooldown values set its recast-attempt interval directly. Zero uses the stored 15-second default. The server decides whether an attempt succeeds.
- Master Swap uses its skill Enabled switch and owner HP threshold on Ward, Occult, and Agile. Threshold 0 disables automatic swapping.
- Tank – Gather starts combat at AutoMobCount, capped by TankMonsterLimit, or when no additional nearby eligible mobs remain. Tank – Pull retains the original behavior. TankMonsterLimit supports up to 30.

## Saving and importing

File > Export > Kimi Settings saves all four Kimi profiles, including automatic settings and regular combos. Import a settings file, review it, then Apply. Tactics and Blueprint graphs are separate files; Apply saves them too, but a Kimi Settings-only export does not include them.

## Map navigation

Type part of a map filename into Navigation Map, choose a matching result, and Apply. Use `/where` to identify the map. Select a new map when changing maps; the AI cannot automatically identify it. Clear the field to disable terrain routing.

The selector contains 1,276 maps. Only the selected terrain is loaded into AI memory. A wrong map selection can cause incorrect routes. Detection, movement, and kite settings support up to 100 cells, but cannot extend client actor visibility or skill range. Off-screen targets remain available only while the client reports them. Terrain routing does not add teleportation or permit attacks through walls.

## Guides

- [User guide](USER_GUIDE.md): tabs, skills, healing, movement, targeting, and combos.
- [Blueprint guide](BLUEPRINT_GUIDE.md): visual programming and examples.
- [Tactics and extras](TACTICS_AND_EXTRAS.md): monster behaviors and advanced options.
- [Changelog](CHANGELOG.md): implemented changes and known limitations.
- [Credits](CREDITS.md): acknowledgments and source notices.

## Troubleshooting

Check the active Kimi profile, skill Enabled switch, level, SP, and relevant monster tactic. A specific monster tactic overrides Default and may block offensive combo skills.

Enable Debug Logging temporarily, reproduce the issue, and report the timestamp, expected behavior, actual behavior, `H_Config.lua`, relevant tactics, and diagnostic log entries. Include the Blueprint file for Blueprint issues. Disable logging afterward. Cast requests do not confirm that the server accepted the skill.
