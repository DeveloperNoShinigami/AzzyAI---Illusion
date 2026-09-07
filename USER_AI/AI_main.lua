-----------------------------
-- Dr. Azzy's Merc/Homun AI
-- Written by Dr. Azzy of iRO Chaos
-- This AI is intended for use on official servers only
-- Permission granted to distribute in unmodified form.
-- You may expand the AI freely through the M_Extra and H_Extra files
-- Customized for the 'Return of Morroc' server by Nathan.
MainVersion="1.7"

ResCmdList			= List.new()
-- As of dev 15, global variables are now in Const_.lua

AutoSkillCooldown	= {}
AutoSkillCooldown[S_ILLUSION_OF_CLAWS]=0
AutoSkillCooldown[S_ILLUSION_CRUSHER]=0
AutoSkillCooldown[S_CHAOTIC_HEAL]=0
AutoSkillCooldown[S_WARM_DEF]=0
AutoSkillCooldown[S_BODY_DOUBLE]=0
AutoSkillCooldown[S_ILLUSION_OF_LIGHT]=0
AutoSkillCooldown[S_ILLUSION_OF_BREATH]=0 	

-----------Combo System State Variables---------
ComboState = {
	enabled = 0,
	currentSlot = 0,      -- Current slot in rotation (0-4, 0=inactive)
	comboCountRemaining = 0, -- How many more times to execute current skill
	lastTarget = 0,        -- Track target for reset-on-change
	lastCycleTime = 0,     -- Track timing between cycles
	slotStates = {}        -- Per-slot state tracking
}

-----------Blueprint Combo Runtime-----------
BlueprintCombos = {}
BlueprintRuntime = {
	active = nil,           -- Active combo table
	currentNodeId = nil,    -- Current node within active combo
	trigger = nil,          -- Trigger that activated the combo
	target = nil,           -- Target resolved by target nodes
	lastActionTime = 0,     -- Timing gate for skills/auto-attacks
	delayUntil = 0,         -- Timestamp when delay nodes finish
	inFlight = false,       -- Whether a skill/attack command was just issued
	repeatRemaining = 0,    -- Remaining repeats for current skill node
	nodeState = {},         -- Per-node state (e.g., repeat counters)
	focusTarget = nil,      -- Target locked by Focus mode
	startTime = 0,          -- Track when combo started (for timeout detection)
	-- End node execution tracking
	comboExecutions = {},   -- Key = combo name, Value = {count = N, lastTarget = ID, lastTime = tick}
	failCooldowns = {},     -- Key = combo name, Value = tick when cooldown expires
}

local function ResetBlueprintRuntime(reason)
	BlueprintRuntime.active = nil
	BlueprintRuntime.currentNodeId = nil
	BlueprintRuntime.trigger = nil
	BlueprintRuntime.target = nil
	BlueprintRuntime.lastActionTime = 0
	BlueprintRuntime.delayUntil = 0
	BlueprintRuntime.inFlight = false
	BlueprintRuntime.repeatRemaining = 0
	BlueprintRuntime.nodeState = {}
	BlueprintRuntime.focusTarget = nil
	-- Note: We do NOT clear failCooldowns here - they persist across combo resets
	BlueprintRuntime.startTime = 0
	if reason then
		TraceAI("[BP_COMBO] Runtime reset: "..reason)
	end
end

local function NormalizePin(pin)
	if pin == nil then return "default" end
	return string.lower(pin)
end

local function ResolveSkillId(skill)
	if skill == nil then return nil end
	if type(skill) == "number" then return skill end
	if type(skill) == "string" then
		-- Numeric string or symbolic constant
		local asNum = tonumber(skill)
		if asNum ~= nil then return asNum end
		if _G[skill] then return _G[skill] end
	end
	return nil
end

local function CompareValues(lhs, rhs, op)
	op = op or "=="
	if lhs == nil or rhs == nil then return false end
	if op == "==" then return lhs == rhs end
	if op == "!=" or op == "~=" then return lhs ~= rhs end
	if op == ">" then return lhs > rhs end
	if op == "<" then return lhs < rhs end
	if op == ">=" then return lhs >= rhs end
	if op == "<=" then return lhs <= rhs end
	return false
end

local function EvaluateConditionNode(node)
	if node == nil then return false end
	local cond = node.condition or ""
	cond = string.lower(cond)
	local op = node.op or node.operator or nil
	local value = node.value or node.threshold

	if cond == "homutype" or cond == "kimitype" then
		return CompareValues(KIMITYPE, value, op or "==")
	elseif cond == "ownerhp" or cond == "owner_hp" or cond == "ownerhp" then
		local owner = GetV(V_OWNER, MyID)
		local hp = owner and HPPercent(owner) or 100
		return CompareValues(hp, value, op or "<=")
	elseif cond == "selfhp" or cond == "kimi_hp" or cond == "kimihp" or cond == "health_below" then
		local hp = HPPercent(MyID)
		local defaultOp = (cond == "health_below") and "<=" or "<="
		return CompareValues(hp, value, op or defaultOp)
	elseif cond == "mobcount" then
		local range = node.range or 10
		local count = GetNearbyMobCount(MyID, range)
		return CompareValues(count, value or 0, op or ">=")
	elseif cond == "distance" then
		-- Distance to target (default: BlueprintRuntime.target or MyEnemy)
		local target = BlueprintRuntime.target or MyEnemy or 0
		local dist = GetDistanceA(MyID, target)
		TraceAI("[BP_COMBO] Distance condition: dist="..tostring(dist).." op="..tostring(op).." value="..tostring(value))
		return CompareValues(dist, value or 0, op or ">=")
	elseif cond == "sp" or cond == "selfsp" then
		-- Current SP percentage
		local sp = SPPercent(MyID)
		return CompareValues(sp, value or 0, op or ">=")
	elseif cond == "ownersp" then
		-- Owner's SP percentage
		local owner = GetV(V_OWNER, MyID)
		local sp = owner and SPPercent(owner) or 100
		return CompareValues(sp, value or 0, op or ">=")
	end
	-- Unknown condition defaults to false
	TraceAI("[BP_COMBO] Unknown condition: "..tostring(node.condition).." (cond='"..tostring(cond).."')")
	return false
end

local function BuildComboConnectionMap(combo)
	combo.connectionsMap = {}
	if combo.connections == nil then return end
	for _, conn in ipairs(combo.connections) do
		local pin = NormalizePin(conn.fromPin or conn.pin)
		combo.connectionsMap[conn.from] = combo.connectionsMap[conn.from] or {}
		combo.connectionsMap[conn.from][pin] = conn.to
		-- Track first seen as default fallback
		if combo.connectionsMap[conn.from].default == nil then
			combo.connectionsMap[conn.from].default = conn.to
		end
	end
end

-- Find upstream source node for a given target pin name (e.g., 'A', 'B')
local function GetUpstreamSourceForPin(combo, toId, toPin)
    if combo == nil or combo.connections == nil then return nil end
    local needle = NormalizePin(toPin)
    for _, conn in ipairs(combo.connections) do
        if conn.to == toId and NormalizePin(conn.toPin) == needle then
            return conn.from
        end
    end
    return nil
end

local function GetNextNodeId(combo, fromId, pin)
	if combo == nil or combo.connectionsMap == nil then return nil end
	local normalized = NormalizePin(pin)
	local conn = combo.connectionsMap[fromId]
	if conn == nil then return nil end
	return conn[normalized] or conn.default
end

local function GetUpstreamSourceForPin(combo, toNodeId, toPinName)
	-- Find the node connected TO this node's input pin
	if combo == nil or combo.connections == nil then return nil end
	for _, conn in ipairs(combo.connections) do
		if conn.to == toNodeId and string.lower(conn.toPin or "") == string.lower(toPinName or "") then
			return conn.from
		end
	end
	return nil
end

local function NormalizeBlueprintCombos()
	BlueprintCombos = {}
	local function addCombo(src)
		if type(src) ~= "table" or src.nodes == nil then return end
		local combo = {
			name = src.name or "UnnamedCombo",
			enabled = (src.enabled ~= false),
			trigger = src.trigger or "OnAttack",
			priority = src.priority or 1,
			nodes = src.nodes or {},
			connections = src.connections or {},
			nodeById = {}
		}
		for _, node in ipairs(combo.nodes) do
			combo.nodeById[node.id] = node
		end
		BuildComboConnectionMap(combo)
		-- Cache trigger node id
		for _, node in ipairs(combo.nodes) do
			if node.type == "trigger" then
				combo.triggerNodeId = node.id
				break
			end
		end
		if combo.triggerNodeId == nil then
			TraceAI("[BP_COMBO] Combo missing trigger node: "..combo.name)
		else
			table.insert(BlueprintCombos, combo)
		end
	end

	-- Primary: single or list returned as ComboTactics
	if ComboTactics ~= nil then
		if ComboTactics.nodes ~= nil then
			addCombo(ComboTactics)
		elseif type(ComboTactics) == "table" then
			for _, c in pairs(ComboTactics) do
				addCombo(c)
			end
		end
	end

	-- Secondary: accept numbered globals ComboTactics1, ComboTactics2, ... defined by the file
	-- This allows multiple combos to live in a single Lua file without a container list.
	for k, v in pairs(_G) do
		if type(v) == "table" then
			local isNumbered = string.match(k, "^ComboTactics%d+$") ~= nil
			if isNumbered then
				addCombo(v)
			end
		end
	end

	-- Sort by priority descending
	table.sort(BlueprintCombos, function(a,b) return (a.priority or 1) > (b.priority or 1) end)
	if #BlueprintCombos > 0 then
		TraceAI("[BP_COMBO] Loaded "..tostring(#BlueprintCombos).." blueprint combo(s)")
	else
		TraceAI("[BP_COMBO] No blueprint combos found after normalization")
	end
end

-- SelectBestAlly: choose a friendly target for support skills
-- Preference order: Owner (if present) -> Self (Kimi) -> Lowest HP% friend
local function SelectBestAlly(myid)
	local best = nil
	local bestPct = 200
	-- Consider owner and self first
	local owner = GetV(V_OWNER, myid)
	local candidates = {}
	if owner ~= nil and owner ~= 0 then table.insert(candidates, owner) end
	table.insert(candidates, myid)
	-- Include visible friends
	for _, actor in ipairs(GetActors()) do
		if IsFriend(actor) == 1 then
			table.insert(candidates, actor)
		end
	end
	for _, id in ipairs(candidates) do
		local hp = GetV(V_HP, id)
		local maxhp = GetV(V_MAXHP, id)
		if hp ~= nil and maxhp ~= nil and maxhp > 0 then
			local pct = (hp * 100) / maxhp
			if pct < bestPct then
				best = id
				bestPct = pct
			end
		end
	end
	return best or myid
end

-- Helpers for smart targeting
local function IsActorVisible(id)
	if id == nil then return false end
	for _, a in ipairs(GetActors()) do
		if a == id then return true end
	end
	return false
end

local function IsValidTarget(id)
	if id == nil then return false end
	local hp = GetV(V_HP, id)
	return (hp ~= nil and hp > 0) and IsActorVisible(id)
end

local function EnumerateEnemies()
	local list = {}
	for _, a in ipairs(GetActors()) do
		if IsMonster(a) == 1 then
			local hp = GetV(V_HP, a)
			if hp ~= nil and hp > 0 then
				table.insert(list, a)
			end
		end
	end
	return list
end

local function EnumerateAllies(myid)
	local list = {}
	local owner = GetV(V_OWNER, myid)
	if owner ~= nil and owner ~= 0 then table.insert(list, owner) end
	table.insert(list, myid)
	for _, a in ipairs(GetActors()) do
		if IsFriend(a) == 1 then
			local hp = GetV(V_HP, a)
			if hp ~= nil and hp > 0 then
				table.insert(list, a)
			end
		end
	end
	return list
end

local function SelectNearestEnemy(myid)
	local best, bestDist = nil, 1e9
	for _, e in ipairs(EnumerateEnemies()) do
		local d = GetDistanceA(myid, e)
		if d ~= nil and d < bestDist then
			best, bestDist = e, d
		end
	end
	if best then TraceAI("[BP_COMBO] Nearest enemy="..tostring(best).." dist="..tostring(bestDist)) end
	return best
end

local function SelectFarthestEnemy(myid)
	local best, bestDist = nil, -1
	for _, e in ipairs(EnumerateEnemies()) do
		local d = GetDistanceA(myid, e)
		if d ~= nil and d > bestDist then
			best, bestDist = e, d
		end
	end
	if best then TraceAI("[BP_COMBO] Farthest enemy="..tostring(best).." dist="..tostring(bestDist)) end
	return best
end

local function SelectStrongestEnemy(myid)
	local best, bestScore = nil, -1
	for _, e in ipairs(EnumerateEnemies()) do
		local maxhp = GetV(V_MAXHP, e) or 0
		local score = maxhp
		if score > bestScore then
			best, bestScore = e, score
		end
	end
	if best then TraceAI("[BP_COMBO] Strongest enemy="..tostring(best).." maxhp="..tostring(bestScore)) end
	return best
end

local function SelectWeakestEnemy(myid)
	local best, bestPct = nil, 200
	for _, e in ipairs(EnumerateEnemies()) do
		local pct = HPPercent(e)
		if pct ~= nil and pct < bestPct then
			best, bestPct = e, pct
		end
	end
	if best then TraceAI("[BP_COMBO] Weakest enemy="..tostring(best).." hp%="..tostring(bestPct)) end
	return best
end

local function SelectNearestAlly(myid)
	local best, bestDist = nil, 1e9
	for _, a in ipairs(EnumerateAllies(myid)) do
		local d = GetDistanceA(myid, a)
		if d ~= nil and d < bestDist then
			best, bestDist = a, d
		end
	end
	if best then TraceAI("[BP_COMBO] Nearest ally="..tostring(best).." dist="..tostring(bestDist)) end
	return best or myid
end

local function SelectFarthestAlly(myid)
	local best, bestDist = nil, -1
	for _, a in ipairs(EnumerateAllies(myid)) do
		local d = GetDistanceA(myid, a)
		if d ~= nil and d > bestDist then
			best, bestDist = a, d
		end
	end
	if best then TraceAI("[BP_COMBO] Farthest ally="..tostring(best).." dist="..tostring(bestDist)) end
	return best or myid
end

local function SelectEnemyByMobId(myid, mobId)
	if mobId == nil then return nil end
	local nearest, nearestDist = nil, 1e9
	for _, e in ipairs(EnumerateEnemies()) do
		local etype = GetV(V_TYPE, e)
		if etype == mobId then
			local d = GetDistanceA(myid, e)
			if d ~= nil and d < nearestDist then
				nearest, nearestDist = e, d
			end
		end
	end
	if nearest then TraceAI("[BP_COMBO] MobID="..tostring(mobId).." target="..tostring(nearest).." dist="..tostring(nearestDist)) end
	return nearest
end

local function ResolveTargetFromNode(node, myid, enemy)
	if node == nil then return enemy end
	local mode = node.targetMode or node.target
	if mode == nil then return enemy end
	mode = string.lower(tostring(mode))
	mode = string.gsub(mode, "-", " ")
	mode = string.gsub(mode, "_", " ")
	TraceAI("[BP_COMBO] Resolving target mode: '"..tostring(mode).."'")
	-- Focus handling: if focus is active and valid, keep it
	if string.find(mode, "focus", 1, true) then
		if IsValidTarget(BlueprintRuntime.focusTarget) then
			TraceAI("[BP_COMBO] Focus active -> target="..tostring(BlueprintRuntime.focusTarget))
			return BlueprintRuntime.focusTarget
		end
	end
	if mode == "owner" then
		local owner = GetV(V_OWNER, myid)
		TraceAI("[BP_COMBO] Target=Owner ("..tostring(owner)..")")  
		return owner
	elseif mode == "kimi" or mode == "self" then
		TraceAI("[BP_COMBO] Target=Self ("..tostring(myid)..")")
		return myid
	elseif mode == "enemy" or mode == "current" then
		TraceAI("[BP_COMBO] Target=Current enemy ("..tostring(enemy)..")")
		return enemy
	elseif mode == "ally" then
		local ally = SelectBestAlly(myid)
		TraceAI("[BP_COMBO] Target=Ally ("..tostring(ally).." HP%="..tostring(HPPercent(ally))..")")
		return ally
	elseif mode == "nearest enemy" then
		return SelectNearestEnemy(myid) or enemy
	elseif mode == "farthest enemy" then
		return SelectFarthestEnemy(myid) or enemy
	elseif mode == "strongest enemy" then
		return SelectStrongestEnemy(myid) or enemy
	elseif mode == "weakest enemy" then
		return SelectWeakestEnemy(myid) or enemy
	elseif mode == "nearest ally" then
		return SelectNearestAlly(myid)
	elseif mode == "farthest ally" then
		return SelectFarthestAlly(myid)
	elseif mode == "mobid" or mode == "mob id" or mode == "boss" then
		local mobId = node.mobId or node.mobID or node.mobid
		local target = SelectEnemyByMobId(myid, mobId)
		return target or enemy
	elseif string.find(mode, "focus", 1, true) then
		-- Set focus target using best available enemy
		local focus = enemy or SelectNearestEnemy(myid)
		BlueprintRuntime.focusTarget = focus
		TraceAI("[BP_COMBO] Focus set -> target="..tostring(focus))
		return focus
	end
	TraceAI("[BP_COMBO] Target=Fallback to enemy ("..tostring(enemy)..")")
	return enemy
end

local function AdvanceBlueprintNode(combo, nextId)
	BlueprintRuntime.currentNodeId = nextId
	BlueprintRuntime.inFlight = false
	BlueprintRuntime.lastActionTime = 0
	BlueprintRuntime.repeatRemaining = 0
	BlueprintRuntime.delayUntil = 0
	BlueprintRuntime.target = BlueprintRuntime.target -- unchanged unless target node sets
end

local function StartBlueprintCombo(triggerName)
	if BlueprintComboEnabled ~= 1 then return end
	if #BlueprintCombos == 0 then return end
	for _, combo in ipairs(BlueprintCombos) do
		if combo.enabled and string.lower(combo.trigger) == string.lower(triggerName) then
			BlueprintRuntime.active = combo
			BlueprintRuntime.trigger = triggerName
			BlueprintRuntime.currentNodeId = combo.triggerNodeId
			BlueprintRuntime.inFlight = false
			BlueprintRuntime.lastActionTime = 0
			BlueprintRuntime.repeatRemaining = 0
			BlueprintRuntime.delayUntil = 0
			BlueprintRuntime.target = nil
			BlueprintRuntime.nodeState = {}
			BlueprintRuntime.startTime = GetTick()  -- Track start time for timeout detection
			TraceAI("[BP_COMBO] Starting combo '"..combo.name.."' for trigger "..triggerName)
			return
		end
	end
end

local function HandleSkillNode(node, myid, enemy)
	local skillid = ResolveSkillId(node.skill)
	if skillid == nil then
		TraceAI("[BP_COMBO] Invalid skill id on node "..tostring(node.id))
		return GetNextNodeId(BlueprintRuntime.active, node.id, "then")
	end
	local repeatCount = node.repeatCount or node.repeats or 1
	local nodeState = BlueprintRuntime.nodeState[node.id] or {remaining = repeatCount, failCount = 0, firstFailTime = 0}
	BlueprintRuntime.nodeState[node.id] = nodeState

	-- Determine target: check for upstream Target pin connection first
	local target
	local targetNodeId = GetUpstreamSourceForPin(BlueprintRuntime.active, node.id, "Target")
	if targetNodeId then
		local targetNode = BlueprintRuntime.active.nodeById[targetNodeId]
		if targetNode and string.lower(targetNode.type or "") == "target" then
			target = ResolveTargetFromNode(targetNode, myid, enemy)
			TraceAI("[BP_COMBO] Skill "..tostring(skillid).." using upstream Target node "..tostring(targetNodeId).." => "..tostring(target))
		else
			-- Upstream connection exists but isn't a target node - use runtime target
			target = BlueprintRuntime.target or enemy
			TraceAI("[BP_COMBO] Skill "..tostring(skillid).." using runtime target (upstream="..tostring(targetNodeId).." not target type) => "..tostring(target))
		end
	elseif BlueprintRuntime.target ~= nil then
		-- No upstream Target pin, use runtime target from previous Target node
		target = BlueprintRuntime.target
		TraceAI("[BP_COMBO] Skill "..tostring(skillid).." using runtime target => "..tostring(target))
	else
		-- No target found anywhere, use enemy
		target = enemy
		TraceAI("[BP_COMBO] Skill "..tostring(skillid).." using fallback enemy => "..tostring(target))
	end

	-- Auto-attack handling
	if skillid == -1 then
		local attackRange = GetV(V_ATTACKRANGE, myid) or 1
		local dist = GetDistanceA(myid, target)
		if dist > attackRange then
			TraceAI("[BP_COMBO] Auto-attack waiting for range ("..dist.." > "..attackRange..")")
			return -1 -- Stay on this node
		end
		if BlueprintRuntime.inFlight then
			if GetTick() - BlueprintRuntime.lastActionTime < (ComboAutoAttackDelay or 500) then
				return -1  -- Still waiting for attack delay
			end
			-- Finish one repeat
			nodeState.remaining = nodeState.remaining - 1
			BlueprintRuntime.inFlight = false
			BlueprintRuntime.lastActionTime = 0
			TraceAI("[BP_COMBO] Auto-attack repeat finished. Remaining: "..nodeState.remaining)
			if nodeState.remaining <= 0 then
				BlueprintRuntime.nodeState[node.id] = nil
				TraceAI("[BP_COMBO] Auto-attack sequence complete on node "..tostring(node.id))
				local nextId = GetNextNodeId(BlueprintRuntime.active, node.id, "then")
				if nextId == nil then
					return 0  -- Signal end of chain
				end
				return nextId
			else
				TraceAI("[BP_COMBO] Auto-attack repeat remaining: "..nodeState.remaining.." - queuing next attack")
				return -1  -- Stay on this node for next repeat
			end
		else
			Attack(myid, target)
			BlueprintRuntime.inFlight = true
			BlueprintRuntime.lastActionTime = GetTick()
			TraceAI("[BP_COMBO] Auto-attack issued (node "..tostring(node.id)..") repeatCount="..repeatCount.." remaining="..nodeState.remaining.." target="..tostring(target))
			return -1  -- Stay on this node until attack completes
		end
	end

	-- Skill handling
	if BlueprintRuntime.inFlight then
		if GetTick() - BlueprintRuntime.lastActionTime < (ComboSkillCastDelay or 500) then
			return -1  -- Still waiting for cast delay
		end
		nodeState.remaining = nodeState.remaining - 1
		BlueprintRuntime.inFlight = false
		BlueprintRuntime.lastActionTime = 0
		if nodeState.remaining <= 0 then
			BlueprintRuntime.nodeState[node.id] = nil
			TraceAI("[BP_COMBO] Skill node complete: "..tostring(skillid))
			local nextId = GetNextNodeId(BlueprintRuntime.active, node.id, "then")
			if nextId == nil then
				return 0  -- Signal end of chain
			end
			return nextId
		else
			TraceAI("[BP_COMBO] Skill repeats remaining: "..nodeState.remaining)
			return -1  -- Stay on this node for next repeat
		end
	else
		local skilllevel = node.level or 1
		local configuredLevel = GetConfiguredSkillLevel(skillid)
		if configuredLevel ~= nil then
			skilllevel = configuredLevel
		end
		if skilllevel == 0 then
			TraceAI("[BP_COMBO] Skill "..skillid.." disabled by configuration; advancing")
			BlueprintRuntime.nodeState[node.id] = nil
			local nextId = GetNextNodeId(BlueprintRuntime.active, node.id, "then")
			if nextId == nil then
				return 0  -- End of chain
			end
			return nextId
		end
		if AutoSkillCooldown[skillid] and GetTick() < AutoSkillCooldown[skillid] then
			TraceAI("[BP_COMBO] Skill "..skillid.." on cooldown - waiting")
			return -1  -- Stay on this node
		end
		-- SP check
		local spcost = GetSkillInfo(skillid, 3, skilllevel) or 0
		if GetV(V_SP, myid) < spcost then
			TraceAI("[BP_COMBO] Insufficient SP for skill "..skillid)
			-- Track failures to prevent infinite stuck combos
			nodeState.failCount = nodeState.failCount + 1
			if nodeState.firstFailTime == 0 then
				nodeState.firstFailTime = GetTick()
			end
			-- After 5 seconds of failures, skip this skill node
			if GetTick() - nodeState.firstFailTime > 5000 then
				TraceAI("[BP_COMBO] Skill "..skillid.." failed too long (no SP) - skipping node")
				BlueprintRuntime.nodeState[node.id] = nil
				local nextId = GetNextNodeId(BlueprintRuntime.active, node.id, "then")
				if nextId == nil then
					return 0  -- End of chain
				end
				return nextId
			end
			return -1  -- Stay on this node
		end
		
		-- Range check (skip for ally-targeted self-skills and special cases)
		local skillRange = GetSkillInfo(skillid, 2, skilllevel) or 1
		local skipRangeCheck = false
		
		-- Chaotic Heal (8014), Warm Def (8006), Body Double (8022) are self/ally skills with range 0 - they work regardless of distance
		if skillid == S_CHAOTIC_HEAL or skillid == S_WARM_DEF or skillid == S_BODY_DOUBLE then
			skipRangeCheck = true
			TraceAI("[BP_COMBO] Skill "..skillid.." is self/ally skill - skipping range check")
		end
		
		-- For other skills with range 0 targeting allies, assume they're self-buffs
		if not skipRangeCheck and skillRange == 0 then
			local targetNodeId = GetUpstreamSourceForPin(BlueprintRuntime.active, node.id, "Target")
			if targetNodeId then
				local targetNode = BlueprintRuntime.active.nodeById[targetNodeId]
				if targetNode and string.find(string.lower(targetNode.targetMode or ""), "ally") then
					skipRangeCheck = true
					TraceAI("[BP_COMBO] Skill "..skillid.." range=0 with ally target - treating as self-buff")
				end
			end
		end
		
		if not skipRangeCheck then
			local dist = GetDistanceA(myid, target)
			if dist > skillRange then
				TraceAI("[BP_COMBO] Target out of range for skill "..skillid.." ("..dist.." > "..skillRange..")")
				-- Track failures to prevent infinite stuck combos
				nodeState.failCount = nodeState.failCount + 1
				if nodeState.firstFailTime == 0 then
					nodeState.firstFailTime = GetTick()
				end
				-- After 3 seconds of range failures, reset combo to allow normal behaviors
				if GetTick() - nodeState.firstFailTime > 3000 then
					TraceAI("[BP_COMBO] Skill "..skillid.." out of range too long - resetting combo")
					BlueprintRuntime.nodeState[node.id] = nil
					ResetBlueprintRuntime("Skill range timeout")
					return -1  -- Stay on this node (runtime already reset)
				end
				return -1  -- Stay on this node
			end
		end
		DoSkill(skillid, skilllevel, target)
		local cd = GetSkillInfo(skillid, 9, skilllevel) or 0
		if cd > 0 then
			AutoSkillCooldown[skillid] = GetTick() + cd
		end
		BlueprintRuntime.inFlight = true
		BlueprintRuntime.lastActionTime = GetTick()
		-- Reset fail tracking on successful cast
		nodeState.failCount = 0
		nodeState.firstFailTime = 0
		TraceAI("[BP_COMBO] Casting skill "..skillid.." Lv."..skilllevel.." repeatCount="..repeatCount.." remaining="..nodeState.remaining.." on target "..tostring(target))
		return -1  -- Stay on this node until cast completes
	end
end

local function RunBlueprintCombos(triggerName, myid, enemy)
	if BlueprintComboEnabled ~= 1 then return false end
	if #BlueprintCombos == 0 then return false end

	-- If active combo trigger differs from current context, reset
	if BlueprintRuntime.active and string.lower(BlueprintRuntime.trigger or "") ~= string.lower(triggerName) then
		ResetBlueprintRuntime("Trigger context changed to "..triggerName)
	end

	-- Global timeout: Reset combos that run for more than 15 seconds
	if BlueprintRuntime.active and BlueprintRuntime.startTime > 0 then
		if GetTick() - BlueprintRuntime.startTime > 15000 then
			TraceAI("[BP_COMBO] Combo running too long (15s timeout) - force reset")
			ResetBlueprintRuntime("Global timeout")
			return false
		end
	end

	if BlueprintRuntime.active == nil then
		-- Check if any combo for this trigger is on cooldown
		local onCooldown = false
		for _, combo in ipairs(BlueprintCombos) do
			if combo.trigger == triggerName then
				local cooldownExpire = BlueprintRuntime.failCooldowns[combo.name] or 0
				if GetTick() < cooldownExpire then
					onCooldown = true
					break
				end
			end
		end
		if not onCooldown then
			TraceAI("[BP_COMBO] Attempting to start combo for trigger: "..triggerName)
			StartBlueprintCombo(triggerName)
			if BlueprintRuntime.active == nil then
				TraceAI("[BP_COMBO] No matching combo found for trigger: "..triggerName)
			end
		end
	end
	if BlueprintRuntime.active == nil then return false end
	TraceAI("[BP_COMBO] Running combo '"..BlueprintRuntime.active.name.."' in trigger "..triggerName)

	local combo = BlueprintRuntime.active
	local safety = 0
	while safety < 12 do
		safety = safety + 1
		local node = combo.nodeById[BlueprintRuntime.currentNodeId]
		if node == nil then
			TraceAI("[BP_COMBO] Combo complete: no more nodes to execute")
			ResetBlueprintRuntime("Combo complete")
			return false
		end
		local ntype = string.lower(node.type or "")
		TraceAI("[BP_COMBO] Processing node "..tostring(BlueprintRuntime.currentNodeId).." type="..tostring(ntype))
		if ntype == "trigger" then
			local nextId = GetNextNodeId(combo, node.id, node.fromPin or "execute")
			AdvanceBlueprintNode(combo, nextId)
		elseif ntype == "condition" then
			local pass = EvaluateConditionNode(node)
			local branch = pass and "true" or "false"
			TraceAI("[BP_COMBO] Condition node "..tostring(node.id).." => "..tostring(pass))
			local nextId = GetNextNodeId(combo, node.id, branch)
			if nextId == nil then
				ResetBlueprintRuntime("No branch target from condition")
				return false
			end
			AdvanceBlueprintNode(combo, nextId)
		elseif ntype == "target" then
			BlueprintRuntime.target = ResolveTargetFromNode(node, myid, enemy)
			TraceAI("[BP_COMBO] Target node set target to "..tostring(BlueprintRuntime.target))
			local nextId = GetNextNodeId(combo, node.id, "then")
			if nextId == nil then
				TraceAI("[BP_COMBO] Target node "..tostring(node.id).." has no exit - combo complete")
				ResetBlueprintRuntime("Target node end of chain")
				return false
			end
			AdvanceBlueprintNode(combo, nextId)
		elseif ntype == "logic" then
			local op = string.lower(node.op or "")
			TraceAI("[BP_COMBO] Logic node "..tostring(node.id).." op='"..tostring(op).."' (raw='"..tostring(node.op).."')")
			if op == "delay" then
				if BlueprintRuntime.delayUntil == 0 then
					BlueprintRuntime.delayUntil = GetTick() + (node.duration or 0)
					TraceAI("[BP_COMBO] Delay node "..tostring(node.id).." waiting "..tostring(node.duration or 0).."ms")
					return true
				elseif GetTick() < BlueprintRuntime.delayUntil then
					return true
				else
					BlueprintRuntime.delayUntil = 0
					local nextId = GetNextNodeId(combo, node.id, "after delay") or GetNextNodeId(combo, node.id, "out")
					if nextId == nil then
						TraceAI("[BP_COMBO] Delay node "..tostring(node.id).." has no exit connection - combo ending")
						ResetBlueprintRuntime("Delay node has no exit")
						return false
					end
					AdvanceBlueprintNode(combo, nextId)
				end
			elseif op == "and" or op == "or" or op == "xor" or op == "nand" or op == "nor" or op == "not" then
				-- Logic gates: evaluate upstream condition nodes connected to pins A (and B for binary ops)
				TraceAI("[BP_COMBO] Evaluating "..tostring(op).." gate at node "..tostring(node.id))
				local aId = GetUpstreamSourceForPin(combo, node.id, "A")
				local bId = GetUpstreamSourceForPin(combo, node.id, "B")
				TraceAI("[BP_COMBO] Upstream A="..tostring(aId).." B="..tostring(bId))
				local aNode = aId and combo.nodeById[aId] or nil
				local bNode = bId and combo.nodeById[bId] or nil
				local aVal = aNode and string.lower(aNode.type or "") == "condition" and EvaluateConditionNode(aNode) or false
				local bVal = bNode and string.lower(bNode.type or "") == "condition" and EvaluateConditionNode(bNode) or false
				TraceAI("[BP_COMBO] Condition values A="..tostring(aVal).." B="..tostring(bVal))
				local pass = false
				if op == "and" then
					pass = aVal and bVal
				elseif op == "or" then
					pass = aVal or bVal
				elseif op == "xor" then
					-- Exclusive OR: true if exactly one is true
					pass = (aVal and not bVal) or (not aVal and bVal)
				elseif op == "nand" then
					-- NOT AND: false only if both true
					pass = not (aVal and bVal)
				elseif op == "nor" then
					-- NOT OR: true only if both false
					pass = not (aVal or bVal)
				elseif op == "not" then
					-- NOT: negate A (ignore B)
					pass = not aVal
				else
					pass = false  -- Unknown op, default to false
				end
				TraceAI("[BP_COMBO] Logic "..string.upper(op).." node "..tostring(node.id).." => "..tostring(pass))
				if pass then
					local nextId = GetNextNodeId(combo, node.id, "out") or GetNextNodeId(combo, node.id, "then")
					AdvanceBlueprintNode(combo, nextId)
				else
					-- End combo for this tick; add cooldown to prevent spam retrying
					local comboName = combo.name or "UnnamedCombo"
					BlueprintRuntime.failCooldowns[comboName] = GetTick() + 1000  -- 1 second cooldown
					TraceAI("[BP_COMBO] Logic "..string.upper(op).." failed - adding 1s cooldown for "..comboName)
					ResetBlueprintRuntime("Logic "..string.upper(op).." evaluated false")
					return false
				end
			else
				TraceAI("[BP_COMBO] Unknown logic op: "..tostring(node.op).." (op='"..tostring(op).."')")
				local nextId = GetNextNodeId(combo, node.id, "then")
				AdvanceBlueprintNode(combo, nextId)
			end
		elseif ntype == "skill" then
			local nextId = HandleSkillNode(node, myid, enemy)
			if nextId ~= nil then
				if nextId == -1 then
					-- HandleSkillNode returns -1 to signal "stay on this node"
					return true
				elseif nextId == 0 then
					-- HandleSkillNode returns 0 to signal "combo complete, no exit"
					TraceAI("[BP_COMBO] Skill node "..tostring(node.id).." complete with no exit - ending combo")
					ResetBlueprintRuntime("Skill node end of chain")
					return false
				else
					-- Valid next node ID
					AdvanceBlueprintNode(combo, nextId)
				end
			end
			return true
		elseif ntype == "end" then
			-- Handle End node - controls combo re-execution
			local repeatMode = node.repeatMode or "Once"
			local comboName = combo.name or "UnnamedCombo"
			
			TraceAI("[BP_COMBO] End node reached: repeatMode="..tostring(repeatMode))
			
			-- Initialize execution tracking if needed
			if BlueprintRuntime.comboExecutions[comboName] == nil then
				BlueprintRuntime.comboExecutions[comboName] = {count = 0, lastTarget = nil, lastTime = 0}
			end
			local execTrack = BlueprintRuntime.comboExecutions[comboName]
			
			-- Check repeat conditions
			if repeatMode == "Once" then
				-- Once: Execute once then immediately allow normal AI to continue
				if execTrack.count == 0 then
					execTrack.count = 1
					execTrack.lastTime = GetTick()
					TraceAI("[BP_COMBO] End: Once mode - first execution complete (count="..execTrack.count.."), allowing AI to continue")
				else
					TraceAI("[BP_COMBO] End: Once mode - already executed "..execTrack.count.." times, allowing AI to continue")
				end
				ResetBlueprintRuntime("Once mode - allow AI to continue")
				return false  -- Don't block AI, let normal enemy selection run
			elseif repeatMode == "OncePerTarget" then
				-- OncePerTarget: Allow once per new target
				local currentTarget = BlueprintRuntime.target or enemy
				if execTrack.lastTarget == currentTarget then
					TraceAI("[BP_COMBO] End: OncePerTarget mode - same target, allowing AI to continue")
					ResetBlueprintRuntime("OncePerTarget - same target")
					return false
				else
					execTrack.lastTarget = currentTarget
					TraceAI("[BP_COMBO] End: OncePerTarget mode - new target, allowing execution")
									execTrack.count = execTrack.count + 1
									execTrack.lastTime = GetTick()
									TraceAI("[BP_COMBO] End: Combo complete. Execution count: "..execTrack.count)
									ResetBlueprintRuntime("OncePerTarget - new target")
									return true  -- Restart combo immediately for new target
				end
			elseif repeatMode == "XTimes" then
				-- XTimes: Allow N executions then block
				local repeatCount = tonumber(node.repeatCount) or 1
				if execTrack.count >= repeatCount then
					TraceAI("[BP_COMBO] End: XTimes mode - executed "..execTrack.count.." times (limit: "..repeatCount.."), allowing AI to continue")
					ResetBlueprintRuntime("XTimes limit reached")
					return false
				else
					execTrack.count = execTrack.count + 1
					execTrack.lastTime = GetTick()
					TraceAI("[BP_COMBO] End: XTimes mode - execution "..execTrack.count.."/"..repeatCount..", restarting combo")
					ResetBlueprintRuntime("XTimes - more executions allowed")
					return true  -- Restart combo
				end
			elseif repeatMode == "Always" then
				-- Always: Unlimited repeats (canRepeat stays true)
				TraceAI("[BP_COMBO] End: Always mode - unlimited repeats, restarting combo")
				execTrack.count = execTrack.count + 1
				execTrack.lastTime = GetTick()
				TraceAI("[BP_COMBO] End: Combo complete. Execution count: "..execTrack.count)
				ResetBlueprintRuntime("Always mode - restart")
				return true  -- Exit immediately, combo will restart next tick
			end
		else
			TraceAI("[BP_COMBO] Unsupported node type: "..tostring(node.type))
			local nextId = GetNextNodeId(combo, node.id, "then")
			AdvanceBlueprintNode(combo, nextId)
		end
	end

	if safety >= 12 then
		TraceAI("[BP_COMBO] Safety stop reached in combo execution")
	end
	return BlueprintRuntime.active ~= nil
end

-----------Config checking----------------

function doInit(myid)
	local logstring="Checking config..."
	-- Initialize Kimi type based on homon type
	KIMITYPE = GetV(V_HOMUNTYPE, MyID)
	if KIMITYPE == 0 or KIMITYPE == nil then
		KIMITYPE = OCCULT  -- Default to Occult if not found
	end
	if (UseAttackSkill==0 and UseSkillOnly==1) then
		UseSkillOnly = 0
		logstring=logstring.."\nUseAttackSkill==0, but UseSkillOnly==1. This will break the AI. UseSkillOnly set to 0. "
	end
	if DanceMinSP < 0 then
		DanceMinSP=math.floor(GetV(V_MAXSP,MyID)*DanceMinSP/100)*-1
	end
	if ChaseSPPauseSP < 0 then
		ChaseSPPauseSP=math.floor(GetV(V_MAXSP,MyID)*ChaseSPPauseSP/100)*-1
	end
	MyMaxSP=GetV(V_MAXSP,MyID)
	MyLastSP=GetV(V_SP,MyID)
	local loadtimesuccess = pcall(loadtimeouts)
	if loadtimesuccess==false then
		logstring=logstring.."\nfailed to load timeouts for owner "..GetV(V_OWNER,MyID).." if this is the first time you've used this account with AzzyAI, disregard this message"
	end
	OutFile=io.open("AAIStartH.txt","a")
	if OutFile == nil then
		Error("No write permissions for RO folder, please fix permissions on the RO folder in order to use AzzyAI. Version Info: "..OutString)
	else
		OutFile:write(logstring)
		OutFile:close()
	end
	local mskill,mlevel=GetMobSkill(MyID)
	if mskill~=0 and mlevel~=0 and AoEReserveSP==1 and AutoMobMode~=0 then
		ReserveSP=GetSkillInfo(mskill,3,mlevel)
	end
	OnInit()
	if AggressiveRelogTracking~=1 then
		GuardTimeout=GuardTimeout+500
		QuickenTimeout=QuickenTimeout+500
	else
		timelag=LastAITime_ART-GetTick()
		GuardTimeout=GuardTimeout+timelag
		QuickenTimeout=QuickenTimeout+timelag
	end
	if BlueprintComboEnabled == 1 then
		NormalizeBlueprintCombos()
		ResetBlueprintRuntime("Init")
	else
		ResetBlueprintRuntime("Blueprint combos disabled")
	end
	UpdateTimeoutFile()
	DoneInit=1
end
function loadtimeouts()
	if IsHomun(MyID)==1 then
		dofile(ConfigPath.."data/H_"..GetV(V_OWNER,MyID).."Timeouts.lua")
	else
		dofile(ConfigPath.."data/M_"..GetV(V_OWNER,MyID).."Timeouts.lua")
	end
	if AggressiveRelogTracking==1 then
		if IsHomun(MyID)==1 then
			dofile(AggressiveRelogPath.."H_"..GetV(V_OWNER,MyID).."Time.lua")
		else
			dofile(AggressiveRelogPath.."M_"..GetV(V_OWNER,MyID).."Time.lua")
		end
	end
end

-----------ENHANCED LOGGING SYSTEM-----------
-- Smart logging function that checks LogEnable table from H_Extra.lua
-- Usage: Log("CATEGORY", "message")
-- Categories match LogEnable keys (e.g., "COMBO_INIT", "COMBO_CAST", etc.)
function Log(category, message)
	-- If LogEnable table exists and this category is enabled, log it
	if LogEnable and LogEnable[category] == 1 then
		TraceAI("["..category.."] "..message)
	end
	-- For backward compatibility, always trace messages without category prefix
	-- if they use the old [BRACKET] format
	if string.sub(message, 1, 1) == "[" then
		TraceAI(message)
	end
end

-----------COMBO SYSTEM FUNCTIONS-----------

-- Map user-config skill level overrides (from H_Config.lua) by skill ID
function GetConfiguredSkillLevel(skillid)
	if skillid == S_ILLUSION_OF_LIGHT then
		return illusionOfLightLevel
	elseif skillid == S_ILLUSION_OF_BREATH then
		return illusionOfBreathLevel
	elseif skillid == S_ILLUSION_CRUSHER then
		return illusionOfCrusherLevel
	elseif skillid == S_ILLUSION_OF_CLAWS then
		return illusionOfClawsLevel
	elseif skillid == S_CHAOTIC_HEAL then
		return chaoticHealLevel
	elseif skillid == S_BODY_DOUBLE then
		return bodyDoubleLevel
	elseif skillid == S_WARM_DEF then
		return warmDefLevel
	end
	return nil
end

-- Configuration-based gating for combo skill execution
function CanUseComboSkill(skillid, level, myid, target)
	-- onlyAOE: If enabled, allow only Illusion of Light in combos (user preference)
	if onlyAOE == 1 then
		if skillid ~= S_ILLUSION_OF_LIGHT then
			return false
		end
	end

	-- For combo execution, respect cooldowns but NOT the UseX flags
	-- UseX flags (UseChaoticHeal, UseBodyDouble, UseWarmDef) are for AUTO-casting outside combos
	-- In combo context, if a skill is in the rotation, it should execute
	
	-- Warm Defense: Check cooldown only (UseWarmDef is for auto-buff, not combos)
	if skillid == S_WARM_DEF then
		if GuardTimeout ~= -1 and GetTick() < GuardTimeout then 
			TraceAI("[COMBO] Warm Def on cooldown, advancing slot")
			return false 
		end
		return true
	end

	-- Body Double: Check cooldown only (UseBodyDouble is for auto-buff, not combos)
	if skillid == S_BODY_DOUBLE then
		if QuickenTimeout ~= -1 and GetTick() < QuickenTimeout then 
			TraceAI("[COMBO] Body Double on cooldown, advancing slot")
			return false 
		end
		return true
	end

	-- Chaotic Heal: Always allow in combos (UseChaoticHeal is for auto-heal, not combos)
	if skillid == S_CHAOTIC_HEAL then
		return true
	end

	-- AoE/Offensive skills: Always allow in combos
	-- Illusion of Light (8034), Illusion Crusher (8031), Illusion of Breath (8024), Illusion of Claws (8009), Master Swap (8005 placeholder)
	-- Auto-casting of these is controlled separately via UseAttackSkill/UseHomunSSkillChase flags
	if skillid == S_ILLUSION_OF_LIGHT or 
	   skillid == S_ILLUSION_CRUSHER or 
	   skillid == S_ILLUSION_OF_BREATH or 
	   skillid == S_ILLUSION_OF_CLAWS or 
	   skillid == S_MASTER_SWAP then
		return true
	end

	-- All other skills: Allow in combos (SP/range/level checked elsewhere in ExecuteCombo)
	return true
end

-- Initialize combo state on first attack
function InitCombo()
	ComboState.currentSlot = 1
	ComboState.comboCountRemaining = ComboSlot1_ComboCount
	ComboState.lastTarget = MyEnemy
	ComboState.lastCycleTime = GetTick()
	ComboState.lastActionTime = nil  -- Clear any stale action timer
	local skillid, count = GetComboSlotInfo(1)
	TraceAI("[COMBO INIT] Starting combo system | Slot: 1 | Skill: "..tostring(skillid).." | Count: "..count.." | Target: "..tostring(MyEnemy))
end

-- Reset combo to slot 1 (on target change or manual reset)
function ResetCombo()
	local oldSlot = ComboState.currentSlot
	ComboState.currentSlot = 1
	ComboState.comboCountRemaining = ComboSlot1_ComboCount
	ComboState.lastCycleTime = GetTick()
	ComboState.lastActionTime = nil  -- Clear any stale action timer
	local skillid, count = GetComboSlotInfo(1)
	TraceAI("[COMBO RESET] Resetting from slot "..oldSlot.." to slot 1 | Skill: "..tostring(skillid).." | Count: "..count)
end

-- Get skill ID and cast count for current slot
function GetComboSlotInfo(slot)
	if slot == 1 then
		return ComboSlot1_SkillID, ComboSlot1_ComboCount
	elseif slot == 2 then
		return ComboSlot2_SkillID, ComboSlot2_ComboCount
	elseif slot == 3 then
		return ComboSlot3_SkillID, ComboSlot3_ComboCount
	elseif slot == 4 then
		return ComboSlot4_SkillID, ComboSlot4_ComboCount
	elseif slot == 5 then
		return ComboSlot5_SkillID, ComboSlot5_ComboCount
	elseif slot == 6 then
		return ComboSlot6_SkillID, ComboSlot6_ComboCount
	elseif slot == 7 then
		return ComboSlot7_SkillID, ComboSlot7_ComboCount
	elseif slot == 8 then
		return ComboSlot8_SkillID, ComboSlot8_ComboCount
	end
	return 0, 0
end

-- Advance to next slot in combo (handles wrapping from 4 -> 1)
function AdvanceComboSlot()
	local oldSlot = ComboState.currentSlot
	if ComboState.currentSlot < 8 then
		ComboState.currentSlot = ComboState.currentSlot + 1
	else
		ComboState.currentSlot = 1  -- Wrap back to start
		TraceAI("[COMBO ADVANCE] Wrapping from slot 8 back to slot 1")
	end
	local skillid, combocount = GetComboSlotInfo(ComboState.currentSlot)
	
	-- If we've hit an empty slot (skillID=0), reset combo to slot 1
	if skillid == 0 then
		TraceAI("[COMBO ADVANCE] Hit empty slot "..ComboState.currentSlot.." - resetting combo to slot 1")
		ComboState.currentSlot = 1
		skillid, combocount = GetComboSlotInfo(1)
	end
	
	ComboState.comboCountRemaining = combocount
	ComboState.lastCycleTime = GetTick()
	local skillname = (skillid == -1) and "AUTO-ATTACK" or ("SKILL "..skillid)
	TraceAI("[COMBO ADVANCE] Slot "..oldSlot.." -> "..ComboState.currentSlot.." | Next: "..skillname.." x"..combocount)
end

-- Check if current state matches combo execution context
function CanExecuteComboInState()
	if MyState == ATTACK_ST and ComboRunDuringAttack == 1 then
		return true
	-- CHASE_ST removed - combo should NOT run during chase, only during attack
	-- Chase state focuses on movement; combo executes once in attack range
	elseif MyState == IDLE_ST and ComboRunDuringIdle == 1 then
		return true
	end
	return false
end

-- Main combo execution function
function ExecuteCombo(myid, target)
	-- Initialize if first time
	if ComboState.currentSlot == 0 then
		InitCombo()
		return
	end

	-- Get current slot skill info EARLY to decide target/validation rules
	local skillid, combocount = GetComboSlotInfo(ComboState.currentSlot)

	-- Support skills can be cast without a monster target; offensive ones require a valid enemy
	local isSupportSkill = (skillid == S_CHAOTIC_HEAL or skillid == S_BODY_DOUBLE or skillid == S_WARM_DEF)
	if not isSupportSkill then
		-- Ensure target is valid before attempting any offensive combo action
		if target == 0 or IsMonster(target) ~= 1 then
			TraceAI("[COMBO] Invalid or non-monster target; skipping offensive combo execution")
			return
		end
	end

	-- CRITICAL: Check target change FIRST, before any other early returns
	-- This ensures combo resets immediately when target changes
	if ComboResetOnTargetChange == 1 and ComboState.lastTarget ~= target then
		TraceAI("[COMBO] Target changed from "..tostring(ComboState.lastTarget).." to "..tostring(target).." - triggering reset")
		ResetCombo()
		ComboState.lastTarget = target
		return
	end
	ComboState.lastTarget = target
	
	-- Check if we're in a valid context to execute combos
	if not CanExecuteComboInState() then
		return
	end
	
	-- Combo timing is managed by ComboState.lastActionTime; do not block on global AutoSkillTimeout
	
	-- Skip if skill not configured (0 = disabled)
	if skillid == 0 then
		AdvanceComboSlot()
		return
	end

	-- Auto-attack slot support: skillid == -1 triggers basic attack instead of skill
	if skillid == -1 then
		-- For auto-attacks, verify we're in melee range (1 cell = ~5 units)
		-- This prevents spamming attacks when out of range
		local distance = GetDistanceA(myid, target)
		local attackRange = GetV(V_ATTACKRANGE, myid)  -- Get actual attack range
		
		if distance > attackRange then
			TraceAI("[COMBO AUTO-ATTACK] Out of range (dist: "..distance..") - waiting before attacking")
			return  -- Don't advance, wait until in range
		end

		-- Simplified timing: rely on ComboAutoAttackDelay without motion gating
		if ComboState.lastActionTime then
			if GetTick() - ComboState.lastActionTime < (ComboAutoAttackDelay or 500) then
				return
			end
			-- Delay complete; advance counters
			ComboState.comboCountRemaining = ComboState.comboCountRemaining - 1
			if ComboState.comboCountRemaining <= 0 then
				TraceAI("[COMBO AUTO-ATTACK] Attack count complete - advancing to next slot")
				AdvanceComboSlot()
			else
				TraceAI("[COMBO AUTO-ATTACK] Continuing - "..ComboState.comboCountRemaining.." more attacks in this slot")
			end
			ComboState.lastActionTime = nil
			return
		else
			TraceAI("[COMBO AUTO-ATTACK] Slot "..ComboState.currentSlot.." | Target: "..target.." | Range: OK | Remaining: "..ComboState.comboCountRemaining.."/"..combocount)
			Attack(myid, target)
			ComboState.lastActionTime = GetTick()
			return
		end
	end
	
	-- Check if this specific skill is on cooldown
	if AutoSkillCooldown[skillid] and GetTick() < AutoSkillCooldown[skillid] then
		local cooldownLeft = math.ceil((AutoSkillCooldown[skillid] - GetTick()) / 1000)
		TraceAI("[COMBO COOLDOWN] Skill "..skillid.." on cooldown - "..cooldownLeft.."s remaining")
		return
	end
	
	-- Get skill level (prefer user configuration overrides from H_Config.lua)
	local skilllevel = 0
	if KIMITYPE and SkillList[KIMITYPE] and SkillList[KIMITYPE][skillid] then
		skilllevel = SkillList[KIMITYPE][skillid]
	end
	local configuredLevel = GetConfiguredSkillLevel(skillid)
	if configuredLevel ~= nil then
		skilllevel = configuredLevel
	end
	if skilllevel == 0 then
		TraceAI("[COMBO CONFIG] Skill "..skillid.." disabled by configuration - advancing slot")
		AdvanceComboSlot()
		return
	end

	-- Respect user configuration gating (e.g., onlyAOE, healing/buff toggles)
	if not CanUseComboSkill(skillid, skilllevel, myid, target) then
		TraceAI("[COMBO CONFIG] Skipping skill "..skillid.." due to config gating - advancing slot")
		AdvanceComboSlot()
		return
	end
	
	-- Choose appropriate cast target for support vs offensive skills
	local castTarget = target
	if skillid == S_CHAOTIC_HEAL or skillid == S_WARM_DEF then
		castTarget = myid
	elseif skillid == S_BODY_DOUBLE then
		castTarget = GetV(V_OWNER, myid)
	end

	-- Get skill's attack range and check if we're in range (use castTarget)
	local skillRange = GetSkillInfo(skillid, 2, skilllevel) or 1
	local distance = GetDistanceA(myid, castTarget)
	if distance > skillRange then
		TraceAI("[COMBO SKILL] Out of range (dist: "..distance..", skill range: "..skillRange..") - waiting before casting")
		return  -- Don't advance, wait until in range
	end
	
	-- Check if we have enough SP to cast
	local spcost = GetSkillInfo(skillid, 3, skilllevel)
	local currentSP = GetV(V_SP, myid)
	local maxSP = GetV(V_MAXSP, myid)
	if currentSP < spcost then
		TraceAI("[COMBO SP] Insufficient SP | Skill: "..skillid.." | Need: "..spcost.." | Have: "..currentSP.."/"..maxSP.." - waiting")
		return
	end
	
	-- Check if we already issued a skill command
	if ComboState.lastActionTime then
		-- Check if skill is instant-cast (cast time = 0)
		local castTime = GetSkillInfo(skillid, 6, skilllevel) or 0
		local isInstantCast = (castTime == 0)
		
		if isInstantCast then
			-- Instant-cast skills don't have MOTION_CASTING - just wait for fixed delay
			if GetTick() < ComboState.lastActionTime + (ComboSkillCastDelay or 500) then
				return  -- Still in delay
			else
				-- Delay complete - advance combo
				ComboState.comboCountRemaining = ComboState.comboCountRemaining - 1
				TraceAI("[COMBO CAST] Instant-cast skill delay complete - advancing")
				if ComboState.comboCountRemaining <= 0 then
					AdvanceComboSlot()
				else
					TraceAI("[COMBO CAST] Continuing - "..ComboState.comboCountRemaining.." more casts in this slot")
				end
				ComboState.lastActionTime = nil
				return
			end
		else
			-- Skill with cast time - wait for casting motion to start or complete
			if GetV(V_MOTION, myid) == MOTION_CASTING or GetV(V_MOTION, myid) == MOTION_SKILL then
				-- Casting started! Decrement counter now that we confirmed it cast
				ComboState.comboCountRemaining = ComboState.comboCountRemaining - 1
				-- Now wait for skill delay before advancing
				if GetTick() < ComboState.lastActionTime + (ComboSkillCastDelay or 500) then
					TraceAI("[COMBO CAST] Skill casting in progress...")
					return
				else
					TraceAI("[COMBO CAST] Skill delay complete - advancing")
					if ComboState.comboCountRemaining <= 0 then
						AdvanceComboSlot()
					else
						TraceAI("[COMBO CAST] Continuing - "..ComboState.comboCountRemaining.." more casts in this slot")
					end
					ComboState.lastActionTime = nil
					return
				end
			elseif GetTick() < ComboState.lastActionTime + 1000 then
				-- Still waiting for casting to start (give it up to 1 second)
				return
			else
				-- Skill failed to cast within 1 second, reset and try again
				TraceAI("[COMBO CAST] Skill failed to cast, resetting action timer")
				ComboState.lastActionTime = nil
				return
			end
		end
	else
		-- First time issuing skill - do NOT decrement counter yet
		TraceAI("[COMBO CAST] Slot "..ComboState.currentSlot.." | Skill: "..skillid.." Lv."..skilllevel.." | SP: "..spcost.." | Target: "..castTarget.." | Remaining: "..ComboState.comboCountRemaining.."/"..combocount)
		DoSkill(skillid, skilllevel, castTarget)
		ComboState.lastActionTime = GetTick()
		-- Update relevant cooldown trackers for combo-cast buffs to avoid immediate recast
		if skillid == S_WARM_DEF then
			local cd = GetSkillInfo(skillid, 9, skilllevel) or 500
			GuardTimeout = GetTick() + cd + (AutoSkillCastTimeout or 500)
			UpdateTimeoutFile()
		elseif skillid == S_BODY_DOUBLE then
			local cd = GetSkillInfo(skillid, 9, skilllevel) or 500
			QuickenTimeout = GetTick() + cd + (AutoSkillCastTimeout or 500)
			UpdateTimeoutFile()
		end
		return
	end
end

--########################################
--### Friend the merc/homun - old one  ###
--### by Misch, new one by Dr. Azzy    ###
--########################################

if (AssumeHomun==1) then
	if (NewAutoFriend==0) then
		ff = {}
		Hactors = GetActors()
		Howner  = GetV(V_OWNER,Hactors[1])
		FX = 0
		FY = 0
		OX,OY = GetV(V_POSITION,Howner)
		for i,v in ipairs(Hactors) do
			if (v ~= Howner) then
				if (IsMonster(v)==0) then
					FX,FY = GetV(V_POSITION,v)
					if (FX==OX and FY==OY and v<100000) then --is not a player
						MyFriends[v]=2
					end
				end
			end
		end
	else
		NeedToDoAutoFriend=1
		TraceAI("Setting NeedToDoAutoFriend")
	end
end
--####################################
--######## Command Processing ########
--####################################

function	OnMOVE_CMD (x,y)
	TraceAI ("OnMOVE_CMD")
	if GetDistanceAPR(GetV(V_OWNER,MyID),x,y) > 15 or x==0 or y==0 then -- Bogus move command
		local ox,oy=GetV(V_POSITION,GetV(V_OWNER,MyID))
		logappend("AAI_ERROR","move command to invalid location "..formatpos(x,y).." owner pos "..formatpos(ox,oy))
		TraceAI("OnMOVE_CMD - Command disregarded; invalid location logged")
		return
	end
	
	ResetCounters()
	if ( x == MyDestX and y == MyDestY and MOTION_MOVE == GetV(V_MOTION,MyID)) then
		return
	end

	--local curX, curY = GetV (V_POSITION,MyID)
	--if (math.abs(x-curX)+math.abs(y-curY) > 15) then
	--	List.pushleft (ResCmdList,{MOVE_CMD,x,y})
	--	x = math.floor((x+curX)/2)  
	--	y = math.floor((y+curY)/2)
	--end
	MyMoveX,MyMoveY=x,y
	MyDestX,MyDestY=x,y
	Move (MyID,MyDestX,MyDestY)	
	MyState = MOVE_CMD_ST
	if (MirAIFriending==1) then --emulate MirAI's annoying friending process
		TraceAI("Starting Friend Routine")
		local actors = GetActors()
		for i,v in ipairs(actors) do
			if (IsMonster(v)~=1 and v~=GetV(V_OWNER,MyID) and v~=MyID) then
				xx,yy=GetV(V_POSITION,v)
				if (xx==x and (yy+1==y or yy-1==y)) then
					
					if (MyFriends[v]==nil) then
						MyFriends[v] = 1
						UpdateFriends()
						MyState=FRIEND_CIRCLE_ST
						TraceAI("Friend Addition")
						NewFriendX,NewFriendY=GetV(V_POSITION,v)
						return
					elseif (MyFriends[v]~=nil) then
						MyFriends[v] = nil
						UpdateFriends()
						MyState=FRIEND_CROSS_ST
						TraceAI("Friend Removal")
						NewFriendX,NewFriendY=GetV(V_POSITION,v)
						return
					end
				end
			end
		end
	end
end


function	OnSTOP_CMD ()

	TraceAI ("OnSTOP_CMD")
	logappend("AAI_ERROR","STOP_CMD sent! This should NEVER HAPPEN!")
	if (GetV(V_MOTION,MyID) ~= MOTION_STAND) then
		Move (MyID,GetV(V_POSITION,MyID))
	end
	MyState = IDLE_ST
	MyDestX = 0
	MyDestY = 0
	MyEnemy = 0
	EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
	EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
	MySkill = 0

end


function	OnATTACK_OBJECT_CMD (id)
	TraceAI ("OnATTACK_OBJECT_CMD")
	ResetCounters()
	MySkill = 0
	MyEnemy = id
	BypassKSProtect=1
	if (UseBerserkAttack==1) then
		BerserkMode=1
	end
	MyState = CHASE_ST
	OnCHASE_ST()
end


function	OnATTACK_AREA_CMD (x,y)

	TraceAI ("OnATTACK_AREA_CMD")
	logappend("AAI_ERROR","ATTACK_AREA_CMD sent! This should NEVER HAPPEN!")
	if (x ~= MyDestX or y ~= MyDestY or MOTION_MOVE ~= GetV(V_MOTION,MyID)) then
		Move (MyID,x,y)	
	end
	MyDestX = x
	MyDestY = y
	MyEnemy = 0
	EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
	EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
	MyState = ATTACK_AREA_CMD_ST
	
end

function	OnPATROL_CMD (x,y)

	TraceAI ("OnPATROL_CMD")
	logappend("AAI_ERROR","PATROL_CMD sent! This should NEVER HAPPEN!")
	MyPatrolX , MyPatrolY = GetV (V_POSITION,MyID)
	MyDestX = x
	MyDestY = y
	Move (MyID,x,y)
	MyState = PATROL_CMD_ST

end


function	OnHOLD_CMD ()

	TraceAI ("OnHOLD_CMD")
	logappend("AAI_ERROR","HOLD_CMD sent! This should NEVER HAPPEN!")
	MyDestX = 0
	MyDestY = 0
	MyEnemy = 0
	EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
	EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
	MyState = HOLD_CMD_ST

end


function	OnSKILL_OBJECT_CMD (level,skill,id)
	TraceAI ("OnSKILL_OBJECT_CMD"..skill.." "..id.." "..level)
	ResetCounters()
	MySkillLevel = level
	MySkill = skill
	MyEnemy = id

	if IsMonster(id)==1 and SuperPassive~=1 then
		BypassKSProtect=1
		if (UseBerserkSkill==1) then
			BerserkMode=1
		end
		MyState = CHASE_ST
		OnCHASE_ST()
	else
		MyState = SKILL_OBJECT_CMD_ST
	end
end


function	OnSKILL_AREA_CMD (level,skill,x,y)
	ResetCounters()
	TraceAI ("OnSKILL_AREA_CMD")
	targetx,targety=Closest(MyID,x,y,AttackRange(MyID,skill,level))
	Move (MyID,targetx,targety)
	MyDestX = x
	MyDestY = y
	MySkillLevel = level
	MySkill = skill
	MyState = SKILL_AREA_CMD_ST
	
end

function	OnFOLLOW_CMD ()
	ReturnToMoveHold = 0 
	if (MyState ~= FOLLOW_CMD_ST) then
		if StickyStandby > 0 then
			ShouldStandby=1
		end
		BetterMoveToOwner (MyID,FollowStayBack)
		MyState = FOLLOW_CMD_ST
		MyEnemy = 0 
		EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
		EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
		MySkill = 0
		TraceAI ("OnFOLLOW_CMD")
	else
		if StickyStandby > 0 then
			ShouldStandby=0
		end
		MyState = IDLE_ST
		MyEnemy = 0 
		EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
		EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
		MySkill = 0
		TraceAI ("FOLLOW_CMD_ST --> IDLE_ST")
	end

end




function	ProcessCommand (msg)

	if	(msg[1] == MOVE_CMD) then
		TraceAI ("MOVE_CMD")
		OnMOVE_CMD (msg[2],msg[3])
	elseif	(msg[1] == STOP_CMD) then
		TraceAI ("STOP_CMD")
		OnSTOP_CMD ()
	elseif	(msg[1] == ATTACK_OBJECT_CMD) then
		TraceAI ("ATTACK_OBJECT_CMD")
		OnATTACK_OBJECT_CMD (msg[2])
	elseif	(msg[1] == ATTACK_AREA_CMD) then
		TraceAI ("ATTACK_AREA_CMD")
		OnATTACK_AREA_CMD (msg[2],msg[3])
	elseif	(msg[1] == PATROL_CMD) then
		TraceAI ("PATROL_CMD")
		OnPATROL_CMD (msg[2],msg[3])
	elseif	(msg[1] == HOLD_CMD) then
		TraceAI ("HOLD_CMD")
		OnHOLD_CMD ()
	elseif	(msg[1] == SKILL_OBJECT_CMD) then
		TraceAI ("SKILL_OBJECT_CMD")
		OnSKILL_OBJECT_CMD (msg[2],msg[3],msg[4],msg[5])
	elseif	(msg[1] == SKILL_AREA_CMD) then
		TraceAI ("SKILL_AREA_CMD")
		OnSKILL_AREA_CMD (msg[2],msg[3],msg[4],msg[5])
	elseif	(msg[1] == FOLLOW_CMD) then
		TraceAI ("FOLLOW_CMD")
		OnFOLLOW_CMD ()
	end
end

function ResetCounters()
	MyPState				= 0
	MyPSkill				= 0
	MyPEnemy				= 0
	MyPSkillLevel			= 0
	MySkillUsedCount		= 0
	ChaseGiveUpCount		= 0
	AttackGiveUpCount		= 0
	ChaseDebuffUsed			= 0
	AttackDebuffUsed		= 0
	BypassKSProtect			= 0
	BerserkMode				= 0
	ReturnToState			= 0
	NewFriend				= 0
	FriendMotionTime		= 0
	FriendCircleIter		= 0
	FriendCircleTimeout		= 0
	AtkPosbugFixTimeout		= 0
	SkillObjectCMDTimeout	= 0
	FollowTryCount			= 0
	MyMoveX,MyMoveY			= 0,0
	if CastSkillMode < 0 then
		CastSkill=0
		CastSkillLevel=0
		CastSkillMode=0
	end
	return
end

--###############################
--######## State Process ########
--###############################


function	OnIDLE_ST ()
	--if ReturnToMoveHold~=0 then 
	--	MyState=MOVE_CMD_HOLD_ST
	--	OnMOVE_CMD_HOLD_ST()
	--	return
	--end
	TraceAI ("OnIDLE_ST")
	ResetCounters()
	MySkill					= 0
	MyDestX					= 0
	MyDestY					= 0
	MyEnemy					= 0
	if RunBlueprintCombos("OnIdle", MyID, MyEnemy) then
		return
	end
	if (DoIdleTasks()==nil) then
		return
	end
	--StickyStandby handling
	if (ShouldStandby==1 and StickyStandby > 0) then
		MyState=FOLLOW_CMD_ST
		return
	end
	--Targeting
	if SuperPassive~=1 then
		local	object = SelectEnemy(GetFriendTargets())
		if (object ~= 0) then							-- MYOWNER_ATTACKED_IN
			MyState = CHASE_ST
			MyEnemy = object
			TraceAI ("IDLE_ST -> CHASE_ST : MYOWNER_ATTACKED_IN")
			if (FastChangeCount < FastChangeLimit and FastChange_I2C ==1) then
				OnCHASE_ST()
			end
			return 
		end
		if (HPPercent(MyID) > AggroHP and (SPPercent(MyID) > AggroSP or AggroSP==0) and (ShouldStandby == 0 or StickyStandby ==0) ) then
			aggro=1
		else
			aggro=0
		end
		object=SelectEnemy(GetEnemyList(MyID,aggro))
		if object~=0 then
			MyState = CHASE_ST
			MyEnemy = object
			TraceAI ("IDLE_ST -> CHASE_ST : ATTACKED_IN")
			if (FastChangeCount < FastChangeLimit and FastChange_I2C ==1) then
				return OnCHASE_ST()
			end
			return	
		end
		if (aggro==1 and TankMonsterCount < TankMonsterLimit) then
			object = SelectEnemy(GetEnemyList(MyID,-1))
			if (object ~= 0) then
				MyState = TANKCHASE_ST
				MyEnemy = object
				TraceAI ("IDLE_ST -> TANKCHASE_ST")
				return
			end
		end
	end
	--Following
	local distance
	if ReturnToMoveHold ~=0 then
		distance = GetDistanceAP(MyID,StickyX,StickyY)
	else
		distance = GetDistanceFromOwner(MyID)
	end
	if (UseIdleWalk~=0 and HPPercent(MyID) > AggroHP and SPPercent(MyID) > math.max(AggroSP,IdleWalkSP)) then -- CHECK
		if ( distance > GetMoveBounds() or distance == -1) then		-- MYOWNER_OUTSIGNT_IN
			MyState = FOLLOW_ST
			TraceAI ("IDLE_ST -> FOLLOW_ST")
			return
		else 
			TraceAI("IDLE_ST -> IDLEWALK_ST, idle walk mode ="..UseIdleWalk)
			MyState=IDLEWALK_ST
		end
	elseif (( distance > DiagonalDist(FollowStayBack+1) and not (ChaseSPPause==1 and GetV(V_SP,MyID) < ChaseSPPauseSP and GetTick() - math.max(LastMovedTime,LastSPTime) > (5000-ChaseSPPauseTime)))or distance == -1) then		-- MYOWNER_OUTSIGNT_IN
		MyState = FOLLOW_ST
		TraceAI ("IDLE_ST -> FOLLOW_ST")
		return OnFOLLOW_ST()
	end	
	if UseAutoHeal==3 then
		if DoHealingTasks(MyID) == 1 then 
			return
		end
	end
	-- Always call DoAutoBuffs during idle; OnAutoBuffs hook will decide whether to skip based on combo runtime
	DoAutoBuffs(-2)
	if UseIdleWalk ~=0 and HPPercent(MyID) > AggroHP and SPPercent(MyID) > math.max(AggroSP,IdleWalkSP) then
		TraceAI("IDLE_ST -> IDLEWALK_ST, idle walk mode ="..UseIdleWalk)
		MyState=IDLEWALK_ST
	end
end

function	OnFOLLOW_ST ()

	TraceAI ("OnFOLLOW_ST - follow try count: "..FollowTryCount.." ownerpos: "..formatpos(GetV(V_POSITION,GetV(V_OWNER,MyID))).."my pos history:"..formatmypos(10))
	local dist = GetDistanceFromOwner(MyID)
	if dist > GetMoveBounds() then 
		ReturnToMoveHold = 0
		StickyX,StickyY=0,0
	end
	if ReturnToMoveHold ~=0 then
		dist = GetDistanceAP(MyID,StickyX,StickyY)
	end
	if (dist <= DiagonalDist(FollowStayBack+1)) then		--  DESTINATION_ARRIVED_IN 
		FollowTryCount=0
		MyState = IDLE_ST
		TraceAI ("FOLLOW_ST -> IDLE_ST ownerpos: "..formatpos(GetV(V_POSITION,GetV(V_OWNER,MyID))).."my pos history:"..formatmypos(10))
		if (FastChangeCount < FastChangeLimit and FastChange_F2I==1) then
			FastChangeCount = FastChangeCount+1
			return OnIDLE_ST()
		end
	else
		if (FollowTryCount > FollowTryPanic and GetV(V_MOTION,MyID)~=MOTION_MOVE) then
			if FollowTryCount > 2*FollowTryPanic then
				if FollowTryCount > 3*FollowTryPanic then 
					TraceAI("FOLLOW_ST -> IDLE_ST - Can't follow even using panic'ed methods - Giving up")
					MyState=IDLE_ST
					if (FastChangeCount < FastChangeLimit and FastChange_F2I==1) then
						FastChangeCount = FastChangeCount+1
						return OnIDLE_ST()
					end
				else
					if ReturnToMoveHold ==0 then
						MoveToOwner(MyID)
						FollowTryCount=FollowTryCount+1
						TraceAI("Emergency follow - MoveToOwner() called")
					else 
						TraceAI("FOLLOW_ST -> IDLE_ST - Can't get to move sticky location even using panic'ed methods - Giving up")
						MyState=IDLE_ST
						if (FastChangeCount < FastChangeLimit and FastChange_F2I==1) then
							FastChangeCount = FastChangeCount+1
							return OnIDLE_ST()
						end
					end
				end
			else
				FollowTryCount=FollowTryCount+1
				if ReturnToMoveHold==0 then
					BetterMoveToOwner (MyID,1)
				else
					BetterMoveToLoc(MyID,1,StickyX,StickyY)
				end
			end
		else
			local dx,dy
			if ReturnToMoveHold==0 then
				dx,dy=BetterMoveToOwnerXY(MyID,FollowStayBack)
			else
				dx,dy=BetterMoveToLocXY(MyID,FollowStayBack,StickyX,StickyY)
			end
			local mx,my=GetV(V_POSITION,MyID)
			--TraceAI("followobstacle dest"..formatpos(dx,dy).." pos "..formatpos(mx,my))
			if math.abs(my-dy) <=1 then
				--TraceAI("math.abs(my-dy) <=1")
				if math.abs(mx-dx) <=1 then --We could be in a bounce loop, better see if we are
					--TraceAI("math.abs(my-dy) <=1")
					if MyPosY[1]==my then
						for v=2,5,1 do
							if MyPosX[v]==dx then
								MoveToOwner(MyID)
								FollowTryCount=FollowTryCount+1
								TraceAI("Bounce loop detected, emergency measures taken")
								break
							end
							if v==5 then
								MyDestX,MyDestY=dx,dy
								Move(MyID,MyDestX,MyDestY)
							end
						end
					else
						MyDestX,MyDestY=dx,dy
						Move(MyID,MyDestX,MyDestY)
					end
				else
					--TraceAI("math.abs(mx-dx) > 1"..math.abs(mx-dx))
					MyDestX,MyDestY=dx,dy
					Move(MyID,MyDestX,MyDestY)
				end
			else
				--TraceAI("math.abs(my-dy) > 1"..math.abs(my-dy))
				MyDestX,MyDestY=dx,dy
				Move(MyID,MyDestX,MyDestY)
			end
			if (GetV(V_MOTION,MyID) ~= MOTION_MOVE) then
				FollowTryCount=FollowTryCount+1
			else
				FollowTryCount=0
			end
		end
		TraceAI ("FOLLOW_ST -> FOLLOW_ST ownerpos: "..formatpos(GetV(V_POSITION,GetV(V_OWNER,MyID))).."my pos history:"..formatmypos(10).."dest cell"..formatpos(MyDestX,MyDestY))
		return
	end
end


function	OnCHASE_ST ()
	MyAttackStanceX,MyAttackStanceY = 0,0
	TraceAI ("OnCHASE_ST")
	local blueprintActive = RunBlueprintCombos("OnChase", MyID, MyEnemy)
	-- Initialize combo on entering chase state if configured to run during chase
	if ComboEnabled == 1 and ComboRunDuringChase == 1 and blueprintActive ~= true then
		if ComboState and ComboState.currentSlot == 0 then
			TraceAI("[CHASE_ST] Entering chase state - initializing combo system")
			InitCombo()
		end
		-- Execute combo during chase - let ExecuteCombo handle range/SP checks
		if ComboState and ComboState.currentSlot > 0 then
			TraceAI("[CHASE_ST] Executing combo slot "..ComboState.currentSlot.." during chase")
			ExecuteCombo(MyID, MyEnemy)
			-- Don't return here - allow movement logic to continue
		end
	end
	aggro = GetAggroCount()
	if(aggro > UseBerserkMobbed and UseBerserkMobbed > 0)then
		BerserkMode=1
	end	
	if DoAutoBuffs(-1) == 1 then
		DoAutoBuffs(2)
	end
	if (UseAttackSkill == 1) and (blueprintActive ~= true) then
		if (MySkill == 0 and MySkillLevel == 0) then
			MySkill,MySkillLevel=GetAtkSkill(MyID)
		end
	end
	if true==IsOutOfSight(MyID,MyEnemy) then
		value="true "
	else
		value="false"
	end
	if(IsNotKS(MyID,MyEnemy)==0) then
		local reason=GetKSReason(MyID,MyEnemy)
		TraceAI ("CHASE_ST -> IDLE_ST : Enemy is taken "..reason)
		MyState = IDLE_ST
		MyEnemy = 0
		EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
		EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
		MyDestX, MyDestY = 0,0
		ChaseGiveUpCount=0
		if (FastChangeCount < FastChangeLimit and FastChange_C2I == 1) then
			FastChangeCount = FastChangeCount+1
			return OnIDLE_ST()
		end
	end
	if (true == IsOutOfSight(MyID,MyEnemy)) then	-- ENEMY_OUTSIGHT_IN
	
		MyState = IDLE_ST
		MyEnemy = 0
		EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
		EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
		MyDestX, MyDestY = 0,0
		TraceAI ("CHASE_ST -> IDLE_ST : Enemy out of sight")
		ChaseGiveUpCount=0
		
		if (FastChangeCount < FastChangeLimit and FastChange_C2I == 1) then
			FastChangeCount = FastChangeCount+1
			return OnIDLE_ST()
		else
			return
		end
	end
	if GetV(V_MOTION,MyID)~=MOTION_MOVE then
		if ChaseGiveUpCount > ChaseGiveUp then
			Unreachable[MyEnemy]=1
			if SelectEnemy(GetEnemyList(MyID,-2)) == MyEnemy then --Oh crap, 
				TraceAI("CHASE_ST -> FOLLOW_ST : Target "..MyEnemy.." marked unreachable but is also rescue target! Trying follow state in hopes of a clean line of attack from owner")
				MyState = FOLLOW_ST
				MyEnemy = 0
				EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
				EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
				MyDestX,MyDestY=0,0
				ChaseGiveUpCount=0

				return OnFOLLOW_ST()
			elseif AllTargetUnreachable==1 then
				MyState = FOLLOW_ST
				MyDestX, MyDestY = 0,0
				TraceAI ("CHASE_ST -> FOLLOW_ST : All targets marked unreachable, and can't reach "..MyEnemy)
				ChaseGiveUpCount=0
				MyEnemy = 0
				EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
				EnemyPosY = {0,0,0,0,0,0,0,0,0,0}

				return OnFOLLOW_ST()
			else 
				MyState = IDLE_ST
				MyDestX, MyDestY = 0,0
				TraceAI ("CHASE_ST -> IDLE_ST : Marking target "..MyEnemy.." unreachable")
				ChaseGiveUpCount=0
				MyEnemy = 0
				EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
				EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
				if (FastChangeCount < FastChangeLimit and FastChange_C2I == 1) then
					FastChangeCount = FastChangeCount+1

					return OnIDLE_ST()
				else
					return
				end
			end
		end
		ChaseGiveUpCount=ChaseGiveUpCount+1
	elseif MyEnemies[3]==MyEnemy and GetDistanceAPR(MyEnemy,MyPosX[3],MyPosY[3]) <= GetDistanceAR(MyID,MyEnemy) then
		ChaseGiveUpCount=ChaseGiveUpCount+1
		TraceAI("CHASE_ST: We're not getting any closer - we were "..GetDistanceAPR(MyEnemy,MyPosX[3],MyPosY[3]).." cells away 2 cycles ago, now "..GetDistanceAR(MyID,MyEnemy).." Increment ChaseGiveUpCount")
	end
	OnChaseStart()
	if OpportunisticTargeting ==1 and MySkill==0 and SuperPassive~=1 and IsRescueTarget(MyEnemy)==0 then
		if (HPPercent(MyID) > AggroHP and (SPPercent(MyID) > AggroSP or AggroSP==0) and (ShouldStandby == 0 or StickyStandby ==0)) then
			aggro=1
		else
			aggro=0
		end
		object=SelectEnemy(GetEnemyList(MyID,aggro),MyEnemy)
		if object ~= 0 then
			TraceAI("Opportunistic target change - dropping target "..MyEnemy.." for target "..object)
			MyEnemy=object	
			EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
			EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
		end
	end
	if (true == IsInAttackSight(MyID,MyEnemy,MySkill,MySkillLevel)) then  -- ENEMY_INATTACKSIGHT_IN
		MyState = ATTACK_ST
		AttackTimeout=GetTick()+AttackTimeLimit
		ExChaseGiveUpCount=ChaseGiveUpCount
		ChaseGiveUpCount=0
		TraceAI ("CHASE_ST -> ATTACK_ST : ENEMY_INATTACKSIGHT_IN")
		if (FastChangeCount < FastChangeLimit and FastChange_C2A == 1) then
			FastChangeCount = FastChangeCount+1
			return OnATTACK_ST()
		else
			return
		end
	elseif UseSkillOnly == -1 and (GetTick() >= AutoSkillTimeout) then
		-- Only run autoskill routine if NOT in combo mode AND no blueprint combo is active
		-- Prevents casting skills outside the configured blueprint combo sequence
		if (ComboEnabled == 0 or ComboRunDuringChase == 0) and blueprintActive ~= true and BlueprintComboEnabled ~= 1 then
			dist=GetDistanceA(MyID,MyEnemy)
		local tact_skill,tact_debuff,tact_sp,tact_skillclass=GetTact(TACT_SKILL,MyEnemy),GetTact(TACT_DEBUFF,MyEnemy),GetTact(TACT_SP,MyEnemy),GetTact(TACT_SKILLCLASS,MyEnemy)
		skilltouse={-1,0,0}
		local SkillList=GetTargetedSkills(MyID)
		local availsp = GetV(V_SP,MyID)
		if BerserkMode~=1 or Berserk_IgnoreMinSP ~=1 then
			availsp = availsp - tact_sp
		end
		TraceAI("Begin autoskill while chasing routine")
		if (tact_skill < 0) then		-- Negative value of TACT_SKILL -> 1 cast of skill
			skill_level=tact_skill*-1	-- with level = to the absolute value of the
			tact_skill=1			-- value of TACT_SKILL.
		else
			skill_level=11
		end
		for i,v in ipairs(SkillList) do

			skilltype=v[1]
			if v[2]~=0 then
				if IsInAttackSight(MyID,MyEnemy,v[2],v[3])==true then
					if (skilltype == MOB_ATK and UseHomunSSkillChase==1 and AutoMobMode~=0  and (MySkillUsedCount < tact_skill or tact_skill==SKILL_ALWAYS or (BerserkMode==1 and Berserk_SkillAlways==1))) then
						local mobskill_level=skill_level
						if AoEFixedLevel == 1 then
							mobskill_level=v[3]
						end
						local mobmode=0
						if AutoMobMode==2 then
							mobmode=1
						end
						mobskillcount=GetMobCount(v[2],math.min(v[3],mobskill_level),MyEnemy,mobmode)
						TraceAI("mobskillcount="..mobskillcount.."tact_skillclass="..tact_skillclass.."class_mob="..CLASS_MOB.."AutoMobCount="..AutoMobCount)
						if (mobskillcount >= AutoMobCount or tact_skillclass == CLASS_MOB) then
							if (availsp >= GetSkillInfo(v[2],3,math.min(v[3],mobskill_level)))then
								if (skilltouse[1] < 2) then
									skilltouse=v
								end
							end
						end
					elseif (skilltype==MAIN_ATK and (MySkillUsedCount < tact_skill or tact_skill==SKILL_ALWAYS or (BerserkMode==1 and Berserk_SkillAlways==1)) and (tact_skillclass < 1 or tact_skillclass==CLASS_MIN_OLD )) then
						if (availsp-ReserveSP >= GetSkillInfo(v[2],3,math.min(v[3],skill_level))) then
							skilltouse=v
						end
					elseif (skilltype==S_ATK and UseHomunSSkillChase==1 and (MySkillUsedCount < tact_skill or tact_skill==SKILL_ALWAYS or (BerserkMode==1 and Berserk_SkillAlways==1)) and (tact_skillclass==CLASS_S or tact_skillclass==CLASS_BOTH or tact_skillclass==CLASS_MIN_S or ((tact_skillclass==CLASS_COMBO_1 or tact_skillclass==CLASS_COMBO_2)))) then
						if (availsp-ReserveSP >= GetSkillInfo(v[2],3,math.min(v[3],skill_level))) then
							skilltouse=v
						end
					end
				end
			end
		end
		if skilltouse[2]~=0 then
			TraceAI("Using skill while chasing:"..skilltouse[2])
			local slvl=skilltouse[3]
			if skill_level~=11 and ( AoEFixedLevel ~= 1 or skilltouse[1]~=MOB_ATK) then
				slvl=skill_level
			end
			DoSkill(skilltouse[2],slvl,MyEnemy)
			if skilltouse[1] == DEBUFF_ATK then
				ChaseDebuffUsed=1
			end
			MySkillUsedCount=MySkillUsedCount+1
		end
	else
		TraceAI("[COMBO GATE] Suppressing chase autoskill (combo/blueprint active)")
	end
	end  -- End of ComboEnabled/autoskill check
	if (GetTact(TACT_CHASE,MyEnemy)~=1) then
		local alt = 0
		if (ChaseGiveUpCount >= 4 and MyPosX[1] == MyPosX[3]  and MyPosY[1]==MyPosY[3]) then
			alt=math.random(2)
			TraceAI("Using alt movement"..alt)
		end
		ex,ey=GetV(V_POSITION,MyEnemy)
		-- If we're not making progress, attempt a small detour step
		-- (Detour behavior handled in GUI template; no runtime change)
		TraceAI("State History: "..MyStates[1].." "..MyStates[2].." "..MyStates[3].." "..MyStates[4].." "..MyStates[5])
		TraceAI("Pos History: "..formatpos(MyPosX[1],MyPosY[1]).." "..formatpos(MyPosX[2],MyPosY[2]).." "..formatpos(MyPosX[3],MyPosY[3]).." "..formatpos(MyPosX[4],MyPosY[4]).." "..formatpos(MyPosX[5],MyPosY[5]))
		TraceAI("Enemy History: "..MyEnemies[1].." "..MyEnemies[2].." "..MyEnemies[3].." "..MyEnemies[4].." "..MyEnemies[5])
		TraceAI("Enemy Pos History: "..formatpos(EnemyPosX[1],EnemyPosY[1]).." "..formatpos(EnemyPosX[2],EnemyPosY[2]).." "..formatpos(EnemyPosX[3],EnemyPosY[3]).." "..formatpos(EnemyPosX[4],EnemyPosY[4]).." "..formatpos(EnemyPosX[5],EnemyPosY[5]))
		TraceAI("current enemy: "..MyEnemy.." "..formatpos(ex,ey))
		if MyStates[1]==CHASE_ST and MyStates[2]==ATTACK_ST and MyStates[3]==CHASE_ST and MyEnemy==MyEnemies[2] and IsPlayer(MyEnemy)~=1 and EnemyPosX[3]==ex and EnemyPosY[3]==ey then
			x,y=AdjustOpp(x,y,ex,ey)
		    Unreachable[MyEnemy]=1
		    TraceAI("CHASE_ST: We transitioned to attack state vs this target 2 cycles ago, now we're chasing it again, and it hasn't moved! Trying to do AdjustOpp, and deprioritizing monster to prevent loop.")
		elseif EnemyPosX[3]~=0 and EnemyPosY[3]~=0 and (ex~=EnemyPosX[3] or ey~=EnemyPosY[3]) and MyEnemy==MyEnemies[3] and alt==0 then
			dx,dy = ex-EnemyPosX[3],ey-EnemyPosY[3]
			r=AttackRange(MyID,MySkill,MySkillLevel)
			x,y = Closest(MyID,ex+dx,ey+dy,r,alt)
			x,y = AdjustStandPoint(x,y,ex,ey,r,alt)
		else
			x,y = GetStandPoint(MyID,MyEnemy,MySkill,MySkillLevel,alt)
			if x==-1 or y==-1 then
				if AttackRange(MyID,MySkill,MySkillLevel) < 2 or alt > 0 then 
					MyState = IDLE_ST
					Unreachable[MyEnemy]=1
					MyEnemy = 0
					EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
					EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
					MyDestX, MyDestY = 0,0
					TraceAI ("CHASE_ST -> IDLE_ST : Cannot attack this target, GetStandPoint() reports that all cells around it are occupied.")
					ChaseGiveUpCount=0
					if (FastChangeCount < FastChangeLimit and FastChange_C2I == 1) then
						FastChangeCount = FastChangeCount+1
						return OnIDLE_ST()
					end
				else
					x,y = GetStandPoint(MyID,MyEnemy,MySkill,MySkillLevel,1)
					if x==-1 or y==-1 then
						MyState = IDLE_ST
						Unreachable[MyEnemy]=1
						MyEnemy = 0
						EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
						EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
						MyDestX, MyDestY = 0,0
						TraceAI ("CHASE_ST -> IDLE_ST : Cannot attack this target, GetStandPoint() can't get an unoccupied cell")
						ChaseGiveUpCount=0
						if (FastChangeCount < FastChangeLimit and FastChange_C2I == 1) then
							FastChangeCount = FastChangeCount+1
							return OnIDLE_ST()
						end
					end
				end
			end
		end
		ox,oy=GetV(V_POSITION,GetV(V_OWNER,MyID))
		if GetDistanceAPR(GetV(V_OWNER,MyID),x,y) < GetMoveBounds() then
			-- Movement debug: current distance and intended destination
			local distNow = GetDistanceA(MyID, MyEnemy)
			local currentAtkRange = AttackRange(MyID, MySkill, MySkillLevel)
			TraceAI("[CHASE_MOVE] dist="..distNow.." atkRange="..tostring(currentAtkRange).." dest="..x..","..y.." enemy="..ex..","..ey)
			if ((x~=MyDestX or y~=MyDestY) or GetV(V_MOTION,MyID)~=MOTION_MOVE)  then
				MyDestX, MyDestY=x,y
				Move (MyID,MyDestX,MyDestY)
				TraceAI ("CHASE_ST -> CHASE_ST : DESTCHANGED_IN "..MyDestX..","..MyDestY.."mypos "..MyPosX[1]..","..MyPosY[1].."owner pos"..ox..","..oy.." enemypos "..ex..","..ey.." GetDistanceAPR="..GetDistanceAPR(GetV(V_OWNER,MyID),x,y))
			else
				TraceAI("CHASE_ST -> CHASE_ST : Destination not changed "..MyDestX..","..MyDestY.."mypos "..MyPosX[1]..","..MyPosY[1].."owner pos"..ox..","..oy.." enemypos "..ex..","..ey.." GetDistanceAPR="..GetDistanceAPR(GetV(V_OWNER,MyID),x,y))
			end
		else --if ChaseGiveUpCount > 4 then
			MyState = IDLE_ST
			Unreachable[MyEnemy]=1
			MyEnemy = 0
			EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
			EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
			MyDestX, MyDestY = 0,0
			TraceAI ("CHASE_ST -> IDLE_ST : Following enemy would exceed move bounds."..x..","..y.."mypos "..MyPosX[1]..","..MyPosY[1].."owner pos"..ox..","..oy.." enemypos "..ex..","..ey.." GetDistanceAPR="..GetDistanceAPR(GetV(V_OWNER,MyID),x,y))
			ChaseGiveUpCount=0
			if (FastChangeCount < FastChangeLimit and FastChange_C2I == 1) then
				FastChangeCount = FastChangeCount+1
				
				return OnIDLE_ST()
			end
		end
	end
	return
end




function OnATTACK_ST ()
	TraceAI ("OnATTACK_ST MyEnemy: "..MyEnemy.." MyPos "..formatpos(GetV(V_POSITION,MyID)).." ("..GetV(V_MOTION,MyID)..") enemypos "..formatpos(GetV(V_POSITION,MyEnemy)).." ("..GetV(V_MOTION,MyEnemy)..") MyTarget: "..GetV(V_TARGET,MyID)..", GetV(V_TARGET: "..GetV(V_HOMUNTYPE, MyEnemy))	

	if (true == IsOutOfSight(MyID,MyEnemy)) then -- first thing's first, if enemy is gone drop it. 
		MyState = IDLE_ST
		MyEnemy = 0
		EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
		EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
		MySkillUseCount= 0
		TraceAI ("ATTACK_ST -> IDLE_ST -- target gone")
		return OnIDLE_ST()
	end
	if (MOTION_DEAD == GetV(V_MOTION,MyEnemy)) then   -- Enemy dead? Okay we're done here - drop it. 
		MyState = IDLE_ST
		MyEnemy = 0
		EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
		EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
		MySkillUseCount= 0
		TraceAI ("ATTACK_ST -> IDLE_ST  Enemy dead")
		return OnIDLE_ST()
	end
	local mytarg=GetV(V_TARGET,MyID)
	if mytarg~=MyEnemy and MyStates[1]==ATTACK_ST then
		AttackGiveUpCount=AttackGiveUpCount+1
		if AttackGiveUpCount > 4 then --MyEnemies[3]==MyEnemy and MyStates[3]==ATTACK_ST and MyStates[2]==ATTACK_ST then
			local tx,ty=GetV(V_POSITION,MyEnemy)
			local x,y=GetV(V_POSITION,MyID)
			if AttackGiveUpCount < 7 then
				Move(MyID,tx,ty)
				TraceAI("ATTACK_ST: We've been attacking for 5 cycles, but we still haven't attacked! Something is wrong - Moving to monster cell")
			elseif AttackGiveUpCount < AttackGiveUp then 
				nx,ny=AdjustOpp(x,y,tx,ty)
				Move(MyID,tx,ty)
				TraceAI("ATTACK_ST: We've been attacking for 3 cycles, but we still haven't attacked! Something is wrong - Moving to adjust opposite")
			elseif AttackGiveUpCount > AttackGiveUp and MyEnemies[AttackGiveUp]==MyEnemy and MyStates[AttackGiveUp]==ATTACK_ST then
				MyState = IDLE_ST
				Unreachable[MyEnemy]=1
				MyEnemy = 0
				EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
				EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
				MySkillUseCount= 0
				TraceAI("ATTACK_ST -> IDLE_ST - We've been attacking for 5 cycles, tried moving around, and still haven't attacked it. Marking unreachable")
				return OnIDLE_ST()
			end
		end
	else --We have attacked it successfully
		if GetV(V_MOTION,MyEnemy)==MOTION_DAMAGE then
			local t=1
			for i,v in ipairs(GetActors()) do
				if v~=MyID then
					if GetV(V_TARGET,v)==MyEnemy then
						t=0
						break
					end
				end
			end
			if t==1 then
				AttackTimeout=GetTick()+AttackTimeLimit
				TraceAI("AttackTimeout Reset - we're clearly attacking successfully")
			end
		end
	end
	if (AttackTimeout < GetTick() and AttackTimeLimit > 0) then -- Attack time limit reached.
		MyState = FOLLOW_ST
		Unreachable[MyEnemy]=1
		MyEnemy = 0
		EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
		EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
		MySkillUseCount= 0
		TraceAI ("ATTACK_ST -> FOLLOW_ST -- attack timeout reached, so we're probably posbugged. Dropping target and returning to owner in the hope that that sorts it out")
		return OnFOLLOW_ST()
	end
	
	local aggro = GetAggroCount()
	if(aggro > UseBerserkMobbed and UseBerserkMobbed > 0)then
		BerserkMode=1
	end	
	DoAutoBuffs(2)
	local skill,level
	if UseSkillOnly==1 then
		skill,level=GetAtkSkill(MyID)
	elseif MySkill~=0 then
		skill,level=MySkill,MySkillLevel
	else 
		skill,level=nil,nil
	end
	if (false == IsInAttackSight(MyID,MyEnemy,skill,level)) then  -- Check if we can attack enemy, if not back to chase 
		ResetCounters()
		MyState=CHASE_ST
		TraceAI ("ATTACK_ST -> CHASE_ST  : ENEMY_OUTATTACKSIGHT_IN MyEnemy: "..MyEnemy.." distance to "..GetDistanceA(MyID,MyEnemy))
		if (FastChangeCount < FastChangeLimit and FastChange_A2C == 1) then
			FastChangeCount = FastChangeCount+1
			return OnCHASE_ST()
		end
	end
	if (MyAttackStanceX==0) then
		x,y=GetV(V_POSITION,MyID)
		MyAttackStanceX,MyAttackStanceY=x,y
		logappend("AAI_DANCE","Attack Stance set to "..MyDestX..","..MyDestY.." current pos: "..x..","..y)
	end
	OnAttackStart()

	-- Blueprint combo system (node-based) takes precedence when enabled
	local blueprintActiveNow = RunBlueprintCombos("OnAttack", MyID, MyEnemy)
	if blueprintActiveNow then
		-- When blueprint combo is running, skip legacy autoskill/combo logic to avoid conflicts
		return
	end

	-- Pre-cast kiting: respect configured kite thresholds
	-- If within the configured threshold, perform a kite adjust before casting
	-- IMPORTANT: Do not kite while an active combo is running, or it will stall the sequence
	local comboActive = false
	if ComboEnabled == 1 and ComboState and ComboState.currentSlot and ComboState.currentSlot > 0 and ComboRunDuringAttack == 1 then
		comboActive = true
	end
	if KiteMonsters == 1 and comboActive ~= true then
		if DoKiteAdjust(MyID, MyEnemy) then
			TraceAI("ATTACK_ST: Pre-cast kiting engaged by threshold")
			-- Skip casting this tick; repositioning will complete first
			return
		end
	end
	local tact_skill,tact_debuff,tact_sp,tact_skillclass=GetTact(TACT_SKILL,MyEnemy),GetTact(TACT_DEBUFF,MyEnemy),GetTact(TACT_SP,MyEnemy),GetTact(TACT_SKILLCLASS,MyEnemy)
	local skill_level
	
	--logappend("AAI_ATK","tact_skill set "..tact_skill)
	--if AutoMobMode==1 then 
	--	mskill,mlevel=GetMobSkill(MyID)
	--	mobcount=GetMobCount(mskill,mlevel,MyEnemy,1)
	--	--TraceAI("mskill/level "..FormatSkill(mskill,mlevel).." mobcount "..mobcount)
	--elseif AutoMobMode==2 then
	--	mskill,mlevel=GetMobSkill(MyID)
	--	mobcount=GetMobCount(mskill,mlevel,MyEnemy,0)
	--else
	--	mobcount=0
	--end
	--Sniping routine (respect UseAttackSkill) — suppressed when combo is active or combo system is enabled
	local comboActive = (ComboEnabled == 1 and ComboRunDuringAttack == 1 and ComboState and ComboState.currentSlot and ComboState.currentSlot > 0)
	if ComboEnabled == 1 then
		TraceAI("[COMBO GATE] Skipping snipe because combo system is enabled")
	elseif comboActive then
		TraceAI("[COMBO GATE] Skipping snipe due to active combo")
	elseif (UseAttackSkill == 1) and (IsHomun(MyID)==1 and SuperPassive~=1 and BerserkMode==0 and (GetTick() >= AutoSkillTimeout) and aggro <= AutoMobCount and GetTact(TACT_SNIPE,MyEnemy)==SNIPE_OK and (ShouldStandby == 0 or StickyStandby ==0)) then
		target=SelectEnemy(GetEnemyList(MyID,2)) -- This actually checks range - I know it's ugly to do it there, skill range checks need to be done at that point so we can pick a low priority target thats in range, instead of a high priority one out of range. 
		if target ~=0 then
			snipeskill=0
			local snipe_tact_skillclass=GetTact(TACT_SKILLCLASS,target)
			if snipeskill==0 and (snipe_tact_skillclass == CLASS_S or snipe_tact_skillclass == CLASS_BOTH or snipe_tact_skillclass == CLASS_MIN_S) then
				snipeskill,snipelevel=GetSAtkSkill(MyID)
			end
			if snipeskill==0 and (snipe_tact_skillclass == CLASS_OLD or snipe_tact_skillclass == CLASS_BOTH or snipe_tact_skillclass == CLASS_MIN_OLD) then
				snipeskill,snipelevel=GetAtkSkill(MyID)
			end
			--TraceAI("snipe "..skill.." level"..level)
			if snipeskill ~=0 then
				slevel = GetTact(TACT_SKILL,target)
				if slevel < 0 then
					slevel=-1*slevel
					if slevel > snipelevel then
						slevel = snipelevel
					end
					if ((GetV(V_SP,MyID)-ReserveSP >= GetTact(TACT_SP,target)+GetSkillInfo(snipeskill,3,slevel))) then
						TraceAI("Snipe attack on "..target.." "..snipeskill.." "..slevel)
						DoSkill(snipeskill,slevel,target)
					end
				end
			end
		end
	end
	
	
	-- Begin skill selection routine
	skilltouse = {-1,0,0}
	-- First digit (1): -1 = no skill, 0 single target, 1 debuff, 2 mob
	-- Second digit (2): skill id
	-- Third digit (3): skill level
	-- HARD COMBO GATING: ensure combo initializes early and preempts auto-skill
	if ComboEnabled == 1 and ComboRunDuringAttack == 1 then
		-- Initialize combo immediately on first tick in ATTACK_ST so autoskill doesn't preempt
		if ComboState and ComboState.currentSlot == 0 then
			TraceAI("[ATTACK_ST] Entering attack state - initializing combo system")
			InitCombo()
		end
		if ComboState and ComboState.currentSlot > 0 then
			TraceAI("[ATTACK_ST] Combo system active (Slot "..ComboState.currentSlot..") - bypassing autoskill selector")
			ExecuteCombo(MyID, MyEnemy)
			return
		end
	end

	if (1==1) then --non paniced attack
		if (MySkill==0 and UseAttackSkill == 1 and GetTick() >= AutoSkillTimeout) then	
			if (tact_skill < 0) then		-- Negative value of TACT_SKILL -> 1 cast of skill
				skill_level=tact_skill*-1	-- with level = to the absolute value of the
				tact_skill=1			-- value of TACT_SKILL.
			else
				skill_level=11
			end
			local SkillList=GetTargetedSkills(MyID)
			TraceAI("Begin autoskill routine")
			local availsp = GetV(V_SP,MyID)
			if BerserkMode~=1 or Berserk_IgnoreMinSP ~=1 then
				availsp = availsp - tact_sp
			end
			for i,v in ipairs(SkillList) do
				skilltype=v[1]
				TraceAI("skilltype ".. skilltype.." MySkillUsedCount "..MySkillUsedCount.." tact_skill ".. tact_skill.." tact_skillclass"..tact_skillclass.."v"..v[1].." "..v[2].." "..v[3])		
				if v[2]~=0 then
					if IsInAttackSight(MyID,MyEnemy,v[2],v[3])==true then
						if (skilltype == MOB_ATK and UseHomunSSkillAttack==1 and AutoMobMode~=0 and (MySkillUsedCount < tact_skill or tact_skill==SKILL_ALWAYS or (BerserkMode==1 and Berserk_SkillAlways==1))) then
							local mobskill_level=skill_level
							if AoEFixedLevel == 1 then
								mobskill_level=v[3]
							end
							local mobmode=0
							if AutoMobMode==2 then
								mobmode=1
							end
							mobskillcount=GetMobCount(v[2],math.min(v[3],mobskill_level),MyEnemy,mobmode)
							-- TraceAI("Attack -> mobskillcount="..mobskillcount.."tact_skillclass="..tact_skillclass.."class_mob="..CLASS_MOB.."AutoMobCount="..AutoMobCount.." "..FormatSkill(v[2],math.min(v[3],mobskill_level)))
							if (mobskillcount >= AutoMobCount or tact_skillclass == CLASS_MOB) then
								if (availsp >= GetSkillInfo(v[2],3,math.min(v[3],mobskill_level)))then
									if (skilltouse[1] < 2) then
										skilltouse=v
									end
								end
							end
						elseif (skilltype==MAIN_ATK and (MySkillUsedCount < tact_skill or tact_skill==SKILL_ALWAYS or (BerserkMode==1 and Berserk_SkillAlways==1)) and (tact_skillclass < 1 or tact_skillclass==CLASS_MIN_OLD )) then
							if (availsp-ReserveSP >= GetSkillInfo(v[2],3,math.min(v[3],skill_level))) then
								skilltouse=v
							end
						elseif (skilltype==S_ATK and UseHomunSSkillAttack==1 and (MySkillUsedCount < tact_skill or tact_skill==SKILL_ALWAYS or (BerserkMode==1 and Berserk_SkillAlways==1)) and (tact_skillclass==CLASS_S or tact_skillclass==CLASS_BOTH or tact_skillclass==CLASS_MIN_S or ((tact_skillclass==CLASS_COMBO_1 or tact_skillclass==CLASS_COMBO_2)))) then
							if (availsp-ReserveSP >= GetSkillInfo(v[2],3,math.min(v[3],skill_level))) then
								skilltouse=v
							end
						end
					end
				end
				TraceAI("skill selected "..skilltouse[2])
			end
		end
		-- Now we finalize the selection
		if skilltouse[1]~= -1 then
			MySkill=skilltouse[2]
			if (IsHomun(MyID)==1 and skill_level~=11 and (skilltouse[1]~=MOB_ATK or AoEFixedLevel ~= 1)) then  	--no need to check what skill
				MySkillLevel=skill_level			--Only homuns can use non-max level
			else							--and they dont have any mob/debuffs
				MySkillLevel=skilltouse[3]
			end
			if (skilltouse[1] == DEBUFF_ATK) then
				AttackDebuffUsed=AttackDebuffUsed+1
			else
				MySkillUsedCount=MySkillUsedCount+1
			end
		end	
	end
	
	-- Now we resolve it
	
	--if (MySkill == 0) then
	if (UseSkillOnly ~= 1) then
		Attack (MyID,MyEnemy)
		TraceAI("Normal attack vs: "..MyEnemy)
	end
	-- else
	if (MySkill ~=0) then
		TraceAI("Skill Attack: "..MySkill.." target: "..MyEnemy.." level:"..MySkillLevel)
		SkillTarget=MyEnemy
		if MySkill ~=0 then
			DoSkill(MySkill,MySkillLevel,SkillTarget,-1,SkillTargetX,SkillTargetY)
		end
	end
	if ((UseSkillOnly ~= 1 and UseDanceAttack==1 and GetV(V_SP,MyID) >= DanceMinSP) or (BerserkMode==1 and Berserk_Dance==1) or (panicmode==1 and Panic_UseDanceAttack==1 and HPPercent(MyID) > FleeHP)) and (IsHomun(MyID)==1 and MySkill==0) and GetDistanceRect(MyEnemy,GetV(V_OWNER,MyID)) < 13 then
		nx,ny=GetDanceCell(MyAttackStanceX,MyAttackStanceY,MyEnemy)
		if GetDistanceAPR(GetV(V_OWNER,MyID),nx,ny) >= GetMoveBounds() then
			logappend("AAI_DANCE","Dance attack canceled, too close to move bounds "..GetDistanceAPR(GetV(V_OWNER,MyID),nx,ny).." "..GetMoveBounds())
		else 
			logappend("AAI_DANCE","Dancing between "..MyAttackStanceX..","..MyAttackStanceY.." and "..nx..","..ny)
			Move(MyID,nx,ny)
			Attack(MyID,MyEnemy)
			Move(MyID,MyAttackStanceX,MyAttackStanceY)
		end
	end
	MySkill = 0
	MySkillLevel=0
end


-------------------
-- TANK ROUTINES --
-------------------

function	OnTANKCHASE_ST ()
	if (UseSkillOnly==1) then
		skill,level=GetAtkSkill(MyID)
	elseif (UseSkillOnly==-1) then
		skill,level=GetAtkSkill(MyID)
		if (GetV(V_SP,MyID)-GetTact(TACT_SP,MyEnemy) < GetSkillInfo(skill,3,level)) then
			skill,level,sp=nil,nil,nil
		end
	else
		skill,level,sp=nil,nil,nil
	end
	TraceAI ("OnTANKCHASE_ST")
	if (true == IsOutOfSight(MyID,MyEnemy) or (ChaseGiveUpCount > ChaseGiveUp and GetV(V_MOTION,MyID)~=MOTION_MOVE)) then	-- ENEMY_OUTSIGHT_IN
		if (ChaseGiveUpCount>ChaseGiveUp) then
			Unreachable[MyEnemy]=1
		end
		
		MyState = IDLE_ST
		MyEnemy = 0
		EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
		EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
		MyDestX, MyDestY = 0,0
		TraceAI ("TANKCHASE_ST -> IDLE_ST : ENEMY_OUTSIGHT_IN")
		ChaseGiveUpCount=0
		
		return
	end
	if (true == IsInAttackSight(MyID,MyEnemy)) then  -- ENEMY_INATTACKSIGHT_IN
		ChaseGiveUpCount=0
		if(IsNotKS(MyID,MyEnemy)==1) then
			MyState = TANK_ST
			MySkillUsedCount=0
			TraceAI ("TANKCHASE_ST -> TANK_ST : ENEMY_INATTACKSIGHT_IN")
			return OnTANK_ST()
		else
			MyState = IDLE_ST
			MyEnemy = 0
			EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
			EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
			MyDestX, MyDestY = 0,0
			TraceAI ("TANKCHASE_ST -> IDLE_ST : Enemy is taken")
		end
		
		return
	end
	TraceAI("Tank chase: Can we skill while chasing?")
	if UseSkillOnly == -1 and (GetTick() >= AutoSkillTimeout) then
		dist=GetDistanceA(MyID,MyEnemy)
		tact_debuff,tact_skill,tact_sp,tact_skillclass=GetTact(TACT_DEBUFF,MyEnemy),GetTact(TACT_SKILL,MyEnemy),GetTact(TACT_SP,MyEnemy),GetTact(TACT_SKILLCLASS,MyEnemy)
		skilltouse={-1,0,0}
		local SkillList=GetTargetedSkills(MyID)
		TraceAI("Begin autoskill while tank chasing routine")
		if (tact_skill < 0) then		-- Negative value of TACT_SKILL -> 1 cast of skill
			skill_level=tact_skill*-1	-- with level = to the absolute value of the
			tact_skill=1			-- value of TACT_SKILL.
		else
			skill_level=11
		end
		for i,v in ipairs(SkillList) do

			skilltype=v[1]
			if v[2]~=0 then
				--logappend("AAI_Chase","skilltype ".. skilltype.." MySkillUsedCount "..MySkillUsedCount.." tact_skill ".. tact_skill.." tact_skillclass ".. tact_skillclass.."v"..v[1].." "..v[2].." "..v[3])
				if (GetV(V_SP,MyID) >= tact_sp+GetSkillInfo(v[2],3,math.min(v[3],skill_level)) and GetSkillInfo(v[2],2,math.min(v[3],skill_level)) >= dist) then 
				--TraceAI("skilltype ".. skilltype.." MySkillUsedCount "..MySkillUsedCount.." tact_skill ".. tact_skill.."v"..v[1].." "..v[2].." "..v[3])
					if (skilltype == MOB_ATK and (MySkillUsedCount < tact_skill or tact_skill==SKILL_ALWAYS or (BerserkMode==1 and Berserk_SkillAlways==1))) then
						if (tact_skillclass == CLASS_MOB) then
							if (skilltouse[1] < 2) then
								skilltouse=v
							end
						end
					elseif (skilltype ==DEBUFF_ATK and ChaseDebuffUsed==0) then
						if (tact_debuff*-1 == v[2] or (tact_debuff==1 and BasicDebuffs[v[2]]~=nil)) then
							skilltouse=v
						end
					elseif (skilltype==MAIN_ATK and (MySkillUsedCount < tact_skill or tact_skill==SKILL_ALWAYS or (BerserkMode==1 and Berserk_SkillAlways==1)) and tact_skillclass~=CLASS_S) then
						skilltouse=v
					elseif (skilltype==S_ATK and UseHomunSSkillChase==1 and (MySkillUsedCount < tact_skill or tact_skill==SKILL_ALWAYS or (BerserkMode==1 and Berserk_SkillAlways==1)) and tact_skillclass~=CLASS_OLD) then
						skilltouse=v
					end
				end
			end
		end
		if skilltouse[2]~=0 then
			TraceAI("Using skill while tank chasing:"..skilltouse[2])
			local slvl=skilltouse[3]
			if skill_level~=11 then
				slvl=skill_level
			end
			DoSkill(skilltouse[2],slvl,MyEnemy,-1)
			if skilltouse[1] == DEBUFF_ATK then
				ChaseDebuffUsed=1
			end
			MySkillUsedCount=MySkillUsedCount+1
		end
	else
		TraceAI("Not in range, and can't use chase skill")
	end
	ChaseGiveUpCount=ChaseGiveUpCount+1
	MyDestX, MyDestY =  GetStandPoint(MyID,MyEnemy,MySkill,MySkillLevel,alt)
	if MyDestX==-1 or MyDestY==1 then
		Unreachable[MyEnemy]=1
		MyState = IDLE_ST
		MyEnemy = 0
		EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
		EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
		MyDestX, MyDestY = 0,0
		TraceAI ("TANKCHASE_ST -> IDLE_ST : target is surrounded so GetStandPoint can't find valid cell. Target dropped and deprioritized")
		ChaseGiveUpCount=0
	end
	if (GetTact(TACT_CHASE,MyEnemy)~=1) then
		Move (MyID,MyDestX,MyDestY)
		TraceAI ("TANKCHASE_ST -> TANKCHASE_ST : DESTCHANGED_IN"..MyDestX.." "..MyDestY)
	end
	return
end 

function OnTANK_ST()
	if (GetV(V_MOTION,MyEnemy)==MOTION_DEAD or IsOutOfSight(MyID,MyEnemy)) then
		MyState=IDLE_ST
		TraceAI("TANK_ST->IDLE_ST - Target dead or out of sight")
		return
	end
	if (GetV(V_TARGET,MyEnemy)~=MyID and (TankHitTimeout + 2500) < GetTick()) then
		if (IsInAttackSight(MyID,MyEnemy)==true) then
			Attack(MyID,MyEnemy)
			TankHitTimeout = GetTick()
		else
			MyState=TANKCHASE_ST
			TraceAI("TANK_ST->TANKCHASE_ST - Target out of range")
		end
		return
	end
	if GetV(V_TARGET,MyEnemy)==MyID then
		TraceAI("TANK_ST->IDLE_ST - Target is tanked successfully")
		MyState=IDLE_ST
	end
end

--------------------
--- REST ROUTINE ---
--------------------
function	OnREST_ST ()
	TraceAI("OnREST_ST")
	if (DoIdleTasks()==nil) then
		return
	end
	if SuperPassive~=1 then
		local	object = SelectEnemy(GetFriendTargets())
		if (object ~= 0) then		--Check for monsters attacking owner
			MyState = CHASE_ST
			MyEnemy = object
			TraceAI ("REST_ST -> CHASE_ST : MYOWNER_ATTACKED_IN")
			return 
		end
		object = SelectEnemy(GetEnemyList(MyID,0))	
		if (object ~= 0) then
			MyState = CHASE_ST
			MyEnemy = object
			TraceAI ("REST_ST -> CHASE_ST : ATTACKED_IN")
			return
		end
	end
	
	--If theres nothing else to do, return to the "rest station"
	
	x,y=GetV(V_POSITION,MyID)
	ox,oy=GetV(V_POSITION,GetV(V_OWNER,MyID))
	xoff=x-ox
	yoff=y-oy
	TraceAI(xoff.." "..ox.." "..x)
	if (GetV(V_MOTION,GetV(V_OWNER,MyID))~=MOTION_SIT) then
		MyState=IDLE_ST
		TraceAI("REST_ST -> IDLE_ST: Owner stood up")
	elseif (xoff~=RestXOff or yoff~=RestYOff) then
		MyDestX=ox+RestXOff
		MyDestY=oy+RestYOff
		TraceAI("REST_ST -> REST_ST: moving from "..formatpos(x,y).." to "..formatpos(MyDestX,MyDestY))
		Move(MyID,MyDestX,MyDestY)
	else
		TraceAI("REST_ST -> REST_ST: At rest station")
	end
end

-----------------------------
---Friend motion routines ---
-----------------------------
function	OnFRIEND_CIRCLE_ST()
	x,y = GetV(V_POSITION,MyID)
	dist = GetDistance(x,y,NewFriendX,NewFriendY)
	TraceAI("OnFRIEND_CIRCLE_ST"..x..","..y.." "..dist) 
	if FriendCircleIter == 0 then
		if dist > 2 then
			Move(MyID,NewFriendX,NewFriendY)
			FriendCircleTimeout=FriendCircleTimeout+1
			if FriendCircleTimeout > 16 then
				MyState=IDLE_ST
				TraceAI("Dropping circle attempt, timeout exceeded")
			end
			return
		else
			FriendCircleIter=1
		end
	end
	if FriendCircleIter ==1 then
		if x==NewFriendX and y==NewFriendY-2 then
			FriendCircleIter=2
			FriendCircleTimeout=0
		else 
		 	Move(MyID,NewFriendX,NewFriendY-2)
		 	FriendCircleTimeout=FriendCircleTimeout+1
		 	if FriendCircleTimeout>4 then
		 		FriendCircleIter=2
				FriendCircleTimeout=0
		 		TraceAI("giving up on circle step 1")	
		 	end
		 	return
		end
	end
	if FriendCircleIter ==2 then
		if x==NewFriendX-2 and y==NewFriendY then
			FriendCircleIter=3
			FriendCircleTimeout=0
		else 
		 	Move(MyID,NewFriendX-2,NewFriendY)
		 	FriendCircleTimeout=FriendCircleTimeout+1
		 	if FriendCircleTimeout>4 then
		 		FriendCircleIter=3
				FriendCircleTimeout=0
		 		TraceAI("giving up on circle step 2")	
		 	end
		 	return
		end
	end
	if FriendCircleIter ==3 then
		if x==NewFriendX and y==NewFriendY+2 then
			FriendCircleIter=4
			FriendCircleTimeout=0
		else 
			Move(MyID,NewFriendX,NewFriendY+2)
			FriendCircleTimeout=FriendCircleTimeout+1
			if FriendCircleTimeout>4 then
				FriendCircleIter=4
				FriendCircleTimeout=0
				TraceAI("giving up on circle step 3")	
			end
			return
		end
	end
	if FriendCircleIter ==4 then
		if x==NewFriendX+2 and y==NewFriendY then
			FriendCircleIter=5
			FriendCircleTimeout=0
		else 
			Move(MyID,NewFriendX+2,NewFriendY)
			FriendCircleTimeout=FriendCircleTimeout+1
			if FriendCircleTimeout>4 then
				FriendCircleIter=5
				FriendCircleTimeout=0
				TraceAI("giving up on circle step 4")	
			end
			return
		end
	end
	if FriendCircleIter == 5 then
		if x==NewFriendX and y==NewFriendY-2 then
			FriendCircleIter=0
			FriendCircleTimeout=0
			MyState=IDLE_ST
			TraceAI("Circle completed")
			return
		else 
			Move(MyID,NewFriendX,NewFriendY-2)
			FriendCircleTimeout=FriendCircleTimeout+1
			if FriendCircleTimeout>4 then
				FriendCircleIter=0
				FriendCircleTimeout=0
				MyState=IDLE_ST
				TraceAI("giving up on circle step 5")	
			end
			return
		end
	end
end

function	OnFRIEND_CROSS_ST()
	x,y = GetV(V_POSITION,MyID)
	dist = GetDistance(x,y,NewFriendX,NewFriendY)
	TraceAI("OnFRIEND_CROSS_ST"..x..","..y.." "..dist) 
	if FriendCircleIter == 0 then
		if dist > 2 then
			Move(MyID,NewFriendX,NewFriendY)
			FriendCircleTimeout=FriendCircleTimeout+1
			if FriendCircleTimeout > 16 then
				MyState=IDLE_ST
				TraceAI("Dropping cross attempt, timeout exceeded")
			end
			return
		else
			FriendCircleIter=1
		end
	end
	if FriendCircleIter ==1 then
		if x==NewFriendX-2 and y==NewFriendY then
			FriendCircleIter=2
			FriendCircleTimeout=0
		else 
		 	Move(MyID,NewFriendX-2,NewFriendY)
		 	FriendCircleTimeout=FriendCircleTimeout+1
		 	if FriendCircleTimeout>4 then
		 		FriendCircleIter=2
				FriendCircleTimeout=0
		 		TraceAI("giving up on cross step 1")	
		 	end
		 	return
		end
	end
	if FriendCircleIter ==2 then
		if x==NewFriendX+2 and y==NewFriendY then
			FriendCircleIter=3
			FriendCircleTimeout=0
		else 
		 	Move(MyID,NewFriendX+2,NewFriendY)
		 	FriendCircleTimeout=FriendCircleTimeout+1
		 	if FriendCircleTimeout>4 then
		 		FriendCircleIter=3
				FriendCircleTimeout=0
		 		TraceAI("giving up on cross step 2")	
		 	end
		 	return
		end
	end
	if FriendCircleIter ==3 then
		if x==NewFriendX-2 and y==NewFriendY then
			FriendCircleIter=4
			FriendCircleTimeout=0
		else 
			Move(MyID,NewFriendX-2,NewFriendY)
			FriendCircleTimeout=FriendCircleTimeout+1
			if FriendCircleTimeout>4 then
				FriendCircleIter=4
				FriendCircleTimeout=0
				TraceAI("giving up on cross step 3")	
			end
			return
		end
	end
	if FriendCircleIter ==4 then
		if x==NewFriendX+2 and y==NewFriendY then
			FriendCircleIter=5
			FriendCircleTimeout=0
		else 
			Move(MyID,NewFriendX+2,NewFriendY)
			FriendCircleTimeout=FriendCircleTimeout+1
			if FriendCircleTimeout>4 then
				FriendCircleIter=5
				FriendCircleTimeout=0
				TraceAI("giving up on cross step 4")	
			end
			return
		end
	end
	if FriendCircleIter == 5 then
		if x==NewFriendX-2 and y==NewFriendY then
			FriendCircleIter=0
			FriendCircleTimeout=0
			MyState=IDLE_ST
			TraceAI("Circle completed")
			return
		else 
			Move(MyID,NewFriendX-2,NewFriendY)
			FriendCircleTimeout=FriendCircleTimeout+1
			if FriendCircleTimeout>4 then
				FriendCircleIter=0
				FriendCircleTimeout=0
				MyState=IDLE_ST
				TraceAI("giving up on cross step 5")	
			end
			return
		end
	end
end

-------------------
--Command State Process
-------------------

function	OnMOVE_CMD_ST ()

	TraceAI ("OnMOVE_CMD_ST")
	if GetDistanceAPR(GetV(V_OWNER,MyID),MyMoveX,MyMoveY) > 15 then
		TraceAI("OnMOVE_CMD_ST -> IDLE_ST: Attempt to move to location off screen")
		logappend("AAI_ERROR","We were in MOVE_CMD_ST trying to move to "..formatpos(MyMoveX,MyMoveY).." while owner standing at "..formatpos(GetV(V_POSITION,GetV(V_OWNER,MyID))))
		MyState=IDLE_ST
		return OnIDLE_ST()
	end
	local x, y = GetV (V_POSITION,MyID)
	if (x == MyMoveX and y == MyMoveY) then	
		StickyX,StickyY=0,0
		if (MoveSticky ~= 0) then
			if (ReturnToMoveHold==0) then
				ReturnToMoveHold = 1
				StickyX,StickyY=x,y
			else 
				ReturnToMoveHold = 0
			end
		end
		TraceAI("OnMOVE_CMD_ST -> IDLE_ST: Arrived at Destination "..formatpos(x,y))
		MyState=IDLE_ST
		return OnIDLE_ST()
	elseif GetV(V_MOTION,MyID) == MOTION_STAND or MyDestX~=MyMoveX or MyDestY~=MyMoveY then
		MyDestX,MyDestY=MyMoveX,MyMoveY
		Move(MyID,MyDestX,MyDestY)
	end
end

function	OnMOVE_CMD_HOLD_ST ()
	
	TraceAI ("OnMOVE_CMD_HOLD_ST")
	MySkillUsedCount		= 0
	ChaseGiveUpCount		= 0
	AttackGiveUpCount		= 0
	ChaseDebuffUsed			= 0
	AttackDebuffUsed		= 0
	BypassKSProtect			= 0
	BerserkMode			= 0
	if (DoIdleTasks()==nil) then
		return
	end
	if (MoveStickyFight==1 and SuperPassive~=1 ) then
		local	object = SelectEnemy(GetFriendTargets())
		if (object ~= 0) then							-- MYOWNER_ATTACKED_IN
			MyState = CHASE_ST
			MyEnemy = object
			TraceAI ("MOVE_CMD_HOLD_ST -> CHASE_ST : MYOWNER_ATTACKED_IN")
			if (FastChangeCount < FastChangeLimit and FastChange_I2C ==1) then
				OnCHASE_ST()
			end
			return 
		end
		object = SelectEnemy(GetEnemyList(MyID,0))
		if (object ~= 0) then							-- ATTACKED_IN
			MyState = CHASE_ST
			MyEnemy = object
			TraceAI ("MOVE_CMD_HOLD_ST -> CHASE_ST : ATTACKED_IN")
			if (FastChangeCount < FastChangeLimit and FastChange_I2C ==1) then
				OnCHASE_ST()
			end
			return
		end
		object = SelectEnemy(GetEnemyList(MyID,-1))
		if (object ~= 0) then
			MyState = TANKCHASE_ST
			MyEnemy = object
			TraceAI ("MOVE_CMD_HOLD_ST -> TANKCHASE_ST")
			return
		end
	end
end


function OnSTOP_CMD_ST ()


end




function OnATTACK_OBJECT_CMD_ST ()

	

end


function OnATTACK_AREA_CMD_ST ()

	TraceAI ("OnATTACK_AREA_CMD_ST")

	local	object = GetOwnerEnemy (MyID)
	if (object == 0) then							
		object = GetMyEnemy (MyID) 
	end

	if (object ~= 0) then							-- MYOWNER_ATTACKED_IN or ATTACKED_IN
		MyState = CHASE_ST
		MyEnemy = object
		return
	end

	local x , y = GetV (V_POSITION,MyID)
	if (x == MyDestX and y == MyDestY) then			-- DESTARRIVED_IN
			MyState = IDLE_ST
	end

end




function OnPATROL_CMD_ST ()

	TraceAI ("OnPATROL_CMD_ST")

	local	object = GetOwnerEnemy (MyID)
	if (object == 0) then							
		object = GetMyEnemy (MyID) 
	end

	if (object ~= 0) then							-- MYOWNER_ATTACKED_IN or ATTACKED_IN
		MyState = CHASE_ST
		MyEnemy = object
		TraceAI ("PATROL_CMD_ST -> CHASE_ST : ATTACKED_IN")
		return
	end

	local x , y = GetV (V_POSITION,MyID)
	if (x == MyDestX and y == MyDestY) then			-- DESTARRIVED_IN
		MyDestX = MyPatrolX
		MyDestY = MyPatrolY
		MyPatrolX = x
		MyPatrolY = y
		Move (MyID,MyDestX,MyDestY)
	end

end


-----------------------------------------------------------------------
--IF ANYONE READING MY CODE HAS A FUCK'S CLUE WHAT THE HOLD COMMAND IS
--OR WHAT IN THE DEVIL IT'S PURPOSE IS, PLEASE ENLIGHTEN ME! -Azzy 
----------------------------------------------------------------------

function OnHOLD_CMD_ST () 

	TraceAI ("OnHOLD_CMD_ST")
	logappend("AAI_ERROR","We're in HOLD_CMD_ST - where did hold cmd come from?")
	if (MyEnemy ~= 0) then
		local d = GetDistance(MyEnemy,MyID)
		if (d ~= -1 and d <= GetV(V_ATTACKRANGE,MyID)) then
				Attack (MyID,MyEnemy)
		else
			MyEnemy = 0
			EnemyPosX = {0,0,0,0,0,0,0,0,0,0}
			EnemyPosY = {0,0,0,0,0,0,0,0,0,0}
		end
		return
	end


	local	object = GetOwnerEnemy (MyID)
	if (object == 0) then							
		object = GetMyEnemy (MyID)
		if (object == 0) then						
			return
		end
	end

	MyEnemy = object

end




function OnSKILL_OBJECT_CMD_ST ()
	if IsInAttackSight(MyID,MyEnemy,MySkill,MySkillLevel) then
		DoSkill(MySkill,MySkillLevel,MyEnemy)
		TraceAI("SKILL_OBJECT_CMD_ST --> IDLE_ST - skill used")
		MyState=IDLE_ST
		MyDestX,MyDestY,MyEnemy,MySkill,MySkillLevel=0,0,0,0,0
		return
	elseif IsOutOfSight(MyID,MyEnemy) then
		TraceAI("SKILL_OBJECT_CMD_ST --> IDLE_ST - target off screen")
		MyState=IDLE_ST
		MyDestX,MyDestY,MyEnemy,MySkill,MySkillLevel=0,0,0,0,0
	elseif SkillObjectCMDTimeout>SkillObjectCMDLimit then
		TraceAI("SKILL_OBJECT_CMD_ST --> IDLE_ST - Couldn't get into range to use skill "..MySkill.." on "..MyEnemy)
		MyState=IDLE_ST
		MyDestX,MyDestY,MyEnemy,MySkill,MySkillLevel=0,0,0,0,0
		return OnIDLE_ST()
	else
		x,y= GetStandPoint(MyID,MyEnemy,MySkill,MySkillLevel,alt)
		if x < 10 and y < 10 then
			local ex,ey=GetV(V_POSITION,MyEnemy)
			local hx,hy=GetV(V_POSITION,MyID)
			local ox,oy=GetV(V_POSITION,GetV(V_OWNER,MyID))
			logappend("AAI_ERROR","Anomalous move in progress: h "..formatpos(hx,hy).." "..formatmotion(GetV(V_MOTION,MyID)).." e "..formatpos(ex,ey).." "..formatmotion(GetV(V_MOTION,MyEnemy)).." o "..formatpos(ox,oy).." "..formatmotion(GetV(V_MOTION,GetV(V_OWNER,MyID))).." dest "..formatpos(x,y))
		end
		Move(MyID,x,y)
		SkillObjectCMDTimeout=SkillObjectCMDTimeout+1
		return
	end
	
	
end




function OnSKILL_AREA_CMD_ST ()

	TraceAI ("OnSKILL_AREA_CMD_ST")

	local x , y = GetV (V_POSITION,MyID)
	if (GetDistance(x,y,MyDestX,MyDestY) <= AttackRange(MyID,MySkill,MySkillLevel)) then	-- DESTARRIVED_IN
		DoSkill(MySkill,MySkillLevel,0,nil,MyDestX,MyDestY)
		MyState = IDLE_ST
		MySkill = 0
	else
		targetx,targety=Closest(MyID,MyDestX,MyDestY,AttackRange(MyID,MySkill,MySkillLevel))
		TraceAI("Moving to "..formatpos(targetx,targety).." from "..formatpos(x,y))
		if GetDistanceAPR(GetV(V_OWNER,MyID),targetx,targety) > GetMoveBounds() then
			MyState = IDLE_ST
			MySkill = 0
			TraceAI("OnSKILL_AREA_CMD_ST -> IDLE_ST Target out of range")
		else
			Move (MyID,targetx,targety)
		end
	end

end







function OnFOLLOW_CMD_ST ()
	TraceAI ("OnFOLLOW_CMD_ST")
	local d = GetDistanceA (GetV(V_OWNER,MyID),MyID)
	if ( d > FollowStayBack) then
		BetterMoveToOwner (MyID,FollowStayBack)
		return
	end
	-- Start the friending process
	if (StandbyFriending == 1) then
		local actors = GetActors()
		for i,v in ipairs(actors) do
			if (IsMonster(v)~=1 and GetV(V_MOTION,GetV(V_OWNER,MyID))==MOTION_SIT) then
				TraceAI("Friend list modification")
				if (IsToRight(GetV(V_OWNER,MyID),v)==1) then
					if (MyFriends[v]==nil) then
						MyFriends[v] = 1
						UpdateFriends()
						MyState=FRIEND_CIRCLE_ST
						ReturnToState=FOLLOW_CMD_ST
						NewFriendX,NewFriendY=GetV(V_POSITION,v)
					end
				elseif (IsToRight(v,GetV(V_OWNER,MyID))==1) then
					if (MyFriends[v]~=nil) then
						MyFriends[v] = nil
						UpdateFriends()
						MyState=FRIEND_CROSS_ST
						ReturnToState=FOLLOW_CMD_ST
						NewFriendX,NewFriendY=GetV(V_POSITION,v)
					end
				end
			end
		end
	end
	-- Okay, that's done. 
	if (DefendStandby == 1 and SuperPassive~=1) then
		local	object = SelectEnemy(GetFriendTargets())
		if (object ~= 0) then							-- MYOWNER_ATTACKED_IN
			MyState = CHASE_ST
			MyEnemy = object
			TraceAI ("FOLLOW_CMD_ST -> CHASE_ST : MYOWNER_ATTACKED_IN")
			if (FastChangeCount < FastChangeLimit and FastChange_I2C ==1) then
				OnCHASE_ST()
			end
			return 
		end
		object = SelectEnemy(GetEnemyList(MyID,0))
		if (object ~= 0) then							-- ATTACKED_IN
			MyState = CHASE_ST
			MyEnemy = object
			TraceAI ("FOLLOW_CMD_ST -> CHASE_ST : ATTACKED_IN ")
			if (FastChangeCount < FastChangeLimit and FastChange_I2C ==1) then
				OnCHASE_ST()
			end
			return
		end
	end
end


--#####################################
--### Targeting Routines start here ###
--#####################################
--[[
targets[actorid]=

MotionClass
-1 = Dead
0  = Stand
1  = Moving
2  = Flinch
3  = Attacking
4  = Skilling
5  = Casting
6  = Other

TargetClass
-2  = Non-friend player
-1  = Monster
0  = None
1  = Self
2  = Friend/Owner
]]--]]

function GetFriendTargets() -- returns list of targets of friends who are attacking
	local targets = {}
	for i,v in ipairs (GetActors()) do
		if (IsFriend(v) == 1) then
			motion=GetV(V_MOTION,v)
			target=GetV(V_TARGET,v)
			if (IsMonster(target)==1) then
				tact=GetTact(TACT_BASIC,target)
				if (FriendAttack[motion]==1 and tact > 0 and tact ~=9) then
					targets[target]={MotionClassLU[GetV(V_MOTION,target)],GetTargetClass(target),GetTact(TACT_BASIC,target),GetTact(TACT_CAST,target)}
					
				end
			end
		end
	end
	return targets
end
-- aggro arguments
-- 2 = snipe
-- 1 = aggro
-- 0 = nonaggro
-- -1 = tank
-- -2 = rescue


function	GetEnemyList (myid,aggro)
	local owner  = GetV(V_OWNER,myid)
	local enemys = {}
	if aggro==2 then
		sskill,slevel=GetSAtkSkill(MyID)
		oskill,olevel=GetAtkSkill(MyID)
	end
	local HomunType = GetV(V_HOMUNTYPE,myid)
	TraceAI("GetEnemyList with aggro "..aggro)
	for k,v in pairs(Targets) do
		tact = GetTact(TACT_BASIC,k)
		casttact=GetTact(TACT_CAST,k)
		if tact==TACT_TANKMOB then
			if GetAggroCount() > AutoMobCount then
				tact = TACT_ATTACK_M
			else
				tact = TACT_TANK
			end
		end
		--TraceAI("Target"..k.." tact:"..tact.." Motion"..v[1].." TClass"..v[2])
		if (0 < tact and (tact < 5 or tact >9) and aggro==1 and (DoNotAttackMoving ~=1 or v[1]~=1) and (tact ~= 14 or AttackLastFullSP==0 or SPPercent(MyID)==100)) or  (tact > 9 and tact < 13 and aggro == 2 and k~=MyEnemy) or  (v[2]>0 and tact>0 and (tact~=9 or v[2]==1) and (v[1]==3 or casttact >= CAST_REACT) and aggro~=2 and (aggro > -1 or (aggro==-2 and IsRescueTarget(k)==1))) or (tact == -1 and aggro==-1 and v[2]~=1) then
			--TraceAI("Tactics say to attack:"..k)
			if (IsNotKS(myid,k)==1 and v[1] > -1) then
				--TraceAI("Is alive and not a KS")
				if aggro == 0 or (aggro~=2 and v[2]>0) then 
					if (GetMoveBounds() >= GetDistanceRect(owner,k)) then
						--TraceAI("Adding to target list: "..k)
						r={v[1],v[2],tact,casttact}
						enemys[k] = r
					--else 
					--	tempx,tempy=GetV(V_POSITION,owner)
					--	targx,targy=GetV(V_POSITION,k)
					--	TraceAI("target ignored "..k.." mypos "..tempx..","..tempy.." enemy "..targx..","..targy.." bounds "..GetMoveBounds().."dist "..GetDistanceRect(owner,k))
					end
				elseif aggro~=2 then
					if (GetAggroDist() >= GetDistanceA(owner,k)) then
						TraceAI("Adding to target list: "..k)
						r={v[1],v[2],tact,casttact}
						enemys[k] = r
					end
				else -- Sniping - so we need to check range on skill, instead of aggrodist
					dist = GetDistanceA(MyID,k)
					tact_skill_class=(GetTact(TACT_SKILLCLASS,k))
					if tact_skill_class==CLASS_MINION and mskill~=0 then
						if dist <= GetSkillInfo(mskill,2,mlevel) then
							r={v[1],v[2],tact,casttact}
							enemys[k] = r
						end
					elseif sskill~=0 and tact_skill_class~=CLASS_OLD then
						if dist <= GetSkillInfo(sskill,2,slevel) then
							r={v[1],v[2],tact,casttact}
							enemys[k] = r
						end
					elseif oskill~=0 and tact_skill_class~=CLASS_S then
						if dist <= GetSkillInfo(oskill,2,olevel) then
							r={v[1],v[2],tact,casttact}
							enemys[k] = r
						end
					end
				end
			end
		end
	end
	return enemys
end

-- format of return
-- enemys[n][1] = Motion Class
-- enemys[n][2] = Target Class
-- enemys[n][3] = tact
-- enemys[n][4] = casttact

function SelectEnemy(enemys,curenemy)
	local min_priority=-1
	local priority
	local min_dis = 100
	local dis
	--local min_aggro = -1
	--local aggro = 0
	local result=0
	local max_reachable=1
	--local min_mobcount=1
	--local mobcount=0
	if curenemy~=nil then -- it's an opportunistic attack
		local dist = GetDistanceA(MyID,curenemy)
		local aggrotemp=0
		if IsFriendOrSelf(GetV(V_TARGET,curenemy)) ==1 then
			aggrotemp=1
		end
		min_priority=convpriority(GetTact(TACT_BASIC,curenemy),aggrotemp)
		if dist < 3 then 
			return curenemy
		else
			min_dis = dist - 3
		end
	end
	for k,v in pairs(enemys) do
		local basepriority = v[3] -- basic tact
		if v[2]>0 and (v[1]==3 or v[4]>=CAST_REACT) then
			aggro=1
		else
			aggro=0
		end
		priority=convpriority(basepriority,aggro)
		--TraceAI(k.." "..basepriority.." "..priority)
		--elseif ((priority==2 or priority==5) and (v[1]==3 or v[4]>=CAST_REACT)) then
		--	aggro=-1
		--elseif then	
		--aggro = 1
		--else
		--	aggro=0
		--end
		dis = GetDistanceA (MyID,k)
		unreachable=Unreachable[k]
		if unreachable == nil then
			unreachable=0
		end
		
		--TraceAI(priority.."/"..min_priority.." "..dis.."/"..min_dis.." "..unreachable.."/"..max_reachable)
		if (unreachable <= max_reachable) then
			--if (aggro >= min_aggro) then
				if (priority > min_priority or (priority==min_priority and dis < min_dis)) then
					--if (dis < min_dis) then
						result = k
						min_dis = dis
						min_priority=priority
						--min_aggro=aggro
						max_reachable=unreachable
					--end
				end
			--end
		end
	end
	if max_reachable==1 then
		AllTargetUnreachable=1
	else 
		AllTargetUnreachable=0
	end
	TraceAI("SelectEnemy returning target "..result)
	return result
end

function convpriority(base,agr)
	local priority
	if base > 9 and base < 13 then --Snipe modes are to be treated as attack
		base = base-8
	end
	if base == 13 then
		if agr == 1 then
			base = 7
		else 
			base = 2
		end
	end
	if base == 14 then
		base= 1
	end
	if base>6 and agr==1 then
		priority=base
	elseif base==4 or base==3 or base==15 then
		priority=base+1
		if agr==0 then 
			priority=priority-2
		end
	elseif (priority==5 and aggro==1) then 
		return 2
	elseif base==2 then
		return 1
	else
		return 0
	end
	return priority
end

--####################################################
--### DoIdleTasks - stuff done in any "idle" state ###
--### like buffs and command processing            ###
--####################################################

function DoIdleTasks()
	local cmd = List.popleft(ResCmdList)
	if (cmd ~= nil) then		
		ProcessCommand (cmd)	-- ¿¹¾à ¸í·É¾î Ã³¸® 
		return 
	end
	if OnIdleTasks()==1 then
		return
	end
	if UseAutoHeal==2 then
		if DoHealingTasks(MyID)==1 then
			return
		end
	end
	-- Gate autobuff during idle when combo is enabled (combo-only mode)
	if ComboEnabled ~= 1 then
		if DoAutoBuffs(1) ~=1 then
			return
		end
	end
	if (GetV(V_MOTION,GetV(V_OWNER,MyID))==MOTION_SIT and MyState~=REST_ST and DoNotUseRest~=1) then
		MyState=REST_ST
		TraceAI("DoIdleTasks - Owner sitting, MyState -> REST_ST")
		return
	end
	return 1
end

function DoAutoBuffs(buffmode)
	-- Allow buffs during idle/follow; combo gating happens only in OnATTACK_ST
	if GetTick() < AutoSkillTimeout then
		return 1
	end
	TraceAI("DoAutoBuffs"..buffmode)

	if (UseOffensiveBuff == buffmode and QuickenTimeout ~= -1 and MyState ~= ATTACK_ST) then
		if (GetTick() > QuickenTimeout) then
			local skill, level = GetQuickenSkill(MyID)

			if (skill <= 0) then
				QuickenTimeout = -1
			elseif (level == 0) then
				-- skill in cooldown
			elseif (GetSkillInfo(skill, 3, level) <= GetV(V_SP, MyID)) then
				-- Body Double should respect owner HP threshold before casting
				if skill == S_BODY_DOUBLE then
					local owner = GetV(V_OWNER, MyID)
					local ohp = HPPercent(owner)
					if BodyDoubleOwnerHP and ohp > BodyDoubleOwnerHP then
						-- Owner HP above threshold; skip casting Body Double
						return
					end
				end
				DoSkill(skill, level, MyID, 2)
				QuickenTimeout = AutoSkillCastTimeout + GetSkillInfo(skill, 9, level)
				UpdateTimeoutFile()

				return
			end
		end
	end

	if (UseDefensiveBuff == buffmode and GuardTimeout ~= -1) then
		if (GetTick() > GuardTimeout) then
			local skill, level = GetGuardSkill(MyID)

			if (skill <= 0) then
				GuardTimeout = -1
			elseif (level == 0) then
				-- skill in cooldown
			elseif (GetSkillInfo(skill, 3, level) <= GetV(V_SP, MyID)) then
				DoSkill(skill, level, MyID, 1)
				GuardTimeout = AutoSkillCastTimeout + GetSkillInfo(skill, 9, level)
				UpdateTimeoutFile()

				return
			end
		end
	end

	return OnAutoBuffs(buffmode)
end

function DoHealingTasks (myid)
	-- Unified auto-heal logic: respect UseChaoticHeal + HealSelfHP/HealOwnerHP thresholds only
	-- Ignore secondary healConditions table to prevent duplicate/conflicting gates
	local rhp = HPPercent(myid)
	local owner = GetV(V_OWNER,myid)
	local ohp = HPPercent(owner)
	local skill, level = GetHealingSkill(myid)

	-- Require auto-heal enabled and valid skill
	if UseChaoticHeal ~= 1 or skill <= 0 then
		return
	end
	-- Respect global autoskill timeout to avoid spamming
	if level == 0 or GetTick() < AutoSkillTimeout then
		return
	end

	local homSp = GetV(V_SP, myid)
	local homMaxSp = GetV(V_MAXSP, myid)
	local spCost = math.floor(homMaxSp * 0.15 + GetSkillInfo(skill, 3, level))

	-- Prioritize self heal if below threshold; else heal owner if below threshold
	if rhp < (ChaoticHealKimiHP or HealSelfHP or 60) then
		if homSp > spCost then
			TraceAI("[AUTO HEAL] Self HP="..rhp.."% casting Chaotic Heal")
			DoSkill(skill, level, myid)
			AutoSkillTimeout = GetTick() + (GetSkillInfo(skill, 9, level) or 500) + AutoSkillCastTimeout
			return 1
		else
			TraceAI("[AUTO HEAL] Insufficient SP for self heal")
			return
		end
	end

	if owner ~= nil and owner > 0 and ohp < (ChaoticHealOwnerHP or HealOwnerHP or 60) then
		if homSp > spCost then
			TraceAI("[AUTO HEAL] Owner HP="..ohp.."% casting Chaotic Heal on owner")
			DoSkill(skill, level, owner)
			AutoSkillTimeout = GetTick() + (GetSkillInfo(skill, 9, level) or 500) + AutoSkillCastTimeout
			return 1
		else
			TraceAI("[AUTO HEAL] Insufficient SP for owner heal")
			return
		end
	end
end

function UpdateTimeoutFile()
	if StickyStandby==2 then
		ShouldStandbyx=ShouldStandby
	else
		ShouldStandbyx=0
	end
	if IsHomun(MyID)==1 then
		OutFile=io.open(ConfigPath.."data/H_"..GetV(V_OWNER,MyID).."Timeouts.lua","w")
	else
		OutFile=io.open(ConfigPath.."data/M_"..GetV(V_OWNER,MyID).."Timeouts.lua","w")
	end
	if OutFile~=nil then
		OutFile:write("GuardTimeout="..TimeoutConv(GuardTimeout).."\nQuickenTimeout="..TimeoutConv(QuickenTimeout).."\nShouldStandby="..ShouldStandbyx.."\nRegenTick[1]="..RegenTick[1])
		OutFile:close()
	else
		TraceAI("Failed to update timeout file")
		logappend("AAI_ERROR","Failed to update timeout file for owner "..GetV(V_OWNER,MyID))
	end
	return
end

function TimeoutConv(a)
	if a==-1 then
		return 0
	else
		return a
	end
end

function	OnIDLEWALK_ST ()
	TraceAI ("OnIDLEWALK_ST")
	ResetCounters()
	MyEnemy					= 0
	if SPPercent(MyID) < IdleWalkSP then
		MyState=IDLE_ST
		TraceAI ("IDLEWALK_ST -> IDLE_ST : SP is below IdleWalkSP - switching to idle mode to regen SP")
		return OnIDLE_ST()
	end
	if (DoIdleTasks()==nil) then
		return
	end
	--Targeting
	if SuperPassive~=1 then
		local	object = SelectEnemy(GetFriendTargets())
		if (object ~= 0) then							-- MYOWNER_ATTACKED_IN
			MyState = CHASE_ST
			MyEnemy = object
			TraceAI ("IDLEWALK_ST -> CHASE_ST : MYOWNER_ATTACKED_IN")
			if (FastChangeCount < FastChangeLimit and FastChange_I2C ==1) then
				OnCHASE_ST()
			end
			return 
		end
		if (HPPercent(MyID) > AggroHP and (SPPercent(MyID) > AggroSP or AggroSP==0) and (ShouldStandby == 0 or StickyStandby ==0)) then
			aggro=1
		else
			aggro=0
		end
		object=SelectEnemy(GetEnemyList(MyID,aggro))
		if object~=0 then
			MyState = CHASE_ST
			MyEnemy = object
			TraceAI ("IDLEWALK_ST -> CHASE_ST : ATTACKED_IN")
			if (FastChangeCount < FastChangeLimit and FastChange_I2C ==1) then
				return OnCHASE_ST()
			end
			return	
		end
		if (aggro==1 and TankMonsterCount < TankMonsterLimit) then
			object = SelectEnemy(GetEnemyList(MyID,-1))
			if (object ~= 0) then
				MyState = TANKCHASE_ST
				MyEnemy = object
				TraceAI ("IDLEWALK_ST -> TANKCHASE_ST")
				return
			end
		end
	end
	--Following
	local distance = GetDistanceAR(MyID,GetV(V_OWNER,MyID))
	if ( distance > GetMoveBounds()) then		-- MYOWNER_OUTSIGNT_IN
		MyState = FOLLOW_ST
		TraceAI ("IDLEWALK_ST -> FOLLOW_ST: Way too far away from owner "..distance)
		return
	end
	if UseAutoHeal==3 then
		if DoHealingTasks(MyID) == 1 then 
			return
		end
	end
	DoAutoBuffs(-2)
	-- end of the usual idle tasks - now we have to do the idle walk part of it, which sucks. 
	local x,y=GetV(V_POSITION,MyID)
	local ox,oy=GetV(V_POSITION,GetV(V_OWNER,MyID))
	local motion=GetV(V_MOTION,MyID)
	if (GetDistanceAPR(MyID,MyDestX,MyDestY)<=1) then --we're there.
		MyDestX,MyDestY=GetIdleWalkDest(MyID)
	elseif (GetDistanceAPR(MyID,MyDestX,MyDestY)>=1) and (IdleWalkTries > 6 or GetDistanceAPR(GetV(V_OWNER,MyID),MyDestX,MyDestY) > GetMoveBounds()) then 
		MyDestX,MyDestY=GetIdleWalkDest(MyID)
		if MyDestX==ox and MyDestY==oy then
			MyDestX,MyDestY=Closest(MyID,MyDestX,MyDestY,1,1)
		end
		IdleWalkTries=0 
		TraceAI("IDLEWALK_ST: New destination: "..MyDestX..","..MyDestY)
		return Move(MyID,MyDestX,MyDestY)
	elseif motion == MOTION_STAND then
		if IdleWalkTries == 4 then
			MyDestX,MyDestY=Closest(MyID,MyDestX,MyDestY,1,1)
			TraceAI("IDLEWALK_ST: Having a bit of trouble - adjust dest to: "..MyDestX..","..MyDestY)
		end
		TraceAI("IDLEWALK_ST: No move after "..IdleWalkTries.." trying again:"..MyDestX..","..MyDestY)
		IdleWalkTries=IdleWalkTries+1
		return Move(MyID,MyDestX,MyDestY)
	else
		--we're en route
		return
	end
end

function GetIdleWalkDest(MyID)
	local mx,my=GetV(V_POSITION,MyID)
	local ox,oy=GetV(V_POSITION,GetV(V_OWNER,MyID))
	local xoff,yoff=mx-ox,my-oy
	TraceAI("Idlewalk"..xoff..","..yoff)
	if UseIdleWalk==1 then --orbit
		local stepsize = 30
		if IdleWalkDistance > 4 then
			stepsize=30
		else
			stepsize=45
		end
		local temp=math.deg(math.atan2(xoff,yoff))+stepsize/2
		if temp < 0 then
			temp=temp+360
		end
		local step = math.floor(temp/stepsize) + 1
		local angle=math.rad(step*stepsize)
		local destx=math.ceil(math.sin(angle)*IdleWalkDistance)+ox
		local desty=math.ceil(math.cos(angle)*IdleWalkDistance)+oy
		TraceAI("Orbit Dest: "..destx..","..desty.." owner: "..ox..","..oy.." mypos: "..mx..","..my)
		return destx,desty	
	elseif UseIdleWalk==2 then -- Cross
		local temp=math.deg(math.atan2(xoff,yoff))+45
		if temp < 0 then
			temp=temp+360
		end
		step = math.floor(temp/90)
		TraceAI("Cross step "..step)
		if step == 0 then -- north goes to south
			return ox,oy-IdleWalkDistance
		elseif step == 1 then -- east goes to north
			return ox,oy+IdleWalkDistance
		elseif step == 2 then -- south goes to west
			return ox-IdleWalkDistance,oy
		else  -- must be west!
			return ox+IdleWalkDistance,oy
		end
	elseif UseIdleWalk==3 then -- Rectangle
		local temp=math.deg(math.atan2(xoff,yoff))+22.5
		local tempt=temp
		if temp < 0 then
			temp=temp+360
		end
		local step = math.floor(temp/45) + 1
		local destx,desty = ox,oy
		if step > 0 and step < 4 then
			destx = ox + IdleWalkDistance
		elseif step >4 and step < 8 then
			destx = ox - IdleWalkDistance
		end
		if step ==8 or step == 1 or step == 7 then
			desty= oy + IdleWalkDistance 
		elseif step > 2 and step < 6 then
			desty=oy-IdleWalkDistance
		end
		TraceAI("Rectangle Dest: "..destx..","..desty.." owner: "..ox..","..oy.." mypos: "..mx..","..my.." temp: "..temp.." "..tempt.." step: "..step)
		return destx,desty
	elseif UseIdleWalk==4 then -- Random
		local step = math.random(0,359)
		local angle=math.rad(step)
		local destx=absceil(math.sin(angle)*IdleWalkDistance)+ox
		local desty=absceil(math.cos(angle)*IdleWalkDistance)+oy
		TraceAI("Random Dest: "..destx..","..desty.." owner: "..ox..","..oy.." mypos: "..mx..","..my) 
		return destx,desty	
	elseif UseIdleWalk==5 or UseIdleWalk==6 then -- Route walk
		local routelen=1
		local step = nil
		local dist = 999
		local posx,posy
		if RelativeRoute==1 then
			posx,posy=xoff,yoff
			TraceAI("Relative route "..xoff..","..yoff)	
		else
			TraceAI("Absolute route "..mx..","..my)	
			posx,posy=mx,my
		end
		
		for k,v in pairs(MyRoute) do
			routelen=k
			if v[1]==posx and v[2]==posy then 
				step=k
				dist=0
				TraceAI("Route Analysis: on route cell "..posx..","..posy.." route step: "..k.." "..v[1]..","..v[2].." current step "..step.."/"..dist)
			else 
				local distance=math.sqrt((v[1]-posx)^2+(v[2]-posy)^2)
				if distance < dist then
					dist=distance
					step=k
					TraceAI("Route Analysis: "..posx..","..posy.." route step: "..k.." "..v[1]..","..v[2].." distance "..distance.." current step "..step.."/"..dist)
				end
			end
		end
		-- now we're at position 'step'	
		local nextstep
		if RouteWalkDirection==1 and step==routelen then
			if UseIdleWalk==5 then
				RouteWalkDirection=-1
				nextstep=step-1
			else 
				nextstep=1
			end
		elseif RouteWalkDirection==-1 and step==1 then
			if UseIdleWalk==5 then
				RouteWalkDirection=1
				nextstep=2
			else 
				nextstep=routelen
			end
		else
			nextstep=step+1*RouteWalkDirection
		end
		TraceAI("Route Walk - nextstep "..nextstep.." from "..step)
		local destx,desty
		if RelativeRoute==1 then
			destx,desty = ox+MyRoute[nextstep][1],oy+MyRoute[nextstep][2]
		else
			destx,desty = MyRoute[nextstep][1],MyRoute[nextstep][2]
		end
		
		TraceAI("Route Dest: "..destx..","..desty.." owner: "..ox..","..oy.." mypos: "..mx..","..my.." pos: "..posx..","..posy.." routelen: "..routelen.." "..dist.." step: "..step.." nextstep= "..nextstep)
		return destx,desty
	else
		logappend("AAI_ERROR","Invalid UseIdleWalk made it all the way to GetIdleWalkDest()")
		UseIdleWalk=0
		return GetV(V_POSITION,MyID)
	end
end

--####################
--### DoKiteAdjust ###
--####################

function DoKiteAdjust(myid,enemy)
	-- Distance validation: Don't kite if we're too close (melee range)
	local distance = GetDistanceA(myid, enemy)
	
	-- Check if kiting is disabled globally
	if KiteMonsters ~= 1 then
		return false
	end
	
	-- Check tactic-specific kiting settings
	local tact_kite = GetTact(TACT_KITE, enemy)
	if tact_kite == KITE_NEVER then
		return false  -- Never kite this mob type
	end
	
	-- Determine if we should kite based on threshold
	local target=GetV(V_TARGET,enemy)
	local kite_threshold
	local step
	
	if (IsFriend(target)==1 or target==MyID) then
		step=KiteStep
		kite_threshold = KiteThreshold
	else
		step=KiteParanoidStep
		kite_threshold = KiteParanoidThreshold
	end
	
	-- Don't kite if distance is greater than threshold (mob is far enough)
	if distance > kite_threshold then
		TraceAI("DoKiteAdjust: Distance "..distance.." > threshold "..kite_threshold..", no need to kite")
		return false
	end
	
	-- If KITE_REACT, only kite when actually being attacked
	if tact_kite == KITE_REACT and (IsFriend(target)~=1 and target~=myid) then
		TraceAI("DoKiteAdjust: KITE_REACT but not being attacked, skipping")
		return false
	end
	
	local x,y=GetV(V_POSITION,myid)
	local ox,oy=GetV(V_POSITION,GetV(V_OWNER,myid))
	local ex,ey=GetV(V_POSITION,enemy)
	local xoptions ={[2]=1,[0]=1,[1]=1}
	local yoptions ={[2]=1,[0]=1,[1]=1}
	local xdirection,ydirection=0,0
	if (x > ex) then
		xoptions[2]=0
	elseif (x < ex) then
		xoptions[1]=0
	else
		yoptions[0]=0
	end
	if (y > ey) then
		yoptions[2]=0
	elseif (y < ey) then
		yoptions[1]=0
	else
		xoptions[0]=0
	end
	if (ox > x) then
		if (xoptions[1]==1) then
			xdirection=1
		elseif (xoptions[0]==1) then
			xdirection=0
		elseif (xoptions[2]==1 and (ox-x+step) <= KiteBounds) then
			xdirection=-1
		elseif	(ey < y) then
			xdirection=0
		else
			xdirection=1
		end
	else
		if (xoptions[2]==1) then
			xdirection=-1
		elseif (xoptions[0]==1) then
			xdirection=0
		elseif (xoptions[1]==1 and (x-ox+step) <= KiteBounds) then
			xdirection=1
		elseif	(ey > y) then
			xdirection=0
		else
			xdirection=-1
		end
	end
	if (oy > y) then
		if (yoptions[1]==1) then
			ydirection=1
		elseif (yoptions[0]==1 and xdirection~=0) then
			ydirection=0
		elseif (yoptions[2]==1 and oy-y+step <= step) then
			ydirection=-1
		elseif	(ex > x and xdirection~=0) then
			ydirection=0
		else
			ydirection=1
		end
	else 
		if (yoptions[2]==1) then
			ydirection=-1
		elseif (yoptions[0]==1 and xdirection~=0) then
			ydirection=0
		elseif (yoptions[1]==1 and y-oy+step <= KiteBounds) then
			ydirection=1
		elseif	(ex < x and xdirection~=0) then
			ydirection=0
		else
			ydirection=-1
		end
	end
	TraceAI("Kiting in "..xdirection..","..ydirection.." direction (distance: "..distance..", threshold: "..kite_threshold..")")
	MyDestX=x+step*xdirection
	MyDestY=y+step*ydirection
	Move(myid,MyDestX,MyDestY)
	return true  -- Kiting executed successfully
end

--#########################
--### TryDetourToEnemy  ###
--#########################
-- Simple step-wise detour toward target when direct approach stalls.
-- Chooses a neighboring cell that reduces distance to the enemy,
-- while respecting KiteBounds relative to the owner.
-- (Removed: TryDetourToEnemy moved to GUI template; runtime file unchanged)

function FailSkillUse(mode) 
	local modex=mode
	if IsPlayer(mode)~=1 then
		mode=13
	end
	if SkillFailCount[mode]==nil then 
		SkillFailCount[mode]=0
	end
	if SkillFailCount[mode] < SkillRetryLimit[mode] then
		if mode == -1 then -- attack state
			MySkillUsedCount=math.max(0,MySkillUsedCount-1)
		elseif mode==1 then
			GuardTimeout=1
		elseif mode==2 then
			QuickenTimeout=1
		elseif IsPlayer(modex) then
			PKFriendsTimeout[modex]=1
		else
			OnFailUnknownMode(mode)
		end
		TraceAI("Skill cast appears to have failed: Mode "..FormatMode(mode).." fail count "..SkillFailCount[mode].." will try again")
		logappend("AAI_SKILLFAIL","Skill cast appears to have failed: Mode "..FormatMode(mode).." fail count "..SkillFailCount[mode].." will try again, QuickenTimeout: "..QuickenTimeout..", GuardTimeout: "..GuardTimeout)
		SkillFailCount[mode]=SkillFailCount[mode]+1
	else
		if (mode~=nil and mode ~=0) then
		TraceAI("Skill cast appears to have failed, but we're past the retry limit, so screw it: Mode "..FormatMode(mode).." fail count "..SkillFailCount[mode])
		logappend("AAI_SKILLFAIL","Skill cast appears to have failed, but we're past the retry limit, so screw it: Mode "..FormatMode(mode).." fail count "..SkillFailCount[mode]..", QuickenTimeout: "..QuickenTimeout..", GuardTimeout: "..GuardTimeout)
		SkillFailCount[mode]=0
	end
	end
	AutoSkillTimeout = 1
	AutoSkillCastTimeout = 1
	if AutoSkillCooldown[CastSkill]~=nil then
		AutoSkillCooldown[CastSkill]=1
	end
	CastSkill=0
	CastSkillLevel=0
	CastSkillMode=0
	CastSkillTime=0
	CastSkillState=0	
end	

--########################
--### Main AI Function ###
--########################

function AI(myid)
	MyID = myid

	if (LastAITime == GetTick()) then --prevent AI from running twice in the same tick. 
		TraceAI("double-AI() call detected, blocked")

		return
	else
		if (LastAITime + 400 < GetTick() and LastAITime > 10) then
			TraceAI("Missed AI calls. Previous AI call was "..LastAITime-GetTick().." ms ago")
			logappend("AAI_SKILLFAIL", "Missed AI calls. Previous AI call was "..LastAITime-GetTick().." ms ago, Cast Skill: "..FormatSkill(CastSkill, CastSkillLevel))

			EnemyPosX = {0,0,0,0,0,0,0,0,0,0} --When we miss AI calls, that means our predictive motion is probly screwed up
			EnemyPosY = {0,0,0,0,0,0,0,0,0,0} --so flush this to prevent homun from getting confused by it, 
		end
		LastAIDelay = GetTick() - LastAITime
		LastAITime = GetTick()

		if MyEnemy ~= 0 then
			local ex, ey = GetV(V_POSITION,MyEnemy)
			for v = 10, 2, -1 do
				EnemyPosX[v], EnemyPosY[v] = EnemyPosX[v - 1], EnemyPosY[v - 1]
			end
			EnemyPosX[1], EnemyPosY[1] = ex, ey, GetV(V_MOTION,MyID)
		end
	end
	if DoneInit== 0 then
		doInit(myid)
	end
	if AggressiveRelogTracking==1 then
		local OutFile
		if IsHomun(MyID)==1 then
			OutFile=io.open(AggressiveRelogPath.."H_"..GetV(V_OWNER,MyID).."Time.lua","w")
		else
			OutFile=io.open(AggressiveRelogPath.."M_"..GetV(V_OWNER,MyID).."Time.lua","w")
		end
		if OutFile~=nil then
			OutFile:write("LastAITime_ART="..LastAITime)
			OutFile:close()
		else
			TraceAI("Failed to update time file for Aggressive Relog Tracking")
			logappend("AAI_ERROR","Failed to update aggressive relog time tracking file for owner "..GetV(V_OWNER,MyID))
		end
	end
	OnAIstart()
	-- ###AUTOFRIEND###
	-- Save the ID to a file so counterpart can friend it
	-- Why is it here instead of at the start?
	-- Because the client wont tell us our ID until AI() is called :-(
	if (NeedToDoAutoFriend==1 and NewAutoFriend==1) then
		TraceAI("Now it's time to do the autofriend")
		local owner=GetV(V_OWNER,myid)
		local OutFile
		if (IsHomun(myid)==1) then
			OutFile=io.open(ConfigPath.."data/H_"..owner..".txt","w")
		else
			OutFile=io.open(ConfigPath.."data/M_"..owner..".txt","w")
		end
		if OutFile~=nil then
			OutFile:write (myid)
			OutFile:close()
		else
			TraceAI("Failed to create autofriending file.")
			logappend("AAI_ERROR","Failed to create autofriending file for owner "..GetV(V_OWNER,MyID))
		end
		NeedToDoAutoFriend=0
	end
	
	--###BOOKKEEPING###
	
	
	-- Hackjob to fix strange behavior, with the timeouts being set to hours, days, or weeks in the future (suspect due to GetTick() returning bad numbers). 

	if GuardTimeout-GetTick() > 350000 then
		logappend("AAI_ERROR","Guard timeout was "..GuardTimeout.." time is "..GetTick())
		GuardTimeout=1
	end

	if QuickenTimeout-GetTick() > 1205000 then
		logappend("AAI_ERROR","Quicken timeout was "..QuickenTimeout.." time is "..GetTick())
		QuickenTimeout=1
	end

	if AIInitTick==0 or AIInitTick==1 or AIInitTick==nil then
		AIInitTick=GetTick()
	end

	FastChangeCount = 0

	--###DATA GATHERING###
	for k,v in pairs(Unreachable) do
		if (IsOutOfSight(myid,k)==true or GetV(V_MOTION,k)==MOTION_DEAD or (IsFriendOrSelf(GetV(V_TARGET,k))==1) and GetV(V_MOTION,k)~=MOTION_STAND) then
			Unreachable[k]=nil
			TraceAI("Marking as reachable"..k)
		end
	end
	
	-- Position sensing
	local x,y=GetV(V_POSITION,MyID)
	local ox,oy=GetV(V_POSITION,GetV(V_OWNER,MyID))
	if MyEnemy ~= 0 then
		local ex,ey=GetV(V_POSITION,MyEnemy)
	else
		ex,ey=0,0
	end
	--myposlog=""
	for v=10,2,-1 do
		MyASAPBuffs[v],MyPosX[v],MyPosY[v],OwnerPosX[v],OwnerPosY[v],EnemyPosX[v],EnemyPosY[v],MyMotions[v],MyStates[v],MyEnemies[v]=MyASAPBuffs[v-1],MyPosX[v-1],MyPosY[v-1],OwnerPosX[v-1],OwnerPosY[v-1],EnemyPosX[v-1],EnemyPosY[v-1],MyMotions[v-1],MyStates[v-1],MyEnemies[v-1]
	end	
	MyPosX[1],MyPosY[1],OwnerPosX[1],OwnerPosY[1],EnemyPosX[1],EnemyPosY[1],MyMotions[1],MyStates[1],MyEnemies[1]=x,y,ox,oy,ex,ey,GetV(V_MOTION,MyID),MyState,MyEnemy
	MyASAPBuffs[1]=0

	--for v=1,10 do
	--	myposlog=myposlog.." "..MyPosX[v]..","..MyPosY[v]
	--end
	--TraceAI("MyPosLog "..myposlog.." x="..x.."y="..y)
	if x~=MyPosX[2] or y~=MyPosY[2] then
		--TraceAI("we moved")
		LastMovedTime=GetTick()
	end
	-- Actor list preprocessing
	local actors=GetActors()
	Actors={}
	OldPlayers=Players
	Players={}
	Monsters={}
	Summons={}
	Retainers={}
	Targets={}
	TakenCells={} --OccupiedCellDetection 
	tMobID=""
	TankMonsterCount=0
	for i,v in ipairs(actors) do
		local x,y = GetV(V_POSITION,v)
		TakenCells[x.."_"..y]=1

		if AAIActors[v]~=1 then
			if IsHomun(myid)==1 then
				logappend("AAI_ACTORS","Actor "..v.." type "..GetV(V_HOMUNTYPE,v).." at "..x..","..y.." Is M="..IsMonster(v))
			else
				logappend("AAI_ACTORS","Actor "..v.." mertype "..GetV(V_MERTYPE,v).." at "..x..","..y.." Is M="..IsMonster(v))
			end
			AAIActors[v]=1
		end
		local x,y = GetV(V_POSITION,v)
		if GetV(V_HOMUNTYPE,v)==1102 and x==174 and 33==y and GetV(V_MOTION,v)==MOTION_STAND then
			-- This is the eden group bathory, ignore it. 
		else
			if (false == IsOutOfSight(myid,v)) then
				Actors[v]=1
				--TraceAI(v.." of type "..GetV(V_HOMUNTYPE,v).." in sight")
				if (v < MagicNumber2) then
					Players[v]=1
					if MyFriends[v]==FRIEND and GetV(V_OWNER,MyID)~=v then --Newly appeared on screen
						if OldPlayers[v]~=1 or AggressiveAutofriend then
							local newfriendhidfile=io.open(ConfigPath.."data/H_"..v..".txt","r")
							local newfriendmidfile=io.open(ConfigPath.."data/M_"..v..".txt","r")
							TraceAI("new friend on screen, checking H_ID"..v)
							if newfriendhidfile~=nil then
								TraceAI("h_id found"..v)
								local newfriendhid=newfriendhidfile:read("*line")
								if newfriendhid~=nil then
									TraceAI("h_id: "..newfriendhid)
									MyFriends[newfriendhid+1-1]=RETAINER --crude type conversion
								end
								newfriendhidfile:close()
							end
							if newfriendmidfile~=nil then
							TraceAI("new friend on screen, checking M_ID"..v)
								local newfriendmid=newfriendmidfile:read("*line")
								TraceAI("m_id found"..v)
								if newfriendmid~=nil then
									TraceAI("m_id: "..newfriendmid)
									MyFriends[newfriendmid+1-1]=RETAINER
								end
								newfriendmidfile:close()
							end
						end	
					end
				elseif IsMonster(v)==1 then
					if LiveMobID == 1 and IsHomun(MyID)==1 then
						tMobID=tMobID.."MobID["..v.."]="..GetV(V_HOMUNTYPE,v).."\n"
					end
					Monsters[v]=1

					if (v < MagicNumber) then
						Summons[v]=1
					end
					if (AutoDetectPlant==1 and IsActive[v]~=1 and IsHomun(myid)~=1) then
						if (GetV(V_MOTION,v)==MOTION_STAND or GetV(V_MOTION,v)==MOTION_DAMAGE or GetV(V_MOTION,v)==MOTION_DEAD) then
							IsActive[v]=0
						else
							IsActive[v]=1
						end
					end
					if (GetTact(TACT_BASIC,v)==TACT_TANK and GetV(V_TARGET,v)==MyID) then
						TankMonsterCount=TankMonsterCount+1
					end
					motionclass=MotionClassLU[GetV(V_MOTION,v)]
					if motionclass==nil then 
						motionclass=3 
					end --CHANGE
					--TraceAI(v.." of type "..GetV(V_HOMUNTYPE,v).." "..motionclass)
					if (motionclass~=-1 and (IsActive[v]==1 or AutoDetectPlant~=1 or IsHomun(myid)==1)) then 
						--TraceAI(v.." of type "..GetV(V_HOMUNTYPE,v).." target added")
						Targets[v]={motionclass,GetTargetClass(GetV(V_TARGET,v))}
					end
				elseif (v < MagicNumber) then		
					Retainers[v]=1
				end
			end
		end
	end
	if LiveMobID == 1 and IsHomun(MyID)==1 and tMobID ~="" then
		local midoutfile=io.open(AggressiveRelogPath.."MobID.lua","w")
		if midoutfile~=nil then
			midoutfile:write(tMobID)
			midoutfile:close()
		else
			TraceAI("Failed to update MobID - check permissions and/or AggressiveRelogPath.")
			logappend("AAI_ERROR","Failed to update aggressive relog time tracking file for owner "..GetV(V_OWNER,MyID))
		end
	end
	if LiveMobID == 1 and IsHomun(MyID)==0 then
		MobID={}
		if pcall(function () dofile(AggressiveRelogPath.."MobID.lua") end) then
			-- do nothing
		else
			logappend("AAI_ERROR", "Failed to load MobID from "..AggressiveRelogPath.."MobID.lua")
		end
	end
	if PVPmode==1 then
		for k,v in pairs(Players) do
			--logappend("AAI_PVP",k.." ("..GetV(V_HOMUNTYPE,k)..") Motion: "..FormatMotion(GetV(V_MOTION,k)).."Is monster? "..IsMonster(k))
			if IsHomun(MyID)==1 then
				TraceAI("PVP: Player "..k.." ("..GetV(V_HOMUNTYPE,k)..") Motion: "..FormatMotion(GetV(V_MOTION,k)).."Is monster? "..IsMonster(k))
			else
				TraceAI("PVP: Player "..k.."  Motion: "..FormatMotion(GetV(V_MOTION,k)).."Is monster? "..IsMonster(k))
			end
			if IsMonster(k)==1 then
				Targets[k] = {MotionClassLU[GetV(V_MOTION,k)],GetTargetClass(GetV(V_TARGET,k))}
			end
		end
	else
		for monsterId,v in pairs(Monsters) do
			if IsMonster(monsterId)==1 then
				Targets[monsterId] = {MotionClassLU[GetV(V_MOTION,monsterId)],GetTargetClass(GetV(V_TARGET,monsterId))}
			end
		end
	end
	
	--New autofriend routine
	if (NewAutoFriend==1 and AssumeHomun==1) then
		friendedOK=0
		retainercount=0
		for k,v in pairs(Retainers) do
			if (k~=MyID) then
				retainercount=retainercount+1
				if (MyFriends[k]==2) then
					friendedOK=1
				end
			end
		end	
		if (friendedOK==0) then
			local owner=GetV(V_OWNER,MyID)
			if (IsHomun(myid)==1) then
				InFile=io.open(ConfigPath.."data/M_"..owner..".txt","r")
			else
				InFile=io.open(ConfigPath.."data/H_"..owner..".txt","r")
			end
			if InFile~=nil then
				retainerid=InFile:read("*a")
				InFile:close()
				retainerid=tonumber(retainerid)
				if (retainerid ~=nil) then
					MyFriends[retainerid]=2
				end
			end
		end
	end

	--SP + Skillfail Watcher
	local sp = GetV(V_SP,MyID)
	local clearcastskill = 0 --There are a buncha globals that need to be zeroed when we're done watching for skill cast success or failure. However, after the initial round of skill failure checking, we still need to know what this skill was in order to check what the skill was. Also, we need to make sure we don't call FailSkillUse() if it looks like it failed, but the SP for it was used. So we set this to 1 if we see it successfully cast, 2 if failed, and then act appropriately afterwards.

	if (CastSkill ~= 0) then
		local castSkillOld = CastSkill
		local castSkillOldlvl = CastSkillLevel -- remember these for the SP watcher down below. 
		local fixedCastSkillOld = GetSkillInfo(CastSkill, 4, CastSkillLevel)
		local variableCastSkillOld = GetSkillInfo(CastSkill, 5, CastSkillLevel)
		local skillCastingTime = fixedCastSkillOld + variableCastSkillOld * CastTimeRatio

		logappend("AAI_SKILLFAIL", "Skillfail Watcher active: " ..FormatSkill(CastSkill, CastSkillLevel).. ", mode: " ..CastSkillMode.. ", delay: " ..LastAIDelay.. ", motion: "..GetV(V_MOTION, MyID)..", QuickenTimeout: "..QuickenTimeout - GetTick().." ms, GuardTimeout: " ..GuardTimeout - GetTick().." ms")
		if (fixedCastSkillOld + variableCastSkillOld == 0) then 
			if LastAIDelay > 500 then
				--Skill cast successfully
				TraceAI("Delay watcher: Skill use successful - mode "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel).." LastAIDelay "..LastAIDelay)
				logappend("AAI_SKILLFAIL","successful skill use detected - mode: "..CastSkillMode..", skill: "..FormatSkill(CastSkill,CastSkillLevel)..", LastAIDelay: "..LastAIDelay)
				clearcastskill=1
				LastASAPTargBuffTime	=0
				LastASAPTargBuffTarg	=0
				LastASAPTargBuffMode	=0
			elseif GetTick() - CastSkillTime > 1000 then
				--Musta failed, we tried to use an instant cast skill 1 second ago and still no sign of it and no sign of delay.
				TraceAI("Delay watcher: no sign of instacast skill casting 1 second later - failed. mode: "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel))
				logappend("AAI_SKILLFAIL", "no sign of instacast skill casting 1 second later - failed. mode: "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel))
				clearcastskill=2
			end
		else
			if (GetV(V_MOTION, MyID) == MOTION_CASTING) then
				CastSkillState = 1
			elseif (CastSkillState > 0) then
				if LastAIDelay > 220 then
					--Skill cast successfully
					if (CastSkillMode == 8) then
						UpdateTimeoutFile()
					end
					TraceAI("Delay watcher: Skill use successful detected by delay - mode "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel).." LastAIDelay "..LastAIDelay)
					logappend("AAI_SKILLFAIL","successful skill use detected by delay - mode "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel).." LastAIDelay "..LastAIDelay)
					clearcastskill=1
				else
					CastSkillState = CastSkillState + 1
					if (CastSkillState > 2) then
						local flinch = 0
						for v=1,10,1 do
							if (MyMotions[v] == MOTION_DAMAGE) then
								flinch = 1
								break
							end
						end
						-- Ground targeted skills aren't interruptible! 
						if (flinch==1 and GetSkillInfo(CastSkill,7,CastSkillLevel)~=2) or skillCastingTime + AutoSkillTimeout + CastSkillTime < GetTick() then 
							TraceAI("Delay watcher: Skill casting detected, but no delay - cast was broken: "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel).." flinch: "..flinch.." expected cast completion: "..(skillCastingTime + CastSkillTime))
							logappend("AAI_SKILLFAIL", "Skill casting detected, but no delay - cast was broken: "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel).." flinch: "..flinch.." expected cast completion: "..(skillCastingTime + CastSkillTime))
							clearcastskill=2
						elseif skillCastingTime + CastSkillTime > GetTick() then
							TraceAI("Delay watcher: Skill use successful - was seen casting, and no sign of interruption, no delay. mode "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel).." LastAIDelay "..LastAIDelay)
							logappend("AAI_SKILLFAIL","Skill use successful - was seen casting, and no sign of interruption, no delay. mode"..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel).." LastAIDelay "..LastAIDelay)
							clearcastskill=1
						end
					end
				end
			else -- CastSkillState is 0, so the skill hasn't started casting, but just in case it slipped by:
				if LastAIDelay > 220 then
					--Skill cast successfully
					if CastSkillMode==8 then
						UpdateTimeoutFile()
					end
					TraceAI("Delay watcher: Skill use successful - mode "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel).." LastAIDelay "..LastAIDelay)
					logappend("AAI_SKILLFAIL","successful skill use detected - mode "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel).." LastAIDelay "..LastAIDelay)
					clearcastskill=1
				elseif GetTick() - CastSkillTime > 1000 then
					TraceAI("Delay watcher: no sign of skill casting 1 second later - failed. mode: "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel))
					logappend("AAI_SKILLFAIL", "no sign of skill casting 1 second later - failed. mode: "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel))
					clearcastskill=2
				end
			end
		end
		-- sp watcher
		local skillOldSpCost = GetSkillInfo(castSkillOld, 3, castSkillOldlvl)
		logappend("AAI_CostSP", "SP: "..sp..", MyLaspSP: "..MyLastSP..", RegenTick: "..RegenTick[1])
		if (sp - MyLastSP == RegenTick[1] - skillOldSpCost or MyLastSP - sp == skillOldSpCost) then
			--if IsHomun(myid)==0 then
				TraceAI("SP watcher: Skill use successful - mode "..CastSkillMode.." skill "..FormatSkill(CastSkill,CastSkillLevel).." LastAIDelay "..LastAIDelay)
				logappend("AAI_SKILLFAIL","successful skill use detected by SP use - mode: "..CastSkillMode..", skill: "..FormatSkill(CastSkill,CastSkillLevel)..", LastAIDelay: "..LastAIDelay)
				clearcastskill=1
			--end
			if (sp - MyLastSP == RegenTick[1] - skillOldSpCost) then
				LastSPTime = GetTick()
			end
		elseif (MyLastSP <= sp) then
			if (sp - MyLastSP == RegenTick[1] and RegenTick[1] ~= 0) then
				LastSPTime = GetTick()
			end
		end
	else
		if sp-MyLastSP == RegenTick[1] and RegenTick[1]~=0 then 
			LastSPTime=GetTick()
		elseif sp-MyLastSP > 100 then
			-- do nothing, owner aid-potted it, ignore this
		elseif sp-MyLastSP > RegenTick[1] or (sp-MyLastSP < RegenTick[1] and sp-MyLastSP > 0)  then
			LastSPTime=GetTick()
			if (sp - MyLastSP > math.floor(GetV(V_MAXSP, MyID)/100)+3) then -- this could be the correct tick to use
				for v=3,2,-1 do
					RegenTick[2][v]=RegenTick[2][v-1]
				end
				RegenTick[2][1]=sp-MyLastSP
				local tickcount=0
				for k,v in pairs(RegenTick[2]) do
					if v~=sp-MyLastSP then
						break
					end
					tickcount=k
				end
				if tickcount==3 then
					RegenTick[1]=sp-MyLastSP
					TraceAI("Watcher: New SP Regen tick set: "..RegenTick[1])
					logappend("AAI_SKILLFAIL","New SP Regen tick set: "..RegenTick[1])
					UpdateTimeoutFile()
				end
			end
		end
	end
	if clearcastskill==1 then
		SkillFailCount[CastSkillMode]=0
		CastSkill=0
		CastSkillLevel=0
		CastSkillMode=0
		CastSkillTime=0
		CastSkillState=0	
	elseif clearcastskill==2 then
		FailSkillUse(CastSkillMode)
	end
	MyLastSP=sp
	
	-----------COMBO SYSTEM EXECUTION-----------
	-- Combo execution is handled inside OnATTACK_ST to align with AzzyAI flow
	-- This prevents timing conflicts with state transitions and movement in CHASE_ST
	
	--###COMMAND PROCESSING###
	local msg	= GetMsg (myid)			-- command
	local rmsg	= GetResMsg (myid)		-- reserved command
	
	if msg[1] == NONE_CMD then
		if rmsg[1] ~= NONE_CMD then
			if List.size(ResCmdList) < 10 then
				List.pushright (ResCmdList,rmsg)
			end
		end
	else
		List.clear (ResCmdList)	-- »õ·Î¿î ¸í·ÉÀÌ ÀÔ·ÂµÇ¸é ¿¹¾à ¸í·ÉµéÀº »èÁ¦ÇÑ´Ù.  
		ProcessCommand (msg)	-- ¸í·É¾î Ã³¸® 
	end
	
	OnAImiddle()
	
	--###EMERGENCY HANDLING###
	
	-- Avoid Routine
	if (IsHomun(MyID)==1 and UseAvoid==1 and (GetTick()-AIInitTick) > 5000) then
		for i,v in ipairs(actors) do
			if MyAvoid[GetV(V_HOMUNTYPE,v)]==1 then
				os.exit()
			end
		end
	end

	if (UseAutoHeal == 1) then
		if DoHealingTasks(MyID) == 1 then
			return
		end
	end
	
	-- Prevent from being left behind
	-- Used only in critical (merc out of MoveBounds) situations
	-- Otherwise, IDLE_ST handles it
	
	local dist2owner=GetDistanceRect(MyID,GetV(V_OWNER,MyID))
	if (MyState ~=FOLLOW_ST and dist2owner > GetMoveBounds()) then
		MyState=FOLLOW_ST
	end
	--Cancel all action during the spawn invulnerability
	if (GetTick() < (MyStart + SpawnDelay)) then
		return
	end
	object = SelectEnemy(GetEnemyList(MyID,-2))

	if (object~=0 and object ~= MyEnemy and MyState~=FOLLOW_ST) then
		MyEnemy=object
		MyState=CHASE_ST
		TraceAI("RESCUE ACTIVATED - Targeting "..object)
	end
	-- New in 1.51 - specialized cast react tactics. 
	for k,v in pairs(Targets) do
		if v[2]==1 or v[2] == 2 then
			tactcast= GetTact(TACT_CAST,k)
			if tactcast > CAST_REACT then
				local skill,level=0,0
				if tactcast > 1000 then
					for ii,vv in ipairs(GetTargetedSkills()) do
						if vv[2]==tactcast and vv[3]~=0 and vv[3]~=nil then
							skill=vv[2]
							level=vv[3]
							break
						end
					end
				else -- generic skill response. 
					for ii,vv in ipairs(GetTargetedSkills()) do
						if tactcast==9 and vv[2]~=0 and vv[3]~=0 and vv[3] ~=nil then
							skill=vv[2]
							level=vv[3]
							break
						elseif
							tactcast==vv[1]-10 and vv[2]~=0 and vv[3]~=0 and vv[3] ~=nil then
							skill=vv[2]
							level=vv[3]
							break
						end
					end
				end
				if skill~=0 then
					MySkill=skill
					MySkillLevel=level
					MyEnemy=k
					MyState=OnSKILL_OBJECT_CMD_ST
					TraceAI("CAST_REACT_(skill) being enabled against target"..k.." with tactic "..tactcast.." with "..FormatSkill(skill,level))
					break
				end
			end
		end
	end

	-- Don't cast buffs during idle when combo is enabled
	if not (ComboEnabled == 1 and MyState == IDLE_ST) then
		if DoAutoBuffs(3) ~= 1 then
			logappend("AAI_ASAP","ASAP BUFF "..MyState)
			return
		end
	end

	--###STATE PROCESSES###
	--TraceAI("SP tracking: Time: "..GetTick().." last moved: "..LastMovedTime.." last sp time "..LastSPTime)
	if (LagReduction) then
		if LagReductionCD > 0 then
			LagReductionCD = LagReductionCD-1
		end
	end
	
  if LagReductionCD > 0 then
		TraceAI("Skipping state functions due to aggressive lag reduction")
	elseif (MyState == IDLE_ST) then
		OnIDLE_ST ()
	elseif (MyState == CHASE_ST) then					
		OnCHASE_ST ()
	elseif (MyState == ATTACK_ST) then
		OnATTACK_ST ()
	elseif (MyState == FOLLOW_ST) then
		OnFOLLOW_ST ()
	elseif (MyState == MOVE_CMD_ST) then
		OnMOVE_CMD_ST ()
	elseif (MyState == STOP_CMD_ST) then
		OnSTOP_CMD_ST ()
	elseif (MyState == ATTACK_OBJECT_CMD_ST) then
		OnATTACK_OBJECT_CMD_ST ()
	elseif (MyState == ATTACK_AREA_CMD_ST) then
		OnATTACK_AREA_CMD_ST ()
	elseif (MyState == PATROL_CMD_ST) then
		OnPATROL_CMD_ST ()
	elseif (MyState == HOLD_CMD_ST) then
		OnHOLD_CMD_ST ()
	elseif (MyState == SKILL_OBJECT_CMD_ST) then
		OnSKILL_OBJECT_CMD_ST ()
	elseif (MyState == SKILL_AREA_CMD_ST) then
		OnSKILL_AREA_CMD_ST ()
	elseif (MyState == FOLLOW_CMD_ST) then
		OnFOLLOW_CMD_ST ()
	elseif (MyState == IDLEWALK_ST) then
		OnIDLEWALK_ST ()
	elseif (MyState == ORBITWALK_ST) then
		OnORBITWALK_ST()
	elseif (MyState == REST_ST) then
		OnREST_ST()
	elseif (MyState == TANKCHASE_ST) then
		OnTANKCHASE_ST()
	elseif (MyState == TANK_ST) then
		OnTANK_ST()
	elseif (MyState == MOVE_CMD_HOLD_ST) then
		OnMOVE_CMD_HOLD_ST()
	elseif (MyState == FRIEND_CROSS_ST) then
		OnFRIEND_CROSS_ST()
	elseif (MyState == FRIEND_CIRCLE_ST) then
		OnFRIEND_CIRCLE_ST()
	else
		if NewState(MyState)==-1 then  
			TraceAI("Invalid State: "..MyState.." -> IDLE_ST")
			logappend("AAI_ERROR","MyState set to invalid state: "..MyState)
			MyState=IDLE_ST
		end
	end
	if (LagReduction) then
		modtwroSend()
	end
	OnAIEnd()
end
