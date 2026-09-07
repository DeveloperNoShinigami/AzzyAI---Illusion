AggroHP                  = 20
AggroSP                  = 0
OldHomunType             = 2
UseSkillOnly             = -1  -- Disabled: prevents Breath from firing during chase; combos will handle all skill usage
-- UseAttackSkill: Enable autoskills (single-target + AoE + sniping) outside of combo rotations
-- Set to 0 to use ONLY combos (if configured) and auto-attacks, plus buffs/heals
-- Set to 1 to allow autoskill selector to fire when combo is idle
UseAttackSkill           = 0
OpportunisticTargeting   = 0
DoNotChase               = 0
UseDanceAttack           = 0
SuperPassive             = 0
UseIdleWalk              = 1
IdleWalkSP               = 80
UseCastleRoute           = 0
RelativeRoute            = 1
UseCastleDefend          = 0
CastleDefendThreshold    = 4
IdleWalkDistance         = 4
FleeHP                   = 0
RescueOwnerLowHP         = 80
LiveMobID                = 0

AttackSkillReserveSP     = 0 
AutoMobMode              = 2 
AutoMobCount             = 2
AutoComboMode            = 0
AutoComboSkill		=0
AutoComboSpheres         = 0
BlueprintComboEnabled    = 1  -- Set to 1 to enable Blueprint Combo System (visual node-based combos)
-- UseHomunSSkillChase & UseHomunSSkillAttack: Legacy flags for AoE/mob autoskill branches
-- When UseAttackSkill           = 0, these have no effect (autoskill is fully disabled)
-- When UseAttackSkill           = 0, these control whether AoE/mob autoskills can execute during Chase/Attack states
-- For Kimi-only builds, recommend keeping both at 0 or using combo system instead
UseHomunSSkillChase      = 0
-- See UseHomunSSkillChase comment above
UseHomunSSkillAttack     = 0 
AutoSkillDelay           = 500 
AoEMaximizeTargets       = 0
CastTimeRatio		= 0
UseDefensiveBuff         = 1
UseOffensiveBuff         = 1 
UseProvokeOwner          = 0
ProvokeOwnerMobbed       = 3
-- Deprecated (legacy homunculus): generic heal thresholds
-- Kimi AI uses Chaotic Heal exclusively. Configure healing via:
-- UseChaoticHeal, ChaoticHealKimiHP, ChaoticHealOwnerHP.
-- The below values are retained for backward compatibility but are not used.
HealSelfHP               = 60 
HealOwnerHP              = 60 
UseAutoHeal              = 3
FollowStayBack           = 4
StationaryAggroDist      = 15
MobileAggroDist          = 15
StationaryMoveBounds     = 15
MobileMoveBounds         = 15
DoNotUseRest             = 0
RestXOff                 = 2
RestYOff                 = 0	
MoveSticky               = 1
MoveStickyFight          = 1
KiteMonsters             = 0
KiteBounds               = 15 
KiteStep                 = 6
KiteParanoidStep         = 6
KiteThreshold            = 8
KiteParanoidThreshold    = 8
KiteParanoid             = 1 
DefendStandby            = 0 
StickyStandby            = 1 
SpawnDelay               = 1000
StandbyFriending         = 1  
MirAIFriending           = 0  
UseAvoid                 = 0  
TankMonsterLimit         = 4
AoEReserveSP             = 0

ChaseSPPause             = 0
ChaseSPPauseSP 			= -60
ChaseSPPauseTime         = 3000
AttackTimeLimit          = 10000
FastChangeLimit          = 10

AssumeHomun              = 1
AttackLastFullSP         = 0
DanceMinSP               = 5
DanceAttackStep          = 6
AutoSkillLimit           = 100
AoEFixedLevel            = 0
ForceKite                = 0
UseBerserkMobbed         = 0
UseBerserkSkill          = 0
UseBerserkAttack         = 0
Berserk_SkillAlways      = 0
Berserk_Dance            = 0
Berserk_IgnoreMinSP      = 0
Berserk_ComboAlways      = 0
PVPmode                  = 0
LastSavedDate            = "1/11/2026 11:55:24 PM"
LagReduction             = 0
DoNotAttackMoving        = 0

-- Master Swap Skill (Emergency Owner HP Save)
UseMasterSwap            = 1        -- 1=enabled, 0=disabled
MasterSwapOwnerHP        = 50       -- Use when owner HP < 15%
MasterSwapCooldown       = 60       -- Cooldown in seconds (60s default)

-- Kimi skill levels (0 disables)
illusionOfClawsLevel     = 0
illusionOfBreathLevel    = 9
illusionOfCrusherLevel   = 0
illusionOfLightLevel     = 0
chaoticHealLevel         = 1
bodyDoubleLevel          = 0
warmDefLevel             = 0
onlyAOE                  = 0

-- Kimi autobuff toggles and thresholds
UseChaoticHeal           = 1
ChaoticHealOwnerHP       = 75
ChaoticHealKimiHP        = 100
UseBodyDouble            = 1
BodyDoubleOwnerHP        = 10
UseWarmDef               = 1
WarmDefCooldown          = 60

-- Skill Combo Configuration
ComboEnabled             = 0        -- 1=enabled, 0=disabled (execute combo rotations in ATTACK_ST)
ComboRunDuringChase      = 1        -- Run combo during chase (current slot dictates behavior: melee=chase in, ranged=maintain distance)
ComboRunDuringAttack     = 1        -- Run combo while attacking
ComboRunDuringIdle       = 0        -- Run combo while idle/following (DISABLED - use normal AI)
ComboResetOnTargetChange = 1        -- Reset combo when target changes
ComboAutoAttackDelay     = 800      -- ms to wait after an auto-attack before advancing combo
ComboSkillCastDelay      = 500      -- ms to wait after a skill cast before advancing combo (default uses skill cooldown)
-- Combo Slot Configuration
-- Use -1 for auto-attack, 0 to skip slot
-- Skill IDs: 8009=Claws, 8024=Breath, 8031=Crusher, 8034=Light, 8014=Heal, 8006=Def, 8022=BodyDouble
ComboSlot1_SkillID       = -1        -- Slot 1: Auto-attack
ComboSlot1_ComboCount    = 2         -- Execute 1 time before advancing
ComboSlot2_SkillID       = 8014        -- Slot 2: Auto-attack
ComboSlot2_ComboCount    = 1         -- Execute 1 time before advancing
ComboSlot3_SkillID       = -1      -- Slot 3: Illusion of Breath (support skill, fires in ATTACK_ST after 2x auto-attacks)
ComboSlot3_ComboCount    = 1         -- Execute 1 time before advancing
ComboSlot4_SkillID       = 8014      -- Slot 4: Chaotic Heal (support skill, fires in ATTACK_ST after Breath)
ComboSlot4_ComboCount    = 1         -- Execute 1 time before advancing (back to slot 1)
ComboSlot5_SkillID       = 8024         -- Disabled
ComboSlot5_ComboCount    = 0         -- Disabled
ComboSlot6_SkillID       = 0
ComboSlot6_ComboCount    = 0
ComboSlot7_SkillID       = 0
ComboSlot7_ComboCount    = 0
ComboSlot8_SkillID       = 0
ComboSlot8_ComboCount    = 0

UseAutoPushback          = 0
AutoPushbackThreshold    = 2
FriendAssistDamageHPThresholdOwner= 80