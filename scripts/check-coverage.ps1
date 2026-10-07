#!/usr/bin/env pwsh
# Fails when line or branch coverage is below 100%.
param([string]$Root = (Join-Path $PSScriptRoot '..'))

$report = Get-ChildItem -Path $Root -Recurse -Filter '*.cobertura.xml' | Sort-Object LastWriteTime | Select-Object -Last 1
if (-not $report) { throw 'No cobertura report found. Run: dotnet test --solution SudokuGen.slnx --coverage --coverage-output-format cobertura' }

$coverage = ([xml](Get-Content $report.FullName)).coverage
"line-rate $($coverage.'line-rate'), branch-rate $($coverage.'branch-rate')"
if ([double]$coverage.'line-rate' -lt 1 -or [double]$coverage.'branch-rate' -lt 1) { throw 'Coverage below 100%.' }
