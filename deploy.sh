#!/bin/bash
echo "==================================="
echo "DesktopMatrix 部署脚本"
echo "==================================="

# 设置工作目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"
cd DesktopMatrix

echo ""
echo "当前工作目录: $(pwd)"
echo ""

echo "1. 清理旧的构建文件..."
rm -rf bin obj publish

echo ""
echo "2. 恢复NuGet包..."
if ! dotnet restore DesktopMatrix.csproj; then
    echo "❌ NuGet包恢复失败！"
    exit 1
fi

echo ""
echo "3. 编译项目..."
if ! dotnet build DesktopMatrix.csproj --configuration Release; then
    echo "❌ 项目编译失败！"
    exit 1
fi

echo ""
echo "4. 发布单文件应用..."
if ! dotnet publish DesktopMatrix.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "./publish"; then
    echo "❌ 应用发布失败！"
    exit 1
fi

echo ""
echo "5. 复制到桌面..."
DESKTOP_DIR="$HOME/Desktop/DesktopMatrix"
mkdir -p "$DESKTOP_DIR"

if [ -d "./publish" ] && [ "$(ls -A ./publish)" ]; then
    cp -r "./publish/"* "$DESKTOP_DIR/"
    echo "✅ 文件复制成功"
else
    echo "❌ 发布目录为空，检查发布过程"
    ls -la ./publish
    exit 1
fi

echo ""
echo "6. 创建快捷启动脚本..."
cat > "$DESKTOP_DIR/启动.sh" << 'EOF'
#!/bin/bash
cd "$(dirname "$0")"
echo "正在启动 DesktopMatrix..."
./DesktopMatrix.exe
EOF
chmod +x "$DESKTOP_DIR/启动.sh"

echo ""
echo "7. 检查发布结果..."
if [ -f "$DESKTOP_DIR/DesktopMatrix.exe" ]; then
    echo "✅ DesktopMatrix.exe 创建成功"
else
    echo "❌ DesktopMatrix.exe 创建失败"
    ls -la "$DESKTOP_DIR"
    exit 1
fi

echo ""
echo "==================================="
echo "部署完成！"
echo "应用已发布到桌面: $DESKTOP_DIR"
echo "运行 '启动.sh' 启动应用"
echo "==================================="