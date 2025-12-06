# TaskMatrix - 任务矩阵管理器

一个基于WPF开发的现代化桌面小组件应用，采用经典的时间管理四象限方法帮助您高效管理任务。

![TaskMatrix](https://img.shields.io/badge/.NET-9.0-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)
![License](https://img.shields.io/badge/License-MIT-green.svg)

![应用预览](./taskmatrix_icon_48.png)

## 🎯 功能特点

### 🎨 桌面小组件设计
- **无边框透明窗口** - 现代化毛玻璃效果背景
- **置顶显示** - 始终保持在最前端，不被其他窗口遮挡
- **可拖拽移动** - 通过顶部标题栏自由拖拽位置
- **紧凑布局** - 280x350小巧尺寸，不占用桌面空间
- **系统托盘集成** - 最小化到托盘，不影响工作
- **自定义图标** - 专属TaskMatrix矩阵图标，提升品牌识别度

### 📋 四象限任务管理
- **🔥 重要紧急** (第一象限) - 立即处理的关键任务
- **💡 重要不紧急** (第二象限) - 长期规划的重要任务
- **⚡ 紧急不重要** (第三象限) - 需要快速处理的琐事
- **🌱 不重要不紧急** (第四象限) - 可以延后的休闲任务

### ⚡ 快速操作
- **一键添加** - 点击"添加"按钮快速创建任务
- **象限选择** - 直观的象限选择界面
- **状态切换** - 复选框快速标记任务完成状态
- **数据持久化** - SQLite数据库存储，重启不丢失
- **快捷键支持** - 键盘操作更高效

## ⌨️ 快捷键

- **Ctrl+Shift+D** - 显示/隐藏窗口
- **Ctrl+Shift+A** - 快速添加任务

## 🏗️ 技术架构

### 架构设计
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   View 层       │    │  Logic 层       │    │   Data 层       │
│                 │    │                 │    │                 │
│ MainWindow.xaml │◄──►│  TaskManager    │◄──►│ DatabaseService │
│ MainWindow.cs   │    │  TrayService    │    │   TaskItem      │
│                 │    │  HotkeyService  │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### 项目结构
```
taskmatrix/
├── 📁 DesktopWidget/               # 主应用程序目录
│   ├── 📁 Models/                  # 数据模型层
│   │   ├── TaskItem.cs             # 任务实体模型
│   │   └── QuadrantType.cs         # 象限类型枚举
│   │
│   ├── 📁 Services/                # 业务服务层
│   │   ├── DatabaseService.cs      # 数据库服务
│   │   ├── TaskManager.cs          # 任务管理器
│   │   ├── TrayService.cs          # 系统托盘服务
│   │   └── HotkeyService.cs        # 快捷键服务
│   │
│   ├── 📁 Controls/                # 自定义控件（预留）
│   ├── 📁 Utils/                   # 工具类（预留）
│   ├── 📄 MainWindow.xaml          # 主窗口界面
│   ├── 📄 MainWindow.xaml.cs       # 主窗口逻辑
│   ├── 📄 App.xaml                 # 应用程序入口
│   ├── 📄 App.xaml.cs              # 应用程序逻辑
│   ├── 📄 SimpleTest.cs            # 简单测试类
│   └── 📄 DesktopWidget.csproj     # 项目配置文件
│
├── 🎨 taskmatrix_icon.ico          # 应用图标 (48x48, 32x32, 16x16)
├── 🎨 taskmatrix_tray.ico          # 系统托盘图标 (16x16优化)
├── 🎨 taskmatrix_icon.svg          # 矢量图标源文件 (如需要)
├── 🖼️ taskmatrix_icon_48.png        # 48x48预览图标
├── 🖼️ taskmatrix_tray_16.png        # 16x16托盘图标预览
├── 🚀 deploy.bat                   # Windows部署脚本
└── 📄 查看日志说明.md               # 日志查看说明文档
```

### 核心技术栈
- **.NET 9.0** - 最新的.NET开发平台
- **WPF (Windows Presentation Foundation)** - Windows桌面应用框架
- **Microsoft.Data.Sqlite 9.0.0** - 轻量级数据库存储，兼容单文件发布
- **Microsoft.Xaml.Behaviors.Wpf 1.1.135** - WPF行为库，支持交互功能
- **Windows Forms** - 系统托盘支持
- **PIL (Python Imaging Library)** - 图标生成工具
- **架构模式** - 清晰的分层架构 (Models/Services/Controls/Utils)

## 🚀 快速开始

### 环境要求
- **.NET 9.0 SDK** 或更高版本
- **Windows 10/11** 操作系统
- **Visual Studio 2022** 或 **VS Code** (推荐)
- **Python 3.x** 和 **Pillow** (用于自定义图标开发，可选)

### 安装步骤

1. **克隆项目**
   ```bash
   git clone https://github.com/your-username/taskmatrix.git
   cd taskmatrix
   ```

2. **快速测试运行**
   ```bash
   cd DesktopWidget
   dotnet run --project DesktopWidget.csproj
   ```

3. **完整部署到桌面**
   ```bash
   # Windows - 双击运行或在命令行执行
   deploy.bat
   ```

## 📦 部署方式

### 方式一：自动部署脚本 (推荐)

**Windows用户：**
```bash
# 双击运行或在命令行执行
deploy.bat
```

部署脚本会自动：
1. 清理旧的构建文件
2. 恢复NuGet包依赖
3. 编译项目为Release版本
4. 发布为单文件可执行程序
5. 复制到用户桌面 (`%USERPROFILE%\Desktop\DesktopWidget\`)
6. 清理不必要的文件（.pdb, .xml）
7. 自动启动应用程序

### 方式二：手动编译部署

```bash
# 1. 清理项目
cd DesktopWidget
dotnet clean

# 2. 恢复依赖
dotnet restore

# 3. 编译项目
dotnet build --configuration Release

# 4. 发布应用
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "./publish"

# 5. 运行应用
./publish/DesktopWidget.exe
```

## 💡 使用指南

### 基本操作

1. **添加任务**
   - 点击"添加"按钮打开任务对话框
   - 输入任务名称
   - 选择目标象限 (Q1-Q4)
   - 点击"添加"确认

2. **管理任务**
   - ✅ 勾选复选框标记完成
   - 📝 任务自动保存到数据库

3. **窗口操作**
   - 拖拽顶部标题栏移动位置
   - 点击右上角 × 按钮隐藏到系统托盘
   - 双击系统托盘图标显示窗口

4. **快捷键操作**
   - `Ctrl+Shift+D` - 显示/隐藏主窗口
   - `Ctrl+Shift+A` - 快速打开添加任务对话框

### 数据存储

- **存储位置**：应用程序目录下的SQLite数据库
- **自动保存**：实时保存，无需手动操作
- **数据安全**：本地存储，保护隐私

## 🎨 自定义配置

### 修改主题颜色

在 `MainWindow.xaml` 中修改对应的颜色值：

```xml
<!-- 象限颜色配置 -->
<SolidColorBrush x:Key="Q1Brush" Color="#FFEBEE"/>  <!-- 重要紧急 - 红色 -->
<SolidColorBrush x:Key="Q2Brush" Color="#E3F2FD"/>  <!-- 重要不紧急 - 蓝色 -->
<SolidColorBrush x:Key="Q3Brush" Color="#FFF3E0"/>  <!-- 紧急不重要 - 橙色 -->
<SolidColorBrush x:Key="Q4Brush" Color="#E8F5E9"/>  <!-- 不重要不紧急 - 绿色 -->
```

### 调整窗口尺寸

修改 `MainWindow.xaml` 中的窗口尺寸：

```xml
<Window Height="350" Width="280"     <!-- 调整这些值 -->
        MinHeight="280" MinWidth="240">   <!-- 最小尺寸限制 -->
```

### 自定义应用图标

如需自定义应用图标，请按以下步骤：

1. **准备图标文件**
   ```bash
   # 安装Pillow (如需重新生成图标)
   pip install Pillow
   ```

2. **替换图标文件**
   - `taskmatrix_icon.ico` - 应用程序图标 (48x48, 32x32, 16x16)
   - `taskmatrix_tray.ico` - 系统托盘图标 (16x16优化)
   - `taskmatrix_icon.svg` - 矢量图标源文件 (如需要)

3. **更新项目配置**
   - 修改 `DesktopWidget.csproj` 中的 `<ApplicationIcon>` 路径
   - 图标会自动加载到系统托盘和应用程序

### 图标设计说明

- **taskmatrix_icon.ico**: 蓝色主题矩阵设计，专业科技感
  - 🔵 左上：重要紧急 (Steel Blue)
  - 🔵 右上：重要不紧急 (Cornflower Blue)
  - 🟠 左下：紧急不重要 (Dark Orange)
  - 🟠 右下：不重要不紧急 (Orange)
  - 中心标识："TM" 代表 TaskMatrix

- **taskmatrix_tray.ico**: 16x16像素优化版本，适合系统托盘显示
  - 简化的2x2网格设计
  - 保持颜色主题一致性

## 🐛 故障排除

### 常见问题

**Q: 应用无法启动**
- 确认已安装 .NET 9.0 Runtime
- 检查Windows版本是否支持
- 尝试以管理员身份运行

**Q: 编译错误**
- 清理项目：`dotnet clean`
- 重新恢复：`dotnet restore`
- 检查SDK版本：`dotnet --version`

**Q: 快捷键不工作**
- 确保应用处于前台状态
- 检查是否有其他应用占用相同快捷键
- 重启应用重新注册快捷键

**Q: 系统托盘图标不显示**
- 检查Windows通知区域设置
- 重启应用
- 确保有足够的系统权限

## 🚀 部署后文件结构

部署完成后，桌面目录结构如下：

```
Desktop\
└── DesktopWidget\
    ├── DesktopWidget.exe          # 主程序（单文件发布）
    └── [其他运行时文件]
```

## 🤝 贡献指南

欢迎提交 Issue 和 Pull Request！

### 开发规范

1. **代码风格**：遵循 C# 官方编码规范
2. **提交信息**：使用 [约定式提交](https://www.conventionalcommits.org/zh-hans/)
3. **分支策略**：`main` ← `develop` ← `feature/*`

### 开发环境设置

```bash
# 安装开发工具
dotnet tool install --global dotnet-format

# 代码格式化
dotnet format

# 运行测试
dotnet test
```

## 📄 许可证

本项目基于 MIT 许可证开源 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🙏 致谢

- 感谢所有贡献者的支持
- 灵感来源于史蒂芬·柯维的《高效能人士的七个习惯》
- UI设计参考了现代化的桌面小组件设计趋势

---

**💡 提示：** 如果这个项目对你有帮助，请给它一个 ⭐ Star！