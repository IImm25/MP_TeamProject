@echo off
setlocal enabledelayedexpansion

REM GMPL Test Runner for Windows (Batch)

set "SCRIPT_DIR=%~dp0"
set "TEST_DIR=%SCRIPT_DIR%tests"
set "MODEL_FILE=%SCRIPT_DIR%modell.mod"

echo Starte GMPL Tests...
echo --------------------------------------------------

set TOTAL=0
set PASSED=0

for %%F in ("%TEST_DIR%\m_test*.dat") do (
    set /a TOTAL+=1
    set "FILENAME=%%~nxF"
    set "EXPECTED="
    set "ACTUAL="

    REM Erwarteten Wert extrahieren (Suche nach "# Maximize WorkHours: X.")
    for /f "tokens=4" %%A in ('findstr /R /C:"# Maximize WorkHours: [0-9]*" "%%F"') do (
        set "EXPECTED=%%A"
        REM Remove trailing dot if exists
        set "EXPECTED=!EXPECTED:.=!"
    )

    if "!EXPECTED!"=="" (
        echo [FEHLER] !FILENAME!: Kein Erwartungswert in den Kommentaren gefunden.
        goto :next_test
    )

    REM glpsol ausführen und Ergebnis extrahieren
    REM Wir suchen nach der Zeile "WorkHours.val = X" oder "WorkHours = X"
    for /f "tokens=3" %%B in ('glpsol --model "%MODEL_FILE%" --data "%%F" 2^>nul ^| findstr /R /C:"WorkHours.* = [0-9]*"') do (
        set "ACTUAL=%%B"
    )

    if "!ACTUAL!"=="" (
        set "ACTUAL=Not Found"
    )

    if "!EXPECTED!"=="!ACTUAL!" (
        echo [PASS] !FILENAME! (Erwartet: !EXPECTED!, Tatsaechlich: !ACTUAL!)
        set /a PASSED+=1
    ) else (
        echo [FAIL] !FILENAME! (Erwartet: !EXPECTED!, Tatsaechlich: !ACTUAL!)
    )

    :next_test
)

echo --------------------------------------------------
echo Statistik: %PASSED% von %TOTAL% Tests erfolgreich bestanden.

if %PASSED%==%TOTAL% (
    exit /b 0
) else (
    exit /b 1
)
