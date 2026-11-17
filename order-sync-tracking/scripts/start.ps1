param([switch]$NoBrowser)
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$logs = Join-Path $project 'logs'
New-Item -ItemType Directory -Force -Path $logs | Out-Null
# Prefer Node.js on PATH; use this PC's bundled Node.js when needed.
$nodeCommand = Get-Command node -ErrorAction SilentlyContinue
if ($nodeCommand) { $nodePath = $nodeCommand.Source }
else {
    $nodePath = 'C:\Program Files\WindowsApps\OpenAI.CodexPrimaryRuntime.v26-1112-611-0_26.1112.611.0_x64__3k8sg7r9htsxt\dependencies\node\bin\node.exe'
}
if (!(Test-Path -LiteralPath $nodePath)) { throw 'Install Node.js LTS, then run this file again.' }
$env:PATH = (Split-Path $nodePath) + ';' + $env:PATH
function Ready($url) {
    try { Invoke-WebRequest $url -TimeoutSec 2 -UseBasicParsing | Out-Null; return $true }
    catch { return $false }
}
if (!(Ready 'http://127.0.0.1:4010/')) {
    # A running receiver answers 404 at its root, so check its listener instead.
    $listener = Get-NetTCPConnection -LocalPort 4010 -State Listen -ErrorAction SilentlyContinue
    if (!$listener) {
        Start-Process -FilePath (Get-Command postman).Source -ArgumentList @('simulation','run','postman/simulations/order-sync.sim.yaml','--port','4010','--no-history') -WorkingDirectory $project -WindowStyle Hidden -RedirectStandardOutput "$logs\receiver.log" -RedirectStandardError "$logs\receiver-error.log"
    }
}
if (!(Ready 'http://localhost:5080/health')) {
    Start-Process -FilePath (Get-Command dotnet).Source -ArgumentList @('run','--no-launch-profile') -WorkingDirectory "$project\backend" -WindowStyle Hidden -RedirectStandardOutput "$logs\api.log" -RedirectStandardError "$logs\api-error.log"
}
if (!(Ready 'http://127.0.0.1:3000')) {
    if (!(Test-Path "$project\dashboard\.next\BUILD_ID")) { throw 'Build the dashboard first. See README setup steps.' }
    Start-Process -FilePath $nodePath -ArgumentList @('node_modules/next/dist/bin/next','start','--hostname','127.0.0.1') -WorkingDirectory "$project\dashboard" -WindowStyle Hidden -RedirectStandardOutput "$logs\dashboard.log" -RedirectStandardError "$logs\dashboard-error.log"
}
for ($attempt = 0; $attempt -lt 30; $attempt++) {
    if ((Ready 'http://localhost:5080/health') -and (Ready 'http://127.0.0.1:3000')) {
        if (!$NoBrowser) { Start-Process 'http://127.0.0.1:3000' }
        Write-Host 'Order Sync is running. Logs are in the logs folder.'
        exit 0
    }
    Start-Sleep -Seconds 1
}
throw 'A service did not start. Read the logs folder for the error.'
