# Desktop Widget - 概要设计方案

## 🎯 项目概述

**项目名称**: DesktopWidget - 桌面四象限任务管理小工具
**项目类型**: 桌面应用程序
**技术栈**: WPF + C# + SQLite
**开发周期**: 3-5天（核心版本）

## 📐 界面设计

### 窗口规格
```
窗口尺寸: 280 x 320 像素
最小尺寸: 240 x 280 像素
最大尺寸: 320 x 360 像素
圆角: 8 像素
透明度: 85%
```

### 布局结构
```
┌─────────────────────────────┐ 280px
│      标题栏 (30px)           │
├─────┬─────┬─────┬─────┤
│  Q1 │  Q2 │ 快  │     │ 60px
│重要 │重要 │ 速  │操   │
│紧急 │不紧 │ 操  │作   │
│(px)│急   │ 作  │(px) │
├─────┼─────┼─────┼─────┤
│  Q3 │  Q4 │     │     │ 120px
│紧急 │重要 │     │     │
│不重 │不紧 │     │     │
│要   │紧急 │     │     │
└─────┴─────┴─────┴─────┘ 320px
```

### 象限设计
```xml
<!-- 每个象限区域 -->
<Border Width="130" Height="130"
        Background="#FFEBEE"
        CornerRadius="4"
        Margin="2">
    <StackPanel>
        <TextBlock Text="🔥 Q1" FontWeight="Bold" Margin="5,2"/>
        <ScrollViewer Height="100" VerticalScrollBarVisibility="Auto">
            <ItemsControl ItemsSource="{Binding Q1Tasks}"/>
        </ScrollViewer>
    </StackPanel>
</Border>
```

## 🏗️ 技术架构

### 架构层次
```
┌─────────────────┐
│   UI Layer      │ WPF用户界面
├─────────────────┤
│ Business Layer  │ 业务逻辑处理
├─────────────────┤
│  Data Layer     │ 数据持久化
├─────────────────┤
│    Utilities    │ 工具类库
└─────────────────┘
```

### 技术选型
| 组件 | 技术选择 | 理由 |
|------|----------|------|
| UI框架 | WPF | 原生Windows支持，性能好 |
| 开发语言 | C# | 与现有项目一致，便于维护 |
| 数据库 | SQLite | 轻量级，无需安装 |
| 窗口管理 | WPF Window | 原生支持透明、置顶 |
| 系统托盘 | NotifyIcon | 标准Windows集成 |
| 配置存储 | JSON | 简单易用，调试方便 |

### 项目结构
```
DesktopWidget/
├── MainWindow.xaml              # 主窗口
├── MainWindow.xaml.cs           # 主窗口逻辑
├── App.xaml                     # 应用程序入口
├── App.xaml.cs                  # 应用程序逻辑
├── Models/                      # 数据模型
│   ├── TaskItem.cs             # 任务实体
│   ├── QuadrantType.cs         # 象限类型
│   └── WidgetConfig.cs         # 配置信息
├── Services/                    # 业务服务
│   ├── TaskManager.cs          # 任务管理服务
│   ├── DatabaseService.cs      # 数据库服务
│   └── ConfigService.cs        # 配置服务
├── Controls/                    # 自定义控件
│   ├── MiniTaskCard.xaml       # 迷你任务卡片
│   └── QuadrantPanel.xaml      # 象限面板
├── Utils/                       # 工具类
│   ├── SystemTrayManager.cs    # 系统托盘管理
│   ├── KeyboardHotkey.cs       # 快捷键处理
│   └── WindowHelper.cs         # 窗口辅助类
├── Resources/                   # 资源文件
│   ├── Styles/                  # 样式文件
│   ├── Images/                  # 图标图片
│   └── Data/                    # 数据文件
└── DesktopWidget.csproj         # 项目文件
```

## 🗄️ 数据设计

### 数据库表结构
```sql
-- 任务表
CREATE TABLE Tasks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Quadrant INTEGER NOT NULL,  -- 1-4 四个象限
    IsCompleted BOOLEAN DEFAULT 0,
    Priority INTEGER DEFAULT 0,
    CreateTime DATETIME DEFAULT CURRENT_TIMESTAMP,
    CompleteTime DATETIME NULL,
    Tags TEXT NULL,              -- JSON格式存储标签
    Description TEXT NULL
);

-- 配置表
CREATE TABLE Config (
    Key TEXT PRIMARY KEY,
    Value TEXT NOT NULL
);
```

### 数据模型
```csharp
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public QuadrantType Quadrant { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? CompleteTime { get; set; }
    public List<string> Tags { get; set; }
    public string Description { get; set; }
}
```

## 🎨 UI/UX设计

### 象限颜色方案
```csharp
public static class QuadrantColors
{
    public static readonly Color Q1 = Color.FromRgb(255, 235, 238);  // #FFEBEE
    public static readonly Color Q2 = Color.FromRgb(227, 242, 253);  // #E3F2FD
    public static readonly Color Q3 = Color.FromRgb(255, 243, 224);  // #FFF3E0
    public static readonly Color Q4 = Color.FromRgb(232, 245, 233);  // #E8F5E9
}
```

### 任务卡片设计
```xml
<UserControl Width="125" Height="25">
    <Border Background="White" CornerRadius="3" Margin="1">
        <Grid>
            <CheckBox x:Name="TaskCheckBox"
                      Content="{Binding Title}"
                      IsChecked="{Binding IsCompleted}"
                      FontSize="9" Margin="2"/>
        </Grid>
    </Border>
</UserControl>
```

## ⚡ 核心功能实现

### 1. 窗口行为控制
```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 窗口设置
        this.WindowStyle = WindowStyle.None;
        this.Topmost = true;
        this.ResizeMode = ResizeMode.CanResizeWithGrip;
        this.ShowInTaskbar = false;
        this.AllowsTransparency = true;
        this.Background = Brushes.Transparent;

        // 设置初始位置（右上角）
        SetInitialPosition();
    }

    private void SetInitialPosition()
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;

        this.Left = screenWidth - this.Width - 20;
        this.Top = 20;
    }
}
```

### 2. 任务管理服务
```csharp
public class TaskManager
{
    private readonly DatabaseService _dbService;

    public ObservableCollection<TaskItem> Q1Tasks { get; set; }
    public ObservableCollection<TaskItem> Q2Tasks { get; set; }
    public ObservableCollection<TaskItem> Q3Tasks { get; set; }
    public ObservableCollection<TaskItem> Q4Tasks { get; set; }

    public TaskManager()
    {
        _dbService = new DatabaseService();
        LoadTasks();
    }

    public TaskItem AddTask(string title, QuadrantType quadrant)
    {
        var task = new TaskItem
        {
            Title = title,
            Quadrant = quadrant,
            CreateTime = DateTime.Now
        };

        _dbService.InsertTask(task);
        AddTaskToQuadrant(task);

        return task;
    }

    public bool MoveTask(int taskId, QuadrantType targetQuadrant)
    {
        var task = _dbService.GetTask(taskId);
        if (task != null)
        {
            task.Quadrant = targetQuadrant;
            _dbService.UpdateTask(task);
            RefreshQuadrants();
            return true;
        }
        return false;
    }
}
```

### 3. 系统托盘集成
```csharp
public class SystemTrayManager
{
    private NotifyIcon _notifyIcon;

    public SystemTrayManager(MainWindow mainWindow)
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = new Icon("Resources/icon.ico"),
            Visible = true
        };

        var contextMenu = new ContextMenu();
        contextMenu.Items.Add(new MenuItem("显示", (s, e) => mainWindow.Show()));
        contextMenu.Items.Add(new MenuItem("隐藏", (s, e) => mainWindow.Hide()));
        contextMenu.Items.Add(new MenuItem("退出", (s, e) => Application.Current.Shutdown()));

        _notifyIcon.ContextMenu = contextMenu;
    }
}
```

## 🚀 开发计划

### Day 1: 核心架构搭建
- [x] 创建WPF项目结构
- [x] 设计主窗口布局
- [x] 创建数据模型
- [x] 搭建数据库服务

### Day 2: 基础功能实现
- [ ] 实现四象限界面
- [ ] 实现任务CRUD操作
- [ ] 实现数据持久化
- [ ] 基础交互逻辑

### Day 3: 高级功能实现
- [ ] 任务拖拽功能
- [ ] 系统托盘集成
- [ ] 快捷键支持
- [ ] 配置文件管理

### Day 4: 优化和测试
- [ ] 性能优化
- [ ] UI细节调整
- [ ] 异常处理
- [ ] 单元测试

### Day 5: 打包部署
- [ ] 生成安装包
- [ ] 编写使用文档
- [ ] 最终测试验证
- [ ] 版本发布

## 📋 风险评估

### 技术风险 (低)
- WPF技术成熟，有丰富文档
- SQLite数据库稳定可靠
- Windows系统集成标准化

### 时间风险 (低)
- 功能需求明确且简单
- 核心代码可复用现有项目
- 开发周期短，风险可控

### 质量风险 (低)
- 代码量少，便于维护
- 测试覆盖容易实现
- 用户体验简单直观

---

**设计版本**: v1.0
**创建日期**: 2025-12-04
**预计完成**: 2025-12-09