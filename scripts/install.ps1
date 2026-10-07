#!/usr/bin/env pwsh
<#
.SYNOPSIS
Installs, upgrades or removes the SudokuGen terminal game (sudoku-tui) for the current user.
.PARAMETER Version
Release to install, for example 1.0.0. Default: the latest release.
.PARAMETER Uninstall
Removes the program and its PATH entry. Save and statistics files stay.
#>
param(
    [string]$Version = 'latest',
    [switch]$Uninstall
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$repo = 'kwdevhq/sudoku-gen'
$asset = 'sudoku-tui-win-x64.zip'
$installDir = Join-Path $env:LOCALAPPDATA 'kwdevhq\sudoku-tui'

function Get-UserPathEntries {
    $raw = [Environment]::GetEnvironmentVariable('Path', 'User')
    if ([string]::IsNullOrEmpty($raw)) { return @() }
    return $raw.Split(';', [StringSplitOptions]::RemoveEmptyEntries)
}

function Set-UserPathEntries([string[]]$entries) {
    [Environment]::SetEnvironmentVariable('Path', ($entries -join ';'), 'User')
}

function Test-SamePath([string]$a, [string]$b) {
    return $a.TrimEnd('\') -ieq $b.TrimEnd('\')
}

if (-not $IsWindows -and $PSVersionTable.PSEdition -eq 'Core') {
    throw 'This installer supports Windows only. Linux and macOS: coming soon.'
}

if ($Uninstall) {
    if (Test-Path $installDir) { Remove-Item $installDir -Recurse -Force }
    Set-UserPathEntries (Get-UserPathEntries | Where-Object { -not (Test-SamePath $_ $installDir) })
    Write-Host 'sudoku-tui removed. Save and statistics files in %LOCALAPPDATA%\SudokuGen are kept.'
    return
}

if (-not [Environment]::Is64BitOperatingSystem -or
    [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne 'X64') {
    Write-Warning 'Only a Windows x64 build exists. It can fail on this system.'
}

$base = if ($Version -eq 'latest') {
    "https://github.com/$repo/releases/latest/download"
} else {
    "https://github.com/$repo/releases/download/v$($Version.TrimStart('v'))"
}

$temp = Join-Path ([IO.Path]::GetTempPath()) "sudoku-tui-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $temp | Out-Null
try {
    $zipPath = Join-Path $temp $asset
    Write-Host "Downloading $asset ($Version)..."
    Invoke-WebRequest "$base/$asset" -OutFile $zipPath
    $expected = ((Invoke-RestMethod "$base/$asset.sha256").ToString().Trim() -split '\s+')[0]
    $actual = (Get-FileHash $zipPath -Algorithm SHA256).Hash
    if ($actual -ine $expected) { throw "Checksum mismatch. Expected $expected, got $actual. Nothing was installed." }

    New-Item -ItemType Directory -Force -Path $installDir | Out-Null
    Expand-Archive $zipPath -DestinationPath $installDir -Force
}
finally {
    Remove-Item $temp -Recurse -Force -ErrorAction SilentlyContinue
}

$entries = @(Get-UserPathEntries)
if (-not ($entries | Where-Object { Test-SamePath $_ $installDir })) {
    Set-UserPathEntries ($entries + $installDir)
}
if (-not ($env:Path.Split(';') | Where-Object { Test-SamePath $_ $installDir })) {
    $env:Path = "$env:Path;$installDir"
}

Write-Host "Installed to $installDir"
Write-Host 'Run: sudoku-tui   (in a new terminal if this one does not find it)'
