# 棋盘与图结构

## 概述

战棋游戏的核心地图数据结构，分布在两层：
- `gameplay/gameplay/Graph/` — 纯数据图结构
- `client/Assets/Game/Scripts/Gameplay/Tilemap/` — Unity Tilemap 封装
- `framework/game.core/Graph/` — 通用图算法工具

## 图数据层（gameplay/gameplay/Graph/）

### BoardGraph
棋盘主图，表示整个战斗地图的拓扑结构。

### LatticeNode
格子节点，棋盘的基本单元。每个格子对应一个 `LatticeGameplayEntity`。

## Unity 表现层

### TileGraph（client/.../Gameplay/Tilemap/）
Unity Tilemap 的封装，将 Unity 的 Tilemap 系统与 gameplay 层的 BoardGraph 做桥接。

### 瓦片资源（client/Assets/GameRes/Tile/）
- `Tile/` — 瓦片资源定义（chest/body/StoneFloor 等）
- `Palette/` — Tilemap 调色板

## 通用图工具（framework/game.core/Graph/）

- `ArrayGraph` — 基于数组的图实现
- `GraphExtension` — 图扩展方法（寻路/遍历等）

## 实体对应关系

| 图元素 | GameplayEntity | EntityLogic |
|---|---|---|
| 棋盘 | BoardGameplayEntity | ProceduralGraph/ 下的表现 |
| 格子 | LatticeGameplayEntity | ProceduralGraph/Lattice 表现 |
| 瓦片节点 | TileNodeGameplayEntity | TileMapGraph/ 下的表现 |
