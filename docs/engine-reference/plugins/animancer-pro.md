# Animancer Pro — 项目使用参考

## 安装

`client/Packages/com.kybernetik.animancer/` (UPM 嵌入式包, 需 Pro 版)

## 核心 API 速查

### 播放动画
```csharp
// 立即播放
AnimancerState state = animancer.Play(clip);

// 交叉淡入 (推荐 0.15~0.25s)
animancer.Play(clip, 0.25f, FadeMode.FixedSpeed);

// 通过 Transition (Inspector 可配置)
animancer.Play(myClipTransition);

// 在指定层播放
animancer.Layers[1].Play(clip, 0.15f);
```

### 层系统
```csharp
var fullBody  = animancer.Layers[0]; // 自动存在
var upperBody = animancer.Layers[1]; // 首次访问时创建
var additive  = animancer.Layers[2];

upperBody.Mask = upperBodyMask; // AvatarMask
additive.IsAdditive = true;

// 淡入/淡出层权重
upperBody.StartFade(1f, 0.2f); // 淡入
upperBody.StartFade(0f, 0.3f); // 淡出
```

### 动画事件
```csharp
var state = animancer.Layers[1].Play(attackClip);
state.Events(this).OnEnd = () => {
    // 攻击动画结束回调
    upperBody.StartFade(0f, 0.2f);
};
```

### ClipTransition (序列化配置)
```csharp
[SerializeField] private ClipTransition _idleClip;
[SerializeField] private ClipTransition _attackClip;

// Inspector 中配置: Clip, FadeDuration, Speed, StartTime, Events
animancer.Layers[0].Play(_idleClip);
```

## 本项目 3 层设计

| 层 | 索引 | Mask | 模式 | 动画 |
|----|------|------|------|------|
| FullBody | 0 | 无 | Override | Idle, Walk, Run, Death |
| UpperBody | 1 | UpperBodyMask | Override | Attack, Block, HitReaction |
| Additive | 2 | 无 | Additive | BreathIdle, HitFlinch |

## 注意事项

- Layers 是 Pro-Only 功能，Lite 版 Runtime Build 不可用
- ClipTransition 的 fadeDuration 在 Lite 中限制为默认 0.25s
- `state.Time = 0` 强制从头播放
- UpdateMode 初始化后不能在 Normal 和 AnimatePhysics 间切换
