@echo off
setlocal
cd /d "%~dp0"
echo === Descargando physpanel v0.1.1 ===
echo.
set ZIP_URL=https://github.com/riverar/physpanel/releases/download/v0.1.1/physpanel_0.1.1_x86_64-pc-windows-msvc.zip
set ZIP_FILE=%TEMP%\physpanel.zip

if not exist "%ZIP_FILE%" (
    echo Descargando %ZIP_URL% ...
    curl.exe -L -s -o "%ZIP_FILE%" "%ZIP_URL%"
    if %errorlevel% neq 0 (
        echo [ERROR] No se pudo descargar. Revisa tu conexion a Internet.
        pause
        exit /b 1
    )
    echo Descarga completa.
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
echo LISTO! physpanel.exe actualizado:
dir physpanel.exe 2>nul
echo.
pause
