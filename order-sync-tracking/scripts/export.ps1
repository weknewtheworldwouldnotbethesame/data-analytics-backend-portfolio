$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$csv = Join-Path $project 'powerbi\data\orders.csv'
Invoke-WebRequest 'http://localhost:5080/api/analytics/export' -OutFile $csv
Write-Host 'Order data exported. Open Power BI and click Refresh.'
