#!/usr/bin/env pwsh
# Runs the same gates as CI: format, warnings-as-errors Release build, then 100% coverage.
Set-Location (Join-Path $PSScriptRoot '..')

dotnet format src/SudokuGen.slnx --verify-no-changes
if ($LASTEXITCODE -ne 0) { Write-Error 'Formatting failed: run "dotnet format".'; exit 1 }

dotnet build src/SudokuGen.slnx -c Release -warnaserror
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& (Join-Path $PSScriptRoot 'check-coverage.ps1')
exit $LASTEXITCODE
