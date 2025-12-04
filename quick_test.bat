@echo off
chcp 65001 >nul
echo ===================================
echo DesktopMatrix 快速测试构建
echo ===================================

REM 设置工作目录
cd /d "%~dp0"
cd DesktopMatrix

echo.
echo 当前工作目录: %CD%
echo.

echo 1. 恢复NuGet包...
dotnet restore DesktopMatrix.csproj
if %errorlevel% neq 0 (
    echo NuGet包恢复失败！
    pause
    exit /b 1
)

echo.
echo 2. 构建项目（Debug模式）...
dotnet build DesktopMatrix.csproj --configuration Debug
if %errorlevel% neq 0 (
    echo 项目构建失败！
    pause
    exit /b 1
)

echo.
echo 3. 启动应用...
echo 正在启动 DesktopMatrix...
start "" dotnet run --project DesktopMatrix.csproj

echo.
echo ===================================
echo 应用已启动！
echo 如果应用未显示，请检查是否有错误提示
echo ===================================

pause