# Cinemachine 2.9.7 — 项目使用参考

## 安装

`com.unity.cinemachine: 2.9.7` (Package Manager)

## 架构概览

```
MainCamera
├── CinemachineBrain — 管理 VCam 优先级切换和混合
└── CinemachineImpulseListener — 接收震动信号

VCam_Default (VirtualCamera)
├── Body: Transposer
├── Aim: Composer
├── Follow: BattleMidPoint
└── LookAt: BattleMidPoint

VCam_ActionCloseup (VirtualCamera)
├── Body: Transposer  
├── Aim: Composer
├── Follow: (动态设置为行动角色)
└── LookAt: (动态设置为目标角色)
```

## 等距俯视相机配置 (VCam_Default)

| 属性 | 值 | 说明 |
|------|-----|------|
| Body | Transposer | 固定偏移跟随 |
| Binding Mode | World Space | 相机不随目标旋转 |
| Follow Offset | (0, 12, -8) | 约56°俯角, 从后上方 |
| X/Y/Z Damping | 1 / 1 / 1 | 平滑跟随 |
| Aim | Composer | 保持注视点在屏幕中央 |
| Dead Zone | 0.1 x 0.1 | 小范围内不跟随 |
| Priority | 10 | 默认激活 |

## 战斗特写相机 (VCam_ActionCloseup)

| 属性 | 值 | 说明 |
|------|-----|------|
| Body | Transposer | |
| Binding Mode | World Space | |
| Follow Offset | (1.5, 3, -4) | 近景,略偏 |
| Damping | 0.5 / 0.5 / 0.5 | 快速响应 |
| Aim | Composer | |
| Dead Zone | 0.05 x 0.05 | 紧跟 |
| Priority | 0 (演出时设20) | |

## 切换逻辑

```csharp
// 攻击开始 → 切到近景
_closeupVCam.Follow = attacker.transform;
_closeupVCam.LookAt = target.transform;
_closeupVCam.Priority = 20;

// 攻击结束 → 恢复全局
await UniTask.Delay(TimeSpan.FromSeconds(duration));
_closeupVCam.Priority = 0;
// Brain 自动混合回 Default (EaseInOut 0.5s)
```

## 相机震动 (Cinemachine Impulse)

```csharp
// 在受击点触发
[SerializeField] CinemachineImpulseSource _impulseSource;

public void TriggerHitImpulse(float force = 0.5f)
{
    _impulseSource.GenerateImpulse(force);
}
```

Brain 上的 ImpulseListener 自动接收，产生屏幕震动效果。

## CinemachineTargetGroup (双角色框定)

```csharp
// 将 Player 和 Enemy 加入组
_targetGroup.AddMember(player.transform, 1f, 1f);
_targetGroup.AddMember(enemy.transform, 1f, 1f);

// VCam_Default.Follow = targetGroup.Transform
// 相机自动调整以框住两者
```

## Blend 配置

- Brain Default Blend: EaseInOut, 0.5s
- 可创建 Custom Blends asset 细化不同 VCam 对之间的过渡时间和曲线
