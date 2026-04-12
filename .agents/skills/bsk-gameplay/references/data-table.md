# 数据表系统

## 概述

数据表是策划数据的主要载体，采用 GameFramework DataTable 方案：策划在 Excel 中编辑 → 导出为 txt/bytes → 运行时加载并解析为强类型 DataRow 对象。

## 数据表类型（gameplay/gameplay/DataTable/）

| 类名 | 描述 | 对应资源文件前缀 |
|---|---|---|
| DRBattle | 战斗关卡配置 | Battle |
| DRRole | 角色属性配置 | Role |
| DRSkill | 技能配置 | Skill |
| DREntity | 实体基础配置 | Entity |
| DRProperty | 属性数值配置 | Property |
| DRVFX | 特效配置 | VFX |
| DRScene | 场景配置 | Scene |
| DRUIForm | UI 表单配置 | UIForm |

### 基类
`DataTable/Base/DataRowBase.cs` — 提供行 ID 和解析接口

## 资源位置

原始数据文件: `client/Assets/GameRes/DataTables/`
支持格式: `.txt` / `.bytes` / `.xlsx`

## 加载流程

在 `ProcedurePreload` 中批量加载：

```csharp
public static readonly string[] DataTableNames = new string[]
{
    "Entity", "Scene", "UIForm", "Role",
    "Property", "VFX", "Skill", "Battle",
};
```

通过 `AssetUtility.GetDataTableAsset(name, true)` 解析路径，由 `GameEntry.DataTable.LoadDataTable` 异步加载。

## 代码生成

编辑器工具位于 `client/Assets/Editor/`：
- `DataTableGenerator/` — DataTable 处理器和代码生成菜单
- `GameFrameworkExtension/DataTable/Editor/` — EPPlus Excel 解析、模板、可视化编辑器、DataTableConfig

### 工作流

1. 策划编辑 Excel（.xlsx）
2. 使用编辑器菜单导出为文本格式
3. 代码生成器根据表头自动生成 `DR{Name}.cs` 的解析代码
4. 将生成的 DR 类放入 `gameplay/gameplay/DataTable/`

## 扩展指南

新增数据表：
1. 创建 Excel 并定义表头
2. 通过编辑器工具导出数据并生成 DR 类代码
3. 将 DR 类放入 `gameplay/gameplay/DataTable/`
4. 将数据文件放入 `client/Assets/GameRes/DataTables/`
5. 在 `ProcedurePreload.DataTableNames` 中添加表名
6. 重新编译 gameplay 工程并 CopyDLL
