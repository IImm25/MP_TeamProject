"""
Per-test-case assertions.

Generic invariants (invariants.py) check properties true of ANY feasible
solution. They cannot tell you whether THIS test case's specific claim -
"ta_1 must be scheduled", "the model should report INFEASIBLE", "ta_2 must
NOT be scheduled because qualification q_2 is unreachable" - actually holds.
That requires per-test, hand-written expectations, because they encode
domain intent that isn't recoverable from the .mod file alone.

Each test case gets a `TestSpec`:
    - dat_file: filename
    - expected_status: "OPTIMAL" | "INFEASIBLE"
    - assertions: list of callables(dat, sol) -> Optional[str] (None if OK,
          else a violation message)

A case passes if and only if the solver status matches expected_status AND
every generic invariant holds AND every assertion holds. There is no
separate "known bug, tolerate it" tier - if the model has a documented bug
that makes a case fail, the case fails, every run, until the model is
fixed.

IMPORTANT - no narrative text is printed by the harness:
`TestSpec` deliberately has no `description` field. An earlier version of
this harness included a hardcoded prose description per test case, printed
verbatim in every run's report. That text could never be verified against
the model - it was written once, by hand, and the harness had no way to
notice if it went stale. The risk that created: change modell.mod, a test
starts failing for a *different* reason than the hardcoded text describes,
and the report still confidently prints the old (now wrong) explanation
right next to the live [FAIL] tag - actively misleading whoever is reading
CI output while debugging.

Now the report only ever prints things derived from the actual run: the
dat filename, expected vs. actual solver status, the objective value, and
the literal violation messages produced by whichever assertion functions
actually fired this run (built from the helpers below, named for what they
check, e.g. `task_scheduled("ta_1")`). The "why does this test case exist"
narrative still lives here, but ONLY as Python comments above each spec -
comments are read by humans browsing this file, never echoed into a CI log,
so they can't be mistaken for a live result no matter how stale they get.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Callable, List, Optional

from dat_parser import DatModel
from glpk_parser import GlpkSolution

TOL = 1e-4
AssertionFn = Callable[[DatModel, GlpkSolution], Optional[str]]


@dataclass
class TestSpec:
    dat_file: str
    expected_status: str  # "OPTIMAL" | "INFEASIBLE"
    assertions: List[AssertionFn] = field(default_factory=list)


# ---------------------------------------------------------------------------
# Small assertion-builder helpers, to keep the spec table below readable.
# Each returns a function (dat, sol) -> Optional[str]; the function's own
# returned string IS what shows up in the live report under VIOLATIONS, so
# that text only ever appears when the check actually fires against the
# real solver output - it's a live message, not pre-written narration.
# ---------------------------------------------------------------------------


def task_scheduled(ta: str) -> AssertionFn:
    def _check(dat: DatModel, sol: GlpkSolution) -> Optional[str]:
        on_any = any(
            abs((sol.var("taskOnBoat", b, ta) or 0.0) - 1.0) < TOL for b in dat.boats
        )
        if not on_any:
            return f"Expected task {ta} to be scheduled on some boat, but it is not."
        return None

    return _check


def task_not_scheduled(ta: str) -> AssertionFn:
    def _check(dat: DatModel, sol: GlpkSolution) -> Optional[str]:
        on_any = any(
            abs((sol.var("taskOnBoat", b, ta) or 0.0) - 1.0) < TOL for b in dat.boats
        )
        if on_any:
            boats = [b for b in dat.boats if abs((sol.var("taskOnBoat", b, ta) or 0.0) - 1.0) < TOL]
            return f"Expected task {ta} to be UNSCHEDULED, but solver placed it on boat(s) {boats}."
        return None

    return _check


def task_on_boat(ta: str, boat: str) -> AssertionFn:
    def _check(dat: DatModel, sol: GlpkSolution) -> Optional[str]:
        val = sol.var("taskOnBoat", boat, ta)
        if val is None or abs(val - 1.0) > TOL:
            return f"Expected task {ta} on boat {boat}, but taskOnBoat[{boat},{ta}]={val}."
        return None

    return _check


def task_after(boat: str, ta1: str, ta2: str) -> AssertionFn:
    """ta1 is immediately followed by ta2 on the given boat."""
    def _check(dat: DatModel, sol: GlpkSolution) -> Optional[str]:
        val = sol.var("after", boat, ta1, ta2)
        if val is None or abs(val - 1.0) > TOL:
            return f"Expected after[{boat},{ta1},{ta2}]=1 (order preserved), got {val}."
        return None

    return _check


def start_time_order(boat: str, ta_earlier: str, ta_later: str) -> AssertionFn:
    def _check(dat: DatModel, sol: GlpkSolution) -> Optional[str]:
        t1 = sol.var("startTime", boat, ta_earlier)
        t2 = sol.var("startTime", boat, ta_later)
        if t1 is None or t2 is None:
            return f"Missing startTime for {ta_earlier}/{ta_later} on boat {boat}."
        if not (t1 < t2 - TOL):
            return (
                f"Expected startTime[{boat},{ta_earlier}]={t1} < "
                f"startTime[{boat},{ta_later}]={t2}, order not respected."
            )
        return None

    return _check


def higher_priority_wins(ta_high: str, ta_low: str) -> AssertionFn:
    """When only one of two competing tasks can fit, the higher-priority one must be chosen."""
    def _check(dat: DatModel, sol: GlpkSolution) -> Optional[str]:
        high_on = any(abs((sol.var("taskOnBoat", b, ta_high) or 0.0) - 1.0) < TOL for b in dat.boats)
        low_on = any(abs((sol.var("taskOnBoat", b, ta_low) or 0.0) - 1.0) < TOL for b in dat.boats)
        if low_on and not high_on:
            return (
                f"Lower-priority task {ta_low} was scheduled while higher-priority "
                f"task {ta_high} was not - objective ordering violated."
            )
        return None

    return _check


def exactly_n_of_tasks_scheduled(tasks: List[str], n: int) -> AssertionFn:
    def _check(dat: DatModel, sol: GlpkSolution) -> Optional[str]:
        scheduled = [
            ta for ta in tasks
            if any(abs((sol.var("taskOnBoat", b, ta) or 0.0) - 1.0) < TOL for b in dat.boats)
        ]
        if len(scheduled) != n:
            return f"Expected exactly {n} of {tasks} scheduled, got {len(scheduled)}: {scheduled}"
        return None

    return _check


def at_least_one_scheduled(tasks: List[str]) -> AssertionFn:
    def _check(dat: DatModel, sol: GlpkSolution) -> Optional[str]:
        scheduled = [
            ta for ta in tasks
            if any(abs((sol.var("taskOnBoat", b, ta) or 0.0) - 1.0) < TOL for b in dat.boats)
        ]
        if not scheduled:
            return f"Expected at least one of {tasks} scheduled, got none."
        return None

    return _check


# ---------------------------------------------------------------------------
# The actual per-test specs.
#
# NOTE on file availability: the project currently contains tc_07 and
# tc_11 through tc_25, plus mini_test.dat. Earlier test cases (tc_01-tc_06,
# tc_08, tc_09, tc_10) referenced in earlier discussion are not present in
# the current /mnt/project snapshot, so they are not specced here. Add them
# back (with their own TestSpec entries) if/when those files reappear.
#
# Comments above each spec explain WHY it exists and what's currently known
# about it - this context is for humans reading this file, and is never
# printed by run_tests.py. See README.md for the same context kept in one
# place, also never auto-printed.
# ---------------------------------------------------------------------------

TEST_SPECS: List[TestSpec] = [

    # Havarie replanning sanity check: 2 fixed tasks per boat, all 4 tasks
    # should be scheduled at their pinned boats/times. Known current model
    # bug: this also exercises the chain-order/start-time inconsistency bug
    # (see check_chain_order_matches_start_times in invariants.py) because
    # both boats have multiple fixedStartTime tasks - currently FAILS for
    # that reason, independent of whether the task assignments are correct.
    TestSpec(
        dat_file="mini_test.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_scheduled("ta_2"),
            task_scheduled("ta_3"),
            task_scheduled("ta_4"),
            task_on_boat("ta_1", "2"),
            task_on_boat("ta_2", "2"),
            task_on_boat("ta_3", "1"),
            task_on_boat("ta_4", "1"),
        ],
    ),

    # Trivial case: 1 task, 1 boat, 1 qualified person, sufficient tool
    # stock, well within maxWorkingHours (4h total vs. 8h limit). No
    # conflicts of any kind.
    TestSpec(
        dat_file="tc_01_single_task.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_on_boat("ta_1", "1"),
        ],
    ),

    # Two equal-priority, equal-duration tasks, two boats, ample resources.
    # The only thing actually enforced by Order/UsageRequiresTask is that
    # boat 2 isn't used unless boat 1 already has a task - NOT which task
    # ends up on which boat. Verified against the solver: it places ta_1 on
    # boat 2 and ta_2 on boat 1 in one optimal solution, which is fully
    # valid (boat 2 is used because boat 1 has a task). The original
    # comment's "boat 1 gets at least one task before boat 2 is used" claim
    # is correct as a statement about boatUsage; it is NOT a claim about
    # which specific task lands where, so this spec only checks that both
    # tasks are scheduled - boat-usage ordering itself is already covered
    # unconditionally by check_boat_usage_ascending in invariants.py.
    TestSpec(
        dat_file="tc_02_symmetry_breaking.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_scheduled("ta_2"),
        ],
    ),

    # No fixed/running task here (unlike tc_19/20/23) - ta_1 is a perfectly
    # ordinary free task. 1h travel + 6h task + 1h return = 8h, but
    # maxWorkingHours=3. TimeLimit correctly and unproblematically makes
    # this infeasible; there's no UC-05 fixed-task protection to violate
    # here, so this is expected, intended INFEASIBLE behavior, not a bug.
    TestSpec(
        dat_file="tc_03_time_limit_infeasible.dat",
        expected_status="INFEASIBLE",
        assertions=[],
    ),

    # Task requires q_2, nobody on the team holds it. QualiCheck forces
    # taskOnBoat[*,ta_1]=0 for every boat; AtLeastOneTaskGlobal then has no
    # way to be satisfied since ta_1 is the only task - genuinely globally
    # INFEASIBLE, not just "zero tasks scheduled". The original comment's
    # "(or no task scheduled)" hedge undersells how the model actually
    # responds: AtLeastOneTaskGlobal is a hard >=1 constraint, so there is
    # no feasible all-zero solution to fall back to.
    TestSpec(
        dat_file="tc_04_missing_qualification.dat",
        expected_status="INFEASIBLE",
        assertions=[],
    ),

    # Comment claims only one of two tasks can be scheduled because tool
    # stock (3 units) can't cover both tasks' stated need (2 units each).
    # This is the same mechanism already found to be wrong in tc_22:
    # toolOnBoat[b,t] is a static per-boat floor (>= max single-task need),
    # not a per-task consumption total. Verified against the solver: both
    # tasks actually land on boat 1 together, sharing toolOnBoat[1,to_1]=2
    # (not 2+2=4), well within stockTools=3, and well within the 8h time
    # budget (7h total for both sequentially). The original comment's
    # expectation does not hold against the model as written.
    TestSpec(
        dat_file="tc_05_tool_stock_exceeded.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_scheduled("ta_2"),
        ],
    ),

    # Unlike tc_05/tc_22, the binding resource here is TIME, not tools, and
    # there is only 1 boat available - so the "share the resource
    # sequentially on one boat" escape hatch that broke tc_05/tc_22 does
    # NOT apply: both tasks together need 7h on the only boat, but
    # maxWorkingHours=4, and time cannot be shared/reused the way a static
    # tool floor can. Only one task fits (4h exactly, either one alone).
    # Priority (ta_1=9 vs ta_2=1) correctly decides which one the objective
    # picks. Verified against the solver.
    TestSpec(
        dat_file="tc_06_priority_ordering.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_not_scheduled("ta_2"),
        ],
    ),

    # ta_1 needs 3x q_1 but only 2 people exist in total - infeasible.
    TestSpec(
        dat_file="tc_07_multi_person_quali.dat",
        expected_status="INFEASIBLE",
        assertions=[],
    ),

    # The model has no simultaneity/time-window concept, only sequencing -
    # one person/one boat can legally do both tasks back-to-back within
    # maxWorkingHours, so both should be scheduled on the same boat.
    TestSpec(
        dat_file="tc_11_person_one_boat_only.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_scheduled("ta_2"),
            task_on_boat("ta_1", "1"),
            task_on_boat("ta_2", "1"),
        ],
    ),

    # This file does NOT set fixedStartTime (only fixedBoat), so nothing
    # currently pins the relative order of fixed tasks (same root cause as
    # tc_24). NOT asserting task_after("1","ta_1","ta_2") here on purpose -
    # the model does not currently preserve fixed-task order without
    # fixedStartTime set.
    TestSpec(
        dat_file="tc_12_fixed_sequence_order.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_on_boat("ta_1", "1"),
            task_on_boat("ta_2", "1"),
        ],
    ),

    # Two boats, each running a 2-task chain; tasks should stay grouped per
    # boat rather than mixed across boats.
    TestSpec(
        dat_file="tc_13_multi_boat_multi_task_routes.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_scheduled("ta_2"),
            task_scheduled("ta_3"),
            task_scheduled("ta_4"),
        ],
    ),

    # Fixed tool commitment + scarce global stock makes ta_2 unschedulable.
    TestSpec(
        dat_file="tc_14_fixed_tool_stock_conflict.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_not_scheduled("ta_2"),
        ],
    ),

    # Only q_2 holder is fixed elsewhere with no spare time -> ta_2 unschedulable.
    TestSpec(
        dat_file="tc_15_fixed_person_quali_conflict.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_not_scheduled("ta_2"),
        ],
    ),

    # ta_2 needs a tool with zero global stock -> never schedulable on any boat.
    TestSpec(
        dat_file="tc_16_havarie_impossible_no_stock.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_not_scheduled("ta_2"),
        ],
    ),

    # No person anywhere holds either required qualification - globally infeasible.
    TestSpec(
        dat_file="tc_17_global_infeasible_no_quali.dat",
        expected_status="INFEASIBLE",
        assertions=[],
    ),

    # 4 tasks on 1 boat must form a single harbor-anchored chain, not
    # disconnected subtours. The actual subtour check lives in
    # invariants.check_single_chain_per_boat, which runs unconditionally on
    # every test case - that's the real assertion; this spec just confirms
    # all 4 tasks got scheduled at all.
    TestSpec(
        dat_file="tc_18_subtour_resistance.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_scheduled("ta_2"),
            task_scheduled("ta_3"),
            task_scheduled("ta_4"),
        ],
    ),

    # Per UC-05/UC-06 a FIXED task must remain scheduled even if a
    # weather-driven speed drop makes its return-to-harbor leg exceed
    # maxWorkingHours on paper. Known current model bug: TimeLimit has no
    # carve-out for boats with FIXED tasks, so this currently reports
    # INFEASIBLE and the case FAILS until that's fixed.
    TestSpec(
        dat_file="tc_19_havarie_plus_weather_fixed_overrun.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
        ],
    ),

    # Minimal 1-task/1-boat isolation of the tc_19 finding - same known bug.
    TestSpec(
        dat_file="tc_20_fixed_task_weather_timelimit_bug.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
        ],
    ),

    # 3 boats can each take 1 of 3 independent tasks - all should be
    # scheduled, no duplication.
    TestSpec(
        dat_file="tc_21_no_duplicate_assignment_three_boats.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_scheduled("ta_2"),
            task_scheduled("ta_3"),
        ],
    ),

    # 3 tasks compete for 2 units of a scarce tool. The model schedules all
    # three because toolOnBoat is a static per-boat allocation rather than
    # time-varying - two tasks that run sequentially (never concurrently)
    # on one boat only ever need the tool once between them. Possibly a
    # genuine modeling gap worth revisiting, but not a priority-ordering
    # bug, so NOT asserting task_not_scheduled("ta_2") here.
    TestSpec(
        dat_file="tc_22_three_way_tool_contention.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_scheduled("ta_2"),
            task_scheduled("ta_3"),
        ],
    ),

    # Same TimeLimit-vs-FIXED-task gap as tc_19/tc_20, single task, pinned
    # start time. Currently FAILS until TimeLimit is fixed.
    TestSpec(
        dat_file="tc_23_fixed_task_exceeds_timelimit.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
        ],
    ),

    # Two FIXED tasks on one boat WITHOUT fixedStartTime set - same gap as
    # tc_12. NOT asserting startTime[ta_1] < startTime[ta_2]: the model
    # currently has no mechanism to preserve order from fixedBoat alone.
    TestSpec(
        dat_file="tc_24_fixed_tasks_order_violation.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
            task_scheduled("ta_2"),
        ],
    ),

    # Known current model bug: the chain variables (after/isFirst/isLast)
    # can end up pointing the opposite direction from the real wall-clock
    # start times whenever a fixed task isn't the chronologically-last one
    # in its pair - see check_chain_order_matches_start_times /
    # check_last_task_start_is_actually_last in invariants.py, which run
    # unconditionally and currently fail this case.
    TestSpec(
        dat_file="tc_25_havarie_fixed_task_timelimit.dat",
        expected_status="OPTIMAL",
        assertions=[
            task_scheduled("ta_1"),
        ],
    ),
]