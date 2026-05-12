@echo off
cd /d "%~dp0"
echo === Skaso Handheld Switcher - Build Completo ===
echo.

REM Paso 1: Descargar recursos si no existen
echo [1/3] Verificando recursos...

if not exist "physpanel.exe" (
    echo   - Descargando physpanel...
    curl.exe -L -s -o "%TEMP%\physpanel.zip" "https://github.com/riverar/physpanel/releases/download/v0.1.1/physpanel_0.1.1_x86_64-pc-windows-msvc.zip"
    powershell -ExecutionPolicy Bypass -Command "Expand-Archive -Path '%TEMP%\physpanel.zip' -DestinationPath '%~dp0' -Force" >nul 2>&1
    del "%TEMP%\physpanel.zip" /q 2>nul
)

if not exist "ViVeTool.exe" (
    echo   - Descargando ViVeTool...
    curl.exe -L -s -o "%TEMP%\vivetool.zip" "https://github.com/thebookisclosed/ViVe/releases/download/v0.3.4/ViVeTool-v0.3.4-IntelAmd.zip"
    if %errorlevel% neq 0 (
        echo   [ERROR] No se pudo descargar ViVeTool. Hazlo manual.
        pause
        exit /b 1
    )
    powershell -ExecutionPolicy Bypass -Command "Expand-Archive -Path '%TEMP%\vivetool.zip' -DestinationPath '%~dp0' -Force" >nul
    del "%TEMP%\vivetool.zip" /q 2>nul
    echo   - ViVeTool.exe, Albacore.ViVe.dll, Newtonsoft.Json.dll, FeatureDictionary.pfs
) else (
    echo   - Recursos listos
)

REM Paso 2: Limpiar
echo [2/3] Limpiando builds anteriores...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

REM Paso 3: Compilar
echo [3/3] Compilando...
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:UseAppHost=true /p:IncludeNativeLibrariesForSelfExtract=true
echo.
if %errorlevel% equ 0 (
    echo LISTO! EXE en: bin\Release\net8.0-windows\win-x64\publish\SkasoHandheldSwitcher.exe
) else (
    echo [ERROR] Compilacion fallida.
    echo Si es error de antivirus, agrega la carpeta a exclusiones de Windows Defender.
)
pause
