-- Refuge testing release: clean profile. Select your Kimi and enable learned skills in the GUI.
AggroHP                  = 20
AggroSP                  = 0
OldHomunType             = 2
UseSkillOnly             = -1  -- Disabled: prevents Breath from firing during chase; combos will handle all skill usage
-- UseAttackSkill: Enable autoskills (single-target + AoE + sniping) outside of combo rotations
-- Set to 0 to use ONLY combos (if configured) and auto-attacks, plus buffs/heals
-- Set to 1 to allow autoskill selector to fire when combo is idle
UseAttackSkill = 0
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
BlueprintComboEnabled = 0  -- Set to 1 to enable Blueprint Combo System (visual node-based combos)
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
UseAutoHeal = 0
FollowStayBack           = 4
StationaryAggroDist      = 100
MobileAggroDist          = 100
StationaryMoveBounds     = 100
MobileMoveBounds         = 100
DoNotUseRest             = 0
RestXOff                 = 2
RestYOff                 = 0	
MoveSticky               = 1
MoveStickyFight          = 1
KiteMonsters             = 0
KiteBounds               = 100 
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
TankMonsterLimit         = 30
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
UseMasterSwap = 0        -- 1=enabled, 0=disabled
MasterSwapOwnerHP        = 50       -- Use when owner HP < 15%
MasterSwapCooldown       = 60       -- Cooldown in seconds (60s default)

-- Kimi skill levels (0 disables)
illusionOfClawsLevel = 0
illusionOfBreathLevel = 0
illusionOfCrusherLevel = 0
illusionOfLightLevel = 0
chaoticHealLevel = 0
bodyDoubleLevel = 0
warmDefLevel = 0
onlyAOE                  = 0

-- Newly catalogued active Kimi skills use a separate enabled bit so the GUI
-- can display levels starting at one without activating them by default.
-- Level values and Enabled flags below define the clean skill profile.
KimiSkillLevels = {[8005] = 1, [8006] = 1, [8033] = 1, [8021] = 1, [8023] = 1, [8022] = 1, [8014] = 1, [8024] = 1, [8034] = 1, [8013] = 1, [8015] = 1, [8009] = 1, [8012] = 1, [8036] = 1, [8031] = 1, [8032] = 1}
KimiSkillEnabled = {[8005] = 0, [8006] = 0, [8033] = 0, [8021] = 0, [8023] = 0, [8022] = 0, [8014] = 0, [8024] = 0, [8034] = 0, [8013] = 0, [8015] = 0, [8009] = 0, [8012] = 0, [8036] = 0, [8031] = 0, [8032] = 0}

-- Kimi autobuff toggles and thresholds
UseChaoticHeal = 0
ChaoticHealOwnerHP       = 75
ChaoticHealKimiHP        = 100
UseBodyDouble = 0
BodyDoubleOwnerHP        = 10
UseWarmDef = 0
WarmDefCooldown          = 60

-- Skill Combo Configuration
ComboEnabled = 0        -- 1=enabled, 0=disabled (execute combo rotations in ATTACK_ST)
ComboRunDuringChase      = 1        -- Run combo during chase (current slot dictates behavior: melee=chase in, ranged=maintain distance)
ComboRunDuringAttack     = 1        -- Run combo while attacking
ComboRunDuringIdle       = 0        -- Run combo while idle/following (DISABLED - use normal AI)
ComboResetOnTargetChange = 1        -- Reset combo when target changes
ComboAutoAttackDelay     = 800      -- ms to wait after an auto-attack before advancing combo
ComboSkillCastDelay      = 500      -- ms to wait after a skill cast before advancing combo (default uses skill cooldown)
-- Combo Slot Configuration
-- Use -1 for auto-attack, 0 to skip slot
-- Skill IDs: 8009=Claws, 8024=Breath, 8031=Crusher, 8034=Light, 8014=Heal, 8006=Def, 8022=BodyDouble
ComboSlot1_SkillID = 0
ComboSlot1_ComboCount = 0
ComboSlot2_SkillID = 0
ComboSlot2_ComboCount = 0
ComboSlot3_SkillID = 0
ComboSlot3_ComboCount = 0
ComboSlot4_SkillID = 0
ComboSlot4_ComboCount = 0
ComboSlot5_SkillID = 0
ComboSlot5_ComboCount = 0
ComboSlot6_SkillID = 0
ComboSlot6_ComboCount = 0
ComboSlot7_SkillID = 0
ComboSlot7_ComboCount = 0
ComboSlot8_SkillID = 0
ComboSlot8_ComboCount = 0

UseAutoPushback          = 0
AutoPushbackThreshold    = 2
FriendAssistDamageHPThresholdOwner= 80

EnableDebugLogging = 0
