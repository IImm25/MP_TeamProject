#!/bin/bash

# Verzeichnis der Testdateien (relativ zum Skript-Standort)
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
TEST_DIR="$SCRIPT_DIR/tests"
MODEL_FILE="$SCRIPT_DIR/modell.mod"

# Farben für die Ausgabe
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo "Starte GMPL Tests..."
echo "--------------------------------------------------"

TOTAL=0
PASSED=0

# Alle .dat Dateien im GMPL Verzeichnis finden
for testfile in $TEST_DIR/m_test*.dat; do
    [ -e "$testfile" ] || continue
    ((TOTAL++))
    
    filename=$(basename "$testfile")
    
    # Erwarteten Wert extrahieren (Suche nach "# Maximize WorkHours: X." oder "# Maximize WorkHours: INFEASIBLE.")
    EXPECTED=$(grep -oE "# Maximize WorkHours: [0-9]+|# Maximize WorkHours: INFEASIBLE" "$testfile" | tail -n 1 | awk '{print $4}')

    if [ -z "$EXPECTED" ]; then
        echo -e "${RED}[FEHLER]${NC} $filename: Kein Erwartungswert in den Kommentaren gefunden."
        continue
    fi

    # glpsol ausführen
    SOLVER_OUTPUT=$(glpsol --model "$MODEL_FILE" --data "$testfile" 2>/dev/null)

    if [ "$EXPECTED" = "INFEASIBLE" ]; then
        # Prüfen ob die Ausgabe INFEASIBLE enthält
        if echo "$SOLVER_OUTPUT" | grep -q "NO PRIMAL FEASIBLE SOLUTION"; then
            echo -e "${GREEN}[PASS]${NC} $filename (Erwartet: INFEASIBLE, Tatsächlich: INFEASIBLE)"
            ((PASSED++))
        else
            ACTUAL=$(echo "$SOLVER_OUTPUT" | grep -oE "WorkHours(\.val)? = [0-9]+" | head -n 1 | awk '{print $3}')
            echo -e "${RED}[FAIL]${NC} $filename (Erwartet: INFEASIBLE, Tatsächlich: WorkHours=$ACTUAL)"
        fi
    else
        # Numerischen WorkHours-Wert extrahieren
        ACTUAL=$(echo "$SOLVER_OUTPUT" | grep -oE "WorkHours(\.val)? = [0-9]+" | head -n 1 | awk '{print $3}')

        if [ "$EXPECTED" -eq "$ACTUAL" ] 2>/dev/null; then
            echo -e "${GREEN}[PASS]${NC} $filename (Erwartet: $EXPECTED, Tatsächlich: $ACTUAL)"
            ((PASSED++))
        else
            echo -e "${RED}[FAIL]${NC} $filename (Erwartet: $EXPECTED, Tatsächlich: $ACTUAL)"
        fi
    fi
done

echo "--------------------------------------------------"
echo "Statistik: $PASSED von $TOTAL Tests erfolgreich bestanden."

if [ $PASSED -eq $TOTAL ]; then
    exit 0
else
    exit 1
fi