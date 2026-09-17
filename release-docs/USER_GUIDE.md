# KimiAI User Guide

Return to Morroc Refuge - testing release. Documentation updated 14 September 2026.

## How to use this guide

The application has nine main tabs: **Kimi Settings**, **Ward**, **Occult**, **Agile**, **Raging**, **Kimi Tactics**, **Blueprint (Shared)**, **PVP Tactics**, and **Extra Options**. Select the configured Kimi in Kimi Settings, then open its named tab to configure skills, automatic casting, and standard combos.

This guide uses the labels and property names from the current executable. Some inherited GUI descriptions refer to older homunculus skills or imply broader behavior than the runtime implements. The explanations here call out those differences. An available control is not a guarantee of verified server support.


### Current tab layout

| Location | Contents |
| --- | --- |
| Kimi Settings | Configured Kimi type, shared movement, targeting, protection, and general behavior. |
| Ward / Occult / Agile / Raging: Skills & Behavior | Applicable skill levels, Enabled switches, and passive information. Shared skills retain shared level and enable values. |
| Each Kimi: Automatic Skills | Independent automatic attack, AoE, SP reserve, delay, and applicable buff controls. |
| Each Kimi: Combo | Independent standard combo settings and eight slots. Skill choices are filtered for that Kimi. |
| Blueprint (Shared) | Shared graph editor; Kimi Type conditions can select a sequence for each type. |

Opening a tab changes only the view. Existing standard combos migrate to the configured Kimi; older global automatic settings are initially inherited by all four profiles. Kimi Settings import/export includes these profiles. Blueprint graphs and monster tactics retain separate save/load controls.

### Occult automatic healing

Enable Chaotic Heal in Skills & Behavior. In Automatic Skills, choose a UseAutoHeal mode. Set ChaoticHealKimiHP and ChaoticHealOwnerHP for the desired thresholds. Automatic healing stops triggering at or above the corresponding threshold. Never disables the check; Always, Idle, and Idle_low control when it runs. Idle_low has lower priority, not a different HP requirement.

HealSelfHP and HealOwnerHP are hidden legacy fallback fields retained in saved files. UseAutoHeal is available on Ward and Occult because it controls healing scheduling.

### AoE sizes and range checks

| Skill | Area |
| --- | --- |
| Illusion of Light | 3x3 square; maximum skill level remains 5. |
| Chaotic Sanctuary | 5x5 square. |
| Blood Sweep | 5x5 square at levels 1-10. |
| Ward's Domain | Radius of 4 cells around Ward. |
| Taunt | Radius of 5 / 5 / 6 / 6 / 7 cells at levels 1-5. |

Self-centered AoE checks now use the effect radius rather than zero self-cast range. Enabled Kimi AoE skills no longer depend on hidden legacy homunculus toggles. Visible AutoMobMode, AutoMobCount, SP, skill enablement, and timing conditions still apply. Area metadata does not add an automatic casting path for Chaotic Sanctuary.

### Current skill names

The GUI uses Bastion Renewal for its level, Enabled switch, and HP threshold. UseAutoHeal schedules automatic casting. Bastion Renewal and the old Warm Def alias both resolve to skill ID 8006. Legacy Lua keys remain unchanged for saved-file compatibility.

## Map navigation

In **Kimi Settings**, click the **Navigation Map** value and type any part of a map filename. Matching maps appear automatically below the field. Click the desired result, then click **Apply**. Clear the value to disable map navigation. Reload the AI after applying. Use `/where` in the game to identify the current map filename, for example `moc_pryd05`.

The selector includes **1,276 maps** from the original Morroc renewal server cache, with imported map overrides applied. Only the selected map's terrain is loaded into AI memory. **Disabled / Unknown map** is the default and retains normal movement. Change the selection whenever you change maps; the AI cannot automatically identify your map. A wrong selection can prevent movement or produce incorrect routes. Missing map data falls back to normal movement.

With a map selected, movement uses clear path segments of up to eight cells around known walls. Repeated movement commands are suppressed while Kimi is progressing along the same segment. There is no fixed wait before searching a new route. Repeating an unchanged failed route has a one-second retry backoff; a different destination or changed position can be checked immediately. Chase routes stay within the configured owner movement boundary, capped at 100 cells. The total walking route can be longer than 100 steps while staying within that boundary. Targets still follow existing tactics, chase permissions, and combo rules.

Kiting rejects blocked tiles and diagonal wall corners. Ranged retreat also checks terrain line of sight. When no valid retreat exists, Kimi can continue an in-range cast instead of repeatedly moving into a known wall. Terrain does not include temporary server obstacles; visible occupied tiles and stalled waypoints are handled separately. This does not guarantee a route to every target or allow attacks through walls.

The EXE includes and deploys `KimiNavigation.lua`, `NavigationMaps.txt`, and `NavigationData.txt`. Keep these files with the distributed runtime. Navigation has passed local route and syntax checks; in-game testing remains necessary.

## Kimi Settings

This tab is a categorized property grid. Select a row to edit its value and read its help text. True/False fields are switches; integer switches use 1/0. HP and SP percentages are measured against the corresponding maximum. Distances are generally map cells. Fields with Delay or Time in their names commonly use milliseconds; exceptions are identified below.

### Skills and available levels

The four types are Ward (6001), Occult (6002), Agile (6003), and Raging (6004). There are 24 distinct skills: 16 active and 8 passive. Shared skills appear under more than one type. KIMI_SKILLS.txt lists IDs, categories, maximum levels, and loyalty requirements.

Every active skill has separate **Level** and **Enabled** controls. A displayed level of 1 does not enable or unlock a skill. Set the level to what the character has learned, then enable it. Passive summaries are reference information; the AI cannot cast passives. The clean release starts with all active skills disabled and all displayed levels at 1.

### Healing and combat timing

Automatic Chaotic Heal and a Heal explicitly placed in a combo are separate uses of the skill. Automatic healing checks HP thresholds. A combo Heal follows its slot or node and does not gain an automatic HP condition unless the graph explicitly includes one.

Automatic healing triggers below the configured HP threshold and stops triggering at or above it. Idle and Idle_low differ in priority, not in their HP requirement. Idle healing has not yet been confirmed under a controlled in-game test. There is no additional fixed idle-heal interval in this release.

Chaotic Heal costs 50% of maximum SP. A regular combo waits at a temporarily unaffordable skill, which can look like a pause or stall. Cast time and cooldown also delay the next eligible action. Native auto-attack commands can continue attacking during these waits, so an attack count is not an exact number of landed hits.

### Targeting and owner protection

With OpportunisticTargeting disabled, normal selection uses tactics priority first and distance as a tie-breaker. When enabled, ordinary selection prefers the nearest eligible reachable target and reconsiders it during chase and combat without interrupting an active cast. Rescue is separate: per-monster Rescue settings, owner HP override, movement bounds, target visibility, and AI state determine eligibility.

The owner-ID compatibility fix is included, but protection still failed in subsequent testing. Throttled `[RESCUE CHECK]` diagnostics record owner and monster target IDs, tactics, distances, and eligibility. Do not rely on this build as verified automatic owner protection.

### Complete settings reference

The following tables cover shared settings and the four named Kimi tabs. AutoSkill and Autobuff options appear under Automatic Skills; standard combo options appear under Combo. Choices are shown where the GUI supplies an enum; numbers are not universal recommended values. Skills use the level range in their explanation. See the fresh-start profile in README.md rather than assuming the GUI's Reset to Defaults matches the packaged profile.

#### Basic Options

| Option | Values | Purpose |
|---|---|---|
| AggroHP | Number | Your Kimi will seek out and attack monsters whenever its HP percent (as percent of maximum HP; a number from 0-100) is greater than this value. When it lacks HP, it will only fight monsters if it is attacked. Set this value to 100 if you do not want the Kimi to attack unless it, the owner, or a friend is attacked. |
| AggroSP | Number | Your Kimi will seek out and attack monsters whenever its SP percent (as percent of maximum SP; a number from 0-100) is greater than this value. When it lacks SP, it will only fight monsters if it is attacked. |
| KiteMonsters | False, True | Set this to true if you want your Kimi to keep its distance from monsters while attacking. |
| SuperPassive | False, True | Suppress normal combat behavior. This is a passive operating mode, not an owner-protection option. |
| UseAttackSkill | False, True | Enable autoskills outside of combos (single-target + AoE when allowed). Set to false to use only combos/auto-attacks and buffs/heals. |
| AssumeHomun | False, True | Legacy compatibility option for identifying a companion when a mercenary and Kimi are both present. It is not a Kimi type selector. |
| DoNotChase | False, True | If you want your Kimi to stand still and only use ranged skills or attacks, set this value to true. |
| UseDanceAttack | False, True | Alternate movement and attack commands. This can affect positioning and timing; it does not change the server's attack-speed rules. |
| UseAvoid | False, True | Exit when a monster listed in H_Avoid.lua is detected. Leave disabled unless you deliberately maintain that list. |
| TankMonsterLimit | 1-30, default 30 | Maximum gathering target. Tank - Pull holds the collected monsters; Tank - Gather uses AutoMobCount as its target, capped by this limit. |
| RescueOwnerLowHP | Number | Positive: request rescue below this owner HP percentage, overriding the monster rescue tactic. Negative: suppress owner rescue above the absolute percentage. Zero: rely on monster rescue tactics. Follow state still blocks rescue; attacker detection requires a client-reported owner target. |
| StationaryAggroDist | Number | When the owner is is not moving, attack monsters within this distance of the owner. |
| MobileAggroDist | Number | When the owner is is moving, attack monsters within this distance of the owner. See also StationaryAggroDist. This should probably be relatively low so that the Kimi does not get left behind after chasing something while the owner is moving |
| OldHomunType | Ward, Occult, Agile, Raging | Choose Ward, Occult, Agile, or Raging as the configured Kimi type. The runtime also checks the server-reported type. |
| OpportunisticTargeting | False, True | Prefer the nearest eligible ordinary target during chase and combat. Rescue remains separate and active casts are not interrupted. |
| AttackLastFullSP | False, True | Require full SP before proactively choosing monsters configured with the Attack Last tactic. |
| DanceMinSP | Number | Minimum SP percentage required for dance attacks when dance attacks are enabled. |
| AttackTimeLimit | Number | Milliseconds allowed before abandoning a target that appears inaccessible or unresponsive. Evidence of successful attacks can refresh the timer. |
| LagReduction | Number | Reduce command frequency. Higher values can noticeably slow reactions; 0 disables this additional reduction. |
| DoNotAttackMoving | False, True | If set to true, Kimi will not attack monsters that are currently moving |
| LiveMobID | False, True | Legacy support for writing observed monster class IDs for a companion AI. This is not required for normal Kimi operation. |

#### AutoSkill Options

| Option | Values | Purpose |
|---|---|---|
| AttackSkillReserveSP | Number | To control SP use, you may not want your Kimi to use skills unless there would be enough SP left to recast some sort of buff. Set this value to this minimum SP value to control this. |
| AutoMobMode | Disabled, Aggressive, All | Controls automatic AoE evaluation, not whether all ordinary targets are selected automatically. Disabled disables that AoE path; Aggressive counts engaged threats; All can count additional eligible nearby monsters. Other skill and tactic gates still apply. |
| AutoMobCount | Number | Minimum qualifying nearby monster count for the automatic AoE path. |
| UseSkillOnly | Attacking, SkillOnly, Chasing | Attacking confines skill use to the attack context; Chasing also permits eligible skills while approaching; SkillOnly requests skills instead of ordinary attacks. Other skill gates and combo precedence still apply. |
| AutoSkillDelay | Number | Extra delay (in milliseconds) added between automatic skill casts. Use 0 to use only cast/spell timing and cooldown windows. Negative values are treated as 0. Actual casts still include the skill's cast time, cooldowns, and SP checks. |
| AoEReserveSP | False, True | Enable this to not use non-AoE skill attacks unless doing so would leave enough SP to cast Kimi's AoE attack (Illusion of Light). |
| AoEFixedLevel | False, True | Enable this to ignore skill level tactics when using AoE attacks. |

#### Kimi Skills - Agile

| Option | Values | Purpose |
|---|---|---|
| IllusionOfClawsLevel | Number | Editable level for Illusion of Claw (8009). Enable it separately; valid levels are 1-10. |
| IllusionOfClawsEnabled | False, True | Enable Illusion of Claw (8009) for Agile Kimi. |
| FadeAwayLevel | Number | Editable level for Fade Away (8012). Enable it separately; valid level is 1. |
| FadeAwayEnabled | False, True | Enable Fade Away (8012) for Agile Kimi. |
| MirageAssaultLevel | Number | Editable level for Mirage Assault (8036). Enable it separately; valid levels are 1-5. |
| MirageAssaultEnabled | False, True | Enable Mirage Assault (8036) for Agile Kimi. |
| Passive Skills (read-only) (`AgilePassiveSkills`) | Read-only | Read-only list of this type's passive skills and maximum levels. Availability in the catalog does not grant learned passives. |

#### Kimi Skills - Occult

| Option | Values | Purpose |
|---|---|---|
| IllusionOfBreathLevel | Number | Editable level for Illusion of Breath (8024). Enable it separately; valid levels are 1-10. |
| IllusionOfBreathEnabled | False, True | Enable Illusion of Breath (8024) for Occult Kimi. |
| IllusionOfLightLevel | Number | Editable level for Illusion of Light (8034). Enable it separately; valid levels are 1-5. |
| IllusionOfLightEnabled | False, True | Enable Illusion of Light (8034) for Occult Kimi. |
| ChaoticHealLevel | Number | Editable level for Chaotic Heal (8014). Enable it separately; valid levels are 1-5. |
| ChaoticHealEnabled | False, True | Enable Chaotic Heal (8014) for Occult Kimi. |
| ChaoticSanctuaryLevel | Number | Editable level for Chaotic Sanctuary (8013), a ground support skill; valid levels are 1-5. |
| ChaoticSanctuaryEnabled | False, True | Enable Chaotic Sanctuary (8013) for Occult Kimi. |
| ArcaneOfferingLevel | Number | Editable level for Arcane Offering (8015), a self support skill; valid levels are 1-5. |
| ArcaneOfferingEnabled | False, True | Enable Arcane Offering (8015) for Occult Kimi. |
| Passive Skills (read-only) (`OccultPassiveSkills`) | Read-only | Read-only list of this type's passive skills and maximum levels. Availability in the catalog does not grant learned passives. |

#### Kimi Skills - Raging

| Option | Values | Purpose |
|---|---|---|
| IllusionOfCrusherLevel | Number | Editable level for Illusion Crusher (8031). Enable it separately; valid levels are 1-5. |
| IllusionOfCrusherEnabled | False, True | Enable Illusion Crusher (8031) for Raging Kimi. |
| BloodSweepLevel | Number | Editable level for Blood Sweep (8032). Enable it separately; valid levels are 1-10. |
| BloodSweepEnabled | False, True | Enable Blood Sweep (8032) for Raging Kimi. |
| Passive Skills (read-only) (`RagingPassiveSkills`) | Read-only | Read-only list of this type's passive skills and maximum levels. Availability in the catalog does not grant learned passives. |

#### Kimi Skills - Common

| Option | Values | Purpose |
|---|---|---|
| BodyDoubleLevel | Number | Editable level for Body Double (8022). Enable it separately; valid levels are 1-5. |
| BodyDoubleEnabled | False, True | Enable Body Double (8022) for Ward, Occult, Agile, or Raging Kimi. |
| QuickDefenseLevel | Number | Editable level for Quick Defense (8023). Enable it separately; valid levels are 1-5. |
| QuickDefenseEnabled | False, True | Enable Quick Defense (8023) for Ward, Occult, Agile, or Raging Kimi. |
| MasterSwapLevel | Number | Editable level for Master Swap (8005). This utility targets the owner; valid levels are 1-5. |
| MasterSwapEnabled | False, True | Enable Master Swap (8005) for Ward, Occult, or Agile Kimi. |

#### Kimi Skills - Ward

| Option | Values | Purpose |
|---|---|---|
| Bastion Renewal Level | Number | Editable level for Bastion Renewal (8006). Enable it separately; valid levels are 1-5. |
| Bastion Renewal Enabled | False, True | Enable Bastion Renewal (8006) for Ward Kimi. |
| WardDomainLevel | Number | Editable level for Ward's Domain (8033), a self centered protection aura; valid levels are 1-5. |
| WardDomainEnabled | False, True | Enable Ward's Domain (8033) for Ward Kimi. |
| TauntLevel | Number | Editable level for Taunt (8021), a self centered area skill; valid levels are 1-5. |
| TauntEnabled | False, True | Enable Taunt (8021) for Ward Kimi. |
| Passive Skills (read-only) (`WardPassiveSkills`) | Read-only | Read-only list of this type's passive skills and maximum levels. Availability in the catalog does not grant learned passives. |

#### Kimi Skills

| Option | Values | Purpose |
|---|---|---|
| OnlyAOE | False, True | Restrict the applicable offensive auto-skill selection to the AoE path. Enable the appropriate AoE skill first. This is not a replacement for configuring explicit combo nodes. |

#### Skill Combo Options

| Option | Values | Purpose |
|---|---|---|
| ComboEnabled | False, True | Enable the regular eight-slot combo. Use ComboRunDuringAttack for the supported attack-driven rotation; this is separate from BlueprintComboEnabled. |
| BlueprintComboEnabled | False, True | Enable graphs from the Combo Tactics editor. See BLUEPRINT_GUIDE.md for graph execution and limitations. |
| ComboRunDuringChase | False, True | Legacy chase toggle. The current execution-context guard rejects CHASE_ST for regular combos, so this option does not make the regular sequence cast during chase. |
| ComboRunDuringAttack | False, True | Run the regular slot sequence while attacking. Enable alongside ComboEnabled to use regular combos. |
| ComboRunDuringIdle | False, True | Legacy idle-context toggle. The current idle handler does not dispatch the regular slot sequence; use a Blueprint idle trigger for graph-based idle actions. |
| ComboResetOnTargetChange | False, True | Restart the regular combo at slot 1 when its target changes. Frequent target changes can therefore repeat early slots. |
| ComboAutoAttackDelay | Number | Delay (ms) after issuing a combo auto-attack before advancing. Counts represent attack commands; continuous attacks may continue while a skill is unavailable. |
| Combo Slot 1 (`ComboSlot1_SkillID`) | Number | Skill for this slot. Select an active skill, Auto-Attack (-1), or an empty slot (0). Encountering an empty slot while advancing returns the regular rotation to slot 1. |
| Combo Slot 1 - Combo Count (`ComboSlot1_ComboCount`) | Number | Number of commands/casts for this slot before advancing. Zero skips the slot. Counts do not prove successful hits or server-side cast acceptance. |
| Combo Slot 2 (`ComboSlot2_SkillID`) | Number | Skill for this slot. Select an active skill, Auto-Attack (-1), or an empty slot (0). Encountering an empty slot while advancing returns the regular rotation to slot 1. |
| Combo Slot 2 - Combo Count (`ComboSlot2_ComboCount`) | Number | Number of commands/casts for this slot before advancing. Zero skips the slot. Counts do not prove successful hits or server-side cast acceptance. |
| Combo Slot 3 (`ComboSlot3_SkillID`) | Number | Skill for this slot. Select an active skill, Auto-Attack (-1), or an empty slot (0). Encountering an empty slot while advancing returns the regular rotation to slot 1. |
| Combo Slot 3 - Combo Count (`ComboSlot3_ComboCount`) | Number | Number of commands/casts for this slot before advancing. Zero skips the slot. Counts do not prove successful hits or server-side cast acceptance. |
| Combo Slot 4 (`ComboSlot4_SkillID`) | Number | Skill for this slot. Select an active skill, Auto-Attack (-1), or an empty slot (0). Encountering an empty slot while advancing returns the regular rotation to slot 1. |
| Combo Slot 4 - Combo Count (`ComboSlot4_ComboCount`) | Number | Number of commands/casts for this slot before advancing. Zero skips the slot. Counts do not prove successful hits or server-side cast acceptance. |
| Combo Slot 5 (`ComboSlot5_SkillID`) | Number | Skill for this slot. Select an active skill, Auto-Attack (-1), or an empty slot (0). Encountering an empty slot while advancing returns the regular rotation to slot 1. |
| Combo Slot 5 - Combo Count (`ComboSlot5_ComboCount`) | Number | Number of commands/casts for this slot before advancing. Zero skips the slot. Counts do not prove successful hits or server-side cast acceptance. |
| Combo Slot 6 (`ComboSlot6_SkillID`) | Number | Skill for this slot. Select an active skill, Auto-Attack (-1), or an empty slot (0). Encountering an empty slot while advancing returns the regular rotation to slot 1. |
| Combo Slot 6 - Combo Count (`ComboSlot6_ComboCount`) | Number | Number of commands/casts for this slot before advancing. Zero skips the slot. Counts do not prove successful hits or server-side cast acceptance. |
| Combo Slot 7 (`ComboSlot7_SkillID`) | Number | Skill for this slot. Select an active skill, Auto-Attack (-1), or an empty slot (0). Encountering an empty slot while advancing returns the regular rotation to slot 1. |
| Combo Slot 7 - Combo Count (`ComboSlot7_ComboCount`) | Number | Number of commands/casts for this slot before advancing. Zero skips the slot. Counts do not prove successful hits or server-side cast acceptance. |
| Combo Slot 8 (`ComboSlot8_SkillID`) | Number | Skill for this slot. Select an active skill, Auto-Attack (-1), or an empty slot (0). Encountering an empty slot while advancing returns the regular rotation to slot 1. |
| Combo Slot 8 - Combo Count (`ComboSlot8_ComboCount`) | Number | Number of commands/casts for this slot before advancing. Zero skips the slot. Counts do not prove successful hits or server-side cast acceptance. |

#### Walk/Follow Options

| Option | Values | Purpose |
|---|---|---|
| FollowStayBack | Number | Your Kimi will stay this many cells behind you when following you. |
| RestXOff | Number | If set to rest, the Kimi will move this many cells east of you when you sit. Setting this to a negative number will cause the Kimi to move west instead. |
| RestYOff | Number | If set to rest, the Kimi will move this many cells north of you when you sit. Setting this to a negative number will cause the Kimi to move south instead. |
| DoNotUseRest | False, True | Set this to false if when you sit down, you want your Kimi to become passive, and, when it becomes idle, will move close to you. |
| SpawnDelay | Number | Upon spawning, the Kimi will wait for this many miliseconds before taking any actions. This prevents it from wasting its immunity time and also prevents it from KSing after teleporting or changing maps. Setting this value to 1000 (1 second) is a good idea. |
| MoveSticky | False, True | Set this to true if you want your Kimi to stay put when told to go somewhere. |
| MoveStickyFight | False, True | Set this to true if you want your Kimi to fight normally if MoveSticky is set to true. |
| UseIdleWalk | None, Circle, Cross, Square, Random, Route_Linear, Route_Circle | Choose an idle movement pattern. Idle movement also depends on HP/SP and other state checks. Route modes require route data; they are not configured simply by selecting the mode. |
| IdleWalkSP | Number | Only use IdleWalk when SP is above this, as a % |
| UseCastleRoute | False, True | Enable this to use castling for route walk. See documentation. |
| RelativeRoute | False, True | Enable this to use relative routes. See documentation |
| IdleWalkDistance | Number | When walking while idle, keep this distance from owner. |
| ChaseSPPause | False, True | Enable this to make the Kimi delay moving to a new target when it is below a specified SP level, AND is expecting an SP-regen tick in the near future. This is an extreme measure to deal with SP problems resulting from the Kimi never staying still long enough to regen any SP (Kimis do not regen sp while moving). WARNING: This will make your Kimi pause after each kill, and this may be undesirable. Use this only if you understand this option. See documentation for details. |
| ChaseSPPauseSP | Number | When ChaseSPPause is enabled, and SP is below this threshold, the Kimi may pause if near an SP tick.If set to a negative number, this is treated as a percentage. Otherwise, it is simply the number of SP. |
| ChaseSPPauseTime | Number | Look-ahead window in milliseconds for the AI's predicted SP recovery tick. A larger value can increase movement pauses; actual SP regeneration is server-controlled. |
| StationaryMoveBounds | Number | This is the farthest from the owner that the Kimi will be allowed to get before dropping all targets and moving back to the owner. This value is used while the owner is NOT moving (see also MobileMoveBounds) To control aggro range of Kimi, use the AggroDist options, not MoveBounds. To set distance from owner that Kimi should return to after killing use FollowStayBack, not MoveBounds. |
| MobileMoveBounds | Number | This is the farthest from the owner that the Kimi will be allowed to get before dropping all targets and moving back to the owner. This value is used while the owner IS moving (see also StationaryMoveBounds) To control aggro range of Kimi, use the AggroDist options, not MoveBounds. This should probably be lower than StationaryMoveBounds to help keep Kimi from being left behind. |

#### Autobuff Options

| Option | Values | Purpose |
|---|---|---|
| UseAutoHeal | Never, Always, Idle, Idle_low | Never disables automatic healing. Always checks healing before ordinary state processing. Idle checks early in idle processing; Idle_low checks later, after targeting/follow decisions. All modes retain HP thresholds, skill-enabled, SP, and cooldown checks. No extra 15-second timer was added. |
| ChaoticHealOwnerHP | Number | Automatic Heal becomes eligible below this owner HP percentage. At or above the threshold it does not trigger on owner HP. The server selects the actual recipient. |
| ChaoticHealKimiHP | Number | Automatic Heal becomes eligible below this Kimi HP percentage. At or above the threshold it does not trigger on Kimi HP. The server selects the actual recipient. |
| UseAutoBD | False, True | Enable automatic Body Double using an HP threshold or timed casting. Requires Body Double Enabled in Skills & Behavior. Available for all four Kimi. |
| BodyDoubleOwnerHP | 0-100 | Owner HP percentage that triggers automatic Body Double. At zero, a positive Cast Interval enables timed casting without an HP check. Both zero disables automatic casting. |
| Body Double Cast Interval | 0-3600 seconds | Seconds between Body Double casts. With HP threshold 0, a positive interval enables timed casting. With a positive HP threshold, that HP condition also applies. Body Double has no built-in cooldown; buff duration does not delay recasting. Saved separately for each Kimi. |
| Bastion Renewal Cooldown | 0-3600 seconds | Seconds between Bastion Renewal attempts on Ward. Positive values override the stored 15-second cooldown; 0 uses that default. The server still decides whether each attempt succeeds. |

#### Kiting Options

| Option | Values | Purpose |
|---|---|---|
| KiteParanoid | False, True | Set this to true if you want your mercenary to kite away frommonsters before being attacked by them |
| KiteStep | Number | Move this many cells when kiting. |
| KiteParanoidStep | Number | Move this many cells when kiting from a monster that.has not yet attacked the mercenary. |
| KiteThreshold | Number | Kite when a monster is within this many cells. |
| KiteParanoidThreshold | Number | Kite when a monster that has not yet attacked is within this many cells. |
| KiteBounds | Number | When kiting do not exceed this distance from owner |
| ForceKite | False, True | Set this to true if you want your Kimi to kite monsters.even if it has no ranged attack. This may result in odd behavior |
| FleeHP | Number | If non-zero and kiting is enabled, only kite below this percent HP |

#### Friending Options

| Option | Values | Purpose |
|---|---|---|
| StandbyFriending | False, True | Allow the legacy standby-and-position method of managing friends. This does not automatically make all nearby players friends. |
| MirAIFriending | False, True | Set this to true if you want the Kimi to emulate MirAI friending. |
| FriendAssistDamageHPThresholdOwner | Number | The custom hook enables owner damage-assist behavior at or below this HP percentage. Zero stops that threshold-based toggling; the underlying FriendAttack setting still matters. This is separate from Rescue Owner. |

#### Standby Options

| Option | Values | Purpose |
|---|---|---|
| DefendStandby | False, True | Set this to true if you want your Kimi to defend you even while in standby. |
| StickyStandby | Disabled, Enabled, Enabled_Relog | Disabled clears sticky standby behavior; Enabled returns to standby after temporary actions; the persistence option also stores standby state between sessions. |

#### Berserk Options

| Option | Values | Purpose |
|---|---|---|
| UseBerserkMobbed | Number | Enable berserk behavior when the number of attackers exceeds this threshold. Zero disables this trigger. |
| UseBerserkSkill | False, True | Set to true to have your Kimi go berzerk when told to use a skill on a target. |
| UseBerserkAttack | False, True | Set this to true to have your Kimi go berzerk when told to attack a target. |
| Berserk_SkillAlways | False, True | Set this to true to ignore skill use limits while in berzerk mode. |
| Berserk_Dance | False, True | Set this to true if you want your Kimi to use dance attack while in berzerk mode. |
| Berserk_IgnoreMinSP | False, True | Ignore the normal minimum/reserve SP policy while in berserk mode. Actual skill SP cost and server restrictions still apply. |
| Berserk_ComboAlways | False, True | Legacy berserk combo policy; it does not enable either the regular combo or Blueprint system. |

#### PVP Options

| Option | Values | Purpose |
|---|---|---|
| PVPmode | False, True | Set this to true if you want to use your Kimi in PVP enabled maps. |

### Regular combo setup

Regular combos are configured in **Kimi Settings â†’ Skill Combo Options**, not on the node canvas. Enable ComboEnabled and ComboRunDuringAttack. Select a skill and count for each consecutive slot. Keep later unused slots empty. Enable the required skills in the skill categories before applying.

A simple Occult test sequence is Auto-Attack Ã—1, Illusion of Breath Ã—1, then an empty slot. Once that works, add support skills deliberately. Adding Chaotic Heal also adds its substantial SP requirement; it is not conditional healing merely because automatic healing uses thresholds.

ComboResetOnTargetChange restarts the sequence on a new target. ComboAutoAttackDelay is a timer between issued attack commands and progression. The runtime also has ComboSkillCastDelay, an internal minimum skill-step delay (500 ms in the clean profile); actual cast time can be longer. It is not a browsable property in the current compiled grid.

The regular combo's chase and idle controls are inherited: the current chase guard rejects regular combo execution in CHASE_ST, and the idle handler does not dispatch the regular sequence. Use the supported attack context. Blueprint graph triggers are a separate mechanism.

## Blueprint (Shared)

This is the visual Blueprint editor. It supports graph construction, node properties, connections, and graph persistence. See [BLUEPRINT_GUIDE.md](BLUEPRINT_GUIDE.md) for each node, execution rules, examples, and discrepancies between exposed options and runtime support.


## Kimi Tactics

Kimi tactics select behavior by monster ID. The row with ID 0 is the runtime fallback when no more specific row is available. The list displays each row's Monster Name.

### Row actions

- **Add** creates a new row with ID 1 and the default tactic values. A double-click in the tactic list performs the same action.
- **Remove** deletes the selected row from the in-memory list. The ID 0 row cannot be removed. Select a different row before changing its ID or name.
- **Monster Name** is the display name written as the comment on the tactic line. It may not be empty when validated.
- **Monster ID** is the numeric monster/class key. IDs must be unique. ID changes are applied as the text is edited, so finish the edit with a valid unique number.

### Overall Tactics fields

**Basic Behavior** controls whether and when this monster can be selected.

| Choice | Runtime meaning |
| --- | --- |
| Tank - Pull | Existing Tank behavior: tag a monster to draw attention, then stop attacking once it targets Kimi. Normal following remains available. |
| Tank - Gather | Collect eligible monsters near Kimi, then use enabled skills or the attack combo in place. Ward uses enabled Taunt from its radius when ready; during cooldown, basic hits can tag more targets unless Skill Only is enabled. |
| Ignore | Do not attack the monster. |
| Attack (low) | Seek and attack at low priority. It does not take a higher priority while already attacking. |
| Attack (medium) | Seek and attack at medium priority. |
| Attack (high) | Seek and attack at high priority. |
| React (low) | React when the self, owner, or a friend is attacked, at low priority. |
| React (medium) | React when the self, owner, or a friend is attacked, at medium priority. |
| React (high) | React when the self, owner, or a friend is attacked, at high priority. |
| React (self) | React only when the Kimi itself is attacked. |
| Snipe (low) | Attempt a one-shot bolt attack while another monster is being attacked, at low priority. |
| Snipe (medium) | The same snipe attempt at medium priority. |
| Snipe (high) | The same snipe attempt at high priority. |
| Attack (low) React (medium) | Treat the monster as low-priority attack work normally, then use medium reaction priority when it is actively attacking. |
| Attack (last) | Use the runtime's special last-attack priority. The runtime may require full SP when the global AttackLastFullSP setting is enabled. |
| Attack (top) | Use the runtime's top attack priority. |

**Use Attack Skills** determines how many configured attack skills may be used for this target.

| Choice | Value written and meaning |
| --- | --- |
| Always | SKILL_ALWAYS (100): continue using eligible attack skills. |
| Never | SKILL_NEVER (0): do not use attack skills for this target. |
| This many times: | A nonnegative count, from 0 through 100 in the control. |
| Once; level: | A negative value. The entered level 1 through 5 is written as -1 through -5; the runtime interprets its absolute value as the skill level for the one cast. |

The numeric field is disabled for Always and Never. A numeric value of 100 in the count mode has the same runtime value as Always.

**Kiting** has three choices:

- **Never** never kite this monster.
- **React** kites only when the monster is actually attacking the self, owner, or a friend.
- **Always** kites whenever the runtime's distance and threshold checks require it.

Kiting requires KiteMonsters = 1. For Breath and Light, standard combos, Blueprint skill nodes, and automatic attacks use positioning bounded by the skill casting range. Kite Never disables tactical retreat; Kite React requires an enemy targeting Kimi, its owner, or a friend. Snipe OK independently requests ranged casting distance. Melee and normal attack steps retain their own attack range.

**Skill Class** selects which kind of attack skill the runtime may use.

| Choice | Skill class |
| --- | --- |
| Any skill | Main and Homun S attack classes. |
| Non-Homun S skills | Main or pre-S/old skills. |
| Homun S skills | Homun S skills. |
| AoE attacks | Mob/AoE attacks. |
| Combo (first skill) | The first-skill combo class. |
| Combo (full combos) | The full-combo class. |
| Summon minions | Minion-summoning skills. |
| Grapple (Tinder Breaker) | Tinder Breaker grapple class. |
| Grapple (CBC) | CBC grapple class. |
| Grapple (EQC) | EQC grapple class. |
| Minions + Pre-S skill | Minion plus pre-S/old skill class. |
| Minions + Homun S | Minion plus Homun S skill class. |

The class is a filter for the skills returned by the runtime. It does not create a skill or make a missing skill available.

**Combos and tactics:** A combo supplies the action order; the target's tactics still control offensive skill permission, cast count, skill class, and SP reserve. Never or an incompatible class skips that offensive step. A temporary SP reserve shortage waits on the step. Once uses one offensive cast at no more than the specified level; numbered limits count offensive combo casts against the current target across combo repeats. Support healing and buffs do not consume that offensive allowance. AoE attacks permits AoE skills; legacy classes without a matching Kimi skill do not gain one by enabling a combo. Rescue may change the target, and the configured combo target-change behavior still applies.

**Rescue** controls which configured target a monster may be pulled away from when the AI enters rescue handling.

| Choice | Runtime target |
| --- | --- |
| Never | No rescue from this row. |
| Friends | A target classified as FRIEND. |
| Owner's merc | A target classified as RETAINER. |
| Self (homunculus) | The Kimi itself. |
| Owner | The owner. |
| All of the above | Friend, retainer, owner, and other friend/self targets accepted by the runtime; the self is excluded by the final all-target check. |

A separate RescueOwnerLowHP setting is checked before this row. A positive value forces owner rescue below that HP percentage; the row may still allow rescue above it. For threshold-only rescue, select Rescue = Owner and set RescueOwnerLowHP to the negative threshold (for example, -80 suppresses owner rescue above 80% HP). Zero leaves rescue to the row. Follow state blocks rescue. The AI relies on the attacker target reported by the client; an HP drop alone does not identify the attacker.

**SP** is the reserve used while selecting attack skills. The control accepts -1 through 1000. -1 means use the global AttackSkillReserveSP value; a nonnegative value reserves that many SP for this target.

**Weight** is a decimal from 0.0 through 20.0 in 0.1 steps. The runtime multiplies mob counts and area-target position weights by this value. A weight of 0 removes the monster from those weighted counts, although the Basic Behavior check still sees its nonzero tactic.

**Kill-steal** controls the target ownership check:

- **Never** uses the normal anti-kill-steal checks.
- **Always** treats the monster as free to attack.
- **Polite** keeps the normal checks and also declines moving targets.

**Chase** controls movement toward this target:

- **Normal** follows the global DoNotChase setting.
- **Always** chases regardless of the global no-chase setting and does not pause for the low-SP chase check.
- **Never** returns the no-chase result.
- **Pause for SP** follows normal chase behavior until the runtime's low-SP pause condition is met, then pauses.

**Snipe OK** keeps Illusion of Breath and Illusion of Light near their maximum casting range on the current target, including during standard and Blueprint combos. Positioning aims for the outer one-cell band of the skill range, within owner movement bounds. It does not insert a skill against another target. Clearing it disables this distance preference; enabled tactical kiting may still reposition Kimi. A normal attack or melee combo step closes to its own required range.


If a ranged retreat makes no positional progress for 500 ms while the target remains in casting range, Kimi stops repeating that retreat and allows casting from its current position. Positioning is reconsidered when either actor moves. This movement check does not add a skill cooldown; normal skill readiness and tactic restrictions still apply. It does not allow casting at an out-of-range target.

**React** controls reactions to a monster's cast:

| Choice | Runtime meaning |
| --- | --- |
| No | Passive cast handling. |
| React to casts | Generic cast reaction. |
| React w/Any | React with any eligible targeted skill. |
| React w/Breeze | React with Silent Breeze. |
| React w/Old | React with a main/old (pre-S) skill. |
| React w/S | React with a Homun S skill. |
| React w/Mob | React with a mob/AoE skill. |
| React w/Debuff | React with a debuff skill class. |
| React w/Minions | React with a minion skill class. |

React to casts makes an actually casting target eligible for cast reaction. Specialized reactions now enter the numeric skill-command state correctly and remain blocked during Follow state. React w/Any is the generic mode that selects the first eligible skill from the current targeted-skill list. The current cast-reaction loop does not match the category codes returned by GetTargetedSkills for the Old, S, Mob, Debuff, or Minions choices, and the returned list has no exact Breeze skill entry. Those specialized choices are therefore serialized but do not select their named skill class in this build.

**Debuff** and **While** are intended to select an automatic debuff during chasing or attacking. The visible Debuff choices are **Never**, **Any**, **Silent Breeze**, and **Volcanic Ash**; While is **Chasing** or **Attacking**.

The current Kimi editor has a mapping defect in this pair. Chasing combinations map to Never, Any, Silent Breeze, and Volcanic Ash as expected. For Attacking, the getter maps Never to the chasing Ash value, Any to automatic attacking debuff, Silent Breeze to attacking Breeze, and Volcanic Ash to the chasing Ash value. Use a manual H_Tactics.lua edit when an exact DEBUFF_ASH_A or other exact attacking value is required.

The current runtime also returns only main, Homun S, and mob entries from GetTargetedSkills. No DEBUFF_ATK entry reaches the auto-skill selectors in this build, so this setting is serialized but does not by itself produce an automatic debuff cast.

The Kimi tab does not display Use Pushback. H_Tactics.lua still stores a pushback slot for file compatibility, but the current AI runtime has no TACT_PUSHBACK handling, so changing that slot manually has no effect in this build.

## PVP Tactics

PVP tactics are used for player-target lookups when PVPmode is nonzero. Monster targets continue to use Kimi tactics. The PVP list displays each row's key rather than a name.

### Target rows and actions

The **Player ID or Friend Class** field accepts an exact player ID or one of the classification keys below.

| Key | How the runtime uses it |
| --- | --- |
| 0 | Default PVP row when no more specific row supplies a value. |
| Exact player ID | Takes precedence for that player. |
| KOS | Kill-on-sight classification. |
| ENEMY | Hostile classification. |
| NEUTRAL | Neutral classification. |
| FRIEND | Friend classification. |
| ALLY | Friendly classification. |
| RETAINER | Mercenary/retainer classification. |

For a player target, the runtime first checks an exact player-ID row, then the friend classification. For a hostile target on a Kimi, it can use a Kimi class row. Missing fields fall back to row 0. The classifications are keys; Basic Behavior still determines the action.

- **Add** creates a row with the temporary key "new". A double-click in the list does the same.
- **Remove** removes the selected row from the in-memory list. The row with key 0 cannot be removed.
- A new row's key must be changed to a nonempty unique ID or classification key before validation.

### Overall Tactics fields

**Basic Behavior** has the same fifteen choices and runtime meanings as Kimi Tactics: **Tank**, **Ignore**, **Attack (low)**, **Attack (medium)**, **Attack (high)**, **React (low)**, **React (medium)**, **React (high)**, **React (self)**, **Snipe (low)**, **Snipe (medium)**, **Snipe (high)**, **Attack (low) React (med)**, **Attack (last)**, and **Attack (top)**. The PVP label uses the shorter “React (med)” wording for the combined choice.

**React** has the same cast-reaction choices as Kimi: **No**, **React to casts**, **React w/Any**, **React w/Breeze**, **React w/Old**, **React w/S**, **React w/Mob**, **React w/Debuff**, and **React w/Minions**. Their intended meanings are the same cast classes described above. In the current runtime, React w/Any is the generic mode that can select the first eligible targeted skill; the named Old, S, Mob, Debuff, Minions, and Breeze specializations do not match a skill in the current cast-reaction list.

**Use Attack Skills** has **Always**, **Never**, **This many times:**, and **Once; level:**. Always writes SKILL_ALWAYS, Never writes SKILL_NEVER, a count writes a nonnegative number, and Once; level: writes the negative of the entered level. The PVP control's once-level numeric field is currently configured from 1 through 99, even though the runtime uses the value as an absolute skill level and the skill itself supplies the practical maximum. Imported negative once-level values are assigned directly to this positive-range control and are not a reliable import path.

**Kiting** has **Never**, **React**, and **Always**, with the same row meanings as Kimi. It still requires global KiteMonsters = 1.

**Use Debuffs** displays **Never**, **Any**, **Silent Breeze**, and **Volcanic Ash**, with **Chasing** or **Attacking** beside it. The intended meanings are the same as Kimi, but the PVP editor currently uses a two-item multiplier for a four-item primary list. After selecting Attacking, the visible labels serialize as Never = DEBUFF_BREEZE_C, Any = DEBUFF_ASH_C, Silent Breeze = DEBUFF_ANY_A, and Volcanic Ash = DEBUFF_BREEZE_A. Changing While alone has no change handler. The editor cannot reliably author the exact attacking Ash value; edit H_PVP_Tact.lua manually when exact debuff constants are needed. As with Kimi, the current GetTargetedSkills result contains no debuff entry, so this setting does not by itself produce an automatic debuff cast.

**Use Pushback** contains **Never**, **Off self**, and **Off self + friend**, corresponding to no pushback, pushing the self clear, or pushing the self and a friend clear. Both the label and combo box are hidden in the current PVP tab, so these choices cannot be changed there. The current AI runtime has no TACT_PUSHBACK handling, so file values do not produce a pushback action in this build.

**Rescue** displays **Never**, **Rescue All**, and **Rescue Retainer**. In the current control, Never writes RESCUE_NEVER, Rescue All writes RESCUE_FRIEND (only the FRIEND classification), and Rescue Retainer writes RESCUE_RETAINER. Self, Owner, and RESCUE_ALL values are not selectable in this three-item list. Use H_PVP_Tact.lua for an exact rescue constant.

**Skill Class** is displayed as:

**Any Attack**, **Old Homun**, **Homun S**, **Combo (once)**, **Combo (full)**, **Summon Minions**, **Grapple (Tinder Breaker)**, **Grapple (CBC)**, **Grapple (EQC)**, **Minion+PreS Skills**, and **Minion+Homun S Skills**.

The current getter has twelve enum cases but the list has only eleven entries. The actual values written by the visible entries are:

| Visible label | Value written by the current getter |
| --- | --- |
| Any Attack | CLASS_BOTH |
| Old Homun | CLASS_OLD |
| Homun S | CLASS_S |
| Combo (once) | CLASS_MOB |
| Combo (full) | CLASS_COMBO_1 |
| Summon Minions | CLASS_COMBO_2 |
| Grapple (Tinder Breaker) | CLASS_MINION |
| Grapple (CBC) | CLASS_GRAPPLE |
| Grapple (EQC) | CLASS_GRAPPLE_1 |
| Minion+PreS Skills | CLASS_GRAPPLE_2 |
| Minion+Homun S Skills | CLASS_MIN_OLD |

CLASS_MIN_S has no visible PVP option. The labels from Combo (once) onward therefore do not match the enum values they appear to name; use manual H_PVP_Tact.lua editing for a specific class.

PVP rows contain no controls or fields for SP reserve, Weight, Kill-steal, Chase, or Snipe OK. A lookup for one of those missing fields falls through to PVP row 0 and then returns 0 when that field is absent. The Basic Behavior snipe labels are still visible, but the ranged monster positioning implemented here does not apply to PVP targets.

## Extra Options

The tab contains one disabled selector showing **Kimi**, a multiline vertical-scroll editor, and the note “Edit Kimi skill levels and custom options not supported by the GUI.” The editor displays the contents of H_Extra.lua as plain text. It does not parse or validate Lua.

The shipped H_Extra.lua contains these user-editable areas:

- **Logging:** COMBO_INIT, COMBO_ADVANCE, COMBO_AUTO_ATTACK, COMBO_CAST, COMBO_SP, COMBO_COOLDOWN, COMBO_ERROR, COMBO_RESET, ATTACK_ST, BP_COMBO, AAI_SKILLFAIL, AAI_CostSP, AAI_MOBCOUNT, AAI_ERROR, AAI_DEBUG, AAI_TACTICS, AAI_KITING, AAI_TARGETING, AAI_AVOIDANCE, AAI_TIMING, and AAI_STATE. Each LogEnable entry is 1 to enable or 0 to disable. Logs are written under USER_AI/ErrorLog as AAI_*.log. The shipped file enables the combo, general, and state entries listed with 1 and leaves tactical and timing entries listed with 0.
- **OnInit:** a hook called once during AI initialization. The shipped function logs key configuration flags.
- **GetMyTact:** a commented example hook for returning a dynamic tactic table. It is not active unless uncommented and customized.
- **Skill levels:** H_Extra.lua only points to H_SkillList.lua for Kimi skill levels; the Extra tab does not edit skill levels.
- **Kimi type constants:** comments identifying WARD (1), OCCULT (2), AGILE (3), and RAGING (4). These comments are reference only.
- **Blueprint combo loading:** when BlueprintComboEnabled is 1, the file tries the configured exported combo path under ConfigPath/data and logs whether it loaded. When it is 0, it logs that blueprint combos are disabled.
- **FriendAttack:** MOTION_ATTACK, MOTION_ATTACK2, MOTION_SKILL, MOTION_CASTING, MOTION_DAMAGE, MOTION_TOSS, MOTION_BIGTOSS, and MOTION_FULLBLAST. Each entry is 1 to assist on that owner/friend motion or 0 to ignore it. The shipped file enables MOTION_DAMAGE only.
- **FriendAssistDamageHPThresholdOwner:** an owner HP percentage gate for MOTION_DAMAGE. A positive value toggles damage assistance at or below that percentage; 0 disables the gate. The shipped default is 50.
- **Helper functions:** GetNearbyMobs and GetNearbyMobCount return nearby monster data for custom Lua. OnAttackStart runs when entering ATTACK_ST. OnAImiddle runs each AI cycle. OnAutoBuffs returns 1 in the shipped file so normal state processing continues. OnFailUnknownMode is an empty hook for custom handling.

### Extra Options persistence

ExtraControl.Save and ExtraControl.Save(string) are no-ops. Apply Settings, the save-on-quit Yes path, and the Extra control itself do not write edited text back to H_Extra.lua. The text edit still marks the form as changed, so the user can see Apply enabled even though applying cannot persist it. Edit H_Extra.lua manually and restart the GUI to load the saved text.

## Main window actions and menus

The main window has nine tabs: **Kimi Settings**, **Ward**, **Occult**, **Agile**, **Raging**, **Kimi Tactics**, **Blueprint (Shared)**, **PVP Tactics**, and **Extra Options**. Each named Kimi tab contains its standard Combo page. The buttons and menu actions are:

| Menu or control | Shortcut | Action |
| --- | --- | --- |
| File > Import > Kimi Settings... | — | Loads a saved Lua settings file into the Kimi Settings grid. Review the values, then click Apply to activate them in this client. |
| File > Import > Kimi Tactics... | — | Opens a Lua file and loads its Kimi tactic rows. |
| File > Export > Kimi Settings... | — | Saves the current Kimi settings to a named Lua file for later import. This does not activate the exported file. Tactics and Blueprint files are saved separately using their own controls. |
| File > Export > Kimi Tactics... | — | Writes the current Kimi tactic collection to a selected Lua file. |
| File > Exit | — | Closes the window. If changes are marked dirty, it asks whether to save. |
| Edit > Apply Settings | Ctrl+S | Runs the full save path for settings, Kimi tactics, combo settings, PVP tactics, and skill list. Extra Options is included in the call but its Save method is a no-op. |
| Edit > Revert | Ctrl+R | Reloads shared settings and per-Kimi automatic and standard combo profiles from H_Config.lua. It does not reload tactics or the Blueprint graph. |
| Edit > Reset to Defaults | Ctrl+D | Resets shared settings and per-Kimi profiles in the editor. Apply is still required to save. |
| Help > Documentation | F1 | Opens Documentation.html when present, otherwise Documentation.pdf. If neither file exists, it shows an error. |
| Help > About... | — | Shows the Kimi AI Config version, Azzy AI credit, Kimi conversion credit, copyright, server, and Kimi-only support text. |
| Apply Settings button | — | Same action as Edit > Apply Settings. |
| Quit button | — | Same close-and-save prompt as File > Exit. |

The top-level menus use Alt+F for File, Alt+E for Edit, and Alt+H for Help.

Kimi Tactics and Kimi Settings are the only import/export targets exposed by File. There are no PVP, Combo, or Extra import/export menu items.

The save prompt has Yes, No, and Cancel buttons, but the current quit handler closes the form after the prompt branch as well. Selecting Cancel therefore closes the window without saving instead of keeping it open.

PVP_Tact.Save updates or appends rows in H_PVP_Tact.lua but does not remove old rows that are no longer in the editor list. Removing a PVP row can therefore leave its old line in the file. The Kimi and PVP tactic loaders append to their static in-memory collections; repeated imports in one GUI session can show duplicate rows.

## Kimi-specific tabs

Kimi Settings contains the shared behavior settings, including movement, targeting, and general behavior. Ward, Occult, Agile, and Raging have separate colored tabs for their skills and related behavior options. Opening a tab changes only the editor view; it does not select a different active Kimi.

Each Kimi has its own standard combo profile. Existing standard combo settings belong to the configured Kimi when an older configuration is first opened. Review the appropriate Kimi tab before enabling a new combo. Skills shared by multiple Kimi retain their shared settings where indicated.

The character portraits use the supplied game screenshots. Ward now uses the supplied Ward screenshot.
The Blueprint editor remains shared and is labeled Blueprint (Shared). The independent profiles in each Kimi's Combo page control the standard slot-based combo system.


Each Kimi tab now includes Automatic Skills. These options are saved independently for each Kimi and are removed from the shared Kimi Settings grid. Healing options appear only for Occult. Shared Blueprints can branch using a Kimi Type condition (Ward 1, Occult 2, Agile 3, Raging 4).


## Ward healing and the 30-cell boundary

In Ward > Skills & Behavior, enable Bastion Renewal and select its learned level. In Automatic Skills, choose UseAutoHeal (Always, Idle, or Idle_low), and set Bastion Renewal HP Threshold. Never disables automatic healing. The default threshold is 100%; automatic healing stops at or above the threshold. Bastion uses Bastion Renewal Cooldown directly when positive; 0 uses the stored 15-second cooldown. A zero HP threshold disables automatic Bastion healing. Legacy UseWarmDef and WarmDefCooldown values remain compatible saved fields but no longer control automatic Bastion casting. Explicit combo casts retain their own sequence and do not use this automatic HP threshold. Bastion keeps skill ID 8006 and compatible WarmDef setting keys.

StationaryAggroDist and MobileAggroDist control target detection distance from the owner. StationaryMoveBounds and MobileMoveBounds control the movement leash. All four accept up to 30 cells; KiteBounds also accepts 30. Use matching values for a full 30-cell area. FollowStayBack controls how close Kimi returns, not its fighting range. After a target dies or disappears, Kimi returns to the owner if no eligible target is found, before starting idle walking.

Enable OpportunisticTargeting for nearest eligible ordinary targets. DoNotChase must be 0, and the monster tactic must permit chasing and the intended attacks. Rescue and owner/friend assistance retain their separate handling. These settings do not increase a skill's casting range.

The AI no longer rejects movement solely because the destination exceeds the old 14-cell owner limit. Long travel uses short movement requests. The client still supplies visible actors and performs pathfinding: 30 is an AI boundary, not a guarantee of 30-cell visibility or a route around every wall. A detected monster behind a wall may remain unreachable.

The included Ward-30-cell-settings.lua can be loaded with File > Import > Kimi Settings, reviewed, then applied. It selects Ward, enables level-1 Bastion with Always healing below 100% HP and no extra interval, enables opportunistic targeting, disables idle walking, and sets the five distance controls to 30. Review skill levels and other preserved profiles before applying it. On this build, OldHomunType 1 is Ward; 3 is Agile.


UseAutoBD replaces the old Use Body Double automatic toggle. Existing files retain their previous value when imported. Body Double Enabled controls whether the skill can be used; UseAutoBD controls automatic HP-based or timed casting. Automatic checks run during combat and idle states independently of offensive buff mode. Zero owner HP threshold uses timed casting when Body Double Cast Interval is positive; both zero disables automatic casting. Cooldown settings apply to automatic, combo, and manual requests; explicit casts do not require the automatic HP threshold. Master Swap is enabled through Skills & Behavior. Ward, Occult, and Agile expose MasterSwapOwnerHP and MasterSwapCooldown under Automatic Skills. A zero threshold disables automatic swapping; a positive threshold casts at or below that owner HP percentage, subject to skill availability and cooldown. UseMasterSwap is no longer a second enable gate.

## Detours during blocked combat movement

Before the normal chase give-up or attack-timeout fallback, Kimi can try sideways waypoints on either side of the approach line, at offsets of 2, 4, 6, and 8 cells. The fallback allows at most eight candidates within a ten-second attempt window. Each issued waypoint gets up to one second for movement before trying another. Reaching a waypoint resumes normal target approach.

Waypoints must remain within the configured owner movement boundary (maximum 30), avoid known occupied cells, and obey the monster's Chase setting. Lost, dead, or disallowed targets cancel the detour. If no candidate works, normal unreachable-target handling resumes. This is a limited movement fallback; the AI has no full map-wall grid and cannot guarantee a route around long walls, enclosed areas, or obstacles requiring travel outside the leash. The client must still execute each movement request. In-game terrain testing is required.

### Returning to the owner and 100-cell limits

Detection distances, movement bounds, and kite bounds support up to 100 cells. These limits do not increase skill casting range or the client's actor visibility. Off-screen combat is possible only while the client continues to report the target; terrain data cannot locate unseen monsters.

With the correct map selected, normal movement uses terrain routing. Following and emergency return choose a reachable tile near the owner instead of a potentially blocked geometric destination. Return recovery can include Kimi's position outside the combat leash when the owner has moved away. No teleport command is added.

Navigation uses bounded A* searches and rejects exact destinations inside walls without searching. An unchanged failed route is cached briefly. Especially complex routes can exceed the search budget; this protects the game from a long synchronous AI calculation.

## Optional debug logging

Use **Kimi Settings > Debugging > Enable Debug Logging**. **False** (the default) disables recurring trace and category log writes. Set **True** when collecting logs for troubleshooting; existing category settings then control category output. Click Apply and reload the AI after changing it. Normal game error dialogs and the startup status report remain available. Existing logs are retained.

With OpportunisticTargeting enabled, nearest eligible target selection runs before terrain routing, including when a skill is selected. It does not reset movement when the same target remains nearest. Nearest means distance from Kimi among eligible candidates; known unreachable targets are deprioritized. It is not a comparison of complete walking-route lengths. Active casts and rescue priorities remain protected. Long terrain searches are continued across AI updates to limit per-update work.

### Manual skills with combos enabled

An explicit skill command takes priority over automated combos and target switching, subject to the skill validity, casting, spawn, and movement-bound checks. Temporarily rejected casts are retried within the command retry limit. A manual command does not enable the skill or bypass SP requirements. Offensive automatic skill selection remains suppressed during an active combo; include Blood Sweep as a combo step when it should be part of that sequence.

Owner return uses walkability, not just straight-line distance or sight. A nearby owner across a gap is not considered reached. Return searches may go beyond combat movement bounds to get home; the selected map and bounded search workload still apply. An unfinished return search finishes toward its saved owner position before replanning toward the latest position.

### Troubleshooting Bastion automatic healing

If Ward remains below the Bastion Renewal HP Threshold with Auto Heal set to Always and Bastion enabled, temporarily enable Debug Logging. Messages beginning `Bastion auto-heal:` identify skill delays, reuse waits, disabled skills, insufficient SP, or cast requests. These messages are limited to one every five seconds. A cast request means the AI issued the request; it does not confirm server acceptance. Disable logging after testing. Lag Reduction now sends an automatically queued heal before the AI ends that update.

### Shared support and AoE controls

Chaotic Heal uses its skill Enabled switch, UseAutoHeal timing mode, and its HP thresholds. The redundant UseChaoticHeal switch is hidden and no longer blocks healing. Zero disables the corresponding HP trigger.

OnlyAOE is available for Ward, Occult, and Raging. Automatic offensive selection uses Taunt, Illusion of Light, or Blood Sweep respectively. Offensive combo steps must be AoE skills; support steps retain their own rules. This setting does not disable basic attacks; UseSkillOnly controls those. Agile does not expose OnlyAOE because its current catalog has no active AoE skill.


### Tank – Gather

Choose Tank - Gather in Kimi Tactics for the monsters to collect. Set AutoMobCount to the desired group size and TankMonsterLimit up to 30 as its cap. Ward needs Taunt Enabled, a usable level, sufficient SP, and a tactic that permits the skill. Gathering respects Taunt's radius and cooldown; Taunt cannot affect bosses under the server's rules. Terrain routing and the configured owner leash still apply.

Gathering switches to combat when AutoMobCount is reached, capped by TankMonsterLimit. If fewer mobs are available and no nearby eligible targets remain to gather, Kimi starts combat with the smaller group. Gathering Taunt also requires that many eligible mobs within its radius. It uses the attack combo when enabled, otherwise permitted automatic skills and basic attacks. It stays with the collected group instead of returning to the owner between tagged targets. Once the group is gone, ordinary targeting and following resume. Skill Only prevents basic-hit tagging during Taunt cooldown. Other Kimi can gather with basic hits; Taunt is Ward-specific. The limit is a desired count, not a guarantee that an AoE will attract exactly that many monsters.

Body Double timed example: enable Body Double and UseAutoBD, set BodyDoubleOwnerHP to 0 and Body Double Cast Interval to 30. Kimi requests a cast as soon as ready, then waits at least 30 seconds between requests without checking owner HP. Buff duration does not delay recasting. SP availability and current cast timing can delay a request. The saved Lua key remains BodyDoubleCooldown for compatibility.


Body Double does not check whether its buff successfully applied or is still active. Automatic casting uses the selected interval and HP mode, skill enable switches, SP, and current cast timing. The interval is measured from the AI cast request; server acceptance is not confirmed.
