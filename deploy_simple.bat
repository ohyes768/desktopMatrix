@echo off
echo ===================================
echo DesktopMatrix Deployment Script
echo ===================================

REM Set working directory
cd /d "%~dp0"
cd DesktopMatrix

echo.
echo Current directory: %CD%
echo.

echo 1. Cleaning old build files...
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"
if exist "publish" rmdir /s /q "publish"
echo    [OK] Clean completed

echo.
echo 2. Restoring NuGet packages...
dotnet restore DesktopMatrix.csproj
if %errorlevel% neq 0 (
    echo    [ERROR] NuGet restore failed!
    pause
    exit /b 1
)
echo    [OK] NuGet packages restored

echo.
echo 3. Building project...
dotnet build DesktopMatrix.csproj --configuration Release
if %errorlevel% neq 0 (
    echo    [ERROR] Build failed!
    pause
    exit /b 1
)
echo    [OK] Build completed

echo.
echo 4. Publishing single-file application...
dotnet publish DesktopMatrix.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "./publish"
if %errorlevel% neq 0 (
    echo    [ERROR] Publish failed!
    pause
    exit /b 1
)
echo    [OK] Publish completed

echo.
echo 5. Copying to desktop...
set DESKTOP_DIR=%USERPROFILE%\Desktop\DesktopMatrix
if not exist "%DESKTOP_DIR%" mkdir "%DESKTOP_DIR%"

if exist ".\publish\*" (
    xcopy ".\publish\*" "%DESKTOP_DIR%\" /E /Y
    echo    [OK] Files copied successfully
) else (
    echo    [ERROR] Publish directory is empty
    pause
    exit /b 1
)

echo.
echo 6. Creating startup script...
echo @echo off > "%DESKTOP_DIR%\Start.bat"
echo cd /d "%%~dp0" >> "%DESKTOP_DIR%\Start.bat"
echo echo Starting DesktopMatrix... >> "%DESKTOP_DIR%\Start.bat"
echo start "" "DesktopMatrix.exe" >> "%DESKTOP_DIR%\Start.bat"
echo    [OK] Startup script created

echo.
echo 7. Checking deployment result...
if exist "%DESKTOP_DIR%\DesktopMatrix.exe" (
    echo    [OK] DesktopMatrix.exe created successfully
    echo    File size:
    dir "%DESKTOP_DIR%\DesktopMatrix.exe" | find "DesktopMatrix.exe"
) else (
    echo    [ERROR] DesktopMatrix.exe not found
    dir "%DESKTOP_DIR%"
    pause
    exit /b 1
)

echo.
echo ===================================
echo Deployment Complete!
echo ===================================
echo Application published to: %DESKTOP_DIR%
echo.
echo Usage:
echo   - Double-click "Start.bat" on desktop to run
echo   - Or run "DesktopMatrix.exe" directly
echo.

REM Ask if user wants to start immediately
set /p choice="Start application now? (y/n): "
if /i "%choice%"=="y" (
    echo Starting DesktopMatrix...
    start "" "%DESKTOP_DIR%\DesktopMatrix.exe"
) else (
    echo Application is ready to run anytime
)

echo.
echo Press any key to exit...
pause >nul