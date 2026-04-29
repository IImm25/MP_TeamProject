#author of this gmpl model: Leonie Skala


/**
* Sets (to be able to iterate)
*/

set TASKS;
set PEOPLE;
set QUALIS; #qualifications
set TOOLS;

param amountBoats;
set BOATS := 1..amountBoats;



/**
* loading parameters (from data file)
*/

param duration{TASKS};
param maxWorkingHours;
param hasQuali{PEOPLE,QUALIS};
param requiredQualis{TASKS,QUALIS};
param requiredTools{TASKS,TOOLS};
param stockTools{TOOLS};



/**
* define decision variables
*/

#1 when task is on this boat, else 0
var taskOnBoat{BOATS,TASKS} binary;

#specifies if boat is used (ensures that boats are filled from the front)
var boatUsage{BOATS} binary;

#specifies if person is on that boat or not
var personOnBoat{BOATS,PEOPLE} binary;

#specifies amount of a tool per boat
var toolOnBoat{BOATS,TOOLS} integer >= 0;



/**
* all constrains
*/

/*duration relevante and general constrains*/

#maximal working hours per used boat
s.t. TimeLimit{b in BOATS}: 
		sum{ta in TASKS} (duration[ta] * taskOnBoat[b,ta]) <= maxWorkingHours * boatUsage[b];

#complete each task at most one time (or not at all)
s.t. DoneAtMostOnce{ta in TASKS}:
		sum{b in BOATS} (taskOnBoat[b,ta]) <= 1;

#at least one task must be done (plannen on a boat)
s.t. AtLeastOneTaskGlobal:
		sum{b in BOATS, ta in TASKS} (taskOnBoat[b,ta]) >= 1;
		
#use boat in ascending order (breaking the symmetry)
s.t. Order{b in BOATS: b > 1}:
		boatUsage[b] <= sum{ta in TASKS} (taskOnBoat[b-1,ta]);


/*people relevante constrains*/

# the amount of people with a certain qualification on board must be at least equal to the most demanding task to be carried out on that boat.
s.t. QualiCheck{b in BOATS, ta in TASKS, q in QUALIS: requiredQualis[ta,q] >= 1}:
		sum{p in PEOPLE} (hasQuali[p,q] * personOnBoat[b,p]) >= (requiredQualis[ta,q] * taskOnBoat[b,ta]);

#each person can only be on one boat at a time
s.t. PersonOneBoat{p in PEOPLE}:
		sum{b in BOATS} personOnBoat[b,p] <= 1;
	
	
/*tool relevante constrains*/

#The amount of tools on boad must be at least equal to the most demanding task to be carried out on that boat.
s.t. ToolAvailable{b in BOATS, ta in TASKS, t in TOOLS: requiredTools[ta,t] > 0}:
		toolOnBoat[b,t] >= requiredTools[ta,t] * taskOnBoat[b,ta];
	
#total stock across all boats
s.t. GlobalToolStock{t in TOOLS}:
    sum{b in BOATS} toolOnBoat[b,t] <= stockTools[t];

#at least one task must be done (plannen on a boat)
s.t. AtLeastOneTaskGlobal:
        sum{b in BOATS, ta in TASKS} (taskOnBoat[b,ta]) >= 1;

/**
* maximize function
*/
maximize WorkHours: sum{b in BOATS, ta in TASKS} (duration[ta] * taskOnBoat[b,ta]);

solve;
display taskOnBoat, personOnBoat, toolOnBoat, boatUsage, WorkHours;
end;