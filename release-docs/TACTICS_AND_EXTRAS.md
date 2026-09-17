# Tactics and Extra Options

This page describes the controls in the Kimi Tactics, PVP Tactics, and Extra Options tabs of KimiAIConfig. A control change edits the selected row in memory. Apply Settings writes the tactic files together with the other configuration files.

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
| Tank | Hit the monster once, then hold it until something kills it. |
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

