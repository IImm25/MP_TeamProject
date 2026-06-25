#author of this gmpl model: Leonie Skala


/**
* Sets (to be able to iterate)
*/

set TASKS;
set PEOPLE;
set QUALIS; # qualifications
set TOOLS;
set PLACES;

param amountBoats;
set BOATS := 1..amountBoats;

param harbor symbolic, default 'w_1';


/**
* loading parameters (from data file)
*/
param maxWorkingHours;
param duration{TASKS};
param hasQuali{PEOPLE,QUALIS};
param requiredQualis{TASKS,QUALIS};
param requiredTools{TASKS,TOOLS};
param stockTools{TOOLS};

# new parameters for the extended model
param drivingSpeed; # in km/h
param taskPrio{TASKS}; # priority of the task (for the objective function)
param taskLocation{TASKS} symbolic in PLACES; # location of the task
param distance{PLACES, PLACES}; # distance matrix in km

# -1 = free, >= 0 = task is locked to that boat number
param fixedBoat{TASKS} integer, default -1;
# start time of fixed task in the committed sequence (-1 = not fixed)
param fixedStartTime{TASKS} default -1;
# -1 = free, >= 0 = person is locked to that boat number
param fixedPerson{PEOPLE} integer, default -1;
# how many of each tool are locked to each boat
param fixedToolAmount{BOATS, TOOLS} integer, default 0;

/**
* define decision variables
*/

# 1 when task is on this boat, else 0
var taskOnBoat{BOATS,TASKS} binary;

# specifies if boat is used (ensures that boats are filled from the front)
var boatUsage{BOATS} binary;

# specifies if person is on that boat or not
var personOnBoat{BOATS,PEOPLE} binary;

# specifies amount of a tool per boat
var toolOnBoat{BOATS,TOOLS} integer >= 0;

# 1 if task ta1 is directly followed by task ta2 on boat b
var after{BOATS, TASKS, TASKS} binary;

/*helper*/

# travel time between two places in hours (avoids repeating the division everywhere)
param travelTime{p1 in PLACES, p2 in PLACES} :=
    distance[p1, p2] / drivingSpeed;

# 1 if task is the first task on boat b (no predecessor)
var isFirst{BOATS, TASKS} binary;

# 1 if task is the last task on boat b (no successor)
var isLast{BOATS, TASKS} binary;

# start time of each task on each boat
var startTime{BOATS, TASKS} >= 0;

# stores startTime of the last task per boat (for TimeLimit)
var lastTaskStart{BOATS} >= 0;

# travel time between consecutive tasks (for gantt output)
var travelBetween{BOATS, TASKS, TASKS} >= 0;

# travel time from last task to harbor (for gantt output)
var travelToHarbor{BOATS, TASKS} >= 0;


/**
* all constrains
*/

/*duration relevante and general constrains*/

# --- time ---

# maximal working hours per used boat (if no fixed tasks are on this boat)
s.t. TimeLimit{b in BOATS}:
    lastTaskStart[b]
    + sum{ta in TASKS} (duration[ta] * isLast[b, ta])
    + sum{ta in TASKS} (travelTime[taskLocation[ta], harbor] * isLast[b, ta])
    <= maxWorkingHours * boatUsage[b];


# --- task assignment ---

# complete each task at most one time (or not at all)
s.t. DoneAtMostOnce{ta in TASKS}:
		sum{b in BOATS} (taskOnBoat[b,ta]) <= 1;

# at least one task must be done (plannen on a boat)
s.t. AtLeastOneTaskGlobal:
		sum{b in BOATS, ta in TASKS} (taskOnBoat[b,ta]) >= 1;
		
# use boat in ascending order (breaking the symmetry)
s.t. Order{b in BOATS: b > 1}:
		boatUsage[b] <= sum{ta in TASKS} (taskOnBoat[b-1,ta]);
		
s.t. UsageRequiresTask{b in BOATS: b > 1}:
		boatUsage[b] <= sum{ta in TASKS} (taskOnBoat[b,ta]);


/*people relevante constrains*/

# the amount of people with a certain qualification on board must be at least equal to the most demanding task to be carried out on that boat.
s.t. QualiCheck{b in BOATS, ta in TASKS, q in QUALIS: requiredQualis[ta,q] >= 1}:
		sum{p in PEOPLE} (hasQuali[p,q] * personOnBoat[b,p]) >= (requiredQualis[ta,q] * taskOnBoat[b,ta]);

# each person can only be on one boat at a time
s.t. PersonOneBoat{p in PEOPLE}:
		sum{b in BOATS} personOnBoat[b,p] <= 1;
	
	
/*tool relevante constrains*/

# The amount of tools on boad must be at least equal to the most demanding task to be carried out on that boat.
s.t. ToolAvailable{b in BOATS, ta in TASKS, t in TOOLS: requiredTools[ta,t] > 0}:
		toolOnBoat[b,t] >= requiredTools[ta,t] * taskOnBoat[b,ta];
	
# total stock across all boats
s.t. GlobalToolStock{t in TOOLS}:
    sum{b in BOATS} toolOnBoat[b,t] <= stockTools[t];

/*additional constrains for the extended model*/

# --- sequencing ---

# after only possible if both tasks are on boat b
s.t. AfterOnlyIfAssigned1{b in BOATS, ta1 in TASKS, ta2 in TASKS: ta1 <> ta2}:
    after[b, ta1, ta2] <= taskOnBoat[b, ta1];

s.t. AfterOnlyIfAssigned2{b in BOATS, ta1 in TASKS, ta2 in TASKS: ta1 <> ta2}:
    after[b, ta1, ta2] <= taskOnBoat[b, ta2];

# each task has at most one successor on a boat
s.t. OneSuccessor{b in BOATS, ta1 in TASKS}:
    sum{ta2 in TASKS: ta2 <> ta1} after[b, ta1, ta2] <= taskOnBoat[b, ta1];

# each task has at most one predecessor on a boat
s.t. OnePredecessor{b in BOATS, ta2 in TASKS}:
    sum{ta1 in TASKS: ta1 <> ta2} after[b, ta1, ta2] <= taskOnBoat[b, ta2];

# every task on a boat must either have a successor OR be the last task
s.t. ChainComplete{b in BOATS, ta in TASKS}:
    sum{ta2 in TASKS: ta2 <> ta} after[b, ta, ta2] + isLast[b, ta]
    = taskOnBoat[b, ta];

# every task on a boat must either have a predecessor OR be the first task
s.t. ChainCompleteFirst{b in BOATS, ta in TASKS}:
    sum{ta2 in TASKS: ta2 <> ta} after[b, ta2, ta] + isFirst[b, ta]
    = taskOnBoat[b, ta];

# exactly one last task per used boat
/*s.t. OneLastTask{b in BOATS}:
    sum{ta in TASKS} isLast[b, ta] = boatUsage[b];*/

# exactly one first task per used boat
s.t. OneFirstTask{b in BOATS}:
    sum{ta in TASKS} isFirst[b, ta] = boatUsage[b];

# --- isFirst/isLast helpers (for time constraints) ---

# isFirst = 1 only if task is on boat AND has no predecessor (upper bound and lower bound)
s.t. DefIsFirstUB{b in BOATS, ta in TASKS}:
    isFirst[b, ta] <= taskOnBoat[b, ta] - sum{ta2 in TASKS: ta2 <> ta} after[b, ta2, ta];

s.t. DefIsFirstLB{b in BOATS, ta in TASKS}:
    isFirst[b, ta] >= taskOnBoat[b, ta] 
                    - sum{ta2 in TASKS: ta2 <> ta} after[b, ta2, ta];

# isLast = 1 only if task is on boat AND has no successor
s.t. DefIsLastUB{b in BOATS, ta in TASKS}:
    isLast[b, ta] <= taskOnBoat[b, ta] - sum{ta2 in TASKS: ta2 <> ta} after[b, ta, ta2];

s.t. DefIsLastLB{b in BOATS, ta in TASKS}:
    isLast[b, ta] >= taskOnBoat[b, ta] 
                   - sum{ta2 in TASKS: ta2 <> ta} after[b, ta, ta2];


# --- start times ---

# first task starts after harbor travel
s.t. StartFirst{b in BOATS, ta in TASKS}:
    startTime[b, ta] >= travelTime[harbor, taskLocation[ta]] * isFirst[b, ta];

# successor starts after predecessor finishes + travel
s.t. StartAfter{b in BOATS, ta1 in TASKS, ta2 in TASKS: ta1 <> ta2}:
    startTime[b, ta2] >= startTime[b, ta1]
                        + duration[ta1]
                        + travelTime[taskLocation[ta1], taskLocation[ta2]]
                        - (1 - after[b, ta1, ta2]) * 2 * maxWorkingHours;

# start time = 0 if task not on boat
s.t. StartOnlyIfAssigned{b in BOATS, ta in TASKS}:
    startTime[b, ta] <= maxWorkingHours * taskOnBoat[b, ta];

# lastTaskStart = startTime of the last task on each boat
s.t. DefLastTaskStart{b in BOATS, ta in TASKS}:
    lastTaskStart[b] >= startTime[b, ta] - maxWorkingHours * (1 - isLast[b, ta]);


# --- Gantt output helpers ---

# travel between consecutive tasks
s.t. CalcTravelBetween{b in BOATS, ta1 in TASKS, ta2 in TASKS: ta1 <> ta2}:
    travelBetween[b, ta1, ta2] = 
        travelTime[taskLocation[ta1], taskLocation[ta2]] * after[b, ta1, ta2];

# last task -> harbor travel
s.t. CalcTravelToHarbor{b in BOATS, ta in TASKS}:
    travelToHarbor[b, ta] = travelTime[taskLocation[ta], harbor] * isLast[b, ta];


# --- havarie replanning constraints ---

#fixed tasks keep their original start time
s.t. KeepFixedStartTime{ta in TASKS: fixedBoat[ta] >= 0 and fixedStartTime[ta] >= 0}:
    startTime[fixedBoat[ta], ta] = fixedStartTime[ta];

# fixed tasks stay on their assigned boat
s.t. KeepFixedTasks{ta in TASKS: fixedBoat[ta] >= 0}:
    taskOnBoat[fixedBoat[ta], ta] = 1;

# people stay on their assigned boat
s.t. KeepPeople{p in PEOPLE: fixedPerson[p] >= 0}:
    personOnBoat[fixedPerson[p], p] = 1;

# tools stay on their assigned boat in the correct quantity
s.t. KeepTools{b in BOATS, t in TOOLS: fixedToolAmount[b,t] > 0}:
    toolOnBoat[b, t] >= fixedToolAmount[b, t];


/**
* maximize function
*/
#maximize WeightedPriority: sum{b in BOATS, ta in TASKS} (taskPrio[ta] * taskOnBoat[b,ta]);

/*maximize Combined:
    # priority of completed tasks (scaled up to dominate)
    sum{b in BOATS, ta in TASKS} (taskPrio[ta] * taskOnBoat[b, ta]) * 10

    # utilization: maximize work hours per used boat
    + sum{b in BOATS, ta in TASKS} (duration[ta] * taskOnBoat[b, ta]);
*/

maximize Combined:
    # priority of completed tasks (scaled up to dominate)
    sum{b in BOATS, ta in TASKS} (taskPrio[ta] * taskOnBoat[b, ta]) * 100

    # utilization: maximize work hours per used boat (scaled to be less than priority but still significant)
    + sum{b in BOATS, ta in TASKS} (duration[ta] * taskOnBoat[b, ta]) * 10

    # minimize total start times (encourages earlier scheduling of tasks, can help with makespan if priorities are equal)
    - sum{b in BOATS, ta in TASKS} (taskPrio[ta] * startTime[b, ta]);  # minimize start times



solve;
display taskOnBoat, personOnBoat, toolOnBoat, boatUsage, /*WorkHours,*/ startTime, travelBetween, travelToHarbor;
end;