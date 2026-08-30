[CmdletBinding()]
param(
    [ValidateSet("run", "build", "test", "restore")]
    [string]$Command = "run",
    [string]$ProjectPath = "telemetry-tracker.csproj",
    [string[]]$ExtraArgs = @()
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-RepositoryRoot {
    return (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

function Import-EnvFile {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    foreach ($line in Get-Content -LiteralPath $Path) {
        $trimmed = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed) -or $trimmed.StartsWith("#")) {
            continue
        }

        $separatorIndex = $trimmed.IndexOf("=")
        if ($separatorIndex -lt 1) {
            continue
        }

        $name = $trimmed.Substring(0, $separatorIndex).Trim()
        $value = $trimmed.Substring($separatorIndex + 1).Trim()
        [Environment]::SetEnvironmentVariable($name, $value, "Process")
    }
}

$repoRoot = Get-RepositoryRoot
Push-Location $repoRoot

try {
    Import-EnvFile -Path (Join-Path $repoRoot ".env")
    Import-EnvFile -Path (Join-Path $repoRoot ".env.agent")

    $workspaceName = [Environment]::GetEnvironmentVariable("TELEMETRY_TRACKER_WORKSPACE_NAME", "Process")
    if ([string]::IsNullOrWhiteSpace($workspaceName)) {
        $workspaceName = Split-Path $repoRoot -Leaf
    }

    Write-Host ("[Agent Workspace] name={0}" -f $workspaceName)

    switch ($Command) {
        "run" {
            $arguments = @("run", "--project", $ProjectPath, "--no-launch-profile") + $ExtraArgs
        }
        "build" {
            $arguments = @("build", $ProjectPath) + $ExtraArgs
        }
        "test" {
            $arguments = @("test", $ProjectPath) + $ExtraArgs
        }
        "restore" {
            $arguments = @("restore", $ProjectPath) + $ExtraArgs
        }
        default {
            throw "Unsupported command '$Command'."
        }
    }

    & dotnet @arguments
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
