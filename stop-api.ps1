<#
.SYNOPSIS
    Safely stops the Careerly API running on port 5142.
#>
param(
    [int]$Port = 5142
)

$listener = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1

if ($listener) {
    $procId = $listener.OwningProcess
    $owner = Get-Process -Id $procId -ErrorAction SilentlyContinue
    $procName = if ($owner) { $owner.ProcessName } else { "Unknown" }

    if ($procName -like "*JobApplicationManagement*" -or $procName -eq "dotnet") {
        Write-Host "Stopping Careerly API (PID: $procId - $procName)..." -ForegroundColor Yellow
        Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 1
        Write-Host "Port $Port is now free." -ForegroundColor Green
    } else {
        Write-Host "Process $procName (PID: $procId) is listening on port $Port but does not appear to be Careerly. Not terminated." -ForegroundColor Red
    }
} else {
    Write-Host "Port $Port is already free. No process is listening." -ForegroundColor Green
}
