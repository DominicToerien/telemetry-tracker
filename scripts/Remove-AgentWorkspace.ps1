[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Name,
    [string]$WorktreeRoot,
    [switch]$DeleteBranch
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-RepositoryRoot {
    return (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

function Get-DefaultWorktreeRoot {
    param(
        [Parameter(Mandatory)]
        [string]$RepositoryRoot
    )

    $parent = Split-Path $RepositoryRoot -Parent
    return Join-Path $parent "telemetry-tracker-worktrees"
}

$repoRoot = Get-RepositoryRoot

if ([string]::IsNullOrWhiteSpace($WorktreeRoot)) {
    $WorktreeRoot = Get-DefaultWorktreeRoot -RepositoryRoot $repoRoot
}

$resolvedWorktreeRoot = [System.IO.Path]::GetFullPath($WorktreeRoot)
$worktreePath = Join-Path $resolvedWorktreeRoot $Name
$metadataPath = Join-Path $worktreePath ".agent-workspace.json"
$branchName = $null

if (Test-Path -LiteralPath $metadataPath) {
    try {
        $metadata = Get-Content -LiteralPath $metadataPath -Raw | ConvertFrom-Json
        $branchName = $metadata.branchName
    }
    catch {
        Write-Warning ("Unable to read metadata from {0}" -f $metadataPath)
    }
}

Push-Location $repoRoot

try {
    & git worktree remove $worktreePath
    if ($LASTEXITCODE -ne 0) {
        throw "git worktree remove failed."
    }

    if ($DeleteBranch -and -not [string]::IsNullOrWhiteSpace($branchName)) {
        & git branch -D $branchName
        if ($LASTEXITCODE -ne 0) {
            throw "git branch delete failed."
        }
    }
}
finally {
    Pop-Location
}

Write-Host ("Removed worktree: {0}" -f $worktreePath)

if ($DeleteBranch -and -not [string]::IsNullOrWhiteSpace($branchName)) {
    Write-Host ("Deleted branch: {0}" -f $branchName)
}
