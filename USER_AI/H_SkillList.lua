-- H_SkillList.lua - Kimi skill catalog and runtime metadata
--
-- The catalog is deliberately kept in the runtime.  The client enforces
-- learned-skill and loyalty rules; the AI only needs to enforce Kimi type,
-- active/passive, configured level, target mode, and local timing data.

-- Skill ids (the internal names are retained for server/client diagnostics).
S_MASTER_SWAP        = 8005 -- HAMI_CASTLE
S_BASTION_RENEWAL    = 8006 -- HAMI_DEFENCE
S_LIVING_BULWARK     = 8007 -- HAMI_SKIN
S_ILLUSION_OF_CLAW   = 8009 -- HFLI_MOON
S_ENHANCED_REFLEXES  = 8010 -- HFLI_FLEET
S_SECOND_WIND        = 8011 -- HFLI_SPEED
S_FADE_AWAY          = 8012 -- HFLI_SBR44
S_CHAOTIC_SANCTUARY  = 8013 -- HVAN_CAPRICE
S_CHAOTIC_HEAL       = 8014 -- HVAN_CHAOTIC
S_ARCANE_OFFERING    = 8015 -- HVAN_INSTRUCT
S_TAUNT              = 8021 -- MH_PAIN_KILLER
S_BODY_DOUBLE        = 8022 -- MH_LIGHT_OF_REGENE
S_QUICK_DEFENSE      = 8023 -- MH_OVERED_BOOST
S_ILLUSION_OF_BREATH = 8024 -- MH_ERASER_CUTTER
S_ILLUSION_CRUSHER   = 8031 -- MH_STAHL_HORN
S_BLOOD_SWEEP        = 8032 -- MH_GOLDENE_FERSE
S_WARD_DOMAIN        = 8033 -- MH_STEINWAND
S_ILLUSION_OF_LIGHT  = 8034 -- MH_HEILIGE_STANGE
S_BLOOD_HUNGER       = 8035 -- MH_ANGRIFFS_MODUS
S_MIRAGE_ASSAULT     = 8036 -- MH_TINDER_BREAKER
S_PREDATORS_FOCUS    = 8037 -- MH_CBC
S_BLOOD_FRENZY       = 8038 -- MH_EQC
S_RETALIATION         = 8039 -- MH_MAGMA_FLOW
S_IRONBLOOD          = 8040 -- MH_GRANITIC_ARMOR

-- Backwards-compatible names used by the original Kimi runtime.
S_ILLUSION_OF_CLAWS = S_ILLUSION_OF_CLAW
S_WARM_DEF          = S_BASTION_RENEWAL
S_WARDS_DOMAIN      = S_WARD_DOMAIN

-- KimiSkillCatalog[type][skillid] is the complete per-Kimi mapping. Values
-- are max learned levels; zero in a user setting disables a skill.
KimiSkillCatalog = {
  [WARD] = {
    [S_MASTER_SWAP]       = {name="Master Swap",       maxLevel=5,  active=true,  internalName="HAMI_CASTLE",       targetMode=1, targetKind="owner", loyal=true,  verified=true},
    [S_BASTION_RENEWAL]   = {name="Bastion Renewal",   maxLevel=5,  active=true,  internalName="HAMI_DEFENCE",       targetMode=0, targetKind="self",  verified=true},
    [S_LIVING_BULWARK]    = {name="Living Bulwark",    maxLevel=5,  active=false, internalName="HAMI_SKIN",          targetMode=0, targetKind="passive", verified=true},
    [S_WARD_DOMAIN]        = {name="Ward's Domain",     maxLevel=5,  active=true,  internalName="MH_STEINWAND",       targetMode=0, targetKind="self",  verified=true, aoeRadius={4,4,4,4,4}, recastLockMs=30000, costMode="maxsp_percent_per_second", costPercent=15},
    [S_RETALIATION]         = {name="Retaliation",       maxLevel=10, active=false, internalName="MH_MAGMA_FLOW",      targetMode=0, targetKind="passive", verified=true},
    [S_TAUNT]               = {name="Taunt",              maxLevel=5,  active=true,  internalName="MH_PAIN_KILLER",     targetMode=0, targetKind="self",  verified=true, aoeRadius={5,5,6,6,7}},
    [S_QUICK_DEFENSE]       = {name="Quick Defense",      maxLevel=5,  active=true,  internalName="MH_OVERED_BOOST",    targetMode=0, targetKind="self",  verified=true, costMode="maxsp_percent", costPercent=10},
    [S_BODY_DOUBLE]         = {name="Body Double",        maxLevel=5,  active=true,  internalName="MH_LIGHT_OF_REGENE", targetMode=0, targetKind="self",  loyal=true, verified=true, cooldownVerified=true},
  },
  [OCCULT] = {
    [S_MASTER_SWAP]         = {name="Master Swap",         maxLevel=5, active=true,  internalName="HAMI_CASTLE",        targetMode=1, targetKind="owner", loyal=true, verified=true},
    [S_CHAOTIC_HEAL]        = {name="Chaotic Heal",        maxLevel=5, active=true,  internalName="HVAN_CHAOTIC",        targetMode=0, targetKind="self",  verified=true, costMode="maxsp_percent", costPercent=50},
    [S_ILLUSION_OF_BREATH]  = {name="Illusion of Breath",  maxLevel=10,active=true,  internalName="MH_ERASER_CUTTER",    targetMode=1, targetKind="enemy", verified=true, castTimeUnverified=true},
    [S_ILLUSION_OF_LIGHT]   = {name="Illusion of Light",   maxLevel=5, active=true,  internalName="MH_HEILIGE_STANGE",   targetMode=1, targetKind="enemy", loyal=true, verified=true},
    [S_CHAOTIC_SANCTUARY]   = {name="Chaotic Sanctuary",   maxLevel=5, active=true,  internalName="HVAN_CAPRICE",        targetMode=2, targetKind="ground", verified=true, aoeSize=5},
    [S_ARCANE_OFFERING]     = {name="Arcane Offering",     maxLevel=5, active=true,  internalName="HVAN_INSTRUCT",       targetMode=0, targetKind="self",  verified=true, costMode="partial_maxsp_percent_per_level", costPercent=5, partialCost=true},
    [S_QUICK_DEFENSE]       = {name="Quick Defense",       maxLevel=5, active=true,  internalName="MH_OVERED_BOOST",     targetMode=0, targetKind="self",  verified=true, costMode="maxsp_percent", costPercent=10},
    [S_BODY_DOUBLE]         = {name="Body Double",         maxLevel=5, active=true,  internalName="MH_LIGHT_OF_REGENE",  targetMode=0, targetKind="self",  loyal=true, verified=true, cooldownVerified=true},
  },
  [AGILE] = {
    [S_MASTER_SWAP]         = {name="Master Swap",        maxLevel=5,  active=true,  internalName="HAMI_CASTLE",        targetMode=1, targetKind="owner", loyal=true, verified=true},
    [S_ILLUSION_OF_CLAW]    = {name="Illusion of Claw",   maxLevel=10, active=true,  internalName="HFLI_MOON",          targetMode=1, targetKind="enemy", verified=true, verifiedLevels=5},
    [S_ENHANCED_REFLEXES]   = {name="Enhanced Reflexes",  maxLevel=5,  active=false, internalName="HFLI_FLEET",         targetMode=0, targetKind="passive", verified=true},
    [S_SECOND_WIND]         = {name="Second Wind",        maxLevel=10, active=false, internalName="HFLI_SPEED",         targetMode=0, targetKind="passive", verified=true},
    [S_FADE_AWAY]           = {name="Fade Away",           maxLevel=1,  active=true,  internalName="HFLI_SBR44",         targetMode=0, targetKind="self",  verified=false, fallback=true},
    [S_MIRAGE_ASSAULT]      = {name="Mirage Assault",     maxLevel=5,  active=true,  internalName="MH_TINDER_BREAKER",  targetMode=1, targetKind="enemy", verified=false, fallback=true},
    [S_QUICK_DEFENSE]       = {name="Quick Defense",       maxLevel=5,  active=true,  internalName="MH_OVERED_BOOST",     targetMode=0, targetKind="self",  verified=true, costMode="maxsp_percent", costPercent=10},
    [S_BODY_DOUBLE]         = {name="Body Double",         maxLevel=5,  active=true,  internalName="MH_LIGHT_OF_REGENE",  targetMode=0, targetKind="self",  loyal=true, verified=true, cooldownVerified=true},
  },
  [RAGING] = {
    [S_ILLUSION_CRUSHER]    = {name="Illusion Crusher",   maxLevel=5,  active=true,  internalName="MH_STAHL_HORN",      targetMode=1, targetKind="enemy", verified=true},
    [S_BLOOD_SWEEP]         = {name="Blood Sweep",        maxLevel=10, active=true,  internalName="MH_GOLDENE_FERSE",   targetMode=1, targetKind="enemy", verified=false, fallback=true, aoeSize=5},
    [S_BLOOD_HUNGER]        = {name="Blood Hunger",       maxLevel=10, active=false, internalName="MH_ANGRIFFS_MODUS",   targetMode=0, targetKind="passive", verified=true},
    [S_PREDATORS_FOCUS]     = {name="Predator's Focus",   maxLevel=1,  active=false, internalName="MH_CBC",              targetMode=0, targetKind="passive", verified=true},
    [S_BLOOD_FRENZY]        = {name="Blood Frenzy",       maxLevel=5,  active=false, internalName="MH_EQC",              targetMode=0, targetKind="passive", verified=true},
    [S_IRONBLOOD]           = {name="Ironblood",           maxLevel=5,  active=false, internalName="MH_GRANITIC_ARMOR",  targetMode=0, targetKind="passive", verified=true},
    [S_QUICK_DEFENSE]       = {name="Quick Defense",       maxLevel=5,  active=true,  internalName="MH_OVERED_BOOST",     targetMode=0, targetKind="self",  verified=true, costMode="maxsp_percent", costPercent=10},
    [S_BODY_DOUBLE]         = {name="Body Double",         maxLevel=5,  active=true,  internalName="MH_LIGHT_OF_REGENE",  targetMode=0, targetKind="self",  loyal=true, verified=true, cooldownVerified=true},
  },
}

-- Flat metadata is shared by all runtime paths. Shared skills have one entry
-- while the per-type catalog entries remain independently inspectable.
KimiSkillMetadata = {}
for kimiType, skills in pairs(KimiSkillCatalog) do
  for skillid, meta in pairs(skills) do
    if KimiSkillMetadata[skillid] == nil then
      KimiSkillMetadata[skillid] = {
        id=skillid, name=meta.name, maxLevel=meta.maxLevel, active=meta.active,
        internalName=meta.internalName, targetMode=meta.targetMode,
        targetKind=meta.targetKind, loyal=meta.loyal, verified=meta.verified,
        verifiedLevels=meta.verifiedLevels, fallback=meta.fallback,
        costMode=meta.costMode, costPercent=meta.costPercent,
        partialCost=meta.partialCost, aoeRadius=meta.aoeRadius,
        aoeSize=meta.aoeSize, recastLockMs=meta.recastLockMs,
        durationLock=meta.durationLock, cooldownVerified=meta.cooldownVerified,
        castTimeUnverified=meta.castTimeUnverified,
        allowedTypes={}
      }
    end
    KimiSkillMetadata[skillid].allowedTypes[kimiType] = true
  end
end

KimiActiveSkillIds = {
  S_MASTER_SWAP, S_BASTION_RENEWAL, S_WARD_DOMAIN, S_TAUNT,
  S_QUICK_DEFENSE, S_BODY_DOUBLE, S_CHAOTIC_HEAL,
  S_ILLUSION_OF_BREATH, S_ILLUSION_OF_LIGHT, S_CHAOTIC_SANCTUARY,
  S_ARCANE_OFFERING, S_ILLUSION_OF_CLAW, S_FADE_AWAY,
  S_MIRAGE_ASSAULT, S_ILLUSION_CRUSHER, S_BLOOD_SWEEP
}
KimiPassiveSkillIds = {
  S_LIVING_BULWARK, S_RETALIATION, S_ENHANCED_REFLEXES, S_SECOND_WIND,
  S_BLOOD_HUNGER, S_PREDATORS_FOCUS, S_BLOOD_FRENZY, S_IRONBLOOD
}
KimiSkillIds = {}
for _, id in ipairs(KimiActiveSkillIds) do table.insert(KimiSkillIds, id) end
for _, id in ipairs(KimiPassiveSkillIds) do table.insert(KimiSkillIds, id) end

-- SkillList contains passives so the GUI/catalog can display them; selectors
-- and DoSkill reject passives through KimiSkillCanCast.
SkillList = {}
for kimiType, skills in pairs(KimiSkillCatalog) do
  SkillList[kimiType] = {}
  for skillid, meta in pairs(skills) do
    SkillList[kimiType][skillid] = meta.maxLevel
  end
end

local function zeros(n)
  local result = {}
  for i=1,n do result[i] = 0 end
  return result
end

-- SkillInfo[id] = {name, range, SP cost, fixed cast, variable cast, delay,
--                  target mode, duration, reuse delay}.
-- Percent costs and partial transfers live in metadata; their legacy absolute
-- SP slot stays zero so the engine never demands an invented upfront amount.
SkillInfo = {}
SkillInfo[0] = {"No Skill", zeros(10), zeros(10), zeros(10), zeros(10), zeros(10), 0, zeros(10), zeros(10)}

SkillInfo[S_MASTER_SWAP] = {
  "Master Swap", {}, {10,10,10,10,10}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, 1, {0,0,0,0,0}, {10000,8000,6000,4000,2000}
}
SkillInfo[S_BASTION_RENEWAL] = {
  "Bastion Renewal", {0,0,0,0,0}, {20,25,30,35,40}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, 0, {2000,4000,6000,8000,10000}, {15000,15000,15000,15000,15000}
}
SkillInfo[S_LIVING_BULWARK] = {"Living Bulwark", {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, 0, {0,0,0,0,0}, {0,0,0,0,0}}
SkillInfo[S_ILLUSION_OF_CLAW] = {
  "Illusion of Claw", {1,1,1,1,1,1,1,1,1,1}, {5,10,15,20,25,0,0,0,0,0}, {100,100,100,100,100,0,0,0,0,0}, {0,0,0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0,0,0}, 1, {0,0,0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0,0,0}
}
SkillInfo[S_ENHANCED_REFLEXES] = {"Enhanced Reflexes", {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, 0, {0,0,0,0,0}, {0,0,0,0,0}}
SkillInfo[S_SECOND_WIND] = {"Second Wind", {0,0,0,0,0,0,0,0,0,0}, zeros(10), zeros(10), zeros(10), zeros(10), 0, zeros(10), zeros(10)}
SkillInfo[S_FADE_AWAY] = {"Fade Away", {}, {0}, {0}, {0}, {0}, 0, {0}, {0}}
SkillInfo[S_CHAOTIC_SANCTUARY] = {
  "Chaotic Sanctuary", {}, {100,100,100,100,100}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, 2, {2000,4000,6000,8000,10000}, {15000,15000,15000,15000,15000}
}
SkillInfo[S_CHAOTIC_HEAL] = {
  "Chaotic Heal", {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, 0, {0,0,0,0,0}, {3000,3000,3000,3000,3000}
}
SkillInfo[S_ARCANE_OFFERING] = {
  "Arcane Offering", {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, 0, {0,0,0,0,0}, {5000,5000,5000,5000,5000}
}
SkillInfo[S_TAUNT] = {
  "Taunt", {0,0,0,0,0}, {30,30,30,30,30}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, 0, {0,0,0,0,0}, {10000,10000,10000,10000,10000}
}
SkillInfo[S_BODY_DOUBLE] = {
  "Body Double", {0,0,0,0,0}, {20,40,60,80,100}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, 0, {10000,20000,30000,40000,50000}, {0,0,0,0,0}
}
SkillInfo[S_QUICK_DEFENSE] = {
  "Quick Defense", {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, 0, {2000,2000,2000,2000,2000}, {10000,10000,10000,10000,10000}
}
SkillInfo[S_ILLUSION_OF_BREATH] = {
  "Illusion of Breath", {7,7,7,7,7,7,7,7,7,7}, {5,10,15,20,25,30,35,40,45,50}, {1000,1222,1444,1666,1888,2111,2333,2555,2777,3000}, {0,0,0,0,0,0,0,0,0,0}, {0,0,0,0,0,0,0,0,0,0}, 1, zeros(10), zeros(10)
}
SkillInfo[S_ILLUSION_CRUSHER] = {
  "Illusion Crusher", {1,1,1,1,1}, {50,75,100,125,150}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, 1, {0,0,0,0,0}, {0,0,0,0,0}
}
SkillInfo[S_BLOOD_SWEEP] = {"Blood Sweep", {}, zeros(10), zeros(10), zeros(10), zeros(10), 1, zeros(10), zeros(10)}
SkillInfo[S_WARD_DOMAIN] = {
  "Ward's Domain", {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, 0, {30000,30000,30000,30000,30000}, {20000,20000,20000,20000,20000}
}
SkillInfo[S_ILLUSION_OF_LIGHT] = {
  "Illusion of Light", {7,7,7,7,7}, {100,100,100,100,100}, {2000,2750,3500,4250,5000}, {0,0,0,0,0}, {0,0,0,0,0}, 1, {0,0,0,0,0}, {4000,4000,4000,4000,4000}
}
SkillInfo[S_BLOOD_HUNGER] = {"Blood Hunger", {0,0,0,0,0,0,0,0,0,0}, zeros(10), zeros(10), zeros(10), zeros(10), 0, zeros(10), zeros(10)}
SkillInfo[S_MIRAGE_ASSAULT] = {"Mirage Assault", {}, zeros(5), zeros(5), zeros(5), zeros(5), 1, zeros(5), zeros(5)}
SkillInfo[S_PREDATORS_FOCUS] = {"Predator's Focus", {0}, {0}, {0}, {0}, {0}, 0, {0}, {0}}
SkillInfo[S_BLOOD_FRENZY] = {"Blood Frenzy", {0,0,0,0,0}, zeros(5), zeros(5), zeros(5), zeros(5), 0, zeros(5), zeros(5)}
SkillInfo[S_RETALIATION] = {"Retaliation", {0,0,0,0,0,0,0,0,0,0}, zeros(10), zeros(10), zeros(10), zeros(10), 0, zeros(10), zeros(10)}
SkillInfo[S_IRONBLOOD] = {"Ironblood", {0,0,0,0,0}, zeros(5), zeros(5), zeros(5), zeros(5), 0, zeros(5), zeros(5)}

-- SkillAOEInfo[id] = {size by level, center mode, optional "radius"}.
-- Self-centered effects use explicit radii; other sizes are square widths.
SkillAOEInfo = {}
SkillAOEInfo[0] = {zeros(10), 0}
SkillAOEInfo[S_TAUNT] = {{5,5,6,6,7}, 1, "radius"}
SkillAOEInfo[S_WARD_DOMAIN] = {{4,4,4,4,4}, 1, "radius"}
SkillAOEInfo[S_CHAOTIC_SANCTUARY] = {{5,5,5,5,5}, 0}
SkillAOEInfo[S_ILLUSION_OF_LIGHT] = {{3,3,3,3,3,3,3,3,3,3}, 0}
-- Blood Sweep affects a 5x5 area at every level (confirmed by the user).
SkillAOEInfo[S_BLOOD_SWEEP] = {{5,5,5,5,5,5,5,5,5,5}, 0}

-- Legacy level globals remain supported for the original seven settings.
KimiLegacySkillLevelNames = {
  [S_ILLUSION_OF_CLAW]   = "illusionOfClawsLevel",
  [S_ILLUSION_OF_BREATH] = "illusionOfBreathLevel",
  [S_ILLUSION_CRUSHER]   = "illusionOfCrusherLevel",
  [S_ILLUSION_OF_LIGHT]  = "illusionOfLightLevel",
  [S_CHAOTIC_HEAL]       = "chaoticHealLevel",
  [S_BODY_DOUBLE]        = "bodyDoubleLevel",
  [S_BASTION_RENEWAL]    = "warmDefLevel",
}

function NormalizeKimiType(value)
  if type(value) == "string" then value = tonumber(value) end
  if value == 6001 then return WARD end
  if value == 6002 then return OCCULT end
  if value == 6003 then return AGILE end
  if value == 6004 then return RAGING end
  if value == WARD or value == OCCULT or value == AGILE or value == RAGING then return value end
  return nil
end

function GetKimiType(myid)
  myid = myid or MyID
  if myid == nil or GetV == nil then return nil end
  return NormalizeKimiType(GetV(V_HOMUNTYPE, myid))
end

function GetKimiSkillMeta(skillid, kimiType)
  skillid = tonumber(skillid)
  if skillid == nil then return nil end
  kimiType = NormalizeKimiType(kimiType)
  if kimiType == nil then kimiType = GetKimiType(MyID) end
  if kimiType == nil or KimiSkillCatalog[kimiType] == nil then return nil end
  return KimiSkillCatalog[kimiType][skillid]
end

function GetKimiConfiguredSkillLevel(skillid)
  skillid = tonumber(skillid)
  if skillid == nil then return nil end
  local enabled = KimiSkillEnabled and KimiSkillEnabled[skillid]
  local tableLevel = KimiSkillLevels and KimiSkillLevels[skillid]
  local legacyName = KimiLegacySkillLevelNames[skillid]
  local legacyLevel = legacyName and _G[legacyName] or nil

  -- New GUI settings use an explicit enabled bit so the displayed level can
  -- start at one while a newly added skill remains disabled.
  if enabled ~= nil then
    if tonumber(enabled) == 0 then return 0 end
    if tableLevel ~= nil then return tonumber(tableLevel) or 0 end
    if legacyLevel ~= nil then return tonumber(legacyLevel) or 0 end
    return nil
  end
  if tableLevel ~= nil then return tonumber(tableLevel) or 0 end
  if legacyLevel ~= nil then return tonumber(legacyLevel) or 0 end
  return nil
end

function GetKimiSkillMaxLevel(skillid, kimiType)
  local meta = GetKimiSkillMeta(skillid, kimiType)
  return meta and meta.maxLevel or 0
end

-- Resolve a requested/configured level through the same type/active/max gate
-- used by every cast path. A configured setting always wins over a blueprint
-- level, and a disabled setting returns zero.
function GetKimiSkillLevel(skillid, requested, myid)
  local kimiType = GetKimiType(myid or MyID)
  local meta = GetKimiSkillMeta(skillid, kimiType)
  if meta == nil or meta.active ~= true then return 0 end
  local configured = GetKimiConfiguredSkillLevel(skillid)
  local level = configured
  if level == nil then level = requested end
  if level == nil then level = meta.maxLevel end
  level = tonumber(level) or 0
  level = math.floor(level)
  if level < 0 then level = 0 end
  if level > meta.maxLevel then level = meta.maxLevel end
  return level
end

function GetKimiSkillCost(skillid, level, myid)
  local id = tonumber(skillid)
  local meta = KimiSkillMetadata[id]
  level = tonumber(level) or 0
  myid = myid or MyID
  if meta and meta.costMode == "maxsp_percent" then
    local maxsp = (GetV and myid) and (GetV(V_MAXSP, myid) or 0) or 0
    return math.floor(maxsp * (meta.costPercent or 0) / 100 + 0.5)
  elseif meta and meta.costMode == "maxsp_percent_per_level" then
    local maxsp = (GetV and myid) and (GetV(V_MAXSP, myid) or 0) or 0
    return math.floor(maxsp * (meta.costPercent or 0) * level / 100 + 0.5)
  elseif meta and meta.costMode == "partial_maxsp_percent_per_level" then
    local maxsp = (GetV and myid) and (GetV(V_MAXSP, myid) or 0) or 0
    local current = (GetV and myid) and (GetV(V_SP, myid) or 0) or 0
    local nominal = math.floor(maxsp * (meta.costPercent or 0) * level / 100 + 0.5)
    return math.min(current, nominal)
  end
  local info = SkillInfo[id]
  local costs = info and info[3]
  if type(costs) == "table" then return tonumber(costs[level] or costs[1]) or 0 end
  return tonumber(costs) or 0
end

function GetKimiSkillReuseDelay(skillid, level)
  local id = tonumber(skillid)
  local meta = KimiSkillMetadata[id]
  local info = SkillInfo[id]
  local reuse = 0
  if info and type(info[9]) == "table" then reuse = tonumber(info[9][level] or info[9][1]) or 0 end
  if meta and meta.durationLock and info and type(info[8]) == "table" then
    local duration = tonumber(info[8][level] or info[8][1]) or 0
    if duration > reuse then reuse = duration end
  end
  local configured = 0
  if id == S_BODY_DOUBLE then configured = tonumber(BodyDoubleCooldown) or 0
  elseif id == S_BASTION_RENEWAL then
    configured = tonumber(BastionRenewalCooldown) or 0
    -- A positive GUI value overrides the catalog fallback for this server.
    if configured > 0 then return configured * 1000 end
  elseif id == S_MASTER_SWAP then configured = tonumber(MasterSwapCooldown) or 0 end
  reuse = math.max(reuse, math.max(0, configured) * 1000)
  if meta and meta.recastLockMs and meta.recastLockMs > reuse then return meta.recastLockMs end
  return reuse
end

function KimiSkillCanCast(skillid, requestedLevel, myid)
  myid = myid or MyID
  local level = GetKimiSkillLevel(skillid, requestedLevel, myid)
  if level <= 0 then return false, 0, "disabled_or_invalid" end
  local meta = GetKimiSkillMeta(skillid, GetKimiType(myid))
  if meta == nil or meta.active ~= true then return false, 0, "wrong_type_or_passive" end
  if AutoSkillCooldown and AutoSkillCooldown[tonumber(skillid)] and GetTick and GetTick() < AutoSkillCooldown[tonumber(skillid)] then
    return false, level, "cooldown"
  end
  local current = (GetV and GetV(V_SP, myid)) or 0
  local cost = GetKimiSkillCost(skillid, level, myid)
  if meta.partialCost or meta.costMode == "maxsp_percent_per_level" or meta.costMode == "maxsp_percent_per_second" then
    -- Offering transfers the amount currently available. Aura drain is per
    -- second, so neither path should require a full nominal upfront amount.
    if current <= 0 then return false, level, "no_sp" end
  elseif current < cost then
    return false, level, "insufficient_sp"
  end
  return true, level, nil
end

function GetKimiSkillTargetMode(skillid)
  local meta = KimiSkillMetadata[tonumber(skillid)]
  if meta and meta.targetMode ~= nil then return meta.targetMode end
  local info = SkillInfo[tonumber(skillid)]
  return info and info[7] or 1
end

function GetKimiSkillTarget(skillid, target, myid)
  local meta = KimiSkillMetadata[tonumber(skillid)]
  if meta and meta.targetKind == "owner" then return GetV(V_OWNER, myid or MyID) end
  if meta and meta.targetMode == 0 then return myid or MyID end
  return target
end
