#!/usr/bin/env pwsh
# Runs the tests with coverage from a clean TestResults and fails unless every report has 100% line and branch coverage.
param(
    [string]$Configuration = 'Debug',
    [switch]$NoBuild,
    [string[]]$TestArgs = @()
)

Set-Location (Join-Path $PSScriptRoot '..')
Remove-Item -Recurse -Force TestResults, tests/*/TestResults -ErrorAction SilentlyContinue

$build = if ($NoBuild) { @('--no-build') } else { @() }
dotnet test --solution SudokuGen.slnx -c $Configuration @build --coverage --coverage-output-format cobertura @TestArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$reports = @(Get-ChildItem -Path . -Recurse -Filter '*.cobertura.xml' -File | Where-Object { $_.FullName -notmatch '[\\/](bin|obj|coverage-report)[\\/]' })
if ($reports.Count -eq 0) { throw 'No cobertura report produced.' }

$failed = $false
foreach ($report in $reports) {
    $coverage = ([xml](Get-Content $report.FullName)).coverage
    "$($report.Name): line-rate $($coverage.'line-rate'), branch-rate $($coverage.'branch-rate')"
    if ([double]$coverage.'line-rate' -lt 1 -or [double]$coverage.'branch-rate' -lt 1) { $failed = $true }
}

if ($failed) { throw 'Coverage below 100%.' }
