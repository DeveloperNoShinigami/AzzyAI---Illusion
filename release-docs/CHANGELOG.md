# Changelog

Documentation synchronized on 14 September 2026. Recent entries describe the current GUI and runtime fixes. Checks cover builds, selected settings round-trips, tab initialization, and AoE boundaries; in-game verification remains necessary.

Still outstanding: missing-route handling and unsupported legacy tactic specializations. Owner rescue and ranged positioning have new code fixes described below; in-game behavior is not yet verified.

## Bastion configured recast interval

- Positive Bastion Renewal Cooldown values now replace the stored 15-second cooldown instead of being clamped to at least 15 seconds. Setting 2 allows another attempt after two seconds when the other casting conditions are met.
- Zero retains the 15-second catalog fallback. HP conditions, SP, and current cast timing remain enforced; the AI does not guarantee server acceptance.

## Tank – Gather respects AutoMobCount

- Changed the gather target to AutoMobCount, capped by TankMonsterLimit.
- Start combat at AutoMobCount, or with a smaller collected group when no nearby eligible targets remain to gather.
- Gathering Taunt now requires the configured group count within its radius instead of firing on a single target. Once combat starts, the collected group is finished before starting another batch.

## Body Double recasting without a duration lock

- Removed the artificial Body Double reuse lock based on its 10–50 second buff duration for all four Kimi. The skill has no built-in cooldown in the supplied skill information.
- Body Double can now be requested once each configured cast interval even while its previous buff may still be active. Enable switches, SP, and current cast timing remain enforced.
- The AI does not confirm successful buff application; the interval runs from the cast request.

## Timed automatic Body Double

- Renamed Body Double Cooldown to Body Double Cast Interval on all four Kimi tabs, with seconds and HP-mode behavior explained in the description.
- Owner HP threshold 0 plus a positive interval now enables timed automatic casting without checking owner HP. Both values 0 disables automatic casting; a positive HP threshold retains HP-based casting.
- UseAutoBD, Body Double Enabled, SP, skill reuse, and active-duration limits remain enforced. The saved BodyDoubleCooldown key is retained for compatibility.

## Tank – Pull and Tank – Gather

- Preserved the existing Tank tactic as Tank - Pull (value -1), and added Tank - Gather (value -3) with GUI save/load support.
- Raised TankMonsterLimit's maximum and new default to 30; the deployed configuration is set to 30.
- Gather approaches within Ward's Taunt radius and casts when the skill is ready. During cooldown, it can tag targets with basic hits unless Skill Only is enabled.
- Gather starts the configured attack combo or permitted skills after reaching the limit or exhausting eligible targets, staying with the group until it is gone. Terrain routing, skill cooldowns, tactic restrictions, and the owner leash remain in effect.
- Checked gathering transitions, Taunt radius/cooldown behavior, terrain line-of-sight handling, and GUI compilation. In-game testing remains necessary.

## Master Swap, Chaotic Heal, and per-Kimi AoE controls

- Restored automatic Master Swap HP and cooldown controls for Ward, Occult, and Agile. Added the corresponding runtime scheduler using Master Swap Enabled, with zero HP threshold disabling automatic casts. UseMasterSwap remains hidden and is not a second enable gate.
- Removed the redundant UseChaoticHeal gate. Automatic healing still respects the skill Enabled switch, UseAutoHeal, HP thresholds, SP, and cooldowns.
- Expanded OnlyAOE to Ward and Raging alongside Occult. Automatic selection now uses the appropriate Kimi skill instead of always requesting Illusion of Light. Offensive combo filtering recognizes catalogued AoE skills and leaves support skills available.
- Reviewed the supplied skill screenshots. Ward and Agile screenshots show Master Swap; Occult is retained based on the existing catalog and user confirmation. No Master Swap is shown in the supplied Raging images.

## Body Double automation and configurable support cooldowns

- Replaced the redundant Use Body Double automatic control with UseAutoBD for each Kimi. The skill enable switch remains in Skills & Behavior, and legacy configurations retain their automatic setting.
- Moved automatic Body Double out of the offensive buff scheduler. It checks the owner HP threshold during combat as well as idle updates. Zero disables the HP trigger; positive values trigger at or below the threshold, subject to skill availability, SP, and reuse limits.
- Added Body Double Cooldown for every Kimi and Bastion Renewal Cooldown for Ward, in seconds. Zero adds no extra delay; configured intervals cannot shorten the runtime skill reuse/duration limits. These limits apply to automatic, combo, and manual requests.
- Hid unused Master Swap automatic controls; Master Swap Enabled remains in Skills & Behavior.
- Checked all Body Double thresholds from 1 through 100 for all four Kimi, disabled triggers, legacy fallback, and cooldown limits. Server acceptance still requires in-game testing.

## Bastion healing dispatch with Lag Reduction

- Fixed the Always auto-heal path returning before sending a skill queued by Lag Reduction. The queued heal is now sent before that return.
- Added optional Bastion diagnostics below the HP threshold: reuse wait, global skill delay, disabled or invalid skill, insufficient SP, and cast request/rejection. Enable Debug Logging to collect these messages; output is limited to one message every five seconds.
- This fixes a reproducible dispatch defect. A cast request does not confirm that the server accepted the skill; in-game testing is still required.

## Bastion healing helper visibility

- Fixed Bastion automatic healing calling a helper that was local to AzzyUtil.lua. GetReadyKimiSkill is now shared with AI_main.lua.
- Verified by loading the actual utility module and checking Bastion HP threshold, reuse, and disabled-skill behavior.

## Owner-return routing follow-up

- Connected the remaining direct owner-return helper and no-target idle return to terrain routing.
- Return arrival now requires a walkable connection, not merely line of sight across a gap. The owner's occupied endpoint is allowed for this connection check.
- Return searches can explore the map beyond the combat leash and continue across AI updates. A moving owner no longer discards an unfinished return search on every position update.
- Verified direct-helper delegation and wall return with the owner tile occupied.

## Manual skill command priority

- Manual targeted skills now use their command state instead of entering automated combo chase. Explicit targeted and ground commands are handled before automatic healing, rescue selection, and combos, while retaining spawn and owner-leash gates.
- Command handlers wait for active casts and retain temporarily rejected casts for bounded retries. Manual targeted movement uses terrain routing to the requested skill range.
- Verified manual Blood Sweep with combos enabled and rejected-cast retry behavior. Offensive auto-skills remain suppressed while a combo runs; that interaction is unchanged pending clarification.

## 100-cell boundaries and opportunistic targeting

- Raised detection, movement, and kite controls and runtime limits to 100; updated active and shipping settings.
- Fixed map navigation bypassing opportunistic selection. Removed the selected-skill exclusion and prevented movement resets when the chosen target is unchanged. Equal-distance targets retain the current target.
- Continued long A* searches in bounded batches across updates instead of rejecting them at the earlier small search limit or blocking a single AI update.
- Verified the formerly blocked recorded corner route, return routing, nearest-target selection before navigation, disabled behavior, and same-target stability.

## Optional debug logging

- Added a saved True/False Enable Debug Logging control to Kimi Settings, defaulting to False. The master switch gates recurring TraceAI, category, and logappend file writes.
- Verified GUI persistence and zero log-file opens from the recurring logging functions while disabled; enabling restores output. Existing logs are preserved.

## Navigation performance correction

- Replaced breadth-first terrain searches with target-directed A* and a bounded search workload. Rejects exact destinations inside walls before searching.
- Retains separate failed-route retry entries so alternating movement requests cannot repeatedly trigger the same failed searches.
- Replaying twelve route requests from the lag report decreased from about 2.7 seconds to 0.1 seconds locally. Wall routing, owner return, and boundary checks still pass; in-game performance remains to be confirmed.

## Terrain-aware owner return and 60-cell boundaries

- Routed normal and emergency owner return through terrain navigation, choosing a reachable stand-off tile. Return recovery includes a starting position outside the current combat leash.
- Raised the five detection, movement-bound, and kite-bound controls and runtime caps to 60. Updated active and shipping settings; skill ranges and client visibility remain unchanged.
- Verified wall return from outside the leash, GUI acceptance of 60-cell settings, route checks, Lua syntax, and EXE build.

## Route search responsiveness

- Removed the shared 250 ms route-search delay. The one-second retry backoff now applies only to the same failed destination, owner boundary, and starting cell. A new target or changed position can be routed immediately.
- Verified that an unreachable destination does not delay a reachable destination requested in the same AI tick.

## Navigation movement and map search follow-up

- Replaced one-cell route commands with checked path segments of up to eight cells. Kept the current endpoint while moving and suppressed repeated commands to reduce stop-start movement.
- Removed the top map selector and dropdown. Typing directly in Navigation Map now opens a matching-map suggestion list below the value field. Click a result or use arrow keys and Enter. Verified typing, popup visibility, selection, and persistence through the actual shared settings grid.
- The moc_pryd05 regression traverses the same 31-cell route with five segment commands. Terrain, owner-boundary, occupied-cell, and settings checks pass; smoothness still needs in-game confirmation.

## 15 September 2026 — Selected-map navigation

- Added a searchable Navigation Map dropdown with 1,276 maps and a disabled default. Selection is included in settings save/load.
- Added packed terrain data, bounded waypoint routing, terrain-aware ranged chase, and wall/corner checks for kiting. Existing tactics and configured movement boundaries remain in effect.
- Included navigation resources in the config EXE and release package so GUI deployment retains the feature.
- Verified the logged moc_pryd05 wall route, owner boundary, occupied-cell rejection, missing-map fallback, and Lua syntax. In-game behavior still needs testing.

## 15 September 2026 — Limited chase detours

- Added bounded lateral waypoint attempts before blocked chases or attack timeouts give up. Successful movement retries the target approach; failed attempts remain bounded by time, count, and the owner leash.
- Preserved no-chase tactics and invalid-target checks. Known occupied cells are skipped; wall walkability still depends on the client's movement handling.
- Verified waypoint retry/progress handling, leash and tactic gates, bounded failure, Lua syntax, and the embedded-runtime EXE build. No claim of full map-aware pathfinding or in-game success.

## 15 September 2026 — Simplified Ward healing controls

- Removed the redundant Auto Bastion Renewal toggle and Bastion Renewal Interval from the GUI. Automatic Bastion now uses its skill Enabled switch, UseAutoHeal scheduling, HP threshold, and skill reuse checks. Legacy toggle/interval values no longer suppress or delay it.
- Changed Use Body Double to a True/False choice while preserving 0/1 Lua compatibility. Its automatic buff path now respects False; explicit combo casts remain separate.
- Verified control visibility, boolean profile save/load, Bastion threshold/reuse behavior, and the rebuilt EXE.

## 15 September 2026 — Ward healing and movement boundaries

- Added Ward's saved Bastion Renewal HP Threshold and exposed UseAutoHeal scheduling for Ward. Connected the automatic toggle and interval to healing; prevented the generic defensive-buff path from bypassing the HP check. Skill ID 8006 is unchanged.
- Raised detection, movement-leash, and kite-bound controls to a maximum of 30. Replaced the hardcoded 14-cell movement rejection with the configured leash; long travel retains short movement requests.
- Opportunistic targeting now prefers nearest eligible ordinary targets during chase and combat. Rescue and active casting remain protected. After a target is lost or killed, no-target idle processing returns to the owner before idle walking.
- Added a named Ward setup. Verified GUI profile persistence, 30-cell controls, movement boundary checks, nearest selection, Bastion HP/reuse gates, and the build. Client visibility and wall pathfinding remain game-dependent.

## Blocked ranged retreat

- Fixed ranged repositioning repeatedly postponing casts when a wall prevents retreat. After 500 ms without positional progress, an in-range target can be cast on from the current position. The failed retreat is not repeated until either actor moves.
- Verified with a mocked blocked-movement regression, Lua syntax check, and config EXE build. In-game terrain testing remains necessary.

## 14 September 2026 — Tactics, ranged combos, and Ward portrait


- Made standard and Blueprint offensive combo skills honor target tactic permissions, cast limits, level limits, skill class, and SP reserves. Support actions keep their separate skill controls.
- Changed Snipe OK to maintain Breath/Light casting distance on the current target. Ranged positioning stays within the skill range and owner movement bounds; normal and melee attack steps keep their own range.
- Prevented generic melee attack recovery from forcing an active skill combo onto the monster's cell. Generic movement now reads the current combo action's range.
- Allowed owner rescue to recognize a living enemy targeting the owner between attack or cast motions. Preserved the configured HP threshold, monster eligibility, movement limits, and Follow-state block. An HP drop alone cannot identify an attacker.
- Fixed specialized cast reactions assigning a function instead of the numeric skill-command state. Cast reactions now require an actual casting motion. Unsupported legacy skill-class choices remain documented.
- Added the supplied Ward screenshot portrait to the GUI. Updated the rescue tooltip and user guide.
- Validation: GUI build and focused mocked Lua checks for tactic gates, ranged positioning, and rescue filtering. Game casting and movement remain subject to in-game testing.

## Bastion Renewal labels

- Replaced visible Warm Def labels with Bastion Renewal in the GUI and guide. Existing Lua setting keys remain compatible with saved configurations.

## Occult healing controls

- Hid legacy HealSelfHP and HealOwnerHP fields. Kept UseAutoHeal because it schedules healing checks, and clarified its description to reference Chaotic Heal and its current HP thresholds.

## Automatic skill profiles and AoE radii

- Moved all AutoSkill and Autobuff controls into each Kimi's Automatic Skills page, with independent saved values. Healing controls are shown only for Occult; type-specific buff controls are filtered appropriately. Older global automatic settings are inherited on migration.
- Recorded Illusion of Light as 3x3, Sanctuary and Blood Sweep as 5x5, Ward's Domain as radius 4, and Taunt as radii 5/5/6/6/7. Radius entries are explicitly distinguished from square widths.
- Fixed self-centered AoE attack checks to use the effect radius rather than zero self-cast range.

## Kimi tabs and combo profiles

- Added colored Ward, Occult, Agile, and Raging tabs with applicable skills and behavior settings. Shared behavior remains in Kimi Settings.
- Added independent standard combo profiles, including named settings import/export and runtime selection for the detected Kimi. Combo skill lists show the appropriate Kimi's active skills.
- Added source screenshot portraits for Occult, Agile, and Raging. Ward initially used a placeholder, replaced by the supplied portrait in the latest entry. The Blueprint editor remains shared and is labeled accordingly.

## Blood Sweep area correction

- Corrected Blood Sweep's AoE from an empty size table to a 5x5 area at levels 1–10. Automatic casting can now count eligible mobs instead of always counting zero. The corrected metadata is included in the config EXE.

## Settings file import and export

- Fixed loading named Kimi settings files so the grid refreshes and Apply becomes available. Saved settings preserve additional Lua configuration entries. File dialog errors are reported without closing the program.

## 14 September 2026 — Kimi AoE availability

- Fixed enabled Kimi AoE skills, including Blood Sweep, being blocked by hidden legacy homunculus toggles during chase and attack. AutoSkill, AoE mode, target-count, and skill readiness checks still apply. The config EXE embeds the corrected runtime so saving settings preserves the fix.

## Refuge testing release â€” 13 September 2026

This records work completed during the Refuge adaptation and testing session. It separates newly implemented changes from inherited features, reversions, and unresolved behavior. No individual semantic version numbers or release dates have been invented for intermediate builds.

### Skills and GUI configuration

- Added a unified catalog for Ward, Occult, Agile, and Raging Kimi: 24 unique skills, 16 active and 8 passive, with 32 per-type memberships.
- Added per-type skill categories, active skill level controls, separate Enabled switches, and passive summaries. Displayed active levels start at 1 and respect catalog maxima.
- Added persistent KimiSkillLevels and KimiSkillEnabled data and compatibility with existing skill-level settings.
- Added shared gates for type compatibility, passive rejection, enabled state, level limits, SP availability, and cooldowns.
- Updated self, owner, enemy, and ground targeting through shared dispatch. Added percentage-based and partial SP cost metadata where supplied.
- Used supplied skill IDs and tooltips for verified metadata, while marking remaining fallback or uncertain timings. Full catalog inclusion does not mean all skills were tested in-game.

### Runtime reliability

- Fixed the startup version diagnostic that could concatenate a nil value.
- Corrected the recognized stock OnAutoBuffs hook so returning without casting does not freeze normal AI state processing.
- Corrected healing and combo-buff timeout calculations that added the current clock twice.
- Corrected regular combo state handling: process an issued skill before checking its newly reduced SP or changed cooldown; clear stale timers on slot advancement; skip zero-count slots; preserve the current slot while waiting for SP.
- Changed generated skill calls to use shared DoSkill dispatch, including ground-target handling.

### GUI installation and saving

- Embedded 15 homunculus runtime/configuration resources in KimiAIConfig.exe.
- Added startup and pre-Apply installation beside the EXE. Six core files update from the embedded build; missing user files are created without replacing existing user files during installation.
- Added one-time core backups, data/ErrorLog creation, and a marker-specific migration for the broken stock H_Extra hook while preserving surrounding custom content.
- Kept Extra Options Save from replacing H_Extra with an empty or generated stub.
- Fixed a setting-name collision: ComboEnabled could read BlueprintComboEnabled. Anchored both readers and the shared writer so the two options load/save independently and comments are not treated as assignments.

### Auto-attack experiment and reversion

- Investigated extra ordinary attacks between combo steps. The native Attack command starts continued attacking; a command count does not equal a hit count.
- Tried animation-based completion tracking with cancellation and retry handling in both combo modes.
- In-game logs showed repeated motion timeouts and a slot-1 stall despite passing mocked motion tests.
- Removed that experiment and restored timer-based command progression in both the runtime and embedded GUI. Regular combos were subsequently reported working.
- Exact per-hit counting remains unsupported. The GUI help and guides describe the actual limitation.

### Owner protection investigation

- Corrected GetTargetClass to recognize the owner and known friends before applying an inherited actor-ID cutoff. A Refuge owner ID below the cutoff reproduced the earlier classification failure.
- A focused test confirmed that the owner attacker enters the rescue list and the existing rescue branch can switch away from another enemy.
- Subsequent in-game testing still failed to protect the owner. This remains unresolved; the classification fix is not presented as a complete solution.
- Added throttled `[RESCUE CHECK]` logging with owner ID/HP, current state and target, monster target IDs, tactics, distances, and eligibility. The diagnostic version is included here.

### Healing clarification

- Documented that Idle and Idle_low differ in priority; both retain HP, SP, and cooldown checks.
- A proposed configurable 15-second idle healing interval was cancelled before deployment. No extra interval was added.
- Healing triggers below its threshold and stops triggering at or above it. The server still chooses the actual Chaotic Heal recipient.

### Documentation and distribution

- Added an offline browser-readable manual and connected the GUI Documentation menu to it, with the existing PDF fallback retained.
- Added installation/update instructions, a complete browsable settings reference, explanations for every tab, a Blueprint programming guide, skill reference, and this changelog.
- Documented inherited controls whose runtime behavior is absent, narrower than their labels, or unverified.
- Clarified AutoSkillDelay: negative values are treated as 0 (no extra pause), and the remaining rhythm is controlled by skill cast/cooldown/SP checks.
- Prepared a flat ZIP for extraction directly into AI_sakray/USER_AI, containing the EXE and required Lua files.
- Replaced inherited skill/combo test presets with a clean profile: no enabled active skills, all displayed active levels 1, blank regular slots, combo systems off, and automatic skill/heal/buff toggles off.
- Removed the inherited sample friend entry from bundled defaults. Personal live settings, logs, backups, and Blueprint graphs are excluded.

### Existing features retained

The property-grid settings UI, monster and PVP tactics, eight-slot regular combo framework, visual Blueprint editor, movement/kiting modes, friending, and legacy standby/berserk options existed in the project. This release documents and adapts them; it does not claim they were all newly built or fully validated.

### Validation and remaining work

The GUI builds for x86/.NET Framework 4.8 with four existing CS0108 warnings. Focused checks cover configuration round-trips, installation and actual Apply, skill/command ordering, and the reproduced owner-ID classification case. Lua 5.0 syntax checks adapt the pre-existing #BlueprintCombos expression in test input only; they do not prove complete server-client compatibility.

Before a general stable release, confirm owner rescue in-game, test each Kimi type and outstanding skill metadata, and exercise the documented Blueprint paths. Automated command-level tests do not establish landed hits, server skill acceptance, or all in-game timing.
