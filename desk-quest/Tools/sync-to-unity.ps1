<#
.SYNOPSIS
    Copies the Desk Quest scripts from this repository into a Unity project.

.DESCRIPTION
    The Unity project lives outside this repository, because a Unity project contains
    generated files that cannot be produced without Unity installed. That means every
    time a fix lands here, the scripts have to be copied across. This does that.

    It copies .cs files only, over the top of what is already there, and deletes
    nothing.

    That restraint matters more than it looks. Unity pairs every file with a .meta file
    holding a generated id, and the scene stores its references to your scripts by that
    id. Delete the folder and re-copy it and Unity issues fresh ids, at which point the
    scene loses track of every component and the game silently stops working. Copying
    over the top leaves the .meta files alone, so the scene keeps working.

.PARAMETER UnityProject
    Path to the Unity project folder, the one containing Assets.

.EXAMPLE
    .\sync-to-unity.ps1 -UnityProject "C:\Users\johns\Desk Quest"

.NOTES
    Written on a machine with no PowerShell available to test it. If it misbehaves,
    copying the Assets\DeskQuest folder across by hand in Explorer does the same job:
    choose replace when Windows asks, never delete first.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$UnityProject
)

$ErrorActionPreference = 'Stop'

$sourceRoot = Join-Path $PSScriptRoot '..\Assets\DeskQuest'

if (-not (Test-Path $sourceRoot)) {
    Write-Error "Cannot find the game scripts at $sourceRoot. Run this from inside the repository."
    exit 1
}

$sourceRoot = (Resolve-Path $sourceRoot).Path

if (-not (Test-Path $UnityProject)) {
    Write-Error "No folder at $UnityProject."
    exit 1
}

$assetsFolder = Join-Path $UnityProject 'Assets'

if (-not (Test-Path $assetsFolder)) {
    Write-Error "$UnityProject does not look like a Unity project; it has no Assets folder."
    exit 1
}

$destinationRoot = Join-Path $assetsFolder 'DeskQuest'
$isFirstCopy = -not (Test-Path $destinationRoot)

$scripts = Get-ChildItem -Path $sourceRoot -Recurse -Filter *.cs -File
$copied = 0

foreach ($script in $scripts) {
    $relativePath = $script.FullName.Substring($sourceRoot.Length).TrimStart('\')
    $destination = Join-Path $destinationRoot $relativePath
    $destinationFolder = Split-Path $destination -Parent

    if (-not (Test-Path $destinationFolder)) {
        New-Item -ItemType Directory -Path $destinationFolder -Force | Out-Null
    }

    Copy-Item -Path $script.FullName -Destination $destination -Force
    $copied++
}

Write-Host "Copied $copied script(s) into $destinationRoot" -ForegroundColor Green

if ($isFirstCopy) {
    Write-Host ""
    Write-Host "First copy. In Unity, wait for the import to finish, then run:" -ForegroundColor Cyan
    Write-Host "  Tools > Desk Quest > Set Up Scene"
}
else {
    Write-Host "Switch to Unity and let it recompile." -ForegroundColor Cyan
}
