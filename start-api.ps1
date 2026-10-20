<#
.SYNOPSIS
    Safely starts the Careerly API without causing port collisions.
.DESCRIPTION
    Checks every listener on the configured port before starting the API.
    If Careerly is already listening, the script reports its process and parent,
    verifies the health endpoint, and does not start a duplicate instance.
    An unrelated listener is reported but never terminated.
#>
param(
    [int]$Port = 5142
)

function Get-ProcessDetails {
    param([int]$ProcessId)

    Get-CimInstance Win32_Process -Filter "ProcessId=$ProcessId" -ErrorAction SilentlyContinue
}

function Test-CareerlyProcess {
    param($Process)

    if (-not $Process) {
        return $false
    }

    if ($Process.Name -ieq 'JobApplicationManagement.API.exe') {
        return $true
    }

    return $Process.Name -ieq 'dotnet.exe' -and
        $Process.CommandLine -match '(?i)JobApplicationManagement\.API(?:\.dll|\.csproj|[\\/]|\s|$)'
}

$listeners = @(
    Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue |
        Sort-Object OwningProcess, LocalAddress
)

if ($listeners.Count -gt 0) {
    $ownerIds = @($listeners.OwningProcess | Sort-Object -Unique)
    $owners = @($ownerIds | ForEach-Object { Get-ProcessDetails -ProcessId $_ })
    $careerlyOwners = @($owners | Where-Object { Test-CareerlyProcess -Process $_ })

    foreach ($owner in $owners) {
        $parent = Get-ProcessDetails -ProcessId $owner.ParentProcessId
        $addresses = @(
            $listeners |
                Where-Object OwningProcess -eq $owner.ProcessId |
                ForEach-Object { "$($_.LocalAddress):$($_.LocalPort)" }
        ) -join ', '

        Write-Host "Listener:       $addresses" -ForegroundColor Cyan
        Write-Host "Process:        $($owner.Name) (PID $($owner.ProcessId))" -ForegroundColor Cyan
        Write-Host "Executable:     $($owner.ExecutablePath)" -ForegroundColor DarkCyan
        Write-Host "Command line:   $($owner.CommandLine)" -ForegroundColor DarkCyan
        if ($parent) {
            Write-Host "Parent process: $($parent.Name) (PID $($parent.ProcessId))" -ForegroundColor DarkCyan
            Write-Host "Parent command: $($parent.CommandLine)" -ForegroundColor DarkCyan
        }
    }

    if ($careerlyOwners.Count -gt 0) {
        $healthUri = "http://localhost:$Port/health"
        $healthy = $false

        try {
            $healthResponse = Invoke-WebRequest -Uri $healthUri -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop
            $healthy = [int]$healthResponse.StatusCode -eq 200
        }
        catch {
            $healthy = $false
        }

        if ($healthy) {
            Write-Host 'Careerly API is already running.' -ForegroundColor Green
            Write-Host "Health check: PASS ($healthUri)" -ForegroundColor Green
            exit 0
        }

        Write-Host 'Careerly owns the configured port, but its health check did not pass.' -ForegroundColor Yellow
        Write-Host 'A second instance will not be started. Inspect the existing instance first.' -ForegroundColor Yellow
        exit 1
    }

    Write-Host "Port $Port is occupied by a non-Careerly process." -ForegroundColor Red
    Write-Host 'No process was stopped, and Careerly was not started.' -ForegroundColor Yellow
    exit 1
}

Write-Host "Port $Port is free. Starting Careerly API..." -ForegroundColor Cyan
$projectPath = Join-Path $PSScriptRoot 'JobApplicationManagement.API\JobApplicationManagement.API.csproj'
& dotnet run --project $projectPath
exit $LASTEXITCODE
