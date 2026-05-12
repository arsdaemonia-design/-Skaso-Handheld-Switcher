@echo off
setlocal
cd /d "%~dp0"
echo === Descargando ViVeTool v0.3.4 ===
echo.
set ZIP_URL=https://github.com/thebookisclosed/ViVe/releases/download/v0.3.4/ViVeTool-v0.3.4-IntelAmd.zip
set ZIP_FILE=%TEMP%\ViVeTool.zip

if not exist "%ZIP_FILE%" (
    echo Descargando %ZIP_URL% ...
    curl.exe -L -s -o "%ZIP_FILE%" "%ZIP_URL%"
    if %errorlevel% neq 0 (
        echo [ERROR] No se pudo descargar. Revisa tu conexion a Internet.
        pause
        exit /b 1
    )
    echo Descarga completa.
) else (
    echo Archivo ya descargado, extrayendo...
)

echo Extrayendo...
powershell -ExecutionPolicy Bypass -Command "try { Expand-Archive -Path '%ZIP_FILE%' -DestinationPath '%~dp0' -Force; Write-Host 'OK' } catch { Write-Host 'Error: ' + $_; exit 1 }"
if %errorlevel% neq 0 (
    echo [ERROR] No se pudo extraer el ZIP.
    pause
    exit /b 1
)

del "%ZIP_FILE%" /q
echo.
echo LISTO! Archivos en la carpeta del proyecto:
dir ViVeTool.exe Albacore.ViVe.dll Newtonsoft.Json.dll FeatureDictionary.pfs 2>nul
echo.
pause
