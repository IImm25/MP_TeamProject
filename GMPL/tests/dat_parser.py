"""
A deliberately minimal parser for GMPL/MathProg `.dat` files.

This is NOT a general MathProg parser. It only understands the subset of
syntax actually used in this project's test-case files:

    set NAME := a b c d;

    param NAME := a 1 b 2 c 3;                       # 1-D list form
    param NAME default V;                             # (ignored - we don't
                                                        #  need defaults; we
                                                        #  only read explicit
                                                        #  entries)
    param NAME : col1 col2 := row1 v11 v12
                               row2 v21 v22;           # 2-D matrix form
    param NAME : col1 col2 :=
                 row1 v11 v12
                 row2 v21 v22;

    param SCALAR := value;                             # scalar param

Comments start with '#' and run to end of line. Statements end with ';'.
We tokenize the whole file (minus comments), then split on ';' to get
statements, then parse each statement by its keyword ('set' or 'param').

This is enough to recover, for every test case:
  - TASKS, PEOPLE, QUALIS, TOOLS, PLACES, BOATS (via amountBoats)
  - fixedBoat, fixedStartTime, fixedPerson, fixedToolAmount
  - stockTools, taskPrio, maxWorkingHours, requiredQualis, requiredTools, hasQuali

which is everything the generic invariant checks need.
"""

from __future__ import annotations

import re
from dataclasses import dataclass, field
from typing import Dict, List, Tuple, Union


@dataclass
class DatModel:
    sets: Dict[str, List[str]] = field(default_factory=dict)
    # 1-D params: name -> {index: value}
    params_1d: Dict[str, Dict[str, float]] = field(default_factory=dict)
    # 2-D params: name -> {(row, col): value}
    params_2d: Dict[str, Dict[Tuple[str, str], float]] = field(default_factory=dict)
    # scalar params: name -> value (value may be str, e.g. harbor := 'w_1')
    scalars: Dict[str, Union[float, str]] = field(default_factory=dict)

    # --- convenience -----------------------------------------------------

    def get_set(self, name: str) -> List[str]:
        return self.sets.get(name, [])

    def get_1d(self, name: str, key: str, default: float = 0.0) -> float:
        return self.params_1d.get(name, {}).get(key, default)

    def get_2d(self, name: str, row: str, col: str, default: float = 0.0) -> float:
        return self.params_2d.get(name, {}).get((row, col), default)

    def get_scalar(self, name: str, default=None):
        return self.scalars.get(name, default)

    @property
    def boats(self) -> List[str]:
        n = int(float(self.get_scalar("amountBoats", 1)))
        return [str(i) for i in range(1, n + 1)]


_COMMENT_RE = re.compile(r"#.*?$", re.MULTILINE)
_STRING_RE = re.compile(r"'([^']*)'")


def _strip_comments(text: str) -> str:
    return _COMMENT_RE.sub("", text)


def _tokenize(stmt: str) -> List[str]:
    """Tokenize a statement, treating quoted strings as single tokens."""
    # Protect ':=' before splitting bare ':' (matrix-form column separator)
    # apart, otherwise ":=" would get mangled into ":" "=" by the second
    # replace.
    stmt = stmt.replace(":=", " <<ASSIGN>> ")
    stmt = stmt.replace(":", " : ")
    stmt = stmt.replace("<<ASSIGN>>", ":=")
    raw_tokens = stmt.split()
    tokens = [t.strip("'") for t in raw_tokens]
    return tokens


def _is_number(tok: str) -> bool:
    try:
        float(tok)
        return True
    except ValueError:
        return False


def parse_dat_file(path: str) -> DatModel:
    with open(path, "r") as f:
        text = f.read()
    return parse_dat_text(text)


def parse_dat_text(text: str) -> DatModel:
    text = _strip_comments(text)
    model = DatModel()

    # Drop the leading 'data;' and trailing 'end;' bookkeeping statements.
    statements = [s.strip() for s in text.split(";")]
    statements = [s for s in statements if s and s.lower() not in ("data", "end")]

    for stmt in statements:
        tokens = _tokenize(stmt)
        if not tokens:
            continue
        kind = tokens[0]

        if kind == "set":
            _parse_set_stmt(tokens, model)
        elif kind == "param":
            _parse_param_stmt(tokens, model)
        # ignore anything else (var/solve/display/etc. don't appear in .dat)

    return model


def _parse_set_stmt(tokens: List[str], model: DatModel) -> None:
    # tokens: ['set', NAME, ':=', a, b, c, ...]
    if len(tokens) < 3 or tokens[2] != ":=":
        return
    name = tokens[1]
    members = tokens[3:]
    model.sets[name] = members


def _parse_param_stmt(tokens: List[str], model: DatModel) -> None:
    # Several shapes possible. tokens[1] is the param name.
    if len(tokens) < 2:
        return
    name = tokens[1]

    if ":=" not in tokens:
        return  # e.g. "param fixedBoat{TASKS} integer, default -1" never appears in .dat
    eq_idx = tokens.index(":=")
    head = tokens[1:eq_idx]   # name, possibly ':' col1 col2 ...
    body = tokens[eq_idx + 1:]

    if ":" in head:
        # 2-D matrix form: NAME : col1 col2 ... := row1 v11 v12 row2 v21 v22
        colon_idx = head.index(":")
        columns = head[colon_idx + 1:]
        _parse_2d_body(name, columns, body, model)
    else:
        # Either a scalar (single value) or a 1-D list (key value key value...)
        if len(body) == 1:
            val = body[0]
            model.scalars[name] = float(val) if _is_number(val) else val
        else:
            _parse_1d_body(name, body, model)


def _parse_1d_body(name: str, body: List[str], model: DatModel) -> None:
    d = model.params_1d.setdefault(name, {})
    i = 0
    n = len(body)
    while i + 1 < n:
        key = body[i]
        val = body[i + 1]
        if _is_number(val):
            d[key] = float(val)
        i += 2


def _parse_2d_body(name: str, columns: List[str], body: List[str], model: DatModel) -> None:
    d = model.params_2d.setdefault(name, {})
    ncols = len(columns)
    i = 0
    n = len(body)
    while i < n:
        row = body[i]
        i += 1
        for c in range(ncols):
            if i >= n:
                break
            val = body[i]
            i += 1
            if _is_number(val):
                d[(row, columns[c])] = float(val)
