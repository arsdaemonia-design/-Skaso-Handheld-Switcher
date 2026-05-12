$zipUrl = "https://github.com/thebookisclosed/ViVe/releases/download/v0.3.4/ViVeTool-v0.3.4-IntelAmd.zip"
$zipPath = "$env:TEMP\ViVeTool.zip"
$extractPath = "$PSScriptRoot"

Write-Host "Descargando ViVeTool v0.3.4..." -ForegroundColor Cyan
Invoke-WebRequest -Uri $zipUrl -OutFile $zipPath -UseBasicParsing

Write-Host "Extrayendo..." -ForegroundColor Cyan
Expand-Archive -Path $zipPath -DestinationPath $extractPath -Force

Remove-Item $zipPath -Force

Write-Host "Listo! Archivos extraidos en: $extractPath" -ForegroundColor Green
Write-Host "  - ViVeTool.exe" -ForegroundColor Green
Write-Host "  - ViVeTool.dll" -ForegroundColor Green
Write-Host "  - Newtonsoft.Json.dll" -ForegroundColor Green

Write-Host ""
Write-Host "IMPORTANTE: En Visual Studio, marca estos 3 archivos como:" -ForegroundColor Yellow
Write-Host "  Accion de compilacion -> Recurso incrustado" -ForegroundColor Yellow
