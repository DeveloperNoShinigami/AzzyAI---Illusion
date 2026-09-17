# Blueprint Combo Guide

This guide documents the Blueprint (Shared) editor in the Refuge testing
build. It describes the toolbox implemented by `GUI/AzzyAIConfig/NodeLibrary.cs`
and `ComboTactControl.cs`, and the behavior currently implemented by the
canonical runtime at `USER_AI/AI_main.lua`. A node being visible in the editor
does not by itself mean that the runtime implements every advertised behavior.

## Monster tactics also apply

Blueprint offensive skill nodes obey the selected monster's skill permission, cast limit, skill class, and SP reserve. A blocked offensive step is skipped; an SP reserve shortage waits. Support actions retain their separate controls. A monster-specific row overrides Default, so a Never row can prevent a skill-only Blueprint from attacking even after rescue selects that monster.

Snipe OK positions Illusion of Breath and Illusion of Light near casting range on the current target. Normal attack and melee nodes retain their own range. Rescue target selection still follows the HP threshold and Follow-state block; it does not guarantee the selected attack is permitted.


If a ranged retreat makes no positional progress for 500 ms while the target remains in casting range, Kimi stops repeating that retreat and allows casting from its current position. Positioning is reconsidered when either actor moves. This movement check does not add a skill cooldown; normal skill readiness and tactic restrictions still apply. It does not allow casting at an out-of-range target.

## Runtime setup

Blueprints use two files:

- `combo_tactics.cbp` is the editor's JSON save file. It is read by the GUI,
  not by the game AI.
- An exported Lua file defines `ComboTactics1`, `ComboTactics2`, and so on.
  The game loads that Lua file from `USER_AI/data` when `H_Extra.lua` calls
  `dofile`.

The current `H_Extra.lua` looks for
`data/ComboTactics_Heal_Sustain_DPS_Combo.lua`. The Export Lua dialog defaults
to `ComboTactics.lua`, so either save the export using the filename currently
listed in `H_Extra.lua` or update that candidate list deliberately. Exporting
does not install or load the Lua file automatically.

Enable `BlueprintComboEnabled` in the Kimi configuration and click Apply. Apply
writes `BlueprintComboEnabled = 1` to `H_Config.lua`, saves the editor state as
`combo_tactics.cbp` beside the executable, and refreshes the embedded runtime
files. A fresh testing profile has Blueprint combos disabled, skill enabled bits
off, displayed skill levels at 1, and regular combo slots blank. Enable the
specific active skills needed by the test. Reload the client's user AI after
Apply; an already loaded AI does not necessarily reread the files.

`ComboEnabled` controls the older four/eight-slot combo system. It is separate
from `BlueprintComboEnabled`. The `ComboRunDuringChase`,
`ComboRunDuringAttack`, `ComboRunDuringIdle`, `ComboResetOnTargetChange`, and
`ComboAutoAttackDelay` settings belong to that older system; the Blueprint
dispatcher does not use those flags to decide whether its three state calls run.
For a clean Blueprint test, leave regular combos and unrelated auto-skill paths
off so that they cannot obscure the result.

## Building a graph

Drag a toolbox entry onto the canvas. Select a node to edit its properties in
the PropertyGrid. Drag from an output pin to an input pin. Execution pins are
triangles; data pins are circles. White is execution, light blue is condition
data, and red is target data. The right-click menu can duplicate or delete a
node or delete a connection. `Ctrl+C`/`Ctrl+V`, `Ctrl+Z`/`Ctrl+Y`, marquee
selection, Delete, middle-button or space-drag panning, and mouse-wheel zoom
are also implemented.

Connections are stored by node and pin GUID. The editor's suggestion menu uses
node type and pin type to recommend compatible nodes. A freehand drag only
checks that one endpoint is an input and the other is an output; it does not
enforce pin-type compatibility. Use the suggestions and inspect the exported
Lua when a connection is important.

For execution pins, the editor removes an existing connection from the source
or target pin before adding the new one. Thus an execution pin has one outgoing
or incoming link. Data pins are less restricted in the editor, but the runtime
connection map retains only the last destination for a given source pin and
the first upstream source found for a given target pin. Avoid fan-out and
duplicate data links.

The runtime starts at a trigger node and advances through named connections.
Conditions choose `true` or `false`; normal flow uses `then` or `out`; target
nodes use `then`; delays use `after delay` or `out`. A missing branch or exit
ends the active combo. There is no graph compiler or cycle validation. The
runtime limits immediate processing to 12 nodes per tick and resets a combo
that remains active for 15 seconds. SP failures can hold a skill node for up to
5 seconds before it is skipped; repeated out-of-range failures can reset a
combo after about 3 seconds.

## Toolbox nodes

### Triggers

Each trigger has one execution output named `Execute` and a `TriggerType`
property:

| Toolbox node | Exported trigger | Runtime polling |
| --- | --- | --- |
| On Attack State | `OnAttack` | Implemented; polled from attack state |
| On Chase State | `OnChase` | Implemented; polled from chase state |
| On Idle State | `OnIdle` | Implemented; polled from idle state |
| On Target Change | `OnTargetChange` | Exportable, but no runtime dispatch |
| On Owner Damage | `OnOwnerDamage` | Exportable, but no runtime dispatch |

The last two entries are valid editor objects but are not event hooks in the
current `AI_main.lua`. An owner-protection graph starting with On Owner Damage
will never start from owner damage. Owner rescue/protection behavior remains an
unresolved release item; use an active state trigger plus an `Owner HP %`
condition only as a diagnostic workaround.

### Skill nodes

The toolbox contains the pseudo-skill `Auto-Attack` (`SkillID = -1`) and active
catalog entries. Passive catalog entries are intentionally omitted. The active
entries are:

| Skill | ID | Catalog max | Default target |
| --- | ---: | ---: | --- |
| Master Swap | 8005 | 5 | Owner |
| Bastion Renewal | 8006 | 5 | Self |
| Ward's Domain | 8033 | 5 | Self |
| Taunt | 8021 | 5 | Self |
| Quick Defense | 8023 | 5 | Self |
| Body Double | 8022 | 5 | Self |
| Chaotic Heal | 8014 | 5 | Self |
| Illusion of Breath | 8024 | 10 | Enemy |
| Illusion of Light | 8034 | 5 | Enemy |
| Chaotic Sanctuary | 8013 | 5 | Ground |
| Arcane Offering | 8015 | 5 | Self |
| Illusion of Claw | 8009 | 10 | Enemy |
| Fade Away | 8012 | 1 | Self |
| Mirage Assault | 8036 | 5 | Enemy |
| Illusion Crusher | 8031 | 5 | Enemy |
| Blood Sweep | 8032 | 10 | Enemy |

Every skill node has:

- Input `Execute` (execution) and `Target` (data).
- Outputs `Then`, `On Success`, and `On Fail` (all execution).
- `SkillLevel`, `MaxLevel`, `RepeatCount`, and `TargetMode` in the property
  grid. Auto-Attack hides its level and uses only `RepeatCount`.

The selected Kimi's `KimiSkillEnabled` and `KimiSkillLevels` settings are the
actual cast gate. A configured level always overrides the level requested by a
Blueprint node; a disabled, passive, or wrong-type skill is skipped. Learned
skill and loyalty rules remain server/client rules. The catalog marks Fade Away,
Mirage Assault, and Blood Sweep as fallback/unverified metadata, and levels of
Illusion of Claw above the verified range still need field confirmation.

The runtime follows the `Then` connection after a skill. It never follows
`On Success` or `On Fail`, even though those pins are drawn and exported. Use
`Then` for a reliable chain. A failed dispatch, cooldown, insufficient SP, or
range check normally keeps the node active; a disabled or rejected Kimi skill
can advance to `Then`.

Skill `TargetMode` is serialized, but `HandleSkillNode` resolves a target from
an upstream Target data connection, the previously selected runtime target, or
the current enemy. It does not use the skill node's `targetMode` property to
select a new actor. Use a Set Target node and connect its `Target` output to the
skill's `Target` input when target selection matters. Kimi metadata still forces
catalog self/owner targeting where applicable.

Auto-Attack `RepeatCount` counts calls to the native `Attack` command. It does
not count exact individual hits. A native attack can continue producing attacks
while the AI waits for the configured timer. The attempted animation-based
counter was removed after it stalled at slot 1; the timer-based progression is
restored and the latest rollback still needs in-game confirmation.

### Condition nodes

Every condition has input `Check` (execution), outputs `True` and `False`
(execution), and `Value` (data). The property grid exposes `ConditionType`,
`Operator`, and `Value`. Operators are `>=`, `<=`, `==`, `>`, and `<`; values
are integer thresholds. Set `<=` explicitly for a low-HP condition. The GUI's
descriptive text does not change the operator's default.

The visible condition entries and current runtime status are:

| Toolbox node | GUI `ConditionType` | Runtime status |
| --- | --- | --- |
| Kimi HP % | `KimiHP` | Supported (`kimihp` alias) |
| Owner HP % | `OwnerHP` | Supported |
| Kimi SP % | `KimiSP` | **Unsupported as emitted**; use `SP` or `SelfSP` in the editable type field |
| Owner SP % | `OwnerSP` | Supported |
| Mob Count | `MobCount` | Supported; runtime default scan range is 10 cells |
| Distance to Target | `Distance` | Supported; compares cell distance to current/runtime target |
| Skill on Cooldown | `Cooldown` | **Unsupported; evaluates false** |
| In Combat | `InCombat` | **Unsupported; evaluates false** |
| Kimi Type | `HomuType` | Supported; values Ward 1, Occult 2, Agile 3, Raging 4 |

The runtime also accepts `selfhp`, `kimi_hp`, `health_below`, `sp`, and
`selfsp`. Unknown condition names evaluate false. A condition's execution
`Check` path is useful for ordinary branching. For a logic gate, its `Value`
output is the meaningful connection; the runtime reads the upstream condition
directly when it evaluates A or B.

### Logic/control nodes

The toolbox currently offers `AND`, `OR`, `XOR`, `NAND`, `NOR`, `NOT`,
`Sequence`, `Loop N Times`, and `Wait Delay`.

| Node | Inputs | Outputs | Properties | Runtime status |
| --- | --- | --- | --- | --- |
| AND / OR / XOR / NAND / NOR | `In`, data `A`, data `B` | `Out` | `LogicType` | Supported; reads condition values upstream of A/B |
| NOT | `In`, data `A` | `Out` | `LogicType` | Supported; negates A |
| Sequence | `In` | `Out` | `LogicType` | **Not implemented; treated as unknown flow** |
| Loop N Times | `In` | `Out`, `Loop Body`, `Completed` | `LogicType`, `LoopCount` | **Not implemented; loop body is not run** |
| Wait Delay | `In` | `Out` | `LogicType`, `DelayMs` | Supported; exported as `duration` and held across ticks |

For AND/OR-style gates, connect condition `Value` to A and B, connect an
execution path into `In`, and connect `Out` to the next action. The current
PropertyGrid enum exposes only Delay, Loop, AND, OR, and Sequence; it omits
XOR, NAND, NOR, and NOT. Those four nodes can be inserted and export their
original `LogicType`, but selecting them may display as Delay and the dropdown
cannot choose them. Do not edit their LogicType unless the resulting operation
is one of the five displayed values.

The editor contains a private range-check factory and deprecated Target Selector
factory, but neither is added to the toolbox. There is no user-creatable range
node or Target Selector node in the current GUI. The old `UseComboDelay` property
is hidden and is not part of the active node templates.

### Target node

`Set Target` has input `Set` (execution), outputs `Then` (execution) and
`Target` (data), and a `TargetMode` dropdown. The editor offers Enemy, Owner,
Self, Ground, Ally, NearestEnemy, FarthestEnemy, StrongestEnemy,
WeakestEnemy, NearestAlly, FarthestAlly, Focus, and MobID, plus a `MobID`
integer field.

The runtime resolves Enemy/Current, Owner, Self/Kimi, Ally, nearest/farthest/
strongest/weakest Enemy, nearest/farthest Ally, Focus, and MobID. `MobID` picks
the nearest visible enemy whose type matches the ID. `Ground` has no branch in
the current resolver and falls back to the enemy target; ground-skill behavior
therefore needs in-game verification. For deterministic use, connect both
`Then` into the skill flow and `Target` into the skill's `Target` data pin.

### End node

`End` has one execution input `In`, no output, and:

- `RepeatMode`: `Once`, `Always`, `OncePerTarget`, or `XTimes`.
- `RepeatCount`: used by `XTimes`.

The runtime tracks executions by combo name. `Always` restarts on the next
tick; `XTimes` restarts until its count is reached; `OncePerTarget` records the
target and does not immediately restart for the same target; `Once` stops the
active run and lets normal AI continue after reaching the end. These modes do
not prevent a later trigger from starting a fresh run, because the dispatcher
does not gate combo start on the stored execution count. Give each exported
combo a stable trigger title/name when using end-mode tracking.

## Examples that match the runtime

### Basic attack chain

Create and connect:

```text
On Attack State.Execute
  -> Set Target.Set
Set Target.Then
  -> Illusion of Claw.Execute
Set Target.Target
  -> Illusion of Claw.Target
Illusion of Claw.Then
  -> Wait Delay.In
Wait Delay.Out
  -> End.In
```

Set the target mode to `Enemy`, skill level to a configured learned level, and
delay to a value appropriate for the skill. The same pattern works with Auto-
Attack; its repeat count is native attack commands.

### Owner-HP emergency gate

Use an `On Attack State` or `On Idle State` trigger, then connect its Execute
output to `Owner HP % .Check`. Set Operator `<=` and Value `30`; connect
`True` to `Body Double.Execute` and `False` to `End.In`. Body Double is still
subject to enabled level, Kimi type, loyalty, SP, cooldown, and server success.
This demonstrates a state-polled owner condition; it does not make the
unimplemented On Owner Damage event work.

### Two-condition gate

Add `Kimi HP %` with `<= 50` and `Owner HP %` with `<= 30`. Connect each
condition's `Value` pin to an `AND` node's A and B pins. Connect an execution
path to `AND.In`, then `AND.Out` to Body Double or another skill. The runtime
evaluates the two upstream condition nodes when it reaches AND; their `Check`
pins do not need to be part of the execution path for this gate pattern.

### Weakest-target action

Connect `On Attack State.Execute` to `Set Target.Set`, set `TargetMode` to
`WeakestEnemy`, connect `Then` to a skill's `Execute`, and connect `Target` to
that skill's `Target`. A later skill can reuse the runtime target if the Target
data link is omitted, but an explicit link is easier to audit.

## Save, load, and export details

Save writes readable JSON containing node IDs/types/categories/titles, positions
and sizes, colors, property dictionaries, pin definitions, and connection
records. Descriptions are intentionally not persisted; Load regenerates them
from the current library. On Load, known nodes receive the current template
pins and new pin GUIDs; saved connections are remapped by pin name/type. Skill
levels are clamped to the current catalog maximum and missing target defaults
are filled in. An unknown node title falls back to its saved pins.

Save Settings and Apply use the executable directory's `combo_tactics.cbp`.
The Save dialog can write another `.cbp`, and the GUI copies that content back
to the default file for the next launch. Keep the `.cbp` with the GUI as the
editable source and keep the exported Lua file under `USER_AI/data` for the
game.

Export creates one Lua combo per trigger node, using the connected component
around that trigger. It assigns numbered globals, sanitizes the trigger title
for the combo name, writes `enabled = true` and `priority = 1`, and emits every
stored connection with `fromPin` and `toPin`. A graph with multiple disconnected
trigger components can therefore produce multiple combos. Include a real
trigger node in every graph. The advertised no-trigger `ALWAYS` fallback emits
an empty combo without a trigger node; the runtime rejects it as “missing trigger
node.”

The runtime accepts numbered globals (`ComboTactics1`, `ComboTactics2`, ...),
or a single `ComboTactics` table. It requires each accepted combo to have a
`nodes` table, a trigger node, and connections whose pin names match the runtime
flow. The Lua export is data consumed by the runtime; it is not a compiled
program, so unsupported node operations remain unsupported after export.

## Release testing notes

Use logs and the actual game client to verify casts. Local level, SP, cooldown,
range, and target checks do not prove that the server accepted or applied a
skill. Test one short `On Attack` graph first, then add a condition, target
selection, delay, and repeat behavior one at a time. Capture Kimi type, enabled
skills/levels, exported Lua, `combo_tactics.cbp`, and the relevant `AAI_*` log
entries with the failure timestamp.

The following are known release limitations:

- Blueprint auto-attack repeat counts are native `Attack` command counts, not
  exact hit counts.
- The animation-counter experiment was reverted after an in-game slot-1 stall;
  timer-based progression is restored. Regular combos were subsequently reported working; full Blueprint coverage remains unverified.
- On Owner Damage and On Target Change nodes have no dispatch call in the
  runtime. Owner-protection activation through that trigger is unresolved.
- The editor advertises more conditions, control nodes, target modes, and pins
  than the current runtime consumes. The compatibility tables above are the
  authoritative testing surface.
- The GUI project embeds runtime files from `USER_AI` at build time. Rebuild the
  GUI after changing canonical Lua; applying an older executable can reinstall
  an older embedded runtime beside the client.

## Sharing one graph across Kimi types

Use a Kimi Type condition to branch for Ward (1), Occult (2), Agile (3), or Raging (4), then connect each branch to compatible skills. This selects behavior for the current Kimi; it does not control four Kimi simultaneously. The independent Combo page on each named tab is the standard slot-based system, separate from Blueprint.
