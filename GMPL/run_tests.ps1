# GMPL Test Runner for Windows (PowerShell)

# Verzeichnis der Testdateien (relativ zum Skript-Standort)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$TestDir = Join-Path $ScriptDir "tests"
$ModelFile = Join-Path $ScriptDir "modell.mod"

Write-Host "Starte GMPL Tests..."
Write-Host "--------------------------------------------------"

$Total = 0
$Passed = 0

# Alle .dat Dateien im GMPL Verzeichnis finden
$TestFiles = Get-ChildItem -Path $TestDir -Filter "m_test*.dat"

foreach ($file in $TestFiles) {
    $testfile = $file.FullName
    $Total++
    
    $filename = $file.Name
    
    # Erwarteten Wert extrahieren (Suche nach "# Maximize WorkHours: X.")
    # Wir nehmen den letzten Treffer, falls mehrere vorhanden sind
    $content = Get-Content $testfile
    $expectedLine = $content | Select-String -Pattern "# Maximize WorkHours: (\d+)" | Select-Object -Last 1
    
    if ($expectedLine -match "# Maximize WorkHours: (\d+)") {
        $Expected = $Matches[1]
    } else {
        Write-Host "[FEHLER] $filename: Kein Erwartungswert in den Kommentaren gefunden." -ForegroundColor Red
        continue
    }
    
    # glpsol ausführen und Ergebnis extrahieren
    # Wir suchen nach der Zeile "WorkHours.val = X" oder "WorkHours = X"
    $glpOutput = glpsol --model "$ModelFile" --data "$testfile" 2>$null
    $actualLine = $glpOutput | Select-String -Pattern "WorkHours(\.val)? = (\d+)" | Select-Object -First 1
    
    if ($actualLine -match "WorkHours(\.val)? = (\d+)") {
        $Actual = $Matches[2]
    } else {
        $Actual = "Not Found"
    }
    
    if ($Expected -eq $Actual) {
        Write-Host "[PASS] $filename (Erwartet: $Expected, Tatsächlich: $Actual)" -ForegroundColor Green
        $Passed++
    } else {
        Write-Host "[FAIL] $filename (Erwartet: $Expected, Tatsächlich: $Actual)" -ForegroundColor Red
    }
}

Write-Host "--------------------------------------------------"
Write-Host "Statistik: $Passed von $Total Tests erfolgreich bestanden."

if ($Passed -eq $Total) {
    exit 0
} else {
    exit 1
}
