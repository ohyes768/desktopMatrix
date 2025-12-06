@echo off
chcp 65001 >nul
echo ===================================
echo TaskMatrix 快速部署脚本
echo ===================================

REM 设置工作目录
cd /d "%~dp0"

echo 当前工作目录: %CD%
echo.

echo 1. 清理旧版本...
if exist "TaskMatrixApp\bin" rmdir /s /q "TaskMatrixApp\bin"
if exist "TaskMatrixApp\obj" rmdir /s /q "TaskMatrixApp\obj"

echo.
echo 2. 恢复依赖包...
cd TaskMatrixApp
dotnet restore
if %errorlevel% neq 0 (
    echo ❌ 包恢复失败！
    pause
    exit /b 1
)

echo.
echo 3. 编译项目...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ❌ 编译失败！
    pause
    exit /b 1
)

echo.
echo 4. 发布应用...
set DESKTOP_DIR=%USERPROFILE%\Desktop\TaskMatrix
if not exist "%DESKTOP_DIR%" mkdir "%DESKTOP_DIR%"

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "%DESKTOP_DIR%"
if %errorlevel% neq 0 (
    echo ❌ 发布失败！
    pause
    exit /b 1
)

echo.
echo 5. 清理不必要的文件...
cd /d "%DESKTOP_DIR%"
if exist "*.pdb" del /q "*.pdb"
if exist "*.xml" del /q "*.xml"

echo.
echo ===================================
echo ✅ 部署完成！
echo 安装位置: %DESKTOP_DIR%
echo 主程序: %DESKTOP_DIR%\TaskMatrix.exe
echo ===================================
echo.
echo 快捷键：
echo   Ctrl+Shift+D  - 显示/隐藏窗口
echo   Ctrl+Shift+A  - 快速添加任务
echo.

echo 按任意键启动应用...
pause >nul
start "" "TaskMatrix.exe"