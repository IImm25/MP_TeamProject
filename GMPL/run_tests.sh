#!/usr/bin/env bash
# Entry point for the GMPL model test harness, invoked by CI as:
#   cd GMPL && bash run_tests.sh | tee ../test-results.txt
#   exit ${PIPESTATUS[0]}
#
# This script is intentionally a thin wrapper: all real logic lives in
# run_tests.py (+ dat_parser.py, glpk_parser.py, invariants.py, test_specs.py)
# next to it. The wrapper's only jobs are:
#   1. fail fast with a clear message if python3 or glpsol aren't available
#   2. run the harness from the directory this script lives in, regardless
#      of the caller's CWD (so it works whether invoked as
#      `bash GMPL/run_tests.sh` from the repo root, or as `bash run_tests.sh`
#      after `cd GMPL`, matching exactly what the CI workflow does)
#   3. propagate the harness's exit code unchanged (0 = all good, including
#      "known bug still reproducing" cases; 1 = a genuine, unexpected
#      mismatch was found)
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

PYTHON_BIN="${PYTHON_BIN:-python3}"

if ! command -v "$PYTHON_BIN" >/dev/null 2>&1; then
    echo "FATAL: '$PYTHON_BIN' not found on PATH." >&2
    exit 2
fi

if ! command -v glpsol >/dev/null 2>&1; then
    echo "FATAL: 'glpsol' not found on PATH. The CI workflow's 'Install glpsol' step" >&2
    echo "should have installed glpk-utils before this script runs - check that step's logs." >&2
    exit 2
fi

"$PYTHON_BIN" tests/run_tests.py "$@"
exit $?