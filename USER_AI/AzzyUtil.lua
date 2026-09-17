-------------------------------
-- This file is part of AzzyAI 1.55
-- If you want to use these functions in your own AI
-- it is reccomended that you use the seperately available
-- version, which does not utilize AAI-specific systems. 
-- Written by Dr. Azzy of iRO Chaos
-- Customized for the 'Return of Morroc' server by Nathan.
AUVersion="1.6"
-------------------------------
--[[
USAGE NOTES


HPPercent(id)
SPPercent(id)
Returns the current hp or sp of id as a percentage. This only works on things which the player can see the hp/sp of, obviously.

Closer(id,x,y)
Returns coordinants of a location 1 cell closer to actor (id) 

ClosestR(id1,id2,range)
Returns coordinants of the cell within <range> cells of <id2> that is closest to <id1>.
Intended for targeting of ranged skills.

BetterMoveToOwner(mercid,range)
Moves to the closest sell withing range cells of the owner. 

IsNotKS(id)
Returns 1 if attacking monster (id) would be a KS, 0 otherwise. 

IsFriend(id)
Returns 1 if (id) is the owner or a friend. 

@@@@@@@@@@@@@@@@@@@@@@@@@
@@@ Monster Functions @@@
@@@@@@@@@@@@@@@@@@@@@@@@@

GetMobCount(id,range,[id2])
Returns the number of monsters within range cells of the actor id, and the number of said monsters which are currently targeting id. If id2 is specified, monsters which are targeting id2 will also be included in the second value returned. 
Useful for deciding whether to use anti-mob skills.
ex:
local mobcount,aggrocount = GetMobCount(MyID,1,GetV(V_OWNER,MyID))

@@@@@@@@@@@@@@@@@@@@@@@
@@@ Skill Functions @@@
@@@@@@@@@@@@@@@@@@@@@@@

GetAtkSkill(id)
Returns a list containing skill id, skill level of the mercenary's single target offencive skill. Crash is not returned in this list, because the damage output spamming crash is lower than the damage output from using normal attacks. 

ex: 
local skill,level= GetAtkSkill(MyID)

GetMobSkill(merctype)
As GetAtkSkill() only for antimob skills

ex
local skill,level = GetMobSkill(MyID)

GetQuickenSkill(id)
Returns a list containing the ID of the Body Double skill

ex
local skill,level = GetQuickenSkill(MyID)

GetGuardSkill(id)
As GetQuickenSkill() only returns the ID of the Warm Defense skill.
--]]--


--#########################
--### Tactics Functions ###
--######################### 

function 	GetTact(t,m)
	local x
	if m==nil then
		TraceAI("GetTact: Error - nil target")
		logappend("AAI_ERROR","GetTact: Error - nil target")
	end
	if t==nil then
		TraceAI("GetTact: Error - nil tactic target:"..m)
		logappend("AAI_ERROR","request for tactic type nil target:"..m)
	end
	if (IsPlayer(m)==1) then
		if (PVPmode~=0) then
			TraceAI("GetTact: Returning pvp tactic")
			return GetPVPTact(t,m)
		else
			TraceAI("GetTact: not pvp mode returning 0"..t)
			return 0
		end
	end
	local e = 0
	if (IsHomun(MyID)==0) then 
		if (MobID[m]==nil) then
			e=GetClass(m)
		else
			e=MobID[m]
		end
	else
		e=GetV(V_HOMUNTYPE,m)
	end
	temp=GetMyTact(e)
	if (temp==nil) then
		if (e >= 1324 and e <= 1363) or (e >= 1938 and e <=1946) then
			temp=GetMyTact(13)
			TraceAI("GetTact: No tactic "..t.." for "..e.."actor: "..m.." but it's a treasure chest.")
		end
		if temp==nil then
			temp=GetMyTact(0)
			--TraceAI("GetTact: No tactic "..t.." for "..e.."actor: "..m.." using default instead")
		end
	end
	if (temp[t]==nil) then
		temp=GetMyTact(0)
		TraceAI("GetTact: Undefined tactic "..t.." for "..e.."actor: "..m.." using default instead")
	end
	x=temp[t]
	if (x==nil) then
		logappend("AAI_ERROR","Default tactic "..t.." is undefined - Please review and correct tactics file or restore default tactics file")
		return 0
	else
		if t==TACT_SP and x==-1 then
			x = AttackSkillReserveSP
		elseif t==TACT_CHASE then
			--TraceAI("TactChase called"..GetV(V_SP,MyID).." "..ChaseSPPauseSP.." "..GetTick().." "..math.max(LastMovedTime,LastSPTime).." "..(5000-ChaseSPPauseTime))
			if GetV(V_SP,MyID) < ChaseSPPauseSP and (GetTick() - math.max(LastMovedTime,LastSPTime)) > (5000-ChaseSPPauseTime) then 
				if ((ChaseSPPause==1 and x~=CHASE_ALWAYS) or x==CHASE_CLEVER) then
					TraceAI("Cleverchase activated: "..m.." ("..e..") ".." LastTick "..math.max(LastMovedTime,LastSPTime))
					return 1
				end
			end
			if x==CHASE_ALWAYS then
				return 0
			elseif x==CHASE_NEVER then
				return 1
			else
				return DoNotChase
			end
		end
		return x
	end
end

function GetMyTact(m)
	return MyTact[m]
end

function	GetPVPTact(t,m)
	local x
	local class
	local e = ENEMY
	if IsMonster(m)==0 then
		TraceAI("pvp tact lookup on ally")
		e= ALLY
	end
	if (MyPVPTact[m]~=nil) then
		e = m
	elseif (MyFriends[m]~=nil) then
		e = MyFriends[m]
	end
	if IsHomun(MyID)==1 then
		class=GetV(V_HOMUNTYPE,m)
	end
	if MyPVPTact[m]~=nil then
		x=MyPVPTact[m][t]
	elseif e~=ALLY and class~=nil then 
		if MyPVPTact[class] ~=nil then
			x=MyPVPTact[class][t]
		end
	elseif MyFriends[e]==nil then
		x=MyPVPTact[e][t]
	end
	if x==nil then
		x=MyPVPTact[0][t]
	end
	if (x==nil) then
		TraceAI("ERROR: Default PVP tactic "..t.." is undefined!")
		logappend("AAI_ERROR","Default PVP tactic "..t.." is undefined! Please correct your PVP tactics file")
		return 0
	else	
	TraceAI("pvp TL on P "..m.."IsMonster "..IsMonster(m).." type "..e.." for tactic "..t.." returning "..x)
		return x
	end
end

function	GetClass(m)
	if (m < MagicNumber) then
		return 10
	elseif (IsActive[m]==0 and AutoDetectPlant==1) then
		return 11
	else
		return 0
	end
end

function IsRescueTarget(id)
	local tactrescue=GetTact(TACT_RESCUE,id)
	local target=GetV(V_TARGET,id)
	local owner=GetV(V_OWNER,MyID)
	if target==owner then
		local hp = HPPercent(owner)
		if RescueOwnerLowHP > hp then
			TraceAI("RescueOwnerLowHP -> rescue owner from "..id)
			return 1
		elseif RescueOwnerLowHP < 0 and -1*RescueOwnerLowHP < hp then
			TraceAI("RescueOwnerLowHP -> rescue of owner canceled "..id)
			return 0
		end
	end
	if tactrescue==RESCUE_NEVER then
		return 0
	else	
		TraceAI("tactrescue not never "..tactrescue.." id "..id)
		if tactrescue==RESCUE_RETAINER then
			if MyFriends[target]==2 then
				return 1
			end
		elseif tactrescue==RESCUE_FRIEND then
			if MyFriends[target]==1 then
				return 1
			end
		elseif tactrescue==RESCUE_ALL then
			if IsFriendOrSelf(target)==1 and target ~=MyID then
				return 1
			end
		elseif tactrescue==RESCUE_OWNER then
			if target==GetV(V_OWNER,MyID) then
				return 1
			end
		elseif tactrescue==RESCUE_SELF then
			if target==MyID then
				return 1
			end
		end
	end
	return 0
end

function GetTargetClass(id)
-- 0 = no target, 1 = self 2= friend, -1 = other monster -2 = other player
	if id == MyID then
		return 1
	elseif id == 0 then
		return 0
	-- Server actor ID ranges differ; recognize the owner/friends first.
	elseif IsFriendOrSelf(id)==1 then
		return 2
	elseif (id > MagicNumber2) then
		if IsFriendOrSelf(id)==1 then
			return 2
		else
			return -2
		end
	elseif IsMonster(id)==1 then
		return -1
	else
		return 0
	end
end

--########################
--### Friend Functions ###
--########################


function IsFriend(id)
	if (id==GetV(V_OWNER,MyID)) then
		return 1
	elseif (id~= MyID) then
		friendclass=MyFriends[id]
		if (friendclass==FRIEND or friendclass==RETAINER) then
			return 1
		end
	end
	return 0
end

function IsPVPFriend(id)
	if (id==GetV(V_OWNER,MyID)) then
		return 1
	elseif (id~= MyID) then
		friendclass=MyFriends[id]
		if (friendclass==FRIEND or friendclass==RETAINER or friendclass==NEUTRAL or friendclass==ALLY or friendclass==NEUTRAL) then
			return 1
		end
	end
	return 0
end

function IsFriendOrSelf(id)
	if (id==GetV(V_OWNER,MyID) or id == MyID) then
		return 1
	else
		if (MyFriends[id]~=nil) then
			return 1	
		end
	end
	return 0
end

function IsPlayer(id)
	if (id<MagicNumber2) then
		return 1
	else
		return 0
	end
end

function UpdateFriends() 
	FriendsFile = io.open(ConfigPath.."A_Friends.lua", "w")
	if FriendsFile~=nil then
		FriendsFile:write (STRING_A_FRIENDS_HEAD)
		for k,v in pairs(MyFriends) do
			if (v==FRIEND or (v==PKFRIEND and PainkillerFriendsSave == 1)) then
				FriendsFile:write ("MyFriends["..k.."]="..v.." -- \n")
			end
		end
		FriendsFile:close()
	else
		TraceAI("Failed to open A_Friends.lua for writing!")
		logappend("AAI_ERROR","Failed to open A_Friends.lua for writing!")
	end
end

--########################
--### Monster functions###
--########################


function GetAggroCount(target)
	local aggrocount=0
	if target ==nil then
		for k,v in pairs(Targets) do
			if v[2] > 0 then -- it's aggressive
				distance = GetDistanceA(GetV(V_OWNER,MyID),k)
				if (distance <= GetMoveBounds()) then
					aggrocount=aggrocount+1*GetTact(TACT_WEIGHT,k)
				end
			end
		end
	else
		for k,v in pairs(Targets) do
			if GetV(V_TARGET,k)==target then
				aggrocount=aggrocount+1*GetTact(TACT_WEIGHT,k)
			end
		end
	end

	return aggrocount
end

function GetMobCount(skill,level,target,aggro)
	local mobcount=0
	if skill==0 or level==0 then 
		return 0
	end
	local aoeInfo = SkillAOEInfo[skill]
	if aoeInfo == nil or aoeInfo[1] == nil or aoeInfo[1][level] == nil or aoeInfo[1][level] <= 0 then
		return 0
	end
	local skillaoe=aoeInfo[1][level]
	local x,y=GetV(V_POSITION,MyID)
	if GetSkillInfo(skill,7)==0 then
		target=MyID
	end
	local range = aoeInfo[3] == "radius" and skillaoe or (skillaoe-1)/2
	local logstring=""
	for k,v in pairs(Targets) do
		local tbas=GetTact(TACT_BASIC,k)
		local dist = GetDistanceAR(target,k)
		logstring=logstring.."|"..k.."v[2]"..v[2].."t"..GetV(V_TARGET,k).."m"..GetV(V_MOTION,k).."aggro"..aggro
		if dist <= range and (v[2]>0 or aggro~=0) and tbas~=0 then
			mobcount=mobcount+1*GetTact(TACT_WEIGHT,k)
			logstring=logstring.."add:"..GetTact(TACT_WEIGHT,k).."new:"..mobcount
		end			
	end
	logappend("AAI_MOBCOUNT","Mobcount on well-behaved skill "..FormatSkill(skill,level).." range "..range.." logstring: "..logstring.." result: "..mobcount)
	return mobcount
end

function GetBestAoETarget(myid,skill,level)
	local mcount=0
	local result=-1
	for k,v in pairs(Targets) do
		if mcount < GetMobCount(skill,level,k,1) then
			result=k
			mcount=GetMobCount(skill,level,k,1)
		end
	end
	return result
end

function GetBestAoECoord(myid,skill,level)
	local mcount =0
	local resultx = -1
	local resulty = -1
	local range=GetSkillInfo(skill,2,level)
	local aoeInfo = SkillAOEInfo[skill]
    local aoesize = aoeInfo[3] == "radius" and aoeInfo[1][level] or (aoeInfo[1][level]-1)/2
	local myx
	local myy
	myx,myy=GetV(V_POSITION,myid)
	local ox,oy=GetV(V_POSITION,GetV(V_OWNER,MyID))
	grid={}
	for xx=ox-20,ox+20 do
		grid[xx]={}
		--for xx=oy-20,oy+20 do
		--	grid[xx][yy]=0
		--end
	end
	for k,v in pairs(Targets) do
		local tbas = GetTact(TACT_BASIC,k)
		x,y=GetV(V_POSITION,k)
		if x < ox+20 and x > ox-20 and y < oy+20 and y > oy-20 and tbas~=0 then 
			if grid[x][y]==nil then
				grid[x][y]=GetTact(TACT_WEIGHT,k)
			else
				grid[x][y]=grid[x][y]+GetTact(TACT_WEIGHT,k)
			end
		end
	end
	for xx=myx-range,myx+range do
		for yy=myy-range,myy+range do
			local thiscount=0
			for xxx=xx-aoesize,xx+aoesize do
				if grid[xxx]~=nil then
					for yyy=yy-aoesize,yy+aoesize do
						if grid[xxx][yyy] ~=nil then
							thiscount=thiscount+grid[xxx][yyy]
						end
					end
				end
			end
			if thiscount>mcount then
				mcount=thiscount
				resultx,resulty=xx,yy
			end
		end
	end
	return resultx,resulty
end

function GetBestBrandishTarget(myid)
	local mobs =0
	local maxmobs = 0
	local besttarget= -1
	for k,v in pairs(Targets) do
		if (GetDistanceA(myid,k) <=1) then --Is a target in range to brandish
			mobs = 0
			for kk,vv in pairs (Targets) do
				local tbas=GetTact(TACT_BASIC,kk)
				if tbas ~= 0 then
					if (GetDistanceA(myid,kk)<=1 and GetDistanceA(k,kk) <=1) then
						mobs=mobs+1
					elseif (GetDistanceA(myid,kk) ==2 and GetDistance3(myid,kk) < 2.5 and GetDistance3(k,kk) < 2.5)then -- sides, 2 cells to either side of target/merc
						mobs=mobs+1
					end
				end
			end
			if (mobs > maxmobs) then
				maxmobs = mobs
				besttarget = v
			end
		end
	end
	return besttarget
end

function GetBestFASTarget(myid)
	local options = GetFASOptions(myid)
	local mincount=0
	local target=-1
	for x,v in pairs(options) do
		for y,v2 in pairs(v) do
			--TraceAI("GetBestFAST "..x.." "..y.." "..v2)
			if v2~=0 then
				count=GetFASTargetCount(myid,x,y)
				if count > mincount then
					mincount = count
					target = v2
				end
			end
		end
	end
	return target,mincount			
end

function GetFASDirection(myid,target)
	local x,y=GetV(V_POSITION,myid)
	if GetDistanceA(myid,target) < 15 then
		local mx,my=GetV(V_POSITION,target)
		local dx=math.abs(x-mx)
		local dy=math.abs(y-my)
		if ((dx==0) or (dx==1 and dy>1) or (dx==2 and dy>6) or (dx==3 and dy>9) or (dx==4 and dy>11)) then 
			if my>=y then
				return 0,1 --n
			elseif my<y then
				return 0,-1 --s
			end
		elseif ((dy==0) or (dy==1 and dx>1) or (dy==2 and dx>6) or (dy==3 and dx>9) or (dy==4 and dx>11)) then
			if mx>x then
				return 1,0 --e
			elseif mx<x then
				return-1,0 --w
			end
		-- diagonals
		elseif mx>x then
			if my>=y then
				return 1,1 --ne
			else
				return 1,-1 --se
			end
		elseif my>=y then
			return -1,1 --nw
		else
			return -1,-1 --sw
		end
	end
end

function GetFASOptions(myid)
	local x,y=GetV(V_POSITION,myid)
	noption=0
	eoption=0
	soption=0
	woption=0
	neoption=0
	nwoption=0
	seoption=0
	swoption=0
	for k,v in pairs(Monsters) do
		TraceAI("FAS loop id: "..k)
		if GetDistanceA(myid,k) < 15 then
			mx,my=GetV(V_POSITION,k)
			dx=math.abs(x-mx)
			dy=math.abs(y-my)
			if ((dx==0) or (dx==1 and dy>1) or (dx==2 and dy>6) or (dx==3 and dy>9) or (dx==4 and dy>11)) then 
				if my>y then
					noption=k
				elseif my<y then
					soption=k
				end
			elseif ((dy==0) or (dy==1 and dx>1) or (dy==2 and dx>6) or (dy==3 and dx>9) or (dy==4 and dx>11)) then
				if mx>x then
					eoption=k
				elseif mx<x then
					woption=k
				end
			-- diagonals
			elseif mx>x then
				if my>=y then
					neoption=k
				else
					seoption=k
				end
			elseif my>=y then
				nwoption=k
			else
				swoption=k
			end
		end
		if (noption~=0 and soption~=0 and eoption~=0 and woption~=0 and neoption~=0 and seoption~=0 and swoption~=0 and nwoption~=0) then
			TraceAI("All Options Avail - break")
			break
		end
	end
	result={[-1]={[-1]=swoption,[0]=woption,[1]=nwoption},[0]={[-1]=soption,[0]=0,[1]=noption},[1]={[-1]=seoption,[0]=eoption,[1]=neoption}}
	TraceAI("Returning FAS Options: "..swoption.."-"..woption.."-"..nwoption.." "..soption.."--"..noption.." "..seoption.."-"..eoption.."-"..neoption)
	return result
end

function GetFASTargetCount(myid,directionx,directiony,aggro)
	local x,y=GetV(V_POSITION,myid)
	local s=directionx+directiony
	TraceAI("GFASTC: "..s)
	count=0
	if s==1 or s==-1 then -- straight
		if directiony==0 then --Horizontal
			ymin=y-1
			ymax=y+1
			if directionx==1 then --East
				xmin=x
				xmax=x+14
			else
				xmin=x-14
				xmax=x
			end
		else	--Vertical
			xmin=x-1
			xmax=x+1
			if directiony==1 then -- north
				ymin=y
				ymax=y+14
			else
				ymin=y-14
				ymax=y
			end
		end
		for k,v in pairs(Targets) do
			mx,my=GetV(V_POSITION,k)
			--logappend("AAI_mobcount","straight monster:"..xmin.."-"..xmax..","..ymin.."-"..ymax.." "..mx..","..my)
			if GetTact(TACT_BASIC,k)~=0 and mx>=xmin and xmax>=mx and my>=ymin and ymax>=my then
				count=count+1
			end
		end
		--logappend("AAI_mobcount","straight:"..xmin.."-"..xmax..","..ymin.."-"..ymax.." count"..count)
		return count
	else	--Diagonal
		count = 0
		for k,v in pairs(Targets) do
			if GetTact(TACT_BASIC,k)~=0 and IsNotKS(MyID,k)==1 then
				mx,my=GetV(V_POSITION,k)
				if directionx==1 then --east
					if (mx+1>=x and x+14>=mx) then -- has prayer of hitting
						if mx==x-1 then
							if my==y+directiony then
								count=count+1
							end
						elseif mx==x then
							if (my==y or my==y+directiony or my==y+2*directiony) then
								count=count+1
							end
						elseif mx==x+14 then
							if my==y+12*directiony then
								count=count+1
							end
						elseif mx==x+13 then
							if (my==y+11*directiony or my==y+10*directiony or my==y+12*directiony) then
								count=count+1
							end
						else	--not a special case
							if directiony==1 then
								miny=y-2+(mx-x)
								maxy=miny+5
							else
								maxy=y+2-(mx-x)
								miny=maxy-5
							end
							if my>=miny and maxy>=my then
								count=count+1
							end
						end
					end
				else --west
					if mx-1<=x and x-14<=mx then -- has prayer of hitting
						if mx==x+1 then
							if my==y+directiony then
								count=count+1
							end
						elseif mx==x then
							if (my==y or my==y+directiony or my==y+2*directiony) then
								count=count+1
							end
						elseif mx==x-14 then
							if my==y+12*directiony then
								count=count+1
							end
						elseif mx==x-13 then
							if (my==y+11*directiony or my==y+10*directiony or my==y+12*directiony) then
								count=count+1
							end
						else	--not a special case
							if directiony==1 then
								miny=y-2+(x-mx)
								maxy=miny+5
							else
								maxy=y+2-(x-mx)
								miny=maxy-5
							end
							if my>=miny and maxy>=my then
								count=count+1
							end
						end
					end
				end
			end
		end
		--logappend("AAI_mobcount","diagonal:"..count)
	end
	return count
end

function IsHiddenOnScreen(myid) --Currently disabled - assume there is never hidden on screen.
	return 1
end

function IsHomun(myid)
	if (GetV(V_HOMUNTYPE,myid)~=nil) then
		return 1
	end
	return 0

end

function IsNotKS(myid,target)
	--TraceAI("Checking for KS:"..target)
	local targettarget=GetV(V_TARGET,target)
	local motion=GetV(V_MOTION,target)
	local tactks=GetTact(TACT_KS,target)
	if (target==MyEnemy and BypassKSProtect==1) then --If owner has told homun to attack explicity, let it.
		return 1
	end
	if target==nil then
		TraceAI("IsNotKS")
	end
	if (IsPlayer(target)==1) then
		--TraceAI("PVP - not KS")
		return 1
	elseif (IsFriend(targettarget)==1 or targettarget==myid) then
		--TraceAI("Not KS - "..target.." fighting friend: "..targettarget)
		return 1
	elseif ((tactks==KS_POLITE or DoNotAttackMoving ==1) and motion==MOTION_MOVE) then
		return 0
	elseif (tactks==KS_ALWAYS) then
		--TraceAI("It's an FFA monster, not a KS")
		return 1
	elseif targettarget > 0 and IsMonster(targettarget)~=1 and Actors[targettarget]==1 then
		--TraceAI("Is KS - "..target.." attacking player "..targettarget.." motion "..motion)
		return 0
	else
		--TraceAI("Not Targeted - seeing if anyone is targeting it")
		local actors = GetActors()
		for i,v in ipairs(actors) do
			if (IsMonster(v)~=1 and IsFriendOrSelf(v)==0) then
				if (GetV(V_TARGET,v) == target and (v > 100000 or KSMercHomun ~=1)) then
					--TraceAI("Is KS - "..target.." is targeted by "..v)
					return 0
				end
			end
		end
	--TraceAI("Not KS - "..target.." is not targeted by any other player.")
	return 1
	end
end
function GetKSReason(myid,target)
	--TraceAI("Checking for KS:"..target)
	local targettarget=GetV(V_TARGET,target)
	local motion=GetV(V_MOTION,target)
	local tactks=GetTact(TACT_KS,target)
	if (target==MyEnemy and BypassKSProtect==1) then --If owner has told homun to attack explicity, let it.
		return "KS Protect bypassed"
	end
	if target==nil then
		TraceAI("IsNotKS")
	end
	if (IsPlayer(target)==1) then
		--TraceAI("PVP - not KS")
		return "PVP, not KS"
	elseif ((tactks==KS_POLITE or DoNotAttackMoving ==1) and motion==MOTION_MOVE) then
		return "KS polite aka DoNotAttackMoving"
	elseif (IsFriend(targettarget)==1 or targettarget==myid) then
		--TraceAI("Not KS - "..target.." fighting friend: "..targettarget)
		return "Not KS - enemy attacking "..targettarget
	elseif (tactks==KS_ALWAYS) then
		--TraceAI("It's an FFA monster, not a KS")
		return "FFA monster"
	elseif targettarget > 0 and IsMonster(targettarget)~=1 and Actors[targettarget]==1 then
		--TraceAI("Is KS - "..target.." attacking player "..targettarget.." motion "..motion)
		if MyFriends[targettarget]~=nil then
			return "KS - enemy is attacking "..targettarget.." MyFriends: "..MyFriends[targettarget]
		else
			return "KS - enemy is attacking "..targettarget
		end
	else
		--TraceAI("Not Targeted - seeing if anyone is targeting it")
		local actors = GetActors()
		for i,v in ipairs(actors) do
			if (IsMonster(v)~=1 and IsFriendOrSelf(v)==0) then
				if (GetV(V_TARGET,v) == target and (v > 100000 or KSMercHomun ~=1)) then
					--TraceAI("Is KS - "..target.." is targeted by "..v)
					if MyFriends[v]~=nil then
						return "KS - enemy attacked by "..v.." MyFriends: "..MyFriends[v]
					else
						return "KS - enemy attacked by "..v
					end
				end
			end
		end
	--TraceAI("Not KS - "..target.." is not targeted by any other player.")
	return "not KS, not targeted by anything"
	end
end
--########################
--### HP/SP % functions###
--########################

function HPPercent(id)
	local maxHP=GetV(V_MAXHP,id)
	local curHP=GetV(V_HP,id)
	percHP=100*curHP/maxHP
	return percHP
end

function SPPercent(id)
	local maxSP=GetV(V_MAXSP,id)
	local curSP=GetV(V_SP,id)
	percSP=100*curSP/maxSP
	return percSP
end

--########################
--### Spacial functions###
--########################
OldMove=Move

function Move(myid,x,y)
	if NavigationReady and NavigationReady() then
        local nx,ny=NavigationWaypoint(myid,x,y,0)
        if nx then return NavigationMove(myid,nx,ny) end
        return -- Never fall back to a move through a known wall.
    end
	local ox,oy = GetV(V_POSITION,myid)
	local dis = GetDistance(ox,oy,x,y)
	local dis2owner=GetDistanceAPR(GetV(V_OWNER,myid),x,y)
	local newx,newy=x,y
	if dis2owner > math.min(100,GetMoveBounds()) then 
		logappend("AAI_ERROR","Attempt to move to location "..x..","..y.." which is "..dis2owner.." cells from owner, call disregarded")
		return 
	elseif dis > 15  then
		--factor = 14/dis
		factor=math.min(14/dis,0.5+((math.random(3)-2)*0.1))
		if dis > 25 then 
			factor =math.min(14/dis,0.4+((math.random(3)-2)*0.1))
		end
		local dx,dy = x-ox,y-oy
		if math.random(2)==1 then
			dx=math.ceil(factor*dx)
		else 
			dx=math.floor(factor*dx)
		end
		if math.random(2)==1 then
			dy=math.ceil(factor*dy)
		else 
			dy=math.floor(factor*dy)
		end
		if _VERSION=="Lua 5.1" then 
			while TakenCells[(x+dx).."_"..(y+dy)] do
				factor=factor-0.08
				if math.random(2)==1 then
					dx=math.ceil(factor*(x-ox))
				else 
					dx=math.floor(factor*(x-ox))
				end
				if math.random(2)==1 then
					dy=math.ceil(factor*(y-oy))
				else 
					dy=math.floor(factor*(y-oy))
				end
			end
		end
		if x==MyDestX and y==MyDestY then
			newx=ox+dx
			newy=oy+dy
			MyDestX,MyDestY=newx,newy
			TraceAI("MOVE: Attempt to move more than 15 cells, destination + MyDest adjusted: "..x..","..y.." "..dis.." "..factor.."new: "..newx..","..newy)
		else
			newx=ox+dx
			newy=oy+dy
			TraceAI("MOVE: Attempt to move more than 15 cells, destination adjusted: "..x..","..y.." "..dis.." "..factor.."new: "..newx..","..newy)
		end
	end
	if (LagReduction and LagReduction ~=0) then
		modtwROMoveX,modtwROMoveY=x,y
		return
	else 
		return OldMove(myid,newx,newy)
	end
end


local function GetEffectiveKimiCastDelay(skill, level)
	local castDelayBase = (GetSkillInfo(skill, 4, level) or 0)
	local castDelayJitter = (GetSkillInfo(skill, 5, level) or 0)
	local isKimiSkill = KimiSkillMetadata and KimiSkillMetadata[tonumber(skill)] ~= nil
	-- In skill-only mode, keep spell timing to cast animation/rate only.
	-- Prevent extra user cooldown delay from stacking onto Kimi cast cycles.
	local configDelay = 0
	if UseSkillOnly ~= 1 or not isKimiSkill then
		configDelay = (AutoSkillDelay or 0)
	end
	return castDelayBase + castDelayJitter * 0.5 + configDelay
end

local function KimiCastAllowed(skill, level, myid)
	if not (KimiSkillMetadata and KimiSkillMetadata[skill]) then return true, level end
	local canCast, resolvedLevel, reason = KimiSkillCanCast(skill, level, myid)
	if not canCast then
		logappend("AAI_ERROR", "Kimi skill "..tostring(skill).." rejected: "..tostring(reason))
		return false, 0
	end
	return true, resolvedLevel
end

-- Direct wrapper calls do not pass through DoSkill, so they stamp their own
-- local timing after dispatch. DoSkill sets KimiSkillDispatching while it
-- calls the wrapper and remains the single stamper for normal AI casts.
local function StampDirectKimiSkill(skill, level)
	if not (KimiSkillMetadata and KimiSkillMetadata[skill]) then return end
	local fixed = GetSkillInfo(skill, 4, level) or 0
	local variable = GetSkillInfo(skill, 5, level) or 0
	local castDelay = GetEffectiveKimiCastDelay(skill, level)
	AutoSkillCooldown[skill] = GetTick() + GetKimiSkillReuseDelay(skill, level) + castDelay
end

OldSkillObject=SkillObject
function SkillObject(myid,lvl,skill,target)
	local allowed, resolvedLevel = KimiCastAllowed(skill, lvl, myid)
	if not allowed then
		return 0
	elseif skill==8041 or skill==8043 or skill==8020 or skill==8025 then
		logappend("AAI_ERROR","Attempted to use skill "..SkillInfo[skill][1].." improperly. Check for corrupt H_SkillInfo or badly behaved addon")
	else
		lvl = resolvedLevel
		if KimiSkillMetadata and KimiSkillMetadata[skill] then
			if GetKimiSkillTargetMode(skill) == 2 then
				logappend("AAI_ERROR", "Ground-target Kimi skill "..tostring(skill).." sent through SkillObject")
				return 0
			end
			target = GetKimiSkillTarget(skill, target, myid)
		end
		if (LagReduction and LagReduction ~=0) then
			modtwROSkillObjectID=skill
			modtwROSkillObjectLV=lvl
			modtwROSkillObjectTarg=target
		else
			local result = OldSkillObject(myid,lvl,skill,target)
			if result ~= 0 and not KimiSkillDispatching then StampDirectKimiSkill(skill, lvl) end
			return result
		end
		if not KimiSkillDispatching then StampDirectKimiSkill(skill, lvl) end
	end
end

OldAttack=Attack
function Attack(myid,target)
	if SuperPassive==1 then
		TraceAI("Notice: Attack() called while in SuperPassive. MyEnemy "..MyEnemy.." MyState "..MyState)
	end
	if UseSkillOnly == 1 then
		return 0
	end
	if (LagReduction and LagReduction ~=0) then
		modtwROAttackTarget=target
	else
		return OldAttack(myid,target)
	end
end

OldSkillGround=SkillGround
function SkillGround(myid,lvl,skill,x,y)
	local allowed, resolvedLevel = KimiCastAllowed(skill, lvl, myid)
	if not allowed then
		return 0
	end
	lvl = resolvedLevel
	if KimiSkillMetadata and KimiSkillMetadata[skill] and GetKimiSkillTargetMode(skill) ~= 2 then
		logappend("AAI_ERROR", "Object-target Kimi skill "..tostring(skill).." sent through SkillGround")
		return 0
	elseif (LagReduction and LagReduction ~=0) then
		modtwROSkillGroundX=x
		modtwROSkillGroundY=y
		modtwROSkillGroundID=skill
		modtwROSkillGroundLV=lvl
	else
		local result = OldSkillGround(myid,lvl,skill,x,y)
		if result ~= 0 and not KimiSkillDispatching then StampDirectKimiSkill(skill, lvl) end
		return result
	end
	if not KimiSkillDispatching then StampDirectKimiSkill(skill, lvl) end
end

function	GetDistance (x1,y1,x2,y2)
	return math.floor(math.sqrt((x1-x2)^2+(y1-y2)^2))
end

function	GetDistance2 (id1, id2)
	local x1, y1 = GetV (V_POSITION,id1)
	local x2, y2 = GetV (V_POSITION,id2)
	if (x1 == -1 or x2 == -1) then
		return -1
	end
	return GetDistance (x1,y1,x2,y2)
end


function	GetDistanceA (id1, id2)
	local x1, y1 = GetV (V_POSITION,id1)
	local x2, y2 = GetV (V_POSITION,id2)
	if (x1 == -1 or x2 == -1) then
		return 999
	end
	return GetDistance (x1,y1,x2,y2)
end

function	GetDistanceAP (id1, x2,y2)
	local x1, y1 = GetV (V_POSITION,id1)
	if (x1 == -1 or x2 == -1) then
		return 999
	end
	return GetDistance (x1,y1,x2,y2)
end

function	GetDistanceAPR (id1, x2,y2)
	local x1, y1 = GetV (V_POSITION,id1)
	if (x1 == -1 or x2 == -1) then
		return 999
	end
	local x=x1-x2
	local y=y1-y2
	return math.max(math.abs(x),math.abs(y))
end

function GetDistanceRect(id1,id2)
	local x1, y1 = GetV (V_POSITION,id1)
	local x2, y2 = GetV (V_POSITION,id2)
	if (x1 == -1 or x2 == -1) then
		return 999
	end
	local x=x1-x2
	local y=y1-y2
	return math.max(math.abs(x),math.abs(y))
end

function GetDistance3(id1,id2) 
	local x1, y1 = GetV (V_POSITION,id1)
	local x2, y2 = GetV (V_POSITION,id2)
	if (x1 == -1 or x2 == -1) then
		return -1
	end
	return math.sqrt((x1-x2)^2+(y1-y2)^2)
end

function GetAggroDist()
	if OwnerPosX[10]==OwnerPosX[1] and OwnerPosY[10]==OwnerPosY[1] then
		return StationaryAggroDist
	else
		return MobileAggroDist
	end
end

function GetMoveBounds()
	if OwnerPosX[10]==OwnerPosX[1] and OwnerPosY[10]==OwnerPosY[1] then
		return StationaryMoveBounds
	else
		return MobileMoveBounds
	end
end
function IsToRight(id1,id2)
	local x1,y1=GetV(V_POSITION,id1)
	local x2,y2=GetV(V_POSITION,id2)
	if (x1+1==x2 and y1==y2) then
		return 1
	else
		return 0
	end
end

function	GetDistanceP (x1,y1,x2,y2)
	if (x1 == -1 or x2 == -1) then
		return 999
	end
	return math.floor(math.sqrt((x1-x2)^2+(y1-y2)^2))
end

function	GetDistanceA (id1, id2)
	local x1, y1 = GetV (V_POSITION,id1)
	local x2, y2 = GetV (V_POSITION,id2)
	if (x1 == -1 or x2 == -1) then
		return 999
	end
	return GetDistance (x1,y1,x2,y2)
end

function	GetDistanceAP (id1, x2,y2)
	local x1, y1 = GetV (V_POSITION,id1)
	if (x1 == -1 or x2 == -1) then
		return 999
	end
	return GetDistance (x1,y1,x2,y2)
end

function	GetDistancePR (x1,y1,x2,y2)
	if (x1 == -1 or x2 == -1) then
		return 999
	end
	return math.max(math.abs(x1-x2),math.abs(y1-y2))
end

function GetDistanceAR(id1,id2)
	local x1, y1 = GetV (V_POSITION,id1)
	local x2, y2 = GetV (V_POSITION,id2)
	if (x1 == -1 or x2 == -1) then
		return 999
	end
	return math.max(math.abs(x1-x2),math.abs(y1-y2))
end

function	GetDistanceAPR (id1, x2,y2)
	local x1, y1 = GetV (V_POSITION,id1)
	if (x1 == -1 or x2 == -1) then
		return 999
	end
	return math.max(math.abs(x1-x2),math.abs(y1-y2))
end






function BetterMoveToOwner(myid,range)
    if NavigationReady and NavigationReady() then
        NavigationReturnToOwner(myid,range)
        return
    end
	if (range==nil) then
		range=1
	end
	local x,y = GetV(V_POSITION,myid)
	local ox,oy = GetV(V_POSITION,GetV(V_OWNER,myid))
	local destx,desty=0,0
	if (x > ox+range) then
		destx=ox+range
	elseif (x < ox - range) then
		destx=ox-range
	else
		destx=x
	end
	if (y > oy+range) then
		desty=oy+range
	elseif (y < oy - range) then
		desty=oy-range
	else
		desty=y
	end
	MyDestX,MyDestY=destx,desty
	Move(myid,MyDestX,MyDestY)
	return
end
function BetterMoveToOwnerXY(myid,range)
	if (range==nil) then
		range=1
	end
	local x,y = GetV(V_POSITION,myid)
	local ox,oy = GetV(V_POSITION,GetV(V_OWNER,myid))
	local destx,desty=0,0
	if (x > ox+range) then
		destx=ox+range
	elseif (x < ox - range) then
		destx=ox-range
	else
		destx=x
	end
	if (y > oy+range) then
		desty=oy+range
	elseif (y < oy - range) then
		desty=oy-range
	else
		desty=y
	end
	return destx,desty
end
function BetterMoveToLoc(myid,range,ox,oy)
	if (range==nil) then
		range=1
	end
	local x,y = GetV(V_POSITION,myid)
	--local ox,oy = GetV(V_POSITION,GetV(V_OWNER,myid))
	local destx,desty=0,0
	if (x > ox+range) then
		destx=ox+range
	elseif (x < ox - range) then
		destx=ox-range
	else
		destx=x
	end
	if (y > oy+range) then
		desty=oy+range
	elseif (y < oy - range) then
		desty=oy-range
	else
		desty=y
	end
	MyDestX,MyDestY=destx,desty
	Move(myid,MyDestX,MyDestY)
	return
end
function BetterMoveToLocXY(myid,range,ox,oy)
	if (range==nil) then
		range=1
	end
	local x,y = GetV(V_POSITION,myid)
	--local ox,oy = GetV(V_POSITION,GetV(V_OWNER,myid))
	local destx,desty=0,0
	if (x > ox+range) then
		destx=ox+range
	elseif (x < ox - range) then
		destx=ox-range
	else
		destx=x
	end
	if (y > oy+range) then
		desty=oy+range
	elseif (y < oy - range) then
		desty=oy-range
	else
		desty=y
	end
	
	return destx,desty
end

function	GetOwnerPosition (id)
	return GetV (V_POSITION,GetV(V_OWNER,id))
end

function	GetDistanceFromOwner (id)
	local x1, y1 = GetOwnerPosition (id)
	local x2, y2 = GetV (V_POSITION,id)
	if (x1 == -1 or x2 == -1) then
		return -1
	end
	return GetDistance (x1,y1,x2,y2)
end

function	IsOutOfSight (id1,id2)
	local x1,y1 = GetV (V_POSITION,id1)
	local x2,y2 = GetV (V_POSITION,id2)
	if (x1 == -1 or x2 == -1 or id1==-1 or id2==-1) then
		return true
	end
	local d = GetDistancePR(x1,y1,x2,y2)
	if d > 20 then
		return true
	else
		return false
	end
end


function IsInAttackSight (myid,target,skill,level)
	if (skill==nil) then
		skill=MySkill
		level=MySkillLevel
	elseif (level==nil) then
		logappend("AAI_ERROR","IsInAttackSight called with nil level but skill is not nil. State:"..STATE_NAME[MyState].." skill "..skill)
	end
	local x1,y1 = GetV (V_POSITION,myid)
	local x2,y2 = GetV (V_POSITION,target)
	if (x1 == -1 or x2 == -1) then
		return false
	end
	local d		= GetDistance (x1,y1,x2,y2)
	local a     = AttackRange(myid,skill,level)
	if a >= d then
		return true;
	else
		return false;
	end
end



function AttackRange(myid,skill,level)
	if myid == MyID and (skill == nil or skill == 0) and GetComboRangeSkill then
		local comboSkill,comboLevel = GetComboRangeSkill(myid)
		if comboSkill ~= nil then skill,level = comboSkill,comboLevel end
	end
	if (skill==nil or level== nil) then
		if skill==nil or level==nil then
			logappend("AAI_ERROR","AttackRange called with invalid arguments State:"..STATE_NAME[MyState].." MySkill "..MySkill.." MySkillLevel"..MySkillLevel.." skill: "..formatval(skill).." level: "..formatval(level))
		end
		skill=MySkill
		level=MySkillLevel
	end
	local meta = KimiSkillMetadata and KimiSkillMetadata[skill]
    if meta and meta.targetMode == 0 and meta.aoeRadius and meta.aoeRadius[level] then
        return meta.aoeRadius[level]
    end
    local a     = 0
	if (skill == 0) then
		a     = GetV(V_ATTACKRANGE,myid)
	else
		a = GetSkillInfo(skill,2,level)
		if a==nil then
			TraceAI("AttackRange() cannot get range for skill "..formatval(skill).." level "..formatval(level))
		end
	end
	if level == nil then
		logappend("AAI_ERROR","AttackRange called with nil level, skill = "..skill.." "..STATE_NAME[MyState])
	end
	TraceAI("Attack Sight - skill:"..skill.."level: "..level.." Range:"..a) 
	return a
end

function Closer(id,ox,oy)
	local x,y=GetV(V_POSITION,id)
	local newx,newy=0,0
	if (ox==x) then
		newx=x
	elseif (ox > x) then
		newx=ox-1
	else
		newx=ox+1
	end
	if (oy==y) then
		newy=y
	elseif (oy > y) then
		newy=oy-1
	else
		newy=oy+1
	end
	return newx,newy
end




function GetStandPoint(myid,target,skill,level,alt)
	local r=AttackRange(myid,skill,level)
	if GetDistanceA(myid,target) <= r then
		logappend("AAI_ERROR","GetStandPoint called while within range")
		return GetV(V_POSITION,MyID)
	end
	local tx,ty=GetV(V_POSITION,target)
	local x,y=-1,-1
	if _VERSION=="Lua 5.1" and myid==MyID and (alt==nil or alt==0) then
		x,y=GetV(V_POSITION_APPLY_SKILLATTACKRANGE,target,skill,level)
	end
	if skill==0 or skill==nil or (tx==x and ty==y) or (tx < 1 or ty < 1) or math.floor(GetDistanceAP(target,x,y)) > r then -- Use normal closest call, since V_POSITION_APPLY_SKILLATTACKRANGE doesn't work for non-skills, or has given us clearly bogus standpoint for skill.
		x,y=Closest(myid,tx,ty,r,alt)
	end
	return AdjustStandPoint(x,y,tx,ty,r,alt)
end

function AdjustStandPoint (x,y,tx,ty,r,alt)
	if _VERSION=="Lua 5.1" then
		-- Now we need to make sure x and y are not on occupied cells, because of the changes to the client
		local bounds=GetMoveBounds()
		local logstring=""
		if (alt==nil) then
			logstring=logstring.."x,y"..x..","..y.." tx,ty"..tx..","..ty.." r "..r.."alt nil bounds "..bounds.." | "
		else 
			logstring=logstring.."x,y"..x..","..y.." tx,ty"..tx..","..ty.." r "..r.."alt "..alt.." bounds "..bounds.." | "
		end
		local i=0
		
		while (TakenCells[x.."_"..y] or GetDistanceAPR(GetV(V_OWNER,MyID),x,y) > bounds) do
			i=i+1
			if (TakenCells[x.."_"..y]) then
				logstring=logstring.."i="..i.."tc="..TakenCells[x.."_"..y].."d ".. GetDistanceAPR(GetV(V_OWNER,MyID),x,y).." :"
			else 
				logstring=logstring.."i="..i.."tc= nil d ".. GetDistanceAPR(GetV(V_OWNER,MyID),x,y).." :"
			end
			if r==1 then
				if alt==nil or alt==0 then
					x,y=AdjustCW(x,y,tx,ty)
					logstring=logstring.."CW "..x..","..y
				elseif alt==1 then
					x,y=AdjustCCW(x,y,tx,ty)
					logstring=logstring.."CCW "..x..","..y
				else --alt=2
					x,y=AdjustOpp(x,y,tx,ty)
					logstring=logstring.."OPP "..x..","..y
					alt=0
				end
				if i > 7 then
					logappend("AAI_CLOSEST","AdjustStandPoint() FAILED  logstring="..logstring)

		TraceAI("AdjustStandPoint() FAILED logstring="..logstring)
					return -1,-1
				end
			else
				if alt==nil or alt==0 then
					x,y=AdjustCWr(x,y,tx,ty,r)
					logstring=logstring.."CWr "..x..","..y
				elseif alt==1 then
					x,y=AdjustCCWr(x,y,tx,ty,r)
					logstring=logstring.."CCWr "..x..","..y
				else --alt=2
					x,y=x*-1,y*-1
					logstring=logstring.."Opp R "..x..","..y
					alt=0
				end
				if i > 7 then
					logappend("AAI_CLOSEST","AdjustStandPoint() FAILED  logstring="..logstring)
					return -1,-1
				end
			end
		end
		logappend("AAI_CLOSEST","AdjustStandPoint() returned "..x.." , "..y.." logstring="..logstring)
	end
	return x,y
end

function Closest(myid,ox,oy,range,alt)
	local x,y=GetV(V_POSITION,myid)
	local dx,dy=ox-x,oy-y
	local newx,newy=0,0
	local logstring="x,y"..x..","..y.." ox,oy"..ox..","..oy
	if range > 1 then
		logstring=logstring.."range > 1 "
		dist=math.sqrt(dx^2+dy^2)
		if (dist < range) then
			newx,newy=x,y
			logstring=logstring.."dist < range new"..newx..","..newy.." "
		else
			factor=range/dist
			xoff=(dx-absceil(dx*factor))
			yoff=(dy-absceil(dy*factor))
			newx,newy=x+xoff,y+yoff
			logstring=logstring.."dist > range new" ..newx..","..newy.." factor:"..factor.." "
		end
		-- This routine tries to find an adjacent cell to the closest cell that will be close enough - to address unwalkable cells near where retainer wants to walk (think: pebbles in thors)
		
		if (alt ~= nil) then
			logstring=logstring.."alt not nil"..alt.." "
			if (alt==1) then
				if (x > ox) then
					newx=newx-1
				elseif (x < ox) then
					newx=newx+1
				else
					alt=2
				end
			end
			if (alt==2) then
				if (y > oy) then
					newy=newy-1
				elseif (y < oy) then
					newy=newy+1
				else
					alt=3
				end
			end
		
			--if (alt==4) then -- This seems to cause more problems than it solves, set to 4 to prevent from running
			--	if (x > ox) then
			--		newx=newx-1
			--		newy=newy+1
			--	elseif (x < ox) then
			--		newx=newx+1
			--		newy=newy+1
			--	elseif (y > oy) then
			--		newy=newy-1
			--		newx=newx+1
			--	else 	
			--		newy=newy+1
			--		newx=newx+1
			--	end
			--end
			logstring=logstring.."end of alt sequence new"..newx..","..newy.." alt "..alt.." "
		end
		--xxx,yyy=GetV(V_POSITION,target)
		--TraceAI("alt is: "..GetDistance(xxx,yyy,newx,newy).." "..xxx..","..yyy.." "..newx..","..newy.."MyPos: "..x..","..y) 	
	else -- simplified, better closest point for melee. 
		logstring=logstring.."range = 1 d"..dx..","..dy.." o"..ox..","..oy.." "
		if dx > 0 then
			newx=ox-1
		elseif dx < 0 then
			newx=ox+1
		else -- dx = 0
			newx=x
		end
		if dy > 0 then
			newy=oy-1
		elseif dy < 0 then
			newy=oy+1
		else -- dy = 0
			newy=y
		end
		logstring=logstring.."new"..newx..","..newy.." "
		if alt~=nil then
			if alt==1 then
				newx,newy=AdjustCW(newx,newy,ox,oy)
				logstring=logstring.."adjust cw"..newx..","..newy.." "
			else
				newx,newy=AdjustCCW(newx,newy,ox,oy)
				logstring=logstring.."adjust ccw"..newx..","..newy.." "
			end
		end
	end
	local logalt="nil"
	if alt~=nil then
		logalt=alt
	end
	if newx<=0 or newy<=0 then 
		logappend("AAI_ERROR","Closest() Failure!"..newx.." , "..newy.." alt ="..logalt.." logstring="..logstring)
	end
	logappend("AAI_CLOSEST","Closest() returned "..newx.." , "..newy.." alt ="..logalt.." logstring="..logstring)
	return newx,newy
end

function AdjustCW(x,y,ox,oy)
	local newx,newy,dy,dx
	if x==ox then
		if y==oy then --stacked!? Unstack!
			newx,newy=ox+1,oy
		else
			dy=y-oy
			newx=x+absunit(dy)
			newy=y
		end
	elseif y==oy then
		dx=x-ox
		newy=y-absunit(dx)
		newx=x
	elseif y>oy then
		if x>ox then
			newy=y-1
			newx=x
		else
			newy=y
			newx=x+1
		end
	else -- y<oy
		if x>ox then
			newx=x-1
			newy=y
		else
			newx=x
			newy=y+1
		end
	end
	return newx,newy
end
function AdjustCCW(x,y,ox,oy)
	local newx,newy,dx,dy
	if x==ox then
		if y==oy then --stacked!? Unstack!
			newx,newy=ox-1,oy
		else
			dy=y-oy
			newx=x-absunit(dy)
			newy=y
		end
	elseif y==oy then
		dx=x-ox
		newy=y+absunit(dx)
		newx=x
	elseif y>oy then
		if x>ox then
			newy=y
			newx=x-1
		else
			newy=y-1
			newx=x
		end
	else -- y<oy
		if x>ox then
			newx=x
			newy=y+1
		else
			newx=x+1
			newy=y
		end
	end
	return newx,newy
end

function AdjustCWr(x,y,ox,oy,r)
	if r==1 then
		return AdjustCW(x,y,ox,oy)
	end
	local newx,newy,dy,dx
	dx,dy=x-ox,y-oy
	if math.abs(dx) == math.abs(dy)  then --we're on a diagonal, this could be a problem, so let's get off it!
		if dx < 0 then
			if dy < 0 then
				newx=x+1
				newy=y
			else
				newx=x
				newy=y-1
			end
		elseif dx > 0 then
			if dy < 0 then
				newy=y+1
				newx=x
			else
				newx=x-1
				newy=y
			end
		else --stacked, which should NEVER HAPPEN but has somehow happened anyway
			newx=x+1
			newy=y+1
			logappend("AAI_ERROR","AdjustCWr called with x,y=ox,oy - this can't happen!")
		end
	else
		if math.abs(dx) < math.abs(dy) then --north or south quadrant
			if dy < 0 then -- south quadrant
				newx=x+1
				newy=y+1
			else --north
				newx=x-1
				newy=y-1
			end
		else --east or west quadrant
			if dx < 0 then --west
				newy=y-1
				newx=x+1
			else
				newx=x-1
				newy=y+1
			end
		end
	end

	if (newx-ox==0 and math.abs(newy-oy)==1) or (newy-oy==0 and math.abs(newx-ox)==1) then
		newx,newy=newx*2,newy*2
	end
	return newx,newy
end

function AdjustCCWr(x,y,ox,oy,r)
	if r==1 then
		return AdjustCCW(x,y,ox,oy)
	end
	local newx,newy,dy,dx
	dx,dy=x-ox,y-oy
	if math.abs(dx) == math.abs(dy)  then --we're on a diagonal, this could be a problem, so let's get off it!
		if dx < 0 then
			if dy < 0 then
				newx=x
				newy=y+1
			else
				newx=x+1
				newy=y
			end
		elseif dx > 0 then
			if dy < 0 then
				newy=y
				newx=x-1
			else
				newx=x
				newy=y-1
			end
		else --stacked, which should NEVER HAPPEN but has somehow happened anyway
			newx=x-1
			newy=y-1
			logappend("AAI_ERROR","AdjustCWr called with x,y=ox,oy - this can't happen!")
		end
	else
		if math.abs(dx) < math.abs(dy) then --north or south quadrant
			if dy < 0 then -- south quadrant
				newx=x-1
				newy=y+1
			else --north
				newx=x+1
				newy=y-1
			end
		else --east or west quadrant
			if dx < 0 then --west
				newy=y+1
				newx=x+1
			else
				newx=x-1
				newy=y-1
			end
		end
	end
	if (newx-ox==0 and math.abs(newy-oy)==1) or (newy-oy==0 and math.abs(newx-ox)==1) then
		newx,newy=newx*2,newy*2
	end
	return newx,newy
end

function AdjustOpp(x,y,ox,oy)
	dx=ox-x
	dy=oy-y
	return x+2*dx,y+2*dy
end
function ClosestR(myid,target,range,alt)
	local ox,oy=GetV(V_POSITION,target)
	return Closest(myid,ox,oy,range,alt)
end

--###############################   ##   ##
--### LUA 5.1 feature support ###   ##   ##
--###############################   ##   ##


function DiagonalDist(distance)
	return math.floor(math.sqrt(distance*distance))
end

function GetDanceCell(x,y,enemy)
	local enemyx, enemyy = GetV(V_POSITION, enemy)
	if IsInAttackSight(MyID, enemy) == false then
		logappend("AAI_ERROR","GetDanceCell() called when enemy out of range!" )
		x, y = Closest(MyID, enemyx, enemyy, 1)
		return x, y
	end

	local step = DanceAttackStep or 1
	if step < 1 then step = 1 end

	-- Pick a random angle around the enemy and step out by the configured distance
	local angle = math.random() * (2 * math.pi)
	local dx = math.floor(enemyx + step * math.cos(angle) + 0.5)
	local dy = math.floor(enemyy + step * math.sin(angle) + 0.5)
	return dx, dy
end

--[[ --disabled for a simpler dance alog, because you can't dance any ranged attacks at the moment, so optimize for range=1

function GetDanceCell(x,y,enemy)
	distance = GetDistance2(MyID,enemy)
	ex,ey=GetV(V_POSITION,enemy)
	s=(((math.random(2)-1)*2)-1)
	t=(((math.random(2)-1)*2)-1)
	if (GetDistance(x-s,y+t,ex,ey) == distance) then
		return x-s,y+t
	elseif (GetDistance(x,y+t,ex,ey) == distance) then
		return x,y+t
	elseif (GetDistance(x+s,y+t,ex,ey) == distance) then
		return x+s,y+t
	elseif (GetDistance(x-s,y,ex,ey) == distance) then
		return x-s,y
	elseif (GetDistance(x+s,y,ex,ey) == distance) then
		return x+s,y
	elseif (GetDistance(x-s,y-t,ex,ey) == distance) then
		return x-s,y-t
	elseif (GetDistance(x,y-t,ex,ey) == distance) then
		return x,y-t
	else
		return x+s,y-t
	end
	
end
]]--]]
--#########################
--### GetSkill functions###
--#########################

-- Shared with AI_main.lua healing; must be visible across Lua files.
function GetReadyKimiSkill(skillid, myid)
	if not (KimiSkillMetadata and KimiSkillMetadata[skillid]) then return 0, 0 end
	local level = GetKimiSkillLevel(skillid, nil, myid)
	if level <= 0 then return 0, 0 end
	if AutoSkillCooldown[skillid] and GetTick() < AutoSkillCooldown[skillid] then return 0, 0 end
	return skillid, level
end

function GetSAtkSkill(myid)
	-- Kimi utility/self skills are selected through buff/heal paths; no
	-- separate sniping slot is needed.
	return 0, 0
end

function GetAtkSkill(myid)
	if IsHomun(myid) ~= 1 then return 0, 0 end
	local kimiType = GetKimiType(myid)
	if onlyAOE == 1 and kimiType ~= AGILE then
		return GetMobSkill(myid)
	end
	if kimiType == OCCULT then
		local skill, level = GetReadyKimiSkill(S_ILLUSION_OF_BREATH, myid)
		if skill ~= 0 then return skill, level end
		return GetReadyKimiSkill(S_ILLUSION_OF_LIGHT, myid)
	elseif kimiType == AGILE then
		local skill, level = GetReadyKimiSkill(S_ILLUSION_OF_CLAWS, myid)
		if skill ~= 0 then return skill, level end
		return GetReadyKimiSkill(S_MIRAGE_ASSAULT, myid)
	elseif kimiType == RAGING then
		return GetReadyKimiSkill(S_ILLUSION_CRUSHER, myid)
	end
	return 0, 0
end

function GetPushbackSkill(myid)
	if (IsHomun(myid)==1) then
		return 0,0
	else
		for i,v in ipairs(PushSkillList) do
			level = SkillList[MercType][v]
			if level ~= nil then
				skill=v
				return skill,level
			end
		end
	end
	return 0,0
end

function GetMobSkill(myid)
	if IsHomun(myid) ~= 1 then return 0, 0 end
	local kimiType = GetKimiType(myid)
	if kimiType == WARD then
		return GetReadyKimiSkill(S_TAUNT, myid)
	elseif kimiType == OCCULT then
		return GetReadyKimiSkill(S_ILLUSION_OF_LIGHT, myid)
	elseif kimiType == RAGING then
		return GetReadyKimiSkill(S_BLOOD_SWEEP, myid)
	end
	return 0, 0
end


function GetQuickenSkill(myid)
	-- Body Double has its own HP-based scheduler.
	return GetReadyKimiSkill(S_QUICK_DEFENSE, myid)
end

function GetGuardSkill(myid)
	local level = 0
	local skill = 0

	if (IsHomun(myid) == 1) then
		if GetKimiType(myid) == WARD then
			-- Bastion Renewal is scheduled by DoHealingTasks and its HP threshold.
			return GetReadyKimiSkill(S_WARD_DOMAIN, myid)
		end
		return 0, 0
	else
		for i,v in ipairs(GuardSkillList) do
			level = SkillList[MercType][v]

			if (level ~= nil) then
				skill = v

				return skill, level
			end
		end
	end

	return 0, 0
end

function GetHealingSkill(myid)
	local level = 0
	local skill = 0

	if (IsHomun(myid) == 1) then
		if GetKimiType(myid) == OCCULT then
			return GetReadyKimiSkill(S_CHAOTIC_HEAL, myid)
		end
	end

	return skill, level
end

function GetSnipeSkill(myid)
	return GetAtkSkill(myid)
end

function GetTargetedSkills(myid)
	s,l=GetAtkSkill(myid)
	Mainatk={MAIN_ATK,s,l}
	s,l=GetSAtkSkill(myid)
	Satk={S_ATK,s,l}
	s,l=GetMobSkill(myid)
	Mobatk={MOB_ATK,s,l}
	result={
		Mainatk,
		Satk,
		Mobatk
	}
	return result
end

function GetSkillInfo(skill, info, level)
	if (skill ~= nil and info ~= nil) then
		if SkillInfo[skill] ~= nil then
			local skillInfoResult = SkillInfo[skill][info]

			if skillInfoResult ~= nil then
				if (level == nil and type(skillInfoResult) == "table") then
					logappend("AAI_ERROR","Attempted to call GetSkillInfo() with only 2 arguments "..skill.." "..info)

					if (type(skillInfoResult) ~= "table" and skillInfoResult ~= nil) then
						return skillInfoResult
					else
						return -1
					end
				elseif type(skillInfoResult) ~= "table" then
					return skillInfoResult
				elseif SkillInfo[skill][info][level] ~=nil then
					return SkillInfo[skill][info][level]
				else
					logappend("AAI_ERROR","GetSkillInfo(): No skill info type "..info.." found for this level "..FormatSkill(skill, level))

					if (info == 2) then
						local skillAttackRangeResult = 0

						if (_VERSION=="Lua 5.1") then
							skillAttackRangeResult = GetV(V_SKILLATTACKRANGE_LEVEL, MyID, skill, level)
						else
							skillAttackRangeResult = GetV(V_SKILLATTACKRANGE, MyID, skill)
						end
						logappend("AAI_ERROR","GetSkillInfo(): Range for unknown skill requested, returning builtin value"..skillAttackRangeResult.." "..skill)

						return skillAttackRangeResult
					elseif SkillInfo[skill][info][1]~= nil then
						return SkillInfo[skill][info][1]
					else
						return 0
					end
				end
			else
				logappend("AAI_ERROR","GetSkillInfo(): No skill info type "..info.." found for this skill "..FormatSkill(skill, level).." called by ")

				if (info == 2) then
					logappend("AAI_ERROR","GetSkillInfo(): Range for unknown skill requested, returning builtin value"..GetV(V_SKILLATTACKRANGE, MyID, skill))

					return GetV(V_SKILLATTACKRANGE, MyID, skill)
				elseif (info == 7) then
					return 1
				else
					return 0
				end
			end
		else
			logappend("AAI_ERROR","GetSkillInfo(): No skill info found for this skill "..FormatSkill(skill,level).." info type requested "..info)

			if (info == 2) then
				logappend("AAI_ERROR","GetSkillInfo(): Range for unknown skill requested, returning builtin value"..GetV(V_SKILLATTACKRANGE,MyID,skill))

				return GetV(V_SKILLATTACKRANGE,MyID,skill)
			elseif (info == 7) then
				return 1
			else
				return 0
			end
		end
	else
		if (skill == nil) then
			skill="nil"
		end
		if (level == nil) then
			level="nil"
		end
		if (info == nil) then
			info="nil"
		end
		logappend("AAI_ERROR","GetSkillInfo(): Invalid arguments - skill="..skill.." level="..level.." info="..info)

		return 0
	end
end
--#######################
--### Other Functions ###
--#######################

function KiteOK(myid)
	mertype=GetV(V_MERTYPE,myid)
	if (mertype==nil) then
		homuntype=modulo(GetV(V_HOMUNTYPE,myid),4)
		if ((homuntype==0 or homuntype==3 )and DoNotChase==1) then
			return 1
		else
			return 0
		end
	elseif (mertype < 11) then
		return 1
	else
		return 0
	end
end

function DoSkill(skill, level, target, mode, targx, targy)
	TraceAI("doskill called skill:"..tostring(skill).." level:"..tostring(level).." target:"..tostring(target))

	if skill==0 or level==0 or skill==nil or level==nil then
		logappend("AAI_ERROR","doskill called skill:"..tostring(skill).." level:"..tostring(level).." target:"..tostring(target).." mode"..tostring(mode).." state "..tostring(STATE_NAME[MyState]).." pstate "..tostring(MyPState))
		return 0
	end

	-- Central Kimi gate protects every caller, including legacy selectors and
	-- command handlers. The client owns learned/loyalty validation.
	local kimiMeta = KimiSkillMetadata and KimiSkillMetadata[skill]
	if kimiMeta then
		local canCast, resolvedLevel, reason = KimiSkillCanCast(skill, level, MyID)
		if not canCast then
			TraceAI("DoSkill: Kimi skill "..tostring(skill).." rejected: "..tostring(reason))
			return 0
		end
		level = resolvedLevel
		target = GetKimiSkillTarget(skill, target, MyID)
	end
	targetMode = kimiMeta and GetKimiSkillTargetMode(skill) or GetSkillInfo(skill, 7)
	local issued

	KimiSkillDispatching = true
	if (targetMode == 0) then
		issued = SkillObject(MyID, level, skill, MyID)
	elseif (targetMode == 1) then
		issued = SkillObject(MyID, level, skill, target or MyID)
	elseif (targetMode == 2) then
		if (targx == nil) then
			positionX, positionY = GetV(V_POSITION,target or MyID)
			issued = SkillGround(MyID, level, skill, positionX, positionY)
		else
			issued = SkillGround(MyID, level, skill, targx, targy)
		end
	end
	KimiSkillDispatching = false
	if issued == 0 then return 0 end
	if (mode ~= nil) then
		if (mode > 0) then
			CastSkillMode = mode
			CastSkill = skill
			CastSkillLevel = level
			CastSkillTime = GetTick()
			CastSkillState = 0
			logappend("AAI_SKILLFAIL", "Mode set "..FormatMode(mode).." "..FormatSkill(skill, level))
		else --mode is negative, call the plugin mode handler. 
			DoSkillHandleMode(skill,level,target,mode,targx,targy)
			logappend("AAI_SKILLFAIL", "Mode set "..FormatMode(mode).." "..FormatSkill(skill, level))
		end
	end
	local timeTick = GetTick();
	local fixedCastSkill = GetSkillInfo(skill, 4, level)
	local variableCastSkill = GetSkillInfo(skill, 5, level)
	local skillDelay = GetSkillInfo(skill, 6, level)
	local skillReuseDelay = kimiMeta and GetKimiSkillReuseDelay(skill, level) or GetSkillInfo(skill, 9, level)
	delay = GetEffectiveKimiCastDelay(skill, level)
	AutoSkillCastTimeout = delay + timeTick

	if (AutoSkillCooldown[skill] ~= nil) then
		AutoSkillCooldown[skill] = timeTick + skillReuseDelay + delay
	end
	delay = delay + skillDelay
	AutoSkillTimeout = timeTick + delay

	if (AutoSkillCooldown[skill] ~= nil) then
		TraceAI("DoSkill: "..FormatSkill(skill, level).." target:"..tostring(target).." mode:"..tostring(targetMode).." delay "..delay.." cooldown: "..AutoSkillCooldown[skill]-GetTick())
	else
		TraceAI("DoSkill: "..FormatSkill(skill, level).." target:"..tostring(target).." mode:"..tostring(targetMode).." delay "..delay)
	end
	TraceAI("DoSkill: "..FormatSkill(skill, level).." target:"..tostring(target).." mode:"..tostring(targetMode).." delay "..delay)
	local cooldownRemaining = AutoSkillCooldown[skill] and (AutoSkillCooldown[skill]-GetTick()) or 0
	logappend("AAI_SKILLFAIL", "DoSkill: "..FormatSkill(skill, level)..", target: "..tostring(target)..", mode: "..tostring(targetMode)..", delay: "..delay.. ", cooldown: "..cooldownRemaining)

	return 1
end

function modtwroSend()
	if modtwROSkillGroundID~=0 then
		logappend("AAI_Lag","Calling skillground function")
		OldSkillGround(MyID,modtwROSkillGroundLV,modtwROSkillGroundID,modtwROSkillGroundX,modtwROSkillGroundY)
		LagReductionCD=LagReduction
	elseif modtwROSkillObjectID~=0 then
		logappend("AAI_Lag","Calling SkillObject function")
		OldSkillObject(MyID,modtwROSkillObjectLV,modtwROSkillObjectID,modtwROSkillObjectTarg)
		LagReductionCD=LagReduction
	else
		if modtwRODidMove==0 then
			if modtwROMoveX~=0 then
				logappend("AAI_Lag","Calling Move function, didmove=0")
				if NavigationReady and NavigationReady() then Move(MyID,modtwROMoveX,modtwROMoveY) else OldMove(MyID,modtwROMoveX,modtwROMoveY) end
				LagReductionCD=LagReduction
				modtwRODidMove=1
				modtwRODidAttack=0
			end
		else
			modtwRODidMove=0
		end
		if modtwRODidAttack==0 then
			if modtwROAttackTarget~=0 then
				logappend("AAI_Lag","Calling attack function, didattack=0")
				OldAttack(MyID,modtwROAttackTarget)
				LagReductionCD=LagReduction
				modtwRODidAttack=1
			end
		else
			modtwRODidAttack=0
		end
	end
	modtwROSkillGroundLV,modtwROSkillGroundID,modtwROSkillGroundX,modtwROSkillGroundY,modtwROSkillObjectLV,modtwROSkillObjectID,modtwROSkillObjectTarg,modtwROAttackTarget=0,0,0,0,0,0,0,0
	modtwROAttackTarget,modtwROMoveX,modtwROMoveY=0,0,0
end








-- I SHOULDNT HAVE TO CODE THIS!
function modulo(a,b)
	return a-math.floor(a/b)*b
end
-- OR THIS!
function len(listin)
	local length=0
	for k,v in pairs(listin) do
		length = length + 1
	end
	return length
end

function absceil(x)
	if x>0 then
		return math.ceil(x)
	else
		return math.floor(x)
	end
end

function absunit(x)
	if x==0 then
		return 0
	elseif x > 0 then
		return 1
	else -- x < 0
		return -1
	end
end


-- Logging functions:

function FormatSkill(skill,level)
	if skill==nil then
		return "nil skill"
	end
	local output
	local name
	if SkillInfo[skill]==nil then
		name=skill
	elseif SkillInfo[skill][1]==nil then
		name=skill
	else
		name=SkillInfo[skill][1]
	end
	output = name.." ("..skill..")"
	if level == nil then
		return output
	else 
		return output.." level "..level
	end
end

function FormatMode(mode)
	if (mode == nil) then
		return "nil mode"
	end
	local output
	local name

	if (MODE_NAME[mode] == nil) then
		name = mode
	else
		name = MODE_NAME[mode]
	end
	output = name.." ("..mode..")"

	return output
end

function FormatMotion(motion)
	if motion==0 then
		return "Standing ("..motion..")"
	elseif motion==1 then
		return "Moving ("..motion..")"
	elseif motion==2 or motion==9 then 
		return "Attacking ("..motion..")"
	elseif motion==3 then 
		return "Dead ("..motion..")"
	elseif motion==4 then 
		return "Flinching  ("..motion..")"
	elseif motion==5 then
		return "Bending over ("..motion..")"
	elseif motion==6 then
		return "Sitting ("..motion..")"
	elseif motion==7 then 
		return "Using skill ("..motion..")"
	elseif motion==8 then
		return "Casting ("..motion..")"
	else
		return motion
	end
end

function formatval (val)
	if val == nil then
		return "nil"
	elseif type(val)=="string" then
		return "'"..val.."'"
	elseif type(val)=="function" then
		return "function"
	end
	return val
end

function logappend (filename,message)
	if EnableDebugLogging ~= 1 then return end
	if LogEnable[filename]==1 then
		TraceAI("Logging "..filename.." - "..message)
		outfile = io.open(ConfigPath.."ErrorLog/"..filename..".log", "a")
		if not outfile then return end
		outfile:seek("end")
		if filename=="AAI_SKILLFAIL" then
			outfile:write(os.date("%c").." ("..GetTick()..") "..TypeString..STATE_NAME[MyState].."\t"..message.."\n")
		else
			outfile:write(os.date("%c").." "..TypeString..STATE_NAME[MyState].."\t"..message.."\n")
		end
		outfile:close()
	end
end

function formatmypos(steps)
	if steps>10 then
		logappend("AAI_ERROR","Invalid number of steps called for formatmypos() "..steps)
		steps=10
	elseif steps <1 then
		logappend("AAI_ERROR","Invalid number of steps called for formatmypos() "..steps)
		steps=1
	end
	local outstring=""
	for v=1,steps,1 do
		outstring=outstring.."("..MyPosX[v]..","..MyPosY[v]..")"
		if v~=step then
			outstring=outstring..","
		end
	end
	return outstring
end

function formatpos(x,y)
	return x..","..y
end
--------------------------------------------
-- List utility
--------------------------------------------
List = {}

function List.new ()
	return { first = 0, last = -1}
end

function List.pushleft (list, value)
	local first = list.first-1
	list.first  = first
	list[first] = value;
end

function List.pushright (list, value)
	local last = list.last + 1
	list.last = last
	list[last] = value
end

function List.popleft (list)
	local first = list.first
	if first > list.last then 
		return nil
	end
	local value = list[first]
	list[first] = nil         -- to allow garbage collection
	list.first = first+1
	return value
end

function List.popright (list)
	local last = list.last
	if list.first > last then
		return nil
	end
	local value = list[last]
	list[last] = nil
	list.last = last-1
	return value 
end

function List.clear (list)
	for i,v in ipairs(list) do
		list[i] = nil
	end
--[[
	if List.size(list) == 0 then
		return
	end
	local first = list.first
	local last  = list.last
	for i=first, last do
		list[i] = nil
	end
--]]
	list.first = 0
	list.last = -1
end

function List.size (list)
	local size = list.last - list.first + 1
	return size
end
