# DesktopMatrix - 桌面四象限任务管理器

一个基于WPF开发的现代化桌面小组件应用，采用经典的时间管理四象限方法帮助您高效管理任务。

![DesktopMatrix](https://img.shields.io/badge/.NET-9.0-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)
![License](https://img.shields.io/badge/License-MIT-green.svg)

## 🎯 功能特点

### 🎨 桌面小组件设计
- **无边框透明窗口** - 现代化毛玻璃效果背景
- **置顶显示** - 始终保持在最前端，不被其他窗口遮挡
- **可拖拽移动** - 通过顶部标题栏自由拖拽位置
- **紧凑布局** - 700x500小巧尺寸，不占用桌面空间

### 📋 四象限任务管理
- **🔥 重要紧急** (第一象限) - 立即处理的关键任务
- **💡 重要不紧急** (第二象限) - 长期规划的重要任务
- **⚡ 紧急不重要** (第三象限) - 需要快速处理的琐事
- **🌱 不重要不紧急** (第四象限) - 可以延后的休闲任务

### ⚡ 快速操作
- **一键添加** - 顶部快速输入框，选择象限即可添加
- **状态切换** - 复选框快速标记任务完成状态
- **视觉反馈** - 完成任务自动变灰，删除按钮悬停高亮
- **数据持久化** - 自动保存任务数据，重启不丢失

## 🏗️ 技术架构

### 架构设计
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Views 层      │    │  ViewModels 层  │    │   Models 层     │
│                 │    │                 │    │                 │
│ MainWindow.xaml │◄──►│ MainViewModel   │◄──►│ TaskItem        │
│ TaskCard.xaml   │    │ TaskViewModel   │    │ QuadrantType    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                    ┌─────────────────┐
                    │  Services 层    │
                    │                 │
                    │ ITaskManager    │
                    │ TaskManager     │
                    │ DataService     │
                    │EncryptionService│
                    └─────────────────┘
```

### 项目结构
```
DesktopMatrix/
├── 📁 Views/                      # 用户界面层
│   ├── MainWindow.xaml           # 主窗口界面
│   ├── MainWindow.xaml.cs        # 主窗口逻辑
│   └── TaskCard.xaml             # 任务卡片组件
│
├── 📁 ViewModels/                 # 视图模型层
│   ├── MainViewModel.cs          # 主窗口视图模型
│   └── TaskViewModel.cs          # 任务视图模型
│
├── 📁 Models/                     # 数据模型层
│   ├── TaskItem.cs               # 任务实体模型
│   └── QuadrantType.cs           # 象限类型枚举
│
├── 📁 Services/                   # 业务服务层
│   ├── ITaskManager.cs           # 任务管理接口
│   ├── TaskManager.cs            # 任务管理实现
│   ├── IDataService.cs           # 数据服务接口
│   ├── DataService.cs            # 数据服务实现
│   └── EncryptionService.cs      # 加密服务
│
├── 📁 Utils/                      # 工具类
│   ├── RelayCommand.cs           # MVVM命令实现
│   ├── BoolToStrikethroughConverter.cs  # 布尔转删除线转换器
│   ├── CompletedColorConverter.cs        # 完成状态颜色转换器
│   ├── QuadrantDisplayConverter.cs      # 象限显示转换器
│   └── Extensions.cs             # 扩展方法
│
├── 📄 App.xaml                    # 应用程序入口
├── 📄 App.xaml.cs                 # 应用程序逻辑
├── 📄 DesktopMatrix.csproj        # 项目配置文件
├── 🚀 deploy.bat                  # Windows部署脚本
├── 🚀 deploy.sh                   # Linux/Mac部署脚本
└── ⚡ quick_test.bat              # 快速测试脚本
```

### 核心技术栈
- **.NET 9.0** - 最新的.NET开发平台
- **WPF (Windows Presentation Foundation)** - Windows桌面应用框架
- **MVVM (Model-View-ViewModel)** - 清晰的架构模式
- **Newtonsoft.Json** - JSON数据序列化
- **Microsoft.Xaml.Behaviors.Wpf** - XAML行为库

## 🚀 快速开始

### 环境要求
- **.NET 9.0 SDK** 或更高版本
- **Windows 10/11** 操作系统
- **Visual Studio 2022** 或 **VS Code** (推荐)

### 安装步骤

1. **克隆项目**
   ```bash
   git clone https://github.com/your-username/desktopMatrix.git
   cd desktopMatrix
   ```

2. **快速测试运行**
   ```bash
   # Windows
   quick_test.bat

   # 或者使用命令行
   cd DesktopMatrix
   dotnet run --project DesktopMatrix.csproj
   ```

3. **完整部署到桌面**
   ```bash
   # Windows
   deploy.bat

   # Linux/Mac (需要WSL环境)
   ./deploy.sh
   ```

## 📦 部署方式

### 方式一：快速部署脚本 (推荐)

**Windows用户：**
```bash
# 双击运行或在命令行执行
deploy.bat
```

**Linux/Mac用户：**
```bash
chmod +x deploy.sh
./deploy.sh
```

部署脚本会自动：
1. 清理旧的构建文件
2. 恢复NuGet包依赖
3. 编译项目为Release版本
4. 发布为单文件可执行程序
5. 复制到用户桌面
6. 创建启动脚本

### 方式二：手动编译部署

```bash
# 1. 清理项目
cd DesktopMatrix
dotnet clean

# 2. 恢复依赖
dotnet restore

# 3. 编译项目
dotnet build --configuration Release

# 4. 发布应用
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "./publish"

# 5. 运行应用
./publish/DesktopMatrix.exe
```

### 方式三：Visual Studio发布

1. 打开 `DesktopMatrix.sln` 解决方案
2. 选择 `Release` 配置
3. 右键项目 → 发布
4. 选择目标文件夹
5. 配置发布设置：
   - 目标框架：.NET 9.0
   - 部署模式：独立
   - 目标运行时：win-x64
   - 生成单个文件：✅

## 💡 使用指南

### 基本操作

1. **添加任务**
   - 在顶部输入框输入任务名称
   - 选择目标象限 (Q1-Q4)
   - 点击 "+" 按钮添加

2. **管理任务**
   - ✅ 勾选复选框标记完成
   - 🗑️ 点击 × 按钮删除任务
   - 📝 任务卡片显示象限标签

3. **窗口操作**
   - 拖拽顶部标题栏移动位置
   - 点击右上角 × 按钮关闭应用

### 数据存储

- **存储位置**：`%USERPROFILE%/DesktopMatrix/`
- **文件格式**：JSON加密存储
- **自动保存**：实时保存，无需手动操作

## 🎨 自定义配置

### 修改主题颜色

在 `MainWindow.xaml` 中修改对应的颜色值：

```xml
<!-- 象限颜色配置 -->
<!-- 第一象限：重要且紧急 -->
<Color x:Key="Q1Primary">#D32F2F</Color>     <!-- 红色 -->
<Color x:Key="Q1Background">#FFEBEE</Color>   <!-- 浅红色 -->

<!-- 第二象限：重要不紧急 -->
<Color x:Key="Q2Primary">#1976D2</Color>     <!-- 蓝色 -->
<Color x:Key="Q2Background">#E3F2FD</Color>   <!-- 浅蓝色 -->

<!-- 第三象限：紧急不重要 -->
<Color x:Key="Q3Primary">#FF9800</Color>     <!-- 橙色 -->
<Color x:Key="Q3Background">#FFF3E0</Color>   <!-- 浅橙色 -->

<!-- 第四象限：不重要不紧急 -->
<Color x:Key="Q4Primary">#4CAF50</Color>     <!-- 绿色 -->
<Color x:Key="Q4Background">#E8F5E9</Color>   <!-- 浅绿色 -->
```

### 调整窗口尺寸

修改 `MainWindow.xaml` 中的窗口尺寸：

```xml
<Window Height="500" Width="700"     <!-- 调整这些值 -->
        MinHeight="400" MinWidth="600">   <!-- 最小尺寸限制 -->
```

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

**Q: 任务数据丢失**
- 检查权限：确保应用有写入用户目录的权限
- 查看日志：检查是否有加密/解密错误

### 日志和调试

启用详细日志：
```xml
<!-- 在 App.xaml 中添加 -->
<Application.Resources>
    <system:Boolean x:Key="EnableDebugLogging">True</system:Boolean>
</Application.Resources>
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