# Stock Warehouse Tracking API - local run script
# Usage:
#   .\run-api.ps1
#   .\run-api.ps1 -Https
#   .\run-api.ps1 -SkipKill

param(
    [switch] $Https,
    [switch] $SkipKill,
    [int] $Port = 5087
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

function Stop-PortListeners([int] $Port) {
    $pids = @(
        Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue |
            Select-Object -ExpandProperty OwningProcess -Unique |
            Where-Object { $_ -and $_ -gt 0 }
    )
    foreach ($procId in $pids) {
        try {
            $proc = Get-Process -Id $procId -ErrorAction Stop
            Write-Host ("==> Port {0}: {1} (PID {2}) kapatiliyor..." -f $Port, $proc.ProcessName, $procId) -ForegroundColor Yellow
            Stop-Process -Id $procId -Force -ErrorAction Stop
        } catch {
            Write-Host ("==> Port {0} PID {1} kapatilamadi: {2}" -f $Port, $procId, $_.Exception.Message) -ForegroundColor DarkYellow
        }
    }
    if ($pids.Count -gt 0) {
        Start-Sleep -Seconds 1
    }
}

$profileName = if ($Https) { "https" } else { "http" }

Write-Host "==> Stock Warehouse API" -ForegroundColor Cyan
Write-Host "    Profile : $profileName"
Write-Host "    Port    : $Port"
Write-Host "    Env     : Development"
Write-Host ""

if (-not $SkipKill) {
    Stop-PortListeners -Port $Port
}

$sapBase = $null
try {
    $cfg = Get-Content ".\appsettings.json" -Raw | ConvertFrom-Json
    $sapBase = $cfg.SapHttp.BaseUrl
} catch { }

if ($sapBase) {
    try {
        $null = Invoke-WebRequest -Uri ("{0}/sap/bc/zstock/stock" -f $sapBase.TrimEnd("/")) -TimeoutSec 3 -UseBasicParsing
        Write-Host "==> SAP HTTP reachable: $sapBase" -ForegroundColor Green
    } catch {
        Write-Host "==> Warning: SAP HTTP not reachable at $sapBase" -ForegroundColor Yellow
        Write-Host "    API will start anyway; stock/product calls may fail until SAP is up." -ForegroundColor Yellow
    }
}

Write-Host "==> Starting API (Ctrl+C to stop)..." -ForegroundColor Cyan
Write-Host "    Swagger: http://localhost:$Port/swagger"
Write-Host "    Health : http://localhost:$Port/health/sap"
Write-Host ""

$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --launch-profile $profileName
