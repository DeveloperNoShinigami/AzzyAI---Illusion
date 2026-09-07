-- H_SkillList.lua	v1.50.1
-- Converted to Lua by Dr. Azzy from
-- Mercenary skill list posted by Neo Saro -and-
-- Homun S skill list ganked from some pserver site.
-- All skill list and skill info values laboriously hand entered by Dr. Azzy
-- This may be distributed and used in AIs freely
-- Customized for the 'Return of Morroc' server by Nathan.

-- Kimi Skills

S_ILLUSION_OF_CLAWS	= 8009
S_WARM_DEF = 8006
S_CHAOTIC_HEAL = 8014
S_BODY_DOUBLE = 8022
S_ILLUSION_OF_BREATH = 8024
S_ILLUSION_CRUSHER = 8031
S_ILLUSION_OF_LIGHT = 8034
S_MASTER_SWAP = 8005

--SkillList[homuntype][skillid] = level

SkillList={}

--####################
--#### Agile Kimi ####
--####################

SkillList[AGILE] = {}
SkillList[AGILE][S_ILLUSION_OF_CLAWS] = 5
SkillList[AGILE][S_ILLUSION_CRUSHER] = 5
SkillList[AGILE][S_CHAOTIC_HEAL] = 5
SkillList[AGILE][S_WARM_DEF] = 5
SkillList[AGILE][S_BODY_DOUBLE] = 5
SkillList[AGILE][S_MASTER_SWAP] = 0

--#####################
--#### Occult Kimi ####
--#####################

SkillList[OCCULT] = {}
SkillList[OCCULT][S_ILLUSION_OF_LIGHT] = 10
SkillList[OCCULT][S_ILLUSION_OF_BREATH] = 10
SkillList[OCCULT][S_CHAOTIC_HEAL] = 5
SkillList[OCCULT][S_WARM_DEF] = 5
SkillList[OCCULT][S_BODY_DOUBLE] = 5
SkillList[OCCULT][S_MASTER_SWAP] = 0

--#####################
--#### Raging Kimi ####
--#####################

SkillList[RAGING] = {}
SkillList[RAGING][S_ILLUSION_OF_CLAWS] = 5
SkillList[RAGING][S_ILLUSION_CRUSHER] = 5
SkillList[RAGING][S_CHAOTIC_HEAL] = 5
SkillList[RAGING][S_WARM_DEF] = 5
SkillList[RAGING][S_BODY_DOUBLE] = 5
SkillList[RAGING][S_MASTER_SWAP] = 0

--#####################
--##### Ward Kimi #####
--#####################

SkillList[WARD] = {}
SkillList[WARD][S_ILLUSION_OF_CLAWS] = 5
SkillList[WARD][S_ILLUSION_CRUSHER] = 5
SkillList[WARD][S_CHAOTIC_HEAL] = 5
SkillList[WARD][S_WARM_DEF] = 5
SkillList[WARD][S_BODY_DOUBLE] = 5
SkillList[WARD][S_MASTER_SWAP] = 0

--####################################

--SkillInfo[id] = {name,range,SP cost,Cast Time (fixed),Cast Time (variable),Delay,targetMode,Duration,reuse delay)
--SkillInfo[id][1] = name
--SkillInfo[id][2] = range
--SkillInfo[id][3] = sp cost
--SkillInfo[id][4] = Fixed cast (ms)
--SkillInfo[id][5] = Variable cast (ms)
--SkillInfo[id][6] = Delay (ms)
--SkillInfo[id][7] = target mode (0 = self targeted, 1 = enemy targeted, 2 = ground targeted) 
--SkillInfo[id][8] = Duration (ms)
--SkillInfo[id][9] = Reuse Delay (ms)

SkillInfo={}
SkillInfo[0] = {"No Skill",{0,0,0,0,0,0,0,0,0,0},{0,0,0,0,0,0,0,0,0,0}}
SkillInfo[S_ILLUSION_OF_CLAWS] = {
  "Illusion of Claws", --Skill Name
  {1,1,1,1,1}, -- Range
  {5,10,15,20,25}, -- SP Cost
  {100,100,100,100,100}, -- Fixed cast (ms)
  {0,0,0,0,0}, -- Variable cast (ms)
  {0,0,0,0,0}, -- Delay (ms)
  1, -- Target mode (0 = self targeted, 1 = enemy targeted, 2 = ground targeted) 
  {0,0,0,0,0}, -- Duration (ms)
  {0,0,0,0,0}, -- Reuse Delay (ms)
}

SkillInfo[S_ILLUSION_CRUSHER] = {
  "Illusion of Crusher", --Skill Name
  {1,1,1,1,1}, -- Range
  {50,75,100,125,150}, -- SP Cost
  {0,0,0,0,0}, -- Fixed cast (ms)
  {0,0,0,0,0}, -- Variable cast (ms)
  {0,0,0,0,0}, -- Delay (ms)
  1, -- Target mode (0 = self targeted, 1 = enemy targeted, 2 = ground targeted) 
  {0,0,0,0,0}, -- Duration (ms)
  {0,0,0,0,0}, -- Reuse Delay (ms)
}

SkillInfo[S_WARM_DEF] = {
  "Warm Defense", --[1] Skill Name
  {0,0,0,0,0}, --[2] Range
  {20,25,30,35,40}, --[3] SP Cost
  {0,0,0,0,0}, -- [4] Fixed cast (ms)
  {0,0,0,0,0}, -- [5] Variable cast (ms)
  {4100,4100,4100,4100,4100}, -- [6] Delay (ms)
  0, -- [7] Target mode (0 = self targeted, 1 = enemy targeted, 2 = ground targeted) 
  {2000,4000,6000,8000,10000}, -- [8] Duration (ms)
  {8350,8350,8350,8350,8350}, -- [9] Reuse Delay (ms)
}

SkillInfo[S_CHAOTIC_HEAL] = {
  "Chaotic Heal",
  {0,0,0,0,0},
  {50, 50, 50, 50, 50},
  {0,0,0,0,0},
  {0,0,0,0,0},
  {0,0,0,0,0},
  0,
  {0,0,0,0,0},
  {0,0,0,0,0}
}

SkillInfo[S_BODY_DOUBLE] = {
  "Body Double", --Skill Name
  {0,0,0,0,0}, -- Range
  {20,40,60,80,100}, -- SP Cost
  {0,0,0,0,0}, -- Fixed cast (ms)
  {0,0,0,0,0}, -- Variable cast (ms)
  {0,0,0,0,0}, -- Delay (ms)
  0, -- Target mode (0 = self targeted, 1 = enemy targeted, 2 = ground targeted) 
  {10000,20000,30000,40000,50000}, -- Duration (ms)
  {17500,17500,23000,30000,25000} -- Reuse Delay (ms)
}
SkillInfo[S_ILLUSION_OF_BREATH] = {
  "Illusion of Breath",  --Skill Name
  {7, 7, 7, 7, 7, 7, 7, 7, 7, 7}, -- Range
  {25, 30, 35, 40, 45, 50, 55, 60, 65, 70}, -- SP Cost
  {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, -- Fixed cast (ms)
  {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, -- Variable cast (ms)
  {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, -- Delay (ms)
  1, -- Target mode (0 = self targeted, 1 = enemy targeted, 2 = ground targeted) 
  {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, -- Duration (ms)
  {0, 0, 0, 0, 0, 0, 0, 0, 0, 0} -- Reuse Delay (ms)
}

SkillInfo[S_ILLUSION_OF_LIGHT] = {
  "Illusion of Light",  --Skill Name
  {7, 7, 7, 7, 7, 7, 7, 7, 7, 7}, -- Range
  {25, 30, 35, 40, 45, 50, 55, 60, 65, 70}, -- SP Cost
  {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, -- Fixed cast (ms)
  {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, -- Variable cast (ms)
  {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, -- Delay (ms)
  1, -- Target mode (0 = self targeted, 1 = enemy targeted, 2 = ground targeted) 
  {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, -- Duration (ms)
  {0, 0, 0, 0, 0, 0, 0, 0, 0, 0} -- Reuse Delay (ms)
}

SkillInfo[S_MASTER_SWAP] = {
  "Master Swap", --Skill Name
  {1,1,1,1,1}, -- Range
  {1,1,1,1,1}, -- SP Cost
  {0,0,0,0,0}, -- Fixed cast (ms)
  {0,0,0,0,0}, -- Variable cast (ms)
  {0,0,0,0,0}, -- Delay (ms)
  0, -- Target mode (0 = self targeted, 1 = enemy targeted, 2 = ground targeted) 
  {0,0,0,0,0}, -- Duration (ms)
  {60000,60000,60000,60000,60000} -- Reuse Delay (ms) - 60 second cooldown
}

--#######################################

--SkillAOEInfo[skill] = {AoE size (size on a side of the AoE - by level),Target Type (0=centered on enemy, 1=centered on caster, skill-id=special - handled in GetAOECount)), other fields for future use}
SkillAOEInfo={}
SkillAOEInfo[0] = {{0,0,0,0,0},0}
SkillAOEInfo[S_ILLUSION_OF_LIGHT] = {
  {3, 3, 3, 3, 3, 3, 3, 3, 3, 3},
  0
}
