-- H_Extra.lua - User customization and extended features for Kimi AI
-- This file is for custom logic extensions and logging configuration
-- LastSavedDate = null (manually edited)

--########################################
-- LOGGING CONFIGURATION
--########################################
-- Enable/disable specific log categories for debugging
-- All logs are written to USER_AI/ErrorLog/ folder
-- Log files: AAI_*.log (one file per category)
-- Set to 1 to enable, 0 to disable

LogEnable = {}

-- COMBO SYSTEM LOGGING (NEW - highly recommended for combo debugging)
LogEnable["COMBO_INIT"] = 1          -- Combo initialization and reset events
LogEnable["COMBO_ADVANCE"] = 1       -- Slot advancement tracking
LogEnable["COMBO_AUTO_ATTACK"] = 1   -- Auto-attack execution in combos
LogEnable["COMBO_CAST"] = 1          -- Skill casting in combos
LogEnable["COMBO_SP"] = 1            -- SP insufficiency warnings
LogEnable["COMBO_COOLDOWN"] = 1      -- Skill cooldown tracking
LogEnable["COMBO_ERROR"] = 1         -- Configuration errors in combos
LogEnable["COMBO_RESET"] = 1         -- Target change resets
LogEnable["ATTACK_ST"] = 1           -- ATTACK_ST state combo gating

-- GENERAL AI LOGGING
LogEnable["AAI_SKILLFAIL"] = 1       -- Skill execution failures
LogEnable["AAI_CostSP"] = 1          -- SP consumption tracking
LogEnable["AAI_MOBCOUNT"] = 1        -- Mob counting for AoE decisions
LogEnable["AAI_ERROR"] = 1           -- General errors
LogEnable["AAI_DEBUG"] = 1           -- General debug messages

-- TACTICAL LOGGING
LogEnable["AAI_TACTICS"] = 0         -- Tactical decision making
LogEnable["AAI_KITING"] = 0          -- Kiting behavior
LogEnable["AAI_TARGETING"] = 0       -- Target selection
LogEnable["AAI_AVOIDANCE"] = 0       -- Mob avoidance

-- PERFORMANCE LOGGING
LogEnable["AAI_TIMING"] = 0          -- Execution timing benchmarks
LogEnable["AAI_STATE"] = 0           -- State machine transitions

--########################################
-- CUSTOM INITIALIZATION HOOK
--########################################
-- This function is called once during AI initialization (DoneInit==0)
-- Use it for custom setup, dynamic config adjustments, etc.
function OnInit()
	-- Example: Adjust Dance SP threshold based on max SP percentage
	-- if DanceMinSP < 0 then
	--     local maxsp = GetV(V_MAXSP, MyID)
	--     DanceMinSP = math.floor(maxsp * (math.abs(DanceMinSP) / 100))
	--     TraceAI("Calculated DanceMinSP: "..DanceMinSP)
	-- end
	
	-- Add your custom initialization logic here
	TraceAI("[INIT] H_Extra.lua custom logic loaded - Logging enabled")
end

--########################################
-- CUSTOM TACTICAL OVERRIDES
--########################################
-- Uncomment and customize this function to dynamically switch tactics
-- based on players on screen, time of day, location, etc.

-- function GetMyTact(mobID)
--     -- Example: Use alternate tactics when specific player is nearby
--     if Players[specificPlayerID] ~= nil then
--         return MyAltTact[mobID] or MyTact[mobID]
--     else
--         return MyTact[mobID]
--     end
-- end

--########################################
-- SKILL LEVEL CONFIGURATION NOTES
--########################################
-- NOTE: Skill levels are now managed in H_SkillList.lua
-- See SkillList[KIMITYPE][SKILL_ID] = level
-- Do NOT duplicate skill level config here unless you're doing
-- conditional skill level adjustments based on runtime state

--########################################
-- KIMI TYPE CONSTANTS (for reference)
--########################################
-- WARD = 1      (Tank/Defense Kimi)
-- OCCULT = 2    (Magic/Support Kimi)
-- AGILE = 3     (Physical/ASPD Kimi)
-- RAGING = 4    (Balanced/Aggressive Kimi)

--########################################
-- END OF H_EXTRA.LUA
--########################################
	[6] = {-5, -5},  -- 5 cells west, 5 cells north
	[7] = {0, -5},   -- 5 cells north
	[8] = {5, -5},   -- 5 cells east, 5 cells north
}

-- Route walk direction: 1 = forward, -1 = backward
RouteWalkDirection = 1

---------------------------------------------------------------------------
-- FRIEND ATTACK TRIGGERS
---------------------------------------------------------------------------
-- When to attack targets engaged by friends/owner
-- Set each to 1 to enable, 0 to disable

FriendAttack={}
FriendAttack[MOTION_ATTACK] = 0      -- Don't assist when owner attacks (keeps focus)
FriendAttack[MOTION_ATTACK2] = 0
FriendAttack[MOTION_SKILL] = 0
FriendAttack[MOTION_CASTING] = 0     -- Don't assist when owner casts
FriendAttack[MOTION_DAMAGE] = 1      -- Assist only when owner takes damage
FriendAttack[MOTION_TOSS] = 0        -- Ignore item throwing
FriendAttack[MOTION_BIGTOSS] = 0
FriendAttack[MOTION_FULLBLAST] = 0

-- HP-BASED FRIEND DAMAGE ASSIST
-- When enabled, assistance on owner's damage (MOTION_DAMAGE) will only trigger
-- if the owner's HP is at or below the configured percentage.
-- Set to 0 to disable gating (always assist on damage when FriendAttack[MOTION_DAMAGE]==1).
-- If H_Config defines FriendAssistDamageHPThresholdOwner, use that; otherwise default to 50
if FriendAssistDamageHPThresholdOwner == nil then
	FriendAssistDamageHPThresholdOwner = 50  -- % HP (0-100). Example: 50 means assist only if owner HP <= 50%
end


---------------------------------------------------------------------------
-- HELPER FUNCTIONS
---------------------------------------------------------------------------

-- Get count of nearby monsters within specified range
function GetNearbyMobs(myid, range)
	local nearby = {}
	local actors = GetActors()
	if actors == nil then
		return nearby
	end
	
	for i, actor in ipairs(actors) do
		if IsMonster(actor) == 1 then
			local distance = GetDistanceA(myid, actor)
			if distance <= range then
				table.insert(nearby, actor)
			end
		end
	end
	return nearby
end

-- Get count of nearby monsters (returns number, not table)
function GetNearbyMobCount(myid, range)
	local count = 0
	local actors = GetActors()
	if actors == nil then
		return 0
	end
	
	for i, actor in ipairs(actors) do
		if IsMonster(actor) == 1 then
			local distance = GetDistanceA(myid, actor)
			if distance <= range then
				count = count + 1
			end
		end
	end
	return count
end

---------------------------------------------------------------------------
-- CUSTOM TACTICAL BEHAVIORS
---------------------------------------------------------------------------

-- OnInit: Called once when AI initializes
function OnInit()
	-- Add custom initialization here
	TraceAI("H_Extra.lua loaded - Kimi AI custom extensions active")
end

-- OnAttackStart: Called when entering ATTACK_ST state
function OnAttackStart()
	-- Add custom attack logic here
end

-- OnAImiddle: Called every AI cycle (use sparingly - expensive)
function OnAImiddle()
	-- Dynamic gating: toggle FriendAttack[MOTION_DAMAGE] based on owner HP threshold
	if FriendAssistDamageHPThresholdOwner ~= nil and FriendAssistDamageHPThresholdOwner > 0 then
		local owner = GetV(V_OWNER, MyID)
		if owner ~= nil and owner > 0 then
			local hp = HPPercent(owner)
			if hp <= FriendAssistDamageHPThresholdOwner then
				FriendAttack[MOTION_DAMAGE] = 1
			else
				FriendAttack[MOTION_DAMAGE] = 0
			end
		end
	end
end

-- OnAutoBuffs: Gate buff casting based on combo mode
-- When combo is active in current state, skip all buffs to prevent non-combo skill spam
-- IMPORTANT: Return 1 to SKIP buffs, return 0/nil to ALLOW buffs
function OnAutoBuffs(buffmode)
	-- Keep this hook simple and safe: always return 1 (no custom action)
	-- Combo gating is handled centrally in AI_main.DoAutoBuffs()
	return 1
end

-- OnFailUnknownMode: Called when skill cast fails with unknown mode
function OnFailUnknownMode(mode)
	-- Add custom skill failure handling here
end

---------------------------------------------------------------------------
-- LOGGING CONFIGURATION
---------------------------------------------------------------------------
-- Enable specific log categories for debugging
LogEnable = {}
LogEnable["AAI_SKILLFAIL"] = 0   -- Skill execution failures
LogEnable["AAI_CostSP"] = 0      -- SP consumption tracking
LogEnable["AAI_MOBCOUNT"] = 0    -- Mob counting for AoE decisions
LogEnable["AAI_DANCE"] = 0       -- Dance attack movement
LogEnable["AAI_ERROR"] = 1       -- Critical errors (always recommended)