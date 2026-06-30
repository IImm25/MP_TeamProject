"""
Parser for GLPK MathProg solution files (the file written by `glpsol ... -o solution.txt`).

GLPK's text solution format looks like:

    Problem:    modell
    Rows:       225
    ...
    Status:     INTEGER OPTIMAL
    Objective:  Combined = 3537.5 (MAXimum)

       No.   Row name        Activity     Lower bound   Upper bound
    ------ ------------    ------------- ------------- -------------
         1 TimeLimit[1]               -5                          -0
    ...

       No. Column name       Activity     Lower bound   Upper bound
    ------ ------------    ------------- ------------- -------------
         1 taskOnBoat[1,ta_1]
                        *              0             0             1
    ...

Tricky bits this parser has to handle:
  - Column/row names can wrap onto their own line when long, with the
    Activity/bounds on the *next* line (see "taskOnBoat[1,ta_1]" above).
  - Binary/integer columns get a "*" marker before the activity; continuous
    columns don't.
  - Indices inside [...] can be multi-dimensional, e.g. after[1,ta_1,ta_2].
  - If the problem is infeasible, GLPK does not write Rows/Columns sections;
    only a Status: line (e.g. "INFEASIBLE (PRIMAL)") or no solution file at
    all is produced, and the *stdout* of glpsol carries the real verdict
    ("PROBLEM HAS NO PRIMAL FEASIBLE SOLUTION" / "...NO INTEGER FEASIBLE...").
"""

from __future__ import annotations

import re
from dataclasses import dataclass, field
from typing import Dict, List, Optional, Tuple


VarKey = Tuple[str, ...]  # e.g. ("taskOnBoat", "1", "ta_1")


@dataclass
class Variable:
    name: str               # e.g. "taskOnBoat"
    indices: Tuple[str, ...]  # e.g. ("1", "ta_1")
    activity: float

    @property
    def key(self) -> VarKey:
        return (self.name,) + self.indices


@dataclass
class GlpkSolution:
    status: str                      # raw status line, e.g. "INTEGER OPTIMAL"
    objective_name: Optional[str]
    objective_value: Optional[float]
    variables: Dict[VarKey, Variable] = field(default_factory=dict)
    raw_stdout: str = ""
    raw_solution_file: str = ""

    # --- convenience accessors -------------------------------------------------

    def var(self, name: str, *indices) -> Optional[float]:
        """Return the activity of a variable, or None if it doesn't exist."""
        key = (name,) + tuple(str(i) for i in indices)
        v = self.variables.get(key)
        return v.activity if v is not None else None

    def var_strict(self, name: str, *indices) -> float:
        val = self.var(name, *indices)
        if val is None:
            raise KeyError(f"Variable {name}{list(indices)} not found in solution")
        return val

    def vars_matching(self, name: str) -> List[Variable]:
        """All variables with a given base name, e.g. all taskOnBoat[*,*]."""
        return [v for v in self.variables.values() if v.name == name]

    @property
    def is_optimal(self) -> bool:
        return "OPTIMAL" in self.status.upper()

    @property
    def is_infeasible(self) -> bool:
        s = self.status.upper()
        return "INFEASIBLE" in s or "NO PRIMAL" in s or "NO INTEGER" in s or "UNDEFINED" in s


_NAME_IDX_RE = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*)\[(.+)\]$")
_PLAIN_NAME_RE = re.compile(r"^([A-Za-z_][A-Za-z0-9_]*)$")


def _split_name_indices(token: str) -> Tuple[str, Tuple[str, ...]]:
    m = _NAME_IDX_RE.match(token)
    if m:
        name = m.group(1)
        idx_str = m.group(2)
        indices = tuple(idx_str.split(","))
        return name, indices
    m2 = _PLAIN_NAME_RE.match(token)
    if m2:
        return m2.group(1), tuple()
    # Fallback: treat the whole token as the name
    return token, tuple()


def _to_float(token: str) -> Optional[float]:
    token = token.strip()
    if token in ("", "<", ">", "="):
        return None
    try:
        return float(token)
    except ValueError:
        return None


def parse_glpk_stdout_status(stdout: str) -> Optional[str]:
    """
    Extract a definitive status from glpsol's stdout, which is the most
    reliable source for infeasibility (the -o solution file is sometimes
    incomplete or absent for infeasible problems).
    """
    text = stdout.upper()
    if "PROBLEM HAS NO PRIMAL FEASIBLE SOLUTION" in text:
        return "INFEASIBLE (NO PRIMAL FEASIBLE SOLUTION)"
    if "PROBLEM HAS NO INTEGER FEASIBLE SOLUTION" in text:
        return "INFEASIBLE (NO INTEGER FEASIBLE SOLUTION)"
    if "PROBLEM HAS NO DUAL FEASIBLE SOLUTION" in text:
        return "INFEASIBLE (NO DUAL FEASIBLE SOLUTION)"
    if "INTEGER OPTIMAL SOLUTION FOUND" in text:
        return "INTEGER OPTIMAL"
    if "INTEGER UNDEFINED" in text:
        return "INTEGER UNDEFINED"
    if "MODELPROCESSING ERROR" in text.replace(" ", "") or "MODEL PROCESSING ERROR" in text:
        return "MODEL ERROR"
    return None


def parse_solution_file(path_or_text: str, is_path: bool = True) -> GlpkSolution:
    if is_path:
        with open(path_or_text, "r") as f:
            text = f.read()
    else:
        text = path_or_text

    status = "UNKNOWN"
    m = re.search(r"^Status:\s*(.+)$", text, re.MULTILINE)
    if m:
        status = m.group(1).strip()

    obj_name = None
    obj_value = None
    m = re.search(r"^Objective:\s*(\S+)\s*=\s*([-\d.eE+]+)", text, re.MULTILINE)
    if m:
        obj_name = m.group(1)
        try:
            obj_value = float(m.group(2))
        except ValueError:
            obj_value = None

    variables: Dict[VarKey, Variable] = {}

    # Isolate the "Column name" section (variables), which comes after the
    # "Row name" section. We only need columns (decision variables) for
    # invariant-checking; rows are constraint activities, which we ignore.
    col_section_match = re.search(
        r"No\.\s+Column name\s+Activity.*?\n-+\s+-+\s+-+\s+-+\s+-+\s*\n(.*?)(?:\nInteger feasibility|\Z)",
        text,
        re.DOTALL,
    )
    if col_section_match:
        col_text = col_section_match.group(1)
        variables.update(_parse_column_block(col_text))

    return GlpkSolution(
        status=status,
        objective_name=obj_name,
        objective_value=obj_value,
        variables=variables,
        raw_solution_file=text,
    )


def _parse_column_block(col_text: str) -> Dict[VarKey, Variable]:
    """
    Parses lines like:
         1 taskOnBoat[1,ta_1]
                        *              0             0             1
    or (continuous var, no '*' marker, sometimes on one line):
        61 startTime[1,ta_1]
                                       0             0
    or rarely, all on one line:
         9 boatUsage[1] *              1             0             1
    """
    variables: Dict[VarKey, Variable] = {}

    lines = col_text.splitlines()
    i = 0
    n = len(lines)
    row_start_re = re.compile(r"^\s*(\d+)\s+(\S+)(.*)$")

    while i < n:
        line = lines[i]
        if not line.strip():
            i += 1
            continue
        m = row_start_re.match(line)
        if not m:
            i += 1
            continue

        var_token = m.group(2)
        rest = m.group(3)

        # The activity may be on this same line (rest), or on the next line
        # if the var name was long enough to force a wrap.
        numbers = _extract_numbers_skip_star(rest)
        if not numbers:
            # look at next line for the activity value
            if i + 1 < n:
                numbers = _extract_numbers_skip_star(lines[i + 1])
                i += 1  # consume the continuation line
        i += 1

        if not numbers:
            continue  # couldn't find an activity; skip rather than crash

        activity = numbers[0]
        name, indices = _split_name_indices(var_token)
        key = (name,) + indices
        variables[key] = Variable(name=name, indices=indices, activity=activity)

    return variables


def _extract_numbers_skip_star(s: str) -> List[float]:
    """Extract numeric tokens from a line, ignoring a leading '*' marker."""
    s = s.strip()
    if s.startswith("*"):
        s = s[1:].strip()
    if not s:
        return []
    tokens = s.split()
    nums = []
    for t in tokens:
        v = _to_float(t)
        if v is not None:
            nums.append(v)
        else:
            break  # stop at first non-numeric token (defensive)
    return nums
