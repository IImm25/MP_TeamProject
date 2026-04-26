$ErrorActionPreference = "Stop"

# ==============================
# Paths
# ==============================
$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$TestDir    = Join-Path $ScriptDir "tests"
$ModelFile  = Join-Path $ScriptDir "modell.mod"

# ==============================
# Counters
# ==============================
$total = 0
$passed = 0
$failed = 0

Write-Host "Starte GMPL Tests..."
Write-Host "--------------------------------------------------"

# ==============================
# Test loop
# ==============================
Get-ChildItem "$TestDir\m_test*.dat" | ForEach-Object {

    $file = $_.FullName
    $name = $_.Name
    $total++

    # ------------------------------
    # Extract EXPECTED value
    # ------------------------------
    $expectedLine = Select-String -Path $file -Pattern "# Maximize WorkHours:" | Select-Object -Last 1

    if (-not $expectedLine) {
        Write-Host "[FEHLER] $name : Kein Erwartungswert gefunden" -ForegroundColor Yellow
        $failed++
        return
    }

    if ($expectedLine.Line -match "Maximize WorkHours:\s*([0-9]+)") {
        $expected = [int]$matches[1]
    } else {
        Write-Host "[FEHLER] $name : Erwartungswert Parsing fehlgeschlagen" -ForegroundColor Yellow
        $failed++
        return
    }

    # ------------------------------
    # Run GLPSOL
    # ------------------------------
    try {
        $output = & glpsol --model $ModelFile --data $file 2>$null
    }
    catch {
        Write-Host "[FEHLER] $name : glpsol execution failed" -ForegroundColor Red
        $failed++
        return
    }

    if (-not $output) {
        Write-Host "[FAIL] $name (Kein Output von glpsol)" -ForegroundColor Red
        $failed++
        return
    }

    # ------------------------------
    # Extract ACTUAL value
    # ------------------------------
    $match = $output | Select-String -Pattern "WorkHours(\.val)?\s*=\s*([0-9]+)"

    if (-not $match) {
        Write-Host "[FAIL] $name (WorkHours nicht gefunden)" -ForegroundColor Red
        $failed++
        return
    }

    if ($match.Line -match "=\s*([0-9]+)") {
        $actual = [int]$matches[1]
    } else {
        Write-Host "[FAIL] $name (Parsing Fehler)" -ForegroundColor Red
        $failed++
        return
    }

    # ------------------------------
    # Compare
    # ------------------------------
    if ($expected -eq $actual) {
        Write-Host "[PASS] $name (Erwartet: $expected, Tatsächlich: $actual)" -ForegroundColor Green
        $passed++
    }
    else {
        Write-Host "[FAIL] $name (Erwartet: $expected, Tatsächlich: $actual)" -ForegroundColor Red
        $failed++
    }
}

# ==============================
# Summary
# ==============================
Write-Host "--------------------------------------------------"
Write-Host "Statistik: $passed von $total Tests bestanden."
Write-Host "Fehlgeschlagen: $failed"

if ($failed -eq 0) {
    exit 0
} else {
    exit 1
}