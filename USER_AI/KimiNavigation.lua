-- Static terrain navigation. The user must select the current map explicitly.
local selected, terrain, route, nextSearch = nil, nil, nil, 0
local blocked = {}
local failures = {}
local pendingSearch=nil
local function key(x,y) return x..","..y end
function NavigationReady()
    local name = NavigationMap or ""
    if selected ~= name then
        selected,terrain,route,nextSearch,blocked=name,nil,nil,0,{}
        failures={}
        pendingSearch=nil
        if name ~= "" and string.find(name,"^[%w_@%-]+$") then
            local f=io.open("./AI_sakray/USER_AI/NavigationData.txt","r")
            if f then
                for line in f:lines() do
                    local _,_,map,w,h,data=string.find(line,"^([^|]+)|(%d+)|(%d+)|([0-9a-f]+)$")
                    if map==name then
                        w,h=tonumber(w),tonumber(h)
                        if string.len(data)==math.ceil(w*h/2) then terrain={w=w,h=h,data=data} end
                        break
                    end
                end
                f:close()
            end
            TraceAI("[NAVIGATION] "..name..(terrain and " loaded" or " unavailable; using normal movement"))
        end
    end
    return terrain ~= nil
end
local function cell(x,y)
    if not terrain or x<0 or y<0 or x>=terrain.w or y>=terrain.h then return 0 end
    local i=y*terrain.w+x
    local n=tonumber(string.sub(terrain.data,math.floor(i/2)+1,math.floor(i/2)+1),16)
    if math.mod(i,2)==1 then return math.floor(n/4) end
    return math.mod(n,4)
end
function NavigationWalkable(x,y)
    return not NavigationReady() or math.mod(cell(x,y),2)==1
end
function NavigationStep(x,y,nx,ny,ignoreOccupant)
    if not NavigationReady() then return true end
    if not NavigationWalkable(nx,ny) or (not ignoreOccupant and TakenCells and TakenCells[nx.."_"..ny]) then return false end
    local untilTick=blocked[key(nx,ny)]
    if untilTick and GetTick()<untilTick then return false end
    if x~=nx and y~=ny then
        return NavigationWalkable(nx,y) and NavigationWalkable(x,ny)
    end
    return true
end
function NavigationSight(x,y,tx,ty)
    if not NavigationReady() then return true end
    local dx,dy=tx-x,ty-y
    local steps=math.max(math.abs(dx),math.abs(dy))
    if steps==0 then return true end
    local px,py=x,y
    for i=1,steps do
        local nx,ny=math.floor(x+dx*i/steps+0.5),math.floor(y+dy*i/steps+0.5)
        if cell(nx,ny)<2 or (nx~=px and ny~=py and (cell(nx,py)<2 or cell(px,ny)<2)) then return false end
        px,py=nx,ny
    end
    return true
end
-- Check every crossed tile before smoothing a route segment.
function NavigationClearWalk(x,y,tx,ty,ignoreEndOccupant)
    local steps=math.max(math.abs(tx-x),math.abs(ty-y))
    if steps==0 then return true end
    local px,py=x,y
    for i=1,steps do
        local nx,ny=math.floor(x+(tx-x)*i/steps+0.5),math.floor(y+(ty-y)*i/steps+0.5)
        if not NavigationStep(px,py,nx,ny,ignoreEndOccupant and i==steps) then return false end
        px,py=nx,ny
    end
    return true
end
local lastMove=nil
function NavigationMove(myid,x,y)
    local px,py=GetV(V_POSITION,myid)
    local now=GetTick()
    if lastMove and lastMove.id==myid and lastMove.x==x and lastMove.y==y then
        if px~=lastMove.px or py~=lastMove.py then
            lastMove.px,lastMove.py,lastMove.tick=px,py,now
            return
        end
        if now-lastMove.tick<750 then return end
    end
    lastMove={id=myid,x=x,y=y,px=px,py=py,tick=now}
    return OldMove(myid,x,y)
end
local function RouteWaypoint(x,y,now)
    if route.x~=x or route.y~=y then route.tick=now;route.x,route.y=x,y end
    -- Retain the current segment endpoint while moving, avoiding command churn.
    local point=route.points[route.index]
    if point and point.x==x and point.y==y then route.index=route.index+1;point=route.points[route.index];route.endpoint=false end
    if point and route.endpoint and NavigationClearWalk(x,y,point.x,point.y) then
        if now-route.tick<1500 then return point.x,point.y end
        blocked[key(point.x,point.y)]=now+3000
        return nil
    end
    local best=nil
    for i=route.index,table.getn(route.points) do
        local p=route.points[i]
        if math.max(math.abs(p.x-x),math.abs(p.y-y))>8 or not NavigationClearWalk(x,y,p.x,p.y) then break end
        best=i
    end
    if best then
        route.index=best;route.endpoint=true
        point=route.points[best]
        return point.x,point.y
    end
    return nil
end
-- Searches within the owner boundary; return recovery also includes the current position.
function NavigationWaypoint(myid,tx,ty,radius,returning)
    if not NavigationReady() then return nil end
    local x,y=GetV(V_POSITION,myid)
    if not NavigationWalkable(x,y) then return nil end -- wrong map or transient position
    local ox,oy=GetV(V_POSITION,GetV(V_OWNER,myid))
    local bounds=math.min(100,GetMoveBounds())
    -- If the owner moved beyond the leash, the return route must include Kimi's
    -- starting cell. This does not expand combat targeting or its configured leash.
    if returning then
        bounds=math.max(terrain.w,terrain.h)
        -- Finish a batched return search even if the owner moves during planning.
        -- The next segment can then replan toward the latest owner position.
        if pendingSearch and pendingSearch.returning and pendingSearch.x==x and pendingSearch.y==y then
            tx,ty=pendingSearch.tx,pendingSearch.ty
            ox,oy=tx,ty
        end
    end
    local now=GetTick()
    radius=radius or 0
    if radius==0 and math.max(math.abs(tx-x),math.abs(ty-y))==1
        and math.max(math.abs(tx-ox),math.abs(ty-oy))<=bounds and NavigationStep(x,y,tx,ty) then
        return tx,ty -- Adjacent kite steps need no search or retry delay.
    end
    if radius==0 and not NavigationWalkable(tx,ty) then return nil end
    local signature=key(tx,ty)..":"..radius..":"..key(ox,oy)..":"..bounds
    if route and route.signature==signature then
        local nx,ny=RouteWaypoint(x,y,now)
        if nx then return nx,ny end
        route=nil
    elseif route then route=nil end
    local searchKey=signature..":"..key(x,y)
    if failures[searchKey] and now<failures[searchKey] then return nil end
    if not pendingSearch or pendingSearch.key~=searchKey then
    -- A* prioritizes progress toward the destination instead of flooding the leash.
    local function estimate(px,py) return math.max(0,math.max(math.abs(px-tx),math.abs(py-ty))-radius) end
    local queue={{x=x,y=y,g=0}}
    local heap={1}
    local heapSize=1
    queue[1].f=estimate(x,y)
    local costs={[key(x,y)]=0}
    local function push(index)
        heapSize=heapSize+1
        local i=heapSize
        while i>1 do
            local parent=math.floor(i/2)
            if queue[heap[parent]].f<=queue[index].f then break end
            heap[i]=heap[parent];i=parent
        end
        heap[i]=index
    end
    local function pop()
        local index=heap[1]
        local last=heap[heapSize]
        heap[heapSize]=nil
        heapSize=heapSize-1
        if heapSize>0 then
            local i=1
            while i*2<=heapSize do
                local child=i*2
                if child<heapSize and queue[heap[child+1]].f<queue[heap[child]].f then child=child+1 end
                if queue[last].f<=queue[heap[child]].f then break end
                heap[i]=heap[child];i=child
            end
            heap[i]=last
        end
        return index
    end
    local finish=nil
    local expanded=0
    local function advance()
    local work=0
    while heapSize>0 and expanded<20000 and work<150 do
        work=work+1
        local head=pop()
        local p=queue[head]
        if p.g==costs[key(p.x,p.y)] then
            expanded=expanded+1
            if estimate(p.x,p.y)==0 and ((returning and NavigationClearWalk(p.x,p.y,tx,ty,true)) or (not returning and NavigationSight(p.x,p.y,tx,ty))) then finish=head;break end
            for dx=-1,1 do for dy=-1,1 do
                local nx,ny=p.x+dx,p.y+dy
                local k=key(nx,ny)
                local g=p.g+1
                if (not costs[k] or g<costs[k]) and math.max(math.abs(nx-ox),math.abs(ny-oy))<=bounds and NavigationStep(p.x,p.y,nx,ny) then
                    costs[k]=g
                    local index=table.getn(queue)+1
                    queue[index]={x=nx,y=ny,parent=head,g=g,f=g+estimate(nx,ny)}
                    push(index)
                end
            end end
        end
    end
    return finish~=nil or heapSize==0 or expanded>=20000,finish,queue
    end
    pendingSearch={key=searchKey,advance=advance,returning=returning,x=x,y=y,tx=tx,ty=ty}
    end
    local complete,finish,queue=pendingSearch.advance()
    if not complete then return nil,true end
    pendingSearch=nil
    if not finish then
        -- Keep independent failures cached; alternating callers must not erase one another.
        for k,expiry in pairs(failures) do if expiry<=now then failures[k]=nil end end
        failures[searchKey]=now+1000
        return nil
    end
    failures[searchKey]=nil
    local points={}
    while queue[finish].parent do
        table.insert(points,1,queue[finish]);finish=queue[finish].parent
    end
    route={signature=signature,points=points,index=1,tick=now,x=x,y=y}
    if points[1] then return RouteWaypoint(x,y,now) end
    return nil
end
function NavigationChase(myid,target)
    if not NavigationReady() or not target or target==0 or IsOutOfSight(myid,target)
        or GetV(V_MOTION,target)==MOTION_DEAD or IsNotKS(myid,target)==0
        or GetTact(TACT_CHASE,target)==1 or GetTact(TACT_BASIC,target)<=0 or DoNotChase==1 then return false end
    if GetV(V_MOTION,myid)==MOTION_CASTING or GetTick()<(AutoSkillCastTimeout or 0) then return false end
    local x,y=GetV(V_POSITION,myid)
    local tx,ty=GetV(V_POSITION,target)
    local skill,level=GetComboRangeSkill(myid)
    if skill==nil and UseAttackSkill==1 then skill,level=GetAtkSkill(myid) end
    local range=AttackRange(myid,skill or 0,level or 1) or 1
    -- Ordinary clear approaches retain the existing skill/combo range logic.
    if NavigationSight(x,y,tx,ty) then return false end
    local ox,oy=GetV(V_POSITION,GetV(V_OWNER,myid))
    if math.max(math.abs(tx-ox),math.abs(ty-oy))>math.min(100,GetMoveBounds()) then return false end
    local nx,ny=NavigationWaypoint(myid,tx,ty,math.max(1,range-1))
    if nx==nil and ny==true then return true end
    if nx then
        NavigationMove(myid,nx,ny)
        ChaseGiveUpCount=0
        return true
    end
    return false
end

-- Choose any reachable stand-off tile, rather than one geometric tile that may be a wall.
function NavigationReturnToOwner(myid,range)
    if not NavigationReady() then return false end
    local x,y=GetV(V_POSITION,myid)
    local ox,oy=GetV(V_POSITION,GetV(V_OWNER,myid))
    if x<0 or y<0 or ox<0 or oy<0 then return false end
    range=math.max(1,range or 1)
    if math.max(math.abs(x-ox),math.abs(y-oy))<=range and NavigationClearWalk(x,y,ox,oy,true) then return false end
    if GetV(V_MOTION,myid)==MOTION_CASTING or GetTick()<(AutoSkillCastTimeout or 0) then return true end
    local nx,ny=NavigationWaypoint(myid,ox,oy,range,true)
    if nx then
        MyDestX,MyDestY=nx,ny
        NavigationMove(myid,nx,ny)
        FollowTryCount=0
    end
    return true
end
local NativeMoveToOwner=MoveToOwner
function MoveToOwner(myid)
    if NavigationReady() then NavigationReturnToOwner(myid,1);return end
    return NativeMoveToOwner(myid)
end
