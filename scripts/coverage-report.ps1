#!/usr/bin/env pwsh
# Runs the tests with coverage and opens an HTML report with per-line highlighting (coverage-report/index.html).
param([switch]$NoOpen)

Set-Location (Join-Path $PSScriptRoot '..')
Remove-Item -Recurse -Force TestResults, coverage-report -ErrorAction SilentlyContinue

dotnet test --solution SudokuGen.slnx --coverage --coverage-output-format cobertura
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet tool restore | Out-Null
dotnet reportgenerator "-reports:TestResults/**/*.cobertura.xml" "-targetdir:coverage-report" "-reporttypes:Html;TextSummary"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Get-Content coverage-report/Summary.txt | Select-Object -First 12
if (-not $NoOpen) { Invoke-Item coverage-report/index.html }
