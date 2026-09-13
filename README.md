# IGAKU

基于 **团结引擎 (Tuanjie Engine / Unity 2022.3.62t10)** 开发的 **2D 本地对战格斗游戏**。
角色与关卡取材自《崩坏：星穹铁道》（如三月七 / 开拓者），玩法流程为
**主菜单 → 角色选择 → 战斗** 的本地双人对战。

## 环境要求

- 团结引擎 (Tuanjie Engine) `2022.3.62t10`，或兼容的 Unity `2022.3.x`
- 渲染管线：Universal Render Pipeline (URP) 2D
- 依赖包见 `Packages/manifest.json`

## 打开与运行

1. 用团结引擎 / Unity Hub 打开本项目根目录（`IGAKU`）。
2. 打开场景 `Assets/_Game/Scenes/MainMenu/MainMenu.scene`。
3. 点击 Play 即可进入主菜单，按流程进入角色选择与战斗。

## 目录结构

```
Assets/
├── _Game/                  # 游戏主线内容
│   ├── Scenes/             # MainMenu / CharacterSelect / Battle 场景
│   ├── Scripts/            # 游戏逻辑（UI、战斗、角色控制）
│   ├── Characters/         # 角色美术资源（三月七、开拓者等）
│   ├── Prefabs/            # 预制体
│   ├── Animations/         # 动画
│   ├── Audio/              # BGM / 音效 / 语音
│   ├── Art / Stages/       # 美术与关卡资源
│   └── Data / Fonts/       # 数据配置与字体
├── Settings/               # URP 与项目渲染设置
└── TextMesh Pro/           # TMP 资源
ProjectSettings/            # 项目配置
Packages/                   # 依赖清单
```

## 说明

- 本仓库只收录源码与游戏资源，`Library/`、`Temp/`、`Logs/`、`UserSettings/`
  等引擎自动生成目录已在 `.gitignore` 中排除，克隆后用引擎打开会自动重建。
