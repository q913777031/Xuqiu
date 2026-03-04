# 艾森豪威尔四象限任务管理工具 — 需求文档

**版本**：v1.0  
**日期**：2026.03.03  

---

## 1. 项目概述

### 1.1 背景

团队日常迭代需要对任务进行优先级管理和汇报。现有方式（文档记录、口头沟通）缺乏直观的优先级可视化和状态跟踪能力，导致汇报效率低、任务遗漏。

### 1.2 定位

团队内部自用的轻量级桌面工具，用于任务优先级管理和迭代汇报。

### 1.3 技术栈

| 项目 | 选型 |
|------|------|
| 框架 | WPF (.NET 9) |
| UI库 | MahApps.Metro + MaterialDesignInXamlToolkit |
| MVVM | CommunityToolkit.Mvvm |
| 数据库 | SQLite (单文件) |
| ORM | FreeSql |
| DI容器 | Autofac |
| 部署 | 独立 exe，与 MVBuilder 无关 |

### 1.4 约束

- 单人使用，无需多用户/网络同步
- 数据存储为本地 SQLite 文件
- 独立应用程序，不依赖 MVBuilder

---

## 2. 功能需求

### 2.1 四象限看板视图（核心）

**描述**：主界面以 2×2 网格展示四个象限，每个象限内以卡片列表展示任务。

**四个象限定义**：

| 象限 | 名称 | 颜色标识 | 行动指引 |
|------|------|----------|----------|
| Q1 | 重要 & 紧急 | 红色 | 立即做 |
| Q2 | 重要 & 不紧急 | 橙色 | 安排时间 |
| Q3 | 不重要 & 紧急 | 蓝色 | 委派他人 |
| Q4 | 不重要 & 不紧急 | 灰色 | 有空再做 |

**验收标准**：

- 四个象限面板等分布局，各象限有颜色标识和任务计数
- 每个象限内的任务以卡片形式纵向排列，支持滚动
- 象限面板头部显示：象限名称、行动指引、任务数量、添加按钮

### 2.2 任务卡片

**描述**：每个任务以卡片形式展示，包含关键信息。

**卡片字段**：

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| 标题 | string | 是 | 任务描述 |
| 负责人 | string | 否 | 指派人员 |
| 预估工期 | string | 否 | 如"3天"、"1周" |
| 状态 | enum | 是 | 未开始/进行中/已完成/阻塞 |
| 阻塞点 | string | 否 | 阻塞原因描述 |
| 创建时间 | DateTime | 自动 | 创建时自动记录 |
| 排序权重 | int | 自动 | 用于象限内排序 |

**状态颜色映射**：

| 状态 | 颜色 |
|------|------|
| 未开始 | 灰色 #9CA3AF |
| 进行中 | 蓝色 #3B82F6 |
| 已完成 | 绿色 #10B981 |
| 阻塞 | 红色 #EF4444 |

**验收标准**：

- 卡片默认展示：标题、状态标签、负责人标签、工期标签
- 阻塞状态的任务卡片额外显示阻塞点标签（红色高亮）
- 已完成的任务标题加删除线

### 2.3 拖拽调整象限

**描述**：支持将任务卡片从一个象限拖拽到另一个象限，自动更新任务的象限归属。

**验收标准**：

- 拖拽时卡片有视觉反馈（半透明 + 轻微旋转）
- 目标象限在 DragOver 时边框高亮
- 松手后任务自动归入目标象限并持久化到数据库
- 支持同象限内上下拖拽排序

### 2.4 任务 CRUD

**新增任务**：

- 每个象限头部有"+"按钮
- 点击弹出对话框，输入任务标题（必填），其余字段可选
- 确认后任务添加到对应象限末尾

**编辑任务**：

- 双击卡片或点击编辑按钮进入编辑模式
- 弹出对话框编辑所有字段
- 支持修改标题、负责人、工期、状态、阻塞点

**删除任务**：

- 卡片右上角删除按钮
- 删除前弹出确认对话框

**验收标准**：

- 所有 CRUD 操作实时持久化到 SQLite
- 操作响应时间 < 100ms

### 2.5 列表视图

**描述**：提供表格形式的全量任务列表，作为四象限视图的补充。

**表格列**：优先级（象限标签）、标题、负责人、工期、状态、阻塞点、操作

**验收标准**：

- 支持与四象限视图一键切换
- 表格内状态字段可直接下拉修改
- 按象限优先级排列（Q1 → Q2 → Q3 → Q4）

### 2.6 状态筛选

**描述**：顶部工具栏提供状态筛选按钮组。

**筛选项**：全部、未开始、进行中、已完成、阻塞

**验收标准**：

- 筛选同时作用于四象限视图和列表视图
- 筛选状态有视觉高亮
- 切换筛选时有平滑过渡

### 2.7 统计栏

**描述**：顶部显示任务统计摘要。

**统计项**：总计、进行中、已完成、阻塞

**验收标准**：

- 数字实时更新
- 各统计项有独立颜色标识

---

## 3. 数据模型

### 3.1 TaskItem 表

```sql
CREATE TABLE TaskItem (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Title       TEXT    NOT NULL,
    Quadrant    INTEGER NOT NULL DEFAULT 0,  -- 0:Q1, 1:Q2, 2:Q3, 3:Q4
    Owner       TEXT,
    Estimate    TEXT,
    Status      INTEGER NOT NULL DEFAULT 0,  -- 0:未开始, 1:进行中, 2:已完成, 3:阻塞
    Blocker     TEXT,
    SortOrder   INTEGER NOT NULL DEFAULT 0,
    CreatedAt   TEXT    NOT NULL,             -- ISO 8601 格式
    UpdatedAt   TEXT    NOT NULL              -- ISO 8601 格式
);
```

---

## 4. 项目结构

```
EisenhowerMatrix/
├── App.xaml                          # 应用入口、Autofac 容器初始化
├── Models/
│   ├── TaskItem.cs                   # 数据库实体
│   ├── QuadrantType.cs               # 象限枚举
│   └── TaskStatus.cs                 # 状态枚举
├── Data/
│   └── AppDbContext.cs               # FreeSql 初始化与配置
├── Services/
│   └── TaskService.cs                # 任务 CRUD 业务逻辑
├── ViewModels/
│   ├── MainViewModel.cs              # 主窗口 ViewModel
│   ├── TaskItemViewModel.cs          # 任务卡片 ViewModel
│   └── TaskEditDialogViewModel.cs    # 编辑对话框 ViewModel
├── Views/
│   ├── MainWindow.xaml               # 主窗口
│   ├── QuadrantPanel.xaml            # 象限面板（UserControl）
│   ├── TaskCard.xaml                 # 任务卡片（UserControl）
│   └── TaskEditDialog.xaml           # 编辑对话框
├── Converters/
│   ├── StatusToColorConverter.cs     # 状态 → 颜色
│   ├── QuadrantToColorConverter.cs   # 象限 → 颜色
│   └── BoolToVisibilityConverter.cs
├── Helpers/
│   └── DragDropHelper.cs             # 拖拽辅助类
└── Resources/
    └── Styles.xaml                    # 全局样式资源
```

---

## 5. 非功能需求

| 维度 | 要求 |
|------|------|
| 启动速度 | 冷启动 < 2 秒 |
| 操作响应 | CRUD 操作 < 100ms |
| 内存占用 | 常驻内存 < 80MB |
| 数据安全 | SQLite 文件本地存储，退出时确保写入完成 |
| 兼容性 | Windows 10/11，.NET 9 运行时 |
| 日志 | NLog，仅记录错误日志 |

---

## 6. 开发计划

| 阶段 | 内容 | 预估工期 |
|------|------|----------|
| Day 1 | 项目搭建、数据模型、FreeSql配置、TaskService | 1 天 |
| Day 2 | 四象限主界面布局、QuadrantPanel、TaskCard | 1 天 |
| Day 3 | 拖拽实现（跨象限 + 象限内排序）、编辑对话框 | 1 天 |
| Day 4 | 列表视图、状态筛选、统计栏 | 1 天 |
| Day 5 | UI打磨、样式调优、测试、打包 | 1 天 |

**总计**：5 个工作日

---

## 7. 后续可扩展方向（本期不做）

- 多人协作（共享 SQLite 或切换为 Server 数据库）
- 导出 Excel/PDF
- 甘特图/时间线视图
- 与飞书/钉钉消息通知集成
- 任务评论/讨论
- 任务标签/分类
