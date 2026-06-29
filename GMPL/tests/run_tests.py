#!/usr/bin/env python3
"""
Test harness for modell.mod.

What this actually checks, that the old "OPTIMAL/INFEASIBLE found -> pass"
approach did not:

  1. Solver status matches what the test case claims (OPTIMAL vs INFEASIBLE),
     read from glpsol's own stdout verdict (most reliable source - the -o
     solution file can be incomplete/absent on infeasibility).
  2. GENERIC invariants - properties that must hold for ANY feasible solution
     to this model, re-derived independently in Python from the raw variable
     values (not by re-reading the LP's own constraint-row activities, which
     would only tell us GLPK's constraints were satisfied, not that they mean
     what we think they mean). Examples: a task is never double-booked across
     boats, every FIXED task is actually still on its committed boat at its
     committed time, tool stock is never oversubscribed, the task chain on
     each boat is a single harbor-anchored path with no disconnected subtour.
  3. PER-TEST assertions - the specific claim each test case's comment makes
     (e.g. "ta_2 must NOT be scheduled", "ta_1 must precede ta_2"), checked
     against actual solver output instead of requiring a human to eyeball a
     raw `display` dump.

A case is PASS if and only if all three layers agree. Otherwise it's FAIL.
There is no third category. If the model has a known, documented bug that
causes a case to fail, the case fails - every run - until the model is
fixed. That's intentional: a "known bug, tolerate it" tier just hides
regressions and lets bugs sit indefinitely. Known bugs are written up in
README.md and in test_specs.py's comments instead, so the context isn't
lost, but they don't change PASS/FAIL.

The printed report below contains no hardcoded narrative text. Every line
is derived from the actual run: the dat filename, expected vs. actual
solver status (both real), the objective value (parsed from glpsol's
output), and the literal violation messages produced by whichever checks
fired this run. There used to be a hand-written "why this test exists"
description per case, printed verbatim every run - it was removed because
it could go stale relative to the model (e.g. a fix changes WHY a case
fails, or fixes it outright, while the old text kept claiming the original
cause) with nothing to catch the mismatch. That context still exists, just
as source comments in test_specs.py and in README.md, which are read by
humans browsing the code, never echoed into a CI log.

Usage:
    python3 run_tests.py                  # run all specced test cases
    python3 run_tests.py tc_18            # run cases matching a substring
    python3 run_tests.py --verbose         # show full violation detail always
"""

from __future__ import annotations

import argparse
import os
import shutil
import subprocess
import sys
import tempfile
from dataclasses import dataclass, field
from typing import List, Optional

from dat_parser import parse_dat_file
from glpk_parser import parse_glpk_stdout_status, parse_solution_file
from invariants import run_all_generic_checks
from test_specs import TEST_SPECS, TestSpec

HERE = os.path.dirname(os.path.abspath(__file__))
MODEL_PATH = os.environ.get("GMPL_MODEL_PATH", os.path.join(HERE, "modell.mod"))
GLPSOL_TIMEOUT_SEC = int(os.environ.get("GMPL_GLPSOL_TIMEOUT_SEC", "60"))


@dataclass
class CaseResult:
    spec: TestSpec
    ran_ok: bool                     # glpsol invoked without crashing/timing out
    parse_error: Optional[str]
    solver_status: str               # normalized status we determined
    expected_status: str
    status_matches: bool
    violations: List[str] = field(default_factory=list)  # generic + assertion violations, combined
    objective_value: Optional[float] = None

    @property
    def passed(self) -> bool:
        if not self.ran_ok or self.parse_error:
            return False
        return self.status_matches and not self.violations


def run_glpsol(dat_path: str):
    with tempfile.NamedTemporaryFile(suffix=".txt", delete=False) as tmp:
        sol_path = tmp.name
    try:
        proc = subprocess.run(
            ["glpsol", "--model", MODEL_PATH, "--data", dat_path, "-o", sol_path],
            capture_output=True,
            text=True,
            timeout=GLPSOL_TIMEOUT_SEC,
        )
        proc.sol_path = sol_path  # type: ignore[attr-defined]
        return proc
    except subprocess.TimeoutExpired as e:
        class _Timeout:
            stdout = (e.stdout.decode() if isinstance(e.stdout, bytes) else (e.stdout or "")) if e.stdout else ""
            stderr = str(e)
            returncode = -1
        t = _Timeout()
        t.sol_path = sol_path  # type: ignore[attr-defined]
        return t


def normalize_status(stdout_status: Optional[str], sol_status: Optional[str]) -> str:
    """
    Collapse the many possible status strings into one of:
    "OPTIMAL", "INFEASIBLE", "MODEL_ERROR", "UNKNOWN"
    Prefers the stdout-derived verdict (more reliable for infeasible cases).
    """
    candidates = [stdout_status, sol_status]
    for c in candidates:
        if not c:
            continue
        cu = c.upper()
        if "MODEL ERROR" in cu or "PROCESSING ERROR" in cu:
            return "MODEL_ERROR"
        if "INFEASIBLE" in cu or "NO PRIMAL" in cu or "NO INTEGER" in cu:
            return "INFEASIBLE"
        if "OPTIMAL" in cu:
            return "OPTIMAL"
    return "UNKNOWN"


def run_one_case(spec: TestSpec) -> CaseResult:
    dat_path = os.path.join(HERE, spec.dat_file)

    if not os.path.exists(dat_path):
        return CaseResult(
            spec=spec, ran_ok=False, parse_error=f"Data file not found: {dat_path}",
            solver_status="N/A", expected_status=spec.expected_status,
            status_matches=False,
        )

    proc = run_glpsol(dat_path)
    stdout = proc.stdout or ""

    if "MathProg model processing error" in stdout or "model processing error" in stdout.lower():
        return CaseResult(
            spec=spec, ran_ok=True,
            parse_error=f"Model/data processing error - file did not even parse:\n{_tail(stdout)}",
            solver_status="MODEL_ERROR", expected_status=spec.expected_status,
            status_matches=False,
        )

    stdout_status = parse_glpk_stdout_status(stdout)
    sol_path = getattr(proc, "sol_path", None)
    sol = None
    sol_status = None
    if sol_path and os.path.exists(sol_path) and os.path.getsize(sol_path) > 0:
        try:
            sol = parse_solution_file(sol_path)
            sol_status = sol.status
        except Exception:
            pass
        finally:
            try:
                os.unlink(sol_path)
            except OSError:
                pass

    norm_status = normalize_status(stdout_status, sol_status)
    status_matches = norm_status == spec.expected_status

    violations: List[str] = []
    objective_value = sol.objective_value if sol else None

    if norm_status == "OPTIMAL" and sol is not None:
        try:
            dat = parse_dat_file(dat_path)
        except Exception as e:
            return CaseResult(
                spec=spec, ran_ok=True, parse_error=f"Failed to parse .dat file: {e!r}",
                solver_status=norm_status, expected_status=spec.expected_status,
                status_matches=status_matches, objective_value=objective_value,
            )
        violations.extend(run_all_generic_checks(dat, sol))
        for assertion_fn in spec.assertions:
            msg = assertion_fn(dat, sol)
            if msg:
                violations.append(msg)

    return CaseResult(
        spec=spec,
        ran_ok=True,
        parse_error=None,
        solver_status=norm_status,
        expected_status=spec.expected_status,
        status_matches=status_matches,
        violations=violations,
        objective_value=objective_value,
    )


def _tail(text: str, n: int = 8) -> str:
    lines = text.strip().splitlines()
    return "\n".join(lines[-n:])


def print_report(results: List[CaseResult], verbose: bool) -> int:
    n_pass = sum(1 for r in results if r.passed)
    n_total = len(results)

    print("=" * 78)
    print(f"MODEL TEST HARNESS - {n_pass}/{n_total} passed")
    print("=" * 78)

    for r in results:
        tag = "PASS" if r.passed else "FAIL"

        print(f"\n[{tag}] {r.spec.dat_file}")

        if r.parse_error:
            print(f"   ERROR: {r.parse_error}")
            continue

        line = f"   status: expected={r.expected_status} actual={r.solver_status}"
        if not r.status_matches:
            line += "   <-- STATUS MISMATCH"
        print(line)

        if r.objective_value is not None:
            print(f"   objective: {r.objective_value}")

        if r.violations:
            print(f"   VIOLATIONS ({len(r.violations)}):")
            for v in r.violations:
                print(f"      - {v}")
        elif verbose:
            print("   (no violations)")

    n_failed = n_total - n_pass

    print("\n" + "=" * 78)
    print(f"SUMMARY: {n_pass} passed, {n_failed} failed (of {n_total})")
    if n_failed:
        print("Failed cases:")
        for r in results:
            if not r.passed:
                print(f"  - {r.spec.dat_file}")
    print("=" * 78)

    # Machine-readable summary line - the CI workflow's PR-comment step looks
    # for a line starting with "Statistik:" verbatim, and separately checks
    # whether the substring "[FAIL]" appears anywhere in the output. Every
    # case that fails is tagged "[FAIL]" above (and only those), so that
    # check lines up automatically; this line is just the human-readable
    # rollup for the PR comment body.
    print(f"Statistik: {n_pass}/{n_total} bestanden, {n_failed} fehlgeschlagen")

    return 1 if n_failed else 0


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("filter", nargs="?", default=None,
                        help="Only run test cases whose filename contains this substring")
    parser.add_argument("--verbose", action="store_true",
                        help="Print a line even for cases with zero violations")
    args = parser.parse_args()

    if shutil.which("glpsol") is None:
        print("FATAL: 'glpsol' not found on PATH. Install glpk-utils "
              "(e.g. 'apt-get install -y glpk-utils') before running this harness.")
        return 2

    if not os.path.exists(MODEL_PATH):
        print(f"FATAL: model file not found at {MODEL_PATH}. "
              f"Set GMPL_MODEL_PATH if modell.mod lives elsewhere.")
        return 2

    specs = TEST_SPECS
    if args.filter:
        specs = [s for s in specs if args.filter in s.dat_file]
        if not specs:
            print(f"No test specs match filter '{args.filter}'")
            return 2

    results = [run_one_case(s) for s in specs]
    return print_report(results, args.verbose)


if __name__ == "__main__":
    sys.exit(main())