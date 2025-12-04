@echo off
REM 设置UTF-8编码
chcp 65001 >nul

echo ====================================
echo DesktopMatrix 部署脚本 (中文版)
echo ====================================

REM 设置工作目录
cd /d "%~dp0"
cd DesktopMatrix

echo.
echo 当前工作目录: %CD%
echo.

echo 1. 清理旧的构建文件...
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"
if exist "publish" rmdir /s /q "publish"
echo    ✓ 清理完成

echo.
echo 2. 恢复NuGet包...
dotnet restore DesktopMatrix.csproj
if %errorlevel% neq 0 (
    echo    ❌ NuGet包恢复失败！
    pause
    exit /b 1
)
echo    ✓ NuGet包恢复成功

echo.
echo 3. 编译项目...
dotnet build DesktopMatrix.csproj --configuration Release
if %errorlevel% neq 0 (
    echo    ❌ 项目编译失败！
    pause
    exit /b 1
)
echo    ✓ 项目编译成功

echo.
echo 4. 发布单文件应用...
dotnet publish DesktopMatrix.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "./publish"
if %errorlevel% neq 0 (
    echo    ❌ 应用发布失败！
    pause
    exit /b 1
)
echo    ✓ 应用发布成功

echo.
echo 5. 复制到桌面...
set DESKTOP_DIR=%USERPROFILE%\Desktop\DesktopMatrix
if not exist "%DESKTOP_DIR%" mkdir "%DESKTOP_DIR%"

if exist ".\publish\*" (
    xcopy ".\publish\*" "%DESKTOP_DIR%\" /E /Y
    echo    ✓ 文件复制成功
) else (
    echo    ❌ 发布目录为空，检查发布过程
    pause
    exit /b 1
)

echo.
echo 6. 创建快捷启动脚本...
echo @echo off > "%DESKTOP_DIR%\启动.bat"
echo chcp 65001 ^>nul >> "%DESKTOP_DIR%\启动.bat"
echo cd /d "%%~dp0" >> "%DESKTOP_DIR%\启动.bat"
echo echo 正在启动 DesktopMatrix... >> "%DESKTOP_DIR%\启动.bat"
echo start "" "DesktopMatrix.exe" >> "%DESKTOP_DIR%\启动.bat"
echo    ✓ 启动脚本创建成功

echo.
echo 7. 检查发布结果...
if exist "%DESKTOP_DIR%\DesktopMatrix.exe" (
    echo    ✓ DesktopMatrix.exe 创建成功
    echo    文件大小:
    dir "%DESKTOP_DIR%\DesktopMatrix.exe" | find "DesktopMatrix.exe"
) else (
    echo    ❌ DesktopMatrix.exe 创建失败
    dir "%DESKTOP_DIR%"
    pause
    exit /b 1
)

echo.
echo ====================================
echo 部署完成！
echo ====================================
echo 应用已发布到桌面: %DESKTOP_DIR%
echo.
echo 使用方式:
echo   - 双击桌面上的 "启动.bat" 运行应用
echo   - 或者直接运行 "DesktopMatrix.exe"
echo.

REM 询问是否立即启动
set /p choice="是否立即启动应用? (y/n): "
if /i "%choice%"=="y" (
    echo 正在启动 DesktopMatrix...
    start "" "%DESKTOP_DIR%\DesktopMatrix.exe"
) else (
    echo 应用已部署完成，随时可以启动
)

echo.
echo 按任意键退出...
pause >nul