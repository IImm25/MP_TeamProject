"""
Generic invariants that must hold for ANY feasible solution to modell.mod,
regardless of which specific test case produced it.

These are deliberately redundant with the .mod constraints themselves -
that's the point. A constraint in the LP can be silently satisfied by GLPK
while still meaning something different than intended (wrong sign, wrong
index, an "OR" that should have been an "AND", etc). Re-deriving the same
property independently, in plain Python, from the raw variable values is
how we catch a model bug that GLPK's own feasibility check would never
flag, because GLPK is only checking *its own* constraints, not the
modeler's intent.

Each check function takes (dat, sol) and returns a list of human-readable
violation strings. An empty list means the invariant holds.
"""

from __future__ import annotations

from typing import List

from dat_parser import DatModel
from glpk_parser import GlpkSolution

TOL = 1e-4  # numerical tolerance for floating point comparisons


def _is_one(x: float) -> bool:
    return x is not None and abs(x - 1.0) < TOL


def _is_zero(x: float) -> bool:
    return x is not None and abs(x) < TOL


# ---------------------------------------------------------------------------
# Each check returns List[str] of violation messages (empty = OK).
# Naming convention: check_<ConstraintName>_<what it verifies>
# ---------------------------------------------------------------------------


def check_task_assigned_at_most_once(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """Mirrors DoneAtMostOnce: a task must not be on 2+ boats simultaneously."""
    violations = []
    for ta in dat.get_set("TASKS"):
        boats_with_task = [
            b for b in dat.boats if _is_one(sol.var("taskOnBoat", b, ta) or 0.0)
        ]
        if len(boats_with_task) > 1:
            violations.append(
                f"Task {ta} is assigned to multiple boats simultaneously: {boats_with_task}"
            )
    return violations


def check_person_on_at_most_one_boat(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """Mirrors PersonOneBoat: a person must not be on 2+ boats simultaneously."""
    violations = []
    for p in dat.get_set("PEOPLE"):
        boats_with_person = [
            b for b in dat.boats if _is_one(sol.var("personOnBoat", b, p) or 0.0)
        ]
        if len(boats_with_person) > 1:
            violations.append(
                f"Person {p} is assigned to multiple boats simultaneously: {boats_with_person}"
            )
    return violations


def check_boat_usage_ascending(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """
    Mirrors Order/UsageRequiresTask: boats must be used in ascending index
    order - boat b can only be "used" if boat b-1 is also used. Catches a
    broken/loosened symmetry-breaking constraint.
    """
    violations = []
    boats = dat.boats
    usage = {b: sol.var("boatUsage", b) or 0.0 for b in boats}
    seen_unused = False
    for b in boats:
        used = _is_one(usage[b])
        if not used:
            seen_unused = True
        elif used and seen_unused:
            violations.append(
                f"Boat {b} is used after an earlier boat was left unused "
                f"(usage map: {usage}) - symmetry breaking violated"
            )
    return violations


def check_quali_coverage(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """
    Mirrors QualiCheck: for every task actually placed on a boat, the
    qualified-person count on that boat must meet the task's requirement.
    Re-derived independently from hasQuali x personOnBoat, rather than
    trusting GLPK's own row activity for QualiCheck.
    """
    violations = []
    quali_req = dat.params_2d.get("requiredQualis", {})
    has_quali = dat.params_2d.get("hasQuali", {})
    for b in dat.boats:
        for ta in dat.get_set("TASKS"):
            on_boat = _is_one(sol.var("taskOnBoat", b, ta) or 0.0)
            if not on_boat:
                continue
            for q in dat.get_set("QUALIS"):
                required = quali_req.get((ta, q), 0.0)
                if required < 1:
                    continue
                supply = sum(
                    has_quali.get((p, q), 0.0) * (sol.var("personOnBoat", b, p) or 0.0)
                    for p in dat.get_set("PEOPLE")
                )
                if supply + TOL < required:
                    violations.append(
                        f"Boat {b} runs task {ta} needing {required}x qualification "
                        f"{q}, but only has {supply} qualified people aboard"
                    )
    return violations


def check_tool_availability(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """
    Mirrors ToolAvailable: every task placed on a boat must have at least
    the required amount of each tool actually on that boat.
    """
    violations = []
    req_tools = dat.params_2d.get("requiredTools", {})
    for b in dat.boats:
        for ta in dat.get_set("TASKS"):
            on_boat = _is_one(sol.var("taskOnBoat", b, ta) or 0.0)
            if not on_boat:
                continue
            for t in dat.get_set("TOOLS"):
                required = req_tools.get((ta, t), 0.0)
                if required <= 0:
                    continue
                have = sol.var("toolOnBoat", b, t) or 0.0
                if have + TOL < required:
                    violations.append(
                        f"Boat {b} runs task {ta} needing {required}x tool {t}, "
                        f"but only has {have} aboard"
                    )
    return violations


def check_global_tool_stock(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """Mirrors GlobalToolStock: total tools loaded across all boats can't exceed stock."""
    violations = []
    stock = dat.params_1d.get("stockTools", {})
    for t in dat.get_set("TOOLS"):
        total = sum(sol.var("toolOnBoat", b, t) or 0.0 for b in dat.boats)
        cap = stock.get(t, 0.0)
        if total > cap + TOL:
            violations.append(
                f"Tool {t}: total loaded across boats ({total}) exceeds stock ({cap})"
            )
    return violations


def check_fixed_tasks_kept(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """
    Mirrors KeepFixedTasks/KeepFixedStartTime: a task with fixedBoat>=0 MUST
    be on that boat in the solution, and if fixedStartTime>=0 its start
    time must match exactly. This directly encodes the UC-05 invariant
    ("FIXED tasks cannot be altered, moved, or removed").
    """
    violations = []
    fixed_boat = dat.params_1d.get("fixedBoat", {})
    fixed_start = dat.params_1d.get("fixedStartTime", {})
    for ta, b in fixed_boat.items():
        if b < 0:
            continue
        b_str = str(int(b))
        on_boat = sol.var("taskOnBoat", b_str, ta)
        if not _is_one(on_boat or 0.0):
            violations.append(
                f"FIXED task {ta} should be on boat {b_str} (taskOnBoat=1) "
                f"but solver has taskOnBoat={on_boat}"
            )
        start_req = fixed_start.get(ta, -1.0)
        if start_req >= 0:
            actual_start = sol.var("startTime", b_str, ta)
            if actual_start is None or abs(actual_start - start_req) > TOL:
                violations.append(
                    f"FIXED task {ta} should start at {start_req} on boat {b_str}, "
                    f"but solver has startTime={actual_start}"
                )
    return violations


def check_fixed_person_kept(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """Mirrors KeepPeople: a person with fixedPerson>=0 must be on that boat."""
    violations = []
    fixed_person = dat.params_1d.get("fixedPerson", {})
    for p, b in fixed_person.items():
        if b < 0:
            continue
        b_str = str(int(b))
        on_boat = sol.var("personOnBoat", b_str, p)
        if not _is_one(on_boat or 0.0):
            violations.append(
                f"FIXED person {p} should be on boat {b_str} (personOnBoat=1) "
                f"but solver has personOnBoat={on_boat}"
            )
    return violations


def check_fixed_tools_kept(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """Mirrors KeepTools/NoAdditionalTools: committed tool amounts on boats with
    fixed tasks must match exactly (no extra, no fewer)."""
    violations = []
    fixed_tool_amt = dat.params_2d.get("fixedToolAmount", {})
    fixed_boat = dat.params_1d.get("fixedBoat", {})
    boats_with_fixed_tasks = {
        str(int(b)) for b in fixed_boat.values() if b >= 0
    }
    for (b, t), required in fixed_tool_amt.items():
        if b not in boats_with_fixed_tasks:
            continue  # NoAdditionalTools only pins boats that have fixed tasks
        actual = sol.var("toolOnBoat", b, t)
        if actual is None or abs(actual - required) > TOL:
            violations.append(
                f"Boat {b} (has a fixed task) should carry exactly {required}x "
                f"tool {t}, but solver has toolOnBoat={actual}"
            )
    return violations


def check_single_chain_per_boat(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """
    Mirrors ChainComplete/ChainCompleteFirst/OneFirstTask but re-derives the
    "exactly one connected chain from harbor, no subtours" property directly
    by walking the after[] graph - this is the check that would catch a
    disconnected-subtour bug (e.g. two separate 2-cycles) that individually
    satisfies OneSuccessor/OnePredecessor but is not a single valid route.
    """
    violations = []
    tasks = dat.get_set("TASKS")
    for b in dat.boats:
        assigned = [ta for ta in tasks if _is_one(sol.var("taskOnBoat", b, ta) or 0.0)]
        if not assigned:
            continue

        # Build successor map from after[]
        succ = {}
        for ta1 in assigned:
            for ta2 in assigned:
                if ta1 == ta2:
                    continue
                if _is_one(sol.var("after", b, ta1, ta2) or 0.0):
                    succ.setdefault(ta1, []).append(ta2)

        # Each task should have at most one successor (enforced by OneSuccessor,
        # re-checked here)
        for ta1, succs in succ.items():
            if len(succs) > 1:
                violations.append(
                    f"Boat {b}: task {ta1} has multiple successors {succs} - not a chain"
                )

        firsts = [ta for ta in assigned if _is_one(sol.var("isFirst", b, ta) or 0.0)]
        lasts = [ta for ta in assigned if _is_one(sol.var("isLast", b, ta) or 0.0)]

        if len(firsts) != 1:
            violations.append(
                f"Boat {b} has {len(assigned)} task(s) assigned but "
                f"isFirst count = {len(firsts)} (expected exactly 1): {firsts}"
            )
        if len(lasts) != 1:
            violations.append(
                f"Boat {b} has {len(assigned)} task(s) assigned but "
                f"isLast count = {len(lasts)} (expected exactly 1): {lasts}"
            )

        # Walk the chain from the first task and verify it visits every
        # assigned task exactly once, ending at the declared last task.
        # This is the actual subtour-detector: a disconnected 2-cycle would
        # pass OneSuccessor/OnePredecessor/ChainComplete but fail this walk.
        if len(firsts) == 1:
            visited = []
            cur = firsts[0]
            guard = 0
            while cur is not None and guard <= len(assigned) + 1:
                if cur in visited:
                    violations.append(
                        f"Boat {b}: chain walk revisited {cur} - cycle detected "
                        f"(visited so far: {visited})"
                    )
                    break
                visited.append(cur)
                nexts = succ.get(cur, [])
                cur = nexts[0] if nexts else None
                guard += 1
            else:
                pass
            if not violations or "cycle detected" not in violations[-1]:
                if set(visited) != set(assigned):
                    violations.append(
                        f"Boat {b}: chain walk from {firsts[0]} visited {visited}, "
                        f"but assigned tasks are {assigned} - disconnected subtour "
                        f"or broken chain"
                    )
                elif len(lasts) == 1 and visited[-1] != lasts[0]:
                    violations.append(
                        f"Boat {b}: chain walk ends at {visited[-1]} but isLast "
                        f"flags {lasts[0]} - inconsistent chain/isLast"
                    )
    return violations


def check_no_additional_people_on_fixed_boats(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """
    Mirrors NoAdditionalPeople: a boat that already carries a fixed task
    cannot receive any *new* (non fixedPerson-committed) person.
    """
    violations = []
    fixed_boat = dat.params_1d.get("fixedBoat", {})
    fixed_person = dat.params_1d.get("fixedPerson", {})
    boats_with_fixed_tasks = {str(int(b)) for b in fixed_boat.values() if b >= 0}

    for b in boats_with_fixed_tasks:
        for p in dat.get_set("PEOPLE"):
            committed_boat = fixed_person.get(p, -1.0)
            is_committed_here = committed_boat >= 0 and str(int(committed_boat)) == b
            on_boat = _is_one(sol.var("personOnBoat", b, p) or 0.0)
            if on_boat and not is_committed_here:
                violations.append(
                    f"Boat {b} has a fixed task and should accept no new people, "
                    f"but person {p} (fixedPerson={committed_boat}) is aboard"
                )
    return violations


def check_chain_order_matches_start_times(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """
    The after[]/isFirst/isLast chain variables are supposed to represent the
    boat's actual visiting order. If after[b,ta1,ta2]=1, ta1 must start at or
    before ta2 in wall-clock time. This is NOT directly enforced by the LP
    when fixedStartTime is involved: StartAfter is skipped whenever the
    SECOND task in the pair has fixedStartTime>=0 (see modell.mod's
    condition "fixedStartTime[ta2] < 0"), which lets the solver declare a
    chain order that contradicts the real start times whenever a fixed task
    sits anywhere but at the true chronological end of the chain. This in
    turn corrupts lastTaskStart (which trusts isLast) and silently weakens
    TimeLimit. This check re-derives "does the declared order match the
    real clock?" directly and independently of which LP constraint was
    supposed to enforce it.
    """
    violations = []
    tasks = dat.get_set("TASKS")
    for b in dat.boats:
        for ta1 in tasks:
            for ta2 in tasks:
                if ta1 == ta2:
                    continue
                if _is_one(sol.var("after", b, ta1, ta2) or 0.0):
                    t1 = sol.var("startTime", b, ta1)
                    t2 = sol.var("startTime", b, ta2)
                    if t1 is not None and t2 is not None and t1 > t2 + TOL:
                        violations.append(
                            f"Boat {b}: after[{ta1},{ta2}]=1 (chain says {ta1} "
                            f"precedes {ta2}) but startTime[{ta1}]={t1} is AFTER "
                            f"startTime[{ta2}]={t2} - chain order contradicts actual "
                            f"clock time (likely caused by a fixedStartTime task "
                            f"skipping StartAfter)"
                        )
    return violations


def check_last_task_start_is_actually_last(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """
    lastTaskStart[b] is supposed to equal the start time of whichever task on
    boat b runs LAST in wall-clock time (used by TimeLimit to bound the
    boat's total time). If isLast disagrees with the real maximum start time
    on that boat - which can happen when chain order and real start times
    diverge (see check_chain_order_matches_start_times) - then TimeLimit is
    silently checking the wrong number, and a boat that is actually busy
    much longer than maxWorkingHours can still show as feasible.
    """
    violations = []
    tasks = dat.get_set("TASKS")
    for b in dat.boats:
        assigned = [ta for ta in tasks if _is_one(sol.var("taskOnBoat", b, ta) or 0.0)]
        if not assigned:
            continue
        true_max_start = max(sol.var("startTime", b, ta) or 0.0 for ta in assigned)
        reported = sol.var("lastTaskStart", b)
        if reported is None:
            continue
        if reported + TOL < true_max_start:
            violations.append(
                f"Boat {b}: lastTaskStart={reported} but the actual latest "
                f"startTime among assigned tasks is {true_max_start} - "
                f"TimeLimit is bounding the wrong (too-early) task, so the "
                f"reported time budget understates real boat usage"
            )
    return violations



ALL_CHECKS = [
    check_task_assigned_at_most_once,
    check_person_on_at_most_one_boat,
    check_boat_usage_ascending,
    check_quali_coverage,
    check_tool_availability,
    check_global_tool_stock,
    check_fixed_tasks_kept,
    check_fixed_person_kept,
    check_fixed_tools_kept,
    check_single_chain_per_boat,
    check_no_additional_people_on_fixed_boats,
    check_chain_order_matches_start_times,
    check_last_task_start_is_actually_last,
]


def run_all_generic_checks(dat: DatModel, sol: GlpkSolution) -> List[str]:
    """Run every generic invariant and return the combined violation list."""
    all_violations: List[str] = []
    for check_fn in ALL_CHECKS:
        try:
            violations = check_fn(dat, sol)
        except Exception as e:  # a check crashing is itself a finding worth surfacing
            violations = [f"[check '{check_fn.__name__}' crashed: {e!r}]"]
        all_violations.extend(violations)
    return all_violations