# 艾森豪威尔四象限任务管理工具 — 需求文档 v2.0

**版本**：v2.0
**日期**：2026.03.04
**基于**：v1.0 已实现功能 + 竞品调研分析

---

## 1. 项目概述

### 1.1 背景

v1.0 已实现基础的四象限看板、任务 CRUD、拖拽、列表视图、状态筛选和统计功能。通过对 13 款竞品（Priority Matrix、Amazing Marvin、Focus Matrix、TickTick、Todoist、Eisenhower.me、Notion、Trello、Keisen、PrioritAI、FocusFour、Dwight、SuperProductivity）的深度调研，发现 **Windows 桌面端缺乏高质量、功能完整的专用 Eisenhower 工具**，现有竞品普遍缺少子任务、循环任务、标签、番茄钟等关键能力。

### 1.2 定位

面向个人及小团队的 **Windows 原生桌面 Eisenhower 任务管理工具**，以四象限为核心范式（非附属视图），兼具完整任务管理深度和生产力辅助功能。

### 1.3 竞争优势

| 维度 | 本产品 | Priority Matrix | Eisenhower.me | TickTick |
|------|--------|----------------|---------------|---------|
| 平台 | Windows 原生 WPF | Windows (贵) | 无 Windows | 跨平台 |
| 核心范式 | Eisenhower 专用 | Eisenhower 专用 | Eisenhower 专用 | 附属视图 |
| 子任务 | ✅ | ✅ | ❌ | ✅ |
| 循环任务 | ✅ | 有限 | ❌ | ✅ |
| 标签系统 | ✅ | ✅ | ❌ | ✅ |
| 番茄钟 | ✅ 内置 | ❌ | 30分钟 | ✅ 内置 |
| 数据分析 | ✅ Eisenhower 专属 | 通用报告 | ❌ | 通用统计 |
| 离线优先 | ✅ SQLite | ✅ | ❌ | ✅ |
| 价格 | 免费/一次性付费 | $12/月 | 免费+订阅 | $3.99/月 |

### 1.4 技术栈

| 项目 | 选型 |
|------|------|
| 框架 | WPF (.NET 9) |
| UI库 | AntDesign.WPF |
| MVVM | CommunityToolkit.Mvvm |
| 数据库 | SQLite (FreeSql ORM) |
| DI容器 | Autofac |
| 日志 | NLog |
| 部署 | 独立 exe，离线优先 |

---

## 2. 功能清单总览

### 2.1 功能优先级矩阵

| 优先级 | 功能模块 | 状态 |
|--------|----------|------|
| **P0 已完成** | 四象限看板、任务卡片、拖拽、CRUD、列表视图、状态筛选、统计栏 | ✅ v1.0 |
| **P1 必做** | 子任务、截止日期与提醒、标签系统、暗色模式、键盘快捷键、撤销/重做、搜索 | 🔲 v2.0 |
| **P2 应做** | 番茄钟/专注模式、多看板/项目、数据分析仪表盘、CSV 导入导出、系统托盘快速添加、自定义象限名称/颜色、任务模板 | 🔲 v2.0 |
| **P3 可做** | 日历视图、每日规划、自然语言日期解析、通知中心集成、任务归档/历史、批量编辑 | 🔲 v2.x |
| **P4 远期** | AI 象限建议、语音输入、时间追踪报告、云同步（可选）、轻量协作、插件系统 | 🔲 v3.0 |

---

## 3. P1 必做功能 — 详细需求

### 3.1 子任务系统

**描述**：支持为任何任务创建一级子任务列表，子任务有独立的完成状态。

**数据模型扩展**：

```sql
ALTER TABLE TaskItem ADD COLUMN ParentId INTEGER NULL REFERENCES TaskItem(Id);
```

| 字段 | 类型 | 说明 |
|------|------|------|
| ParentId | int? | 父任务 ID，NULL 表示顶级任务 |

**交互设计**：
- 任务卡片底部显示子任务进度条（如 "2/5 完成"）
- 点击卡片展开子任务列表
- 子任务支持快速添加（卡片内输入框 + 回车）
- 子任务支持勾选完成、删除
- 父任务状态不自动联动（用户手动控制）

**验收标准**：
- 可为任意任务添加/删除子任务
- 子任务进度在卡片上可视化显示
- 子任务数据持久化到 SQLite
- 子任务不单独出现在象限卡片列表中

---

### 3.2 截止日期与提醒

**描述**：任务支持设置截止日期，到期前通过 Windows 通知中心发送提醒。

**数据模型扩展**：

```sql
ALTER TABLE TaskItem ADD COLUMN DueDate TEXT NULL;         -- ISO 8601
ALTER TABLE TaskItem ADD COLUMN ReminderTime TEXT NULL;     -- ISO 8601
ALTER TABLE TaskItem ADD COLUMN IsOverdue INTEGER DEFAULT 0;
```

**交互设计**：
- 编辑对话框增加日期选择器（DatePicker）
- 提醒时间：可选"到期时"、"提前1小时"、"提前1天"、"自定义"
- 卡片上显示截止日期标签，过期任务标签变红
- 到期提醒通过 Windows Toast Notification 推送

**验收标准**：
- 可设置/清除截止日期和提醒时间
- 过期任务在卡片上有红色过期标识
- 到期前触发 Windows 系统通知
- 统计栏增加"已过期"计数

---

### 3.3 标签/标记系统

**描述**：支持为任务添加多个彩色标签，用于分类和筛选。

**数据模型**：

```sql
CREATE TABLE Tag (
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    Name     TEXT    NOT NULL UNIQUE,
    Color    TEXT    NOT NULL DEFAULT '#3B82F6'
);

CREATE TABLE TaskTag (
    TaskId   INTEGER NOT NULL REFERENCES TaskItem(Id) ON DELETE CASCADE,
    TagId    INTEGER NOT NULL REFERENCES Tag(Id) ON DELETE CASCADE,
    PRIMARY KEY (TaskId, TagId)
);
```

**交互设计**：
- 编辑对话框增加标签选择区（已有标签多选 + 新建标签）
- 卡片上在状态标签旁显示任务标签（AntDesign Tag 组件）
- 筛选栏增加按标签筛选（支持多选）
- 标签管理：在设置中可管理所有标签（改名、改色、删除）

**预置标签建议**：
- 工作、个人、学习、健康、财务（用户可自定义）

**验收标准**：
- 可创建/编辑/删除标签
- 可为任务分配多个标签
- 按标签筛选任务
- 标签在卡片和列表视图中可视化显示

---

### 3.4 暗色模式

**描述**：支持亮色/暗色主题切换，跟随系统设置或手动切换。

**交互设计**：
- 顶部工具栏增加主题切换按钮（日/月图标）
- 三种模式：亮色、暗色、跟随系统
- 使用 AntDesign.WPF 的 `ThemeHelper.SetBaseTheme()` 切换
- 切换时平滑过渡

**验收标准**：
- 亮色/暗色模式所有界面元素正确渲染
- 四象限颜色在暗色模式下适配（降低饱和度）
- 用户偏好持久化，重启后保持

---

### 3.5 键盘快捷键

**描述**：提供完整的键盘快捷键体系，提升操作效率。

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+N` | 快速新建任务（弹出对话框） |
| `Ctrl+1/2/3/4` | 在 Q1/Q2/Q3/Q4 新建任务 |
| `Ctrl+F` | 聚焦搜索框 |
| `Ctrl+L` | 切换四象限/列表视图 |
| `Ctrl+Z` | 撤销 |
| `Ctrl+Y` | 重做 |
| `Delete` | 删除选中任务 |
| `Enter` | 编辑选中任务 |
| `Ctrl+D` | 切换暗色/亮色模式 |
| `Esc` | 关闭对话框/取消操作 |

**验收标准**：
- 所有快捷键可用且不与系统快捷键冲突
- 工具栏或菜单中显示快捷键提示

---

### 3.6 撤销/重做

**描述**：支持任务操作的撤销和重做，防止误操作。

**支持的可撤销操作**：
- 新增任务 → 撤销后删除
- 删除任务 → 撤销后恢复
- 编辑任务 → 撤销后回退到编辑前状态
- 移动任务（拖拽跨象限）→ 撤销后移回原象限
- 状态变更 → 撤销后回退状态

**实现方案**：
- 维护操作栈（最近 50 步）
- 每个操作记录 Before/After 状态快照

**验收标准**：
- `Ctrl+Z` 撤销最近操作
- `Ctrl+Y` 重做已撤销操作
- 操作栈在应用内会话期间有效

---

### 3.7 全局搜索

**描述**：支持按关键词搜索任务，实时过滤显示。

**交互设计**：
- 顶部工具栏增加搜索输入框
- 实时搜索（输入即过滤，300ms 防抖）
- 搜索范围：标题、负责人、阻塞点、标签名
- 搜索结果高亮匹配文字
- 搜索同时作用于四象限视图和列表视图

**验收标准**：
- 搜索响应时间 < 100ms
- 清空搜索框恢复完整视图
- 支持与状态筛选叠加使用

---

## 4. P2 应做功能 — 详细需求

### 4.1 番茄钟/专注模式

**描述**：内置番茄钟计时器，可关联到具体任务，支持专注模式。

**交互设计**：
- 任务卡片增加"开始专注"按钮（播放图标）
- 点击后进入专注模式：
  - 底部或侧边弹出计时器面板
  - 显示当前任务名称 + 倒计时
  - 默认 25 分钟工作 + 5 分钟休息（可自定义）
  - 每 4 个番茄钟后长休息 15 分钟
- 完成后弹出提示：继续 / 标记完成 / 休息
- 记录每个任务的累计专注时间和番茄数

**数据模型扩展**：

```sql
CREATE TABLE PomodoroRecord (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    TaskId      INTEGER NOT NULL REFERENCES TaskItem(Id),
    StartTime   TEXT    NOT NULL,
    EndTime     TEXT    NOT NULL,
    Duration    INTEGER NOT NULL,  -- 秒
    Completed   INTEGER NOT NULL DEFAULT 1
);

-- TaskItem 新增
ALTER TABLE TaskItem ADD COLUMN TotalFocusTime INTEGER DEFAULT 0;  -- 累计专注秒数
ALTER TABLE TaskItem ADD COLUMN PomodoroCount  INTEGER DEFAULT 0;  -- 完成番茄数
```

**验收标准**：
- 可从任务卡片启动番茄钟
- 计时器支持暂停、继续、放弃
- 完成番茄钟后记录到数据库
- 卡片上显示累计番茄数图标

---

### 4.2 多看板/项目管理

**描述**：支持创建多个独立看板（项目），每个看板有独立的四象限。

**数据模型扩展**：

```sql
CREATE TABLE Board (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT    NOT NULL,
    Description TEXT,
    Color       TEXT    DEFAULT '#3B82F6',
    SortOrder   INTEGER NOT NULL DEFAULT 0,
    CreatedAt   TEXT    NOT NULL,
    IsArchived  INTEGER NOT NULL DEFAULT 0
);

-- TaskItem 新增
ALTER TABLE TaskItem ADD COLUMN BoardId INTEGER NOT NULL DEFAULT 1 REFERENCES Board(Id);
```

**交互设计**：
- 左侧增加看板导航栏（可折叠）
- 看板列表支持拖拽排序
- 每个看板有独立的名称、颜色标识
- 统计栏显示当前看板的统计数据
- 支持看板归档（不删除数据）
- 默认"我的任务"看板（不可删除）

**验收标准**：
- 可创建/重命名/删除/归档看板
- 切换看板时四象限和列表视图更新
- 任务属于且仅属于一个看板
- 跨看板搜索（可选）

---

### 4.3 数据分析仪表盘

**描述**：提供 Eisenhower 专属的数据分析视图，帮助用户优化时间分配。

**统计指标**：

| 指标 | 说明 | 可视化 |
|------|------|--------|
| 象限分布 | 各象限任务数占比 | 饼图/环形图 |
| 完成趋势 | 近7/30天每日完成任务数 | 折线图 |
| 象限流转 | 任务在象限间的移动统计 | 桑基图/表格 |
| 平均停留时间 | 任务从创建到完成的平均天数（按象限） | 柱状图 |
| Q2 占比趋势 | 重要不紧急任务占比变化（越高越好） | 趋势线 |
| 阻塞分析 | 阻塞次数最多的任务/负责人 | 排行表 |
| 番茄钟统计 | 每日/每周专注时间 | 柱状图 |
| 负责人工作量 | 各负责人任务分布 | 柱状图 |

**交互设计**：
- 新增"分析"视图（与四象限、列表并列的第三个视图）
- 支持时间范围选择：近7天、近30天、近90天、自定义
- 图表可点击查看详细数据

**验收标准**：
- 图表数据实时更新
- 支持至少4种统计图表
- 可切换时间范围

---

### 4.4 CSV 导入/导出

**描述**：支持任务数据的 CSV 格式导入和导出，方便数据迁移和备份。

**导出格式**：

```csv
标题,象限,负责人,预估工期,状态,阻塞点,截止日期,标签,创建时间
"修复登录Bug",Q1,"张三","2天",进行中,"等待后端接口","2026-03-10","工作,紧急","2026-03-01"
```

**导入规则**：
- 支持 UTF-8 编码 CSV
- 首行为列名（自动识别映射）
- 必填字段：标题
- 象限默认 Q1，状态默认"未开始"
- 导入前预览，确认后写入

**验收标准**：
- 可导出当前看板/全部任务为 CSV
- 可导入 CSV 创建批量任务
- 导入时显示预览和错误提示
- 支持中文字段名

---

### 4.5 系统托盘与全局快速添加

**描述**：最小化到系统托盘，支持全局热键快速添加任务。

**交互设计**：
- 关闭窗口时最小化到系统托盘（可配置为直接退出）
- 托盘图标右键菜单：打开、快速添加、退出
- 全局热键 `Ctrl+Shift+E`：弹出轻量级快速添加窗口
  - 只需输入标题 + 选择象限
  - 回车确认，自动添加并关闭
- 托盘图标 Badge 显示待办数量

**验收标准**：
- 最小化后驻留系统托盘
- 全局热键在任何应用中可用
- 快速添加窗口 200ms 内弹出

---

### 4.6 自定义象限名称与颜色

**描述**：支持用户自定义四个象限的名称、行动指引和颜色。

**数据模型扩展**：

```sql
CREATE TABLE QuadrantConfig (
    Quadrant    INTEGER PRIMARY KEY,  -- 0-3
    Name        TEXT    NOT NULL,
    ActionHint  TEXT    NOT NULL,
    Color       TEXT    NOT NULL,
    BoardId     INTEGER NOT NULL DEFAULT 1 REFERENCES Board(Id)
);
```

**默认值**：

| 象限 | 默认名称 | 默认指引 | 默认颜色 |
|------|----------|----------|----------|
| Q1 | 重要 & 紧急 | 立即做 | #EF4444 |
| Q2 | 重要 & 不紧急 | 安排时间 | #F97316 |
| Q3 | 不重要 & 紧急 | 委派他人 | #3B82F6 |
| Q4 | 不重要 & 不紧急 | 有空再做 | #9CA3AF |

**验收标准**：
- 可在设置中修改象限名称、指引、颜色
- 修改后四象限视图和列表视图同步更新
- 每个看板可有独立的象限配置

---

### 4.7 任务模板

**描述**：支持将常用任务保存为模板，一键创建。

**数据模型**：

```sql
CREATE TABLE TaskTemplate (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT    NOT NULL,
    Title       TEXT    NOT NULL,
    Quadrant    INTEGER NOT NULL DEFAULT 0,
    Owner       TEXT,
    Estimate    TEXT,
    Tags        TEXT,    -- JSON array of tag names
    HasSubtasks TEXT     -- JSON array of subtask titles
);
```

**交互设计**：
- 编辑对话框增加"保存为模板"按钮
- 新建任务对话框增加"从模板创建"下拉
- 模板管理页面：编辑/删除模板

**验收标准**：
- 可将任务保存为模板
- 可从模板一键创建任务（含子任务）
- 模板数据持久化

---

## 5. P3 可做功能 — 简要需求

### 5.1 日历视图

- 按截止日期在日历上显示任务
- 支持日/周/月视图
- 可在日历上拖拽调整截止日期

### 5.2 每日规划

- "今日任务"视图：从各象限拉取今天要做的任务
- 支持排定每日优先执行顺序
- 与番茄钟配合使用

### 5.3 自然语言日期解析

- 输入框支持 "明天"、"下周一"、"3天后" 等自然语言
- 自动解析为日期并设置截止日期

### 5.4 通知中心集成

- 使用 Windows Toast Notification API
- 截止日期提醒
- 番茄钟完成提醒
- 每日未完成任务摘要（可配置时间）

### 5.5 任务归档与历史

- 已完成任务自动归档（可配置天数）
- 归档任务可查看但不显示在主视图
- 支持按时间范围浏览历史任务

### 5.6 批量编辑

- 列表视图中支持多选任务
- 批量修改状态、象限、负责人、标签
- 批量删除

---

## 6. P4 远期功能 — 方向描述

### 6.1 AI 象限建议

根据任务标题关键词和历史模式，建议任务应归入哪个象限。使用本地规则引擎或可选的 LLM API 调用。

### 6.2 语音输入

集成 Windows 语音识别 API，支持语音创建任务。

### 6.3 时间追踪报告

记录每个任务的实际工作时间（手动计时或番茄钟累积），生成周报/月报。

### 6.4 云同步（可选）

可选的云同步功能，数据加密后存储到用户自有云存储（OneDrive/WebDAV）。不强制联网。

### 6.5 轻量协作

通过导出/导入共享看板文件，或局域网内 P2P 同步。

### 6.6 插件系统

提供简单的插件接口，允许社区开发自定义功能。

---

## 7. 数据模型（v2.0 完整）

### 7.1 表结构

```sql
-- 看板
CREATE TABLE Board (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT    NOT NULL,
    Description TEXT,
    Color       TEXT    DEFAULT '#3B82F6',
    SortOrder   INTEGER NOT NULL DEFAULT 0,
    CreatedAt   TEXT    NOT NULL,
    IsArchived  INTEGER NOT NULL DEFAULT 0
);

-- 象限配置
CREATE TABLE QuadrantConfig (
    Quadrant    INTEGER NOT NULL,
    BoardId     INTEGER NOT NULL DEFAULT 1 REFERENCES Board(Id),
    Name        TEXT    NOT NULL,
    ActionHint  TEXT    NOT NULL,
    Color       TEXT    NOT NULL,
    PRIMARY KEY (Quadrant, BoardId)
);

-- 任务
CREATE TABLE TaskItem (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Title           TEXT    NOT NULL,
    Quadrant        INTEGER NOT NULL DEFAULT 0,
    Owner           TEXT,
    Estimate        TEXT,
    Status          INTEGER NOT NULL DEFAULT 0,
    Blocker         TEXT,
    SortOrder       INTEGER NOT NULL DEFAULT 0,
    CreatedAt       TEXT    NOT NULL,
    UpdatedAt       TEXT    NOT NULL,
    -- v2.0 新增
    BoardId         INTEGER NOT NULL DEFAULT 1 REFERENCES Board(Id),
    ParentId        INTEGER NULL REFERENCES TaskItem(Id),
    DueDate         TEXT,
    ReminderTime    TEXT,
    TotalFocusTime  INTEGER DEFAULT 0,
    PomodoroCount   INTEGER DEFAULT 0,
    IsArchived      INTEGER DEFAULT 0
);

-- 标签
CREATE TABLE Tag (
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    Name     TEXT    NOT NULL UNIQUE,
    Color    TEXT    NOT NULL DEFAULT '#3B82F6'
);

-- 任务-标签关联
CREATE TABLE TaskTag (
    TaskId   INTEGER NOT NULL REFERENCES TaskItem(Id) ON DELETE CASCADE,
    TagId    INTEGER NOT NULL REFERENCES Tag(Id) ON DELETE CASCADE,
    PRIMARY KEY (TaskId, TagId)
);

-- 番茄钟记录
CREATE TABLE PomodoroRecord (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    TaskId      INTEGER NOT NULL REFERENCES TaskItem(Id),
    StartTime   TEXT    NOT NULL,
    EndTime     TEXT    NOT NULL,
    Duration    INTEGER NOT NULL,
    Completed   INTEGER NOT NULL DEFAULT 1
);

-- 任务模板
CREATE TABLE TaskTemplate (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT    NOT NULL,
    Title       TEXT    NOT NULL,
    Quadrant    INTEGER NOT NULL DEFAULT 0,
    Owner       TEXT,
    Estimate    TEXT,
    Tags        TEXT,
    SubtaskTitles TEXT
);

-- 操作历史（撤销/重做）
CREATE TABLE OperationLog (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    OperationType TEXT NOT NULL,
    EntityType  TEXT NOT NULL,
    EntityId    INTEGER NOT NULL,
    BeforeJson  TEXT,
    AfterJson   TEXT,
    Timestamp   TEXT NOT NULL
);

-- 用户设置
CREATE TABLE AppSettings (
    Key         TEXT PRIMARY KEY,
    Value       TEXT NOT NULL
);
```

### 7.2 设置键值定义

| Key | 类型 | 默认值 | 说明 |
|-----|------|--------|------|
| `theme` | string | `Light` | 主题：Light / Dark / System |
| `pomodoro.work` | int | `25` | 工作时长（分钟） |
| `pomodoro.break` | int | `5` | 短休息时长 |
| `pomodoro.longBreak` | int | `15` | 长休息时长 |
| `pomodoro.longBreakInterval` | int | `4` | 长休息间隔 |
| `tray.minimizeToTray` | bool | `true` | 关闭时最小化到托盘 |
| `tray.globalHotkey` | string | `Ctrl+Shift+E` | 全局快速添加热键 |
| `archive.autoDays` | int | `30` | 自动归档天数（0=不自动） |
| `notification.dailySummary` | bool | `true` | 每日摘要通知 |
| `notification.dailySummaryTime` | string | `09:00` | 每日摘要时间 |
| `lastBoardId` | int | `1` | 上次打开的看板 ID |

---

## 8. UI 新增视图规划

### 8.1 视图结构

```
┌──────────────────────────────────────────────────────┐
│  [看板导航栏]  │  [主内容区]                           │
│                │                                      │
│  📋 我的任务   │  ┌─ 统计栏 ──────────────────────┐  │
│  📋 工作项目   │  │ 总计 | 进行中 | 已完成 | 阻塞 | 过期 │  │
│  📋 个人生活   │  └────────────────────────────────┘  │
│               │  ┌─ 工具栏 ──────────────────────┐  │
│  + 新建看板    │  │ 筛选 | 搜索 | 视图切换 | 🌙 | ⚙️ │  │
│               │  └────────────────────────────────┘  │
│  ─────────    │                                      │
│  ⚙️ 设置      │  ┌─ 内容 ─────────────────────────┐  │
│  📊 分析      │  │ 四象限 / 列表 / 分析 / 日历      │  │
│               │  │                                  │  │
│               │  └──────────────────────────────────┘  │
└──────────────────────────────────────────────────────┘
```

### 8.2 番茄钟面板

```
┌─────────────────────────────────┐
│  🍅 专注模式                     │
│                                 │
│     修复登录Bug                  │
│                                 │
│       ╭─────────╮              │
│       │  18:32  │              │
│       ╰─────────╯              │
│                                 │
│   [暂停]  [放弃]  [完成]         │
│                                 │
│   今日：🍅🍅🍅 (3个番茄)         │
└─────────────────────────────────┘
```

---

## 9. 非功能需求

| 维度 | 要求 |
|------|------|
| 启动速度 | 冷启动 < 2 秒 |
| 操作响应 | CRUD 操作 < 100ms |
| 内存占用 | 常驻内存 < 100MB |
| 数据安全 | SQLite 本地存储，WAL 模式，退出确保写入 |
| 兼容性 | Windows 10/11，.NET 9 |
| 无障碍 | 键盘完整可达，屏幕阅读器支持 |
| 国际化 | 预留 i18n 接口，v2.0 仅支持中文 |
| 日志 | NLog，仅记录 Error 级别 |
| 自动更新 | 预留更新检查接口（远期实现） |

---

## 10. 开发阶段规划

### v2.0 Phase 1（P1 必做）

| 模块 | 内容 | 预估 |
|------|------|------|
| 子任务 | 数据模型 + UI + CRUD | 2天 |
| 截止日期与提醒 | DatePicker + 通知 + 过期标识 | 2天 |
| 标签系统 | Tag 模型 + 关联 + 筛选 + 管理 | 2天 |
| 暗色模式 | AntDesign ThemeHelper + 持久化 | 1天 |
| 键盘快捷键 | InputBindings + 全局绑定 | 1天 |
| 撤销/重做 | 操作栈 + 命令模式 | 2天 |
| 全局搜索 | 搜索框 + 实时过滤 + 高亮 | 1天 |

### v2.0 Phase 2（P2 应做）

| 模块 | 内容 | 预估 |
|------|------|------|
| 番茄钟 | 计时器 UI + 记录 + 统计 | 3天 |
| 多看板 | Board 模型 + 导航栏 + 切换 | 2天 |
| 数据分析 | 图表组件 + 统计查询 | 3天 |
| CSV 导入导出 | 解析/生成 + 预览 | 1天 |
| 系统托盘 | NotifyIcon + 全局热键 | 1天 |
| 自定义象限 | 设置页 + 配置持久化 | 1天 |
| 任务模板 | 模板 CRUD + 应用 | 1天 |

---

## 11. 参考竞品

详见 [CompetitiveAnalysis-EisenhowerMatrix.md](./CompetitiveAnalysis-EisenhowerMatrix.md)

| 竞品 | 平台 | 核心启示 |
|------|------|----------|
| Priority Matrix | Win/Mac/Web | 深度 MS365 集成，团队协作 |
| Amazing Marvin | 全平台 | 模块化策略系统，极致自定义 |
| Focus Matrix | Apple | 简洁专注，与番茄钟配对 |
| TickTick | 全平台 | 内置番茄钟 + 习惯追踪 + 矩阵视图 |
| Todoist | 全平台 | 自然语言输入，庞大集成生态 |
| Eisenhower.me | iOS/Web | 官方品牌，专注定时器 |
| PrioritAI | 移动端 | AI 分类，语音输入 |
| SuperProductivity | Win/Mac/Linux | 开源，开发者友好 |
