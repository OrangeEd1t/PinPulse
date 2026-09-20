# PinPulse

[English](README.md) | [简体中文](README.zh-CN.md)

PinPulse 是一款面向 Windows 和 macOS 的轻量级桌面数据监控工具。它会定时请求 JSON API，通过可配置模板提取数据，并显示在可拖动、始终置顶的悬浮窗中。默认配置展示 BTC 和 ETH 价格，但相同的监控项结构也可用于汇率、网站可用性、市场指标、构建状态、天气及其他 JSON 数据。

## 功能特点

- 支持配置 `GET`、`POST`、`PUT`、`PATCH`、`DELETE` 和 `HEAD` 请求
- 支持类 JSONPath 的数据提取与数字格式化
- 每个监控项可单独设置刷新间隔和超时时间
- 刷新失败时保留最近一次成功结果
- 可拖动、始终置顶的悬浮窗，支持自动或固定宽度
- 可配置字体、换行、背景颜色和透明背景
- 支持中文和英文界面
- Windows 和 macOS 均支持登录时启动
- Windows 提供可视化设置窗口，可编辑并测试监控项

## 快速开始

### Windows

环境要求：

- 64 位 Windows
- .NET Framework 4.x，并包含 `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe` 编译器
- PowerShell

在仓库根目录构建并运行：

```powershell
.\build.ps1
.\PinPulse.exe
```

构建过程不依赖 NuGet。脚本会编译 `src/` 下的全部 `.cs` 文件，并在仓库根目录生成 `PinPulse.exe`。

首次启动会在可执行文件旁创建 `config.json`。程序仅允许同时运行一个实例。

Windows 操作方式：

- 按住鼠标左键拖动悬浮文字。
- 右键单击系统托盘图标，可显示/隐藏窗口、立即刷新、打开设置、打开配置目录或退出。
- 双击托盘图标可打开设置。
- **常规**页用于设置语言、默认刷新/超时、开机启动及整体显示文本。
- **窗口**页用于设置宽度、字体、换行、背景颜色和透明度。
- **监控项**页用于新增、复制、排序、启用、测试和编辑数据源。

### macOS

环境要求：

- macOS 11 或更高版本
- 已安装带有 `swiftc` 的 Xcode Command Line Tools

构建并运行：

```bash
bash macos/build.sh
open dist/macos/PinPulse.app
```

脚本会生成 `dist/macos/PinPulse.app`。首次启动时，程序会创建：

```text
~/Library/Application Support/PinPulse/config.json
```

通过菜单栏图标或右键单击悬浮窗，可以刷新、显示/隐藏窗口、打开或重新加载配置、切换登录时启动以及退出。macOS 当前通过 JSON 文件配置，尚未移植 Windows 的可视化设置窗口。

## 配置说明

可从 [`config.example.json`](config.example.json) 开始修改。Windows 的有效配置位于 `PinPulse.exe` 旁；macOS 的有效配置位于上文所示的用户 Application Support 目录。

```json
{
  "language": "zh-CN",
  "refreshSeconds": 300,
  "requestTimeoutSeconds": 10,
  "displayTemplate": "{items}",
  "itemSeparator": "   ",
  "startWithWindows": false,
  "items": [
    {
      "type": "coin",
      "symbol": "BTC",
      "quoteCurrency": "USD",
      "id": "btc",
      "name": "BTC",
      "enabled": true,
      "url": "https://api.alternative.me/v2/ticker/?convert=USD&limit=10",
      "method": "GET",
      "template": "BTC ${$.data.1.quotes.USD.price:0.00}"
    }
  ],
  "windowFixedWidth": 0,
  "windowMinWidth": 190,
  "windowMaxWidth": 520,
  "windowFontFamily": "Microsoft YaHei UI",
  "windowFontSize": 10,
  "windowFontBold": false,
  "windowTextWrap": false,
  "windowBackgroundColor": "#FFFFFF",
  "windowBackgroundTransparent": true
}
```

### 全局字段

| 字段 | 作用 |
| --- | --- |
| `language` | `zh-CN` 或 `en-US`；省略时按系统语言选择 |
| `refreshSeconds` | 默认刷新间隔，范围为 1～86400 秒 |
| `requestTimeoutSeconds` | 默认请求超时，范围为 3～120 秒 |
| `displayTemplate` | 最终显示格式；通过 Windows 设置保存时必须包含 `{items}` |
| `itemSeparator` | 各监控项结果之间的分隔文本；支持 `\n`、`\t` 转义 |
| `startWithWindows` | 在两个平台上控制登录时启动；为兼容既有配置而保留了这个历史字段名 |
| `windowFixedWidth` | 固定宽度（像素）；设为 `0` 时自动调整宽度 |
| `windowMinWidth` / `windowMaxWidth` | 自动调整宽度时的最小值和最大值 |
| `windowFontFamily` / `windowFontSize` / `windowFontBold` | 悬浮窗字体设置 |
| `windowTextWrap` | 是否允许多行换行 |
| `windowBackgroundColor` | `#RRGGBB` 格式的背景颜色 |
| `windowBackgroundTransparent` | 为 `true` 时使用透明背景 |
| `windowLeft` / `windowTop` | 窗口位置；拖动窗口后由程序自动维护 |

整体显示模板支持 `{items}`、`{count}`、`{date}` 和 `{time}`。日期格式为 `yyyy-MM-dd`，时间格式为 `HH:mm:ss`。

示例：

```json
"displayTemplate": "[{time}] {items}"
```

```json
"displayTemplate": "共 {count} 项\n{items}"
```

### 监控项字段

| 字段 | 作用 |
| --- | --- |
| `type` | `coin`、`exchangeRate`、`httpStatus` 或 `customApi` |
| `symbol` | `coin` 类型使用的币种代码 |
| `baseCurrency` / `quoteCurrency` | `exchangeRate` 类型使用的基础/计价货币 |
| `id` | 稳定且唯一的缓存键 |
| `name` | 显示名称，也是请求失败时的回退标签 |
| `enabled` | 为 `false` 时保留配置但不显示该项 |
| `url` | 请求地址 |
| `method` | HTTP 方法，默认为 `GET` |
| `headers` | JSON 对象形式的请求头 |
| `body` | 非 `GET`/`HEAD` 请求可使用的 UTF-8 请求体 |
| `template` | 用于渲染响应的文本和数据路径 |
| `intervalSeconds` | 单项刷新间隔；设为 `0` 或省略时使用全局值 |
| `timeoutSeconds` | 单项超时时间；设为 `0` 或省略时使用全局值 |

程序按所有已启用监控项中的最短有效间隔唤醒，但只有到达各自刷新时间的监控项才会重新请求。请求失败时会继续显示缓存结果；若从未成功请求，则显示 `<名称>: --`。

内置监控项类型会自动补齐常用配置：

- `coin`：为已知币种生成 Alternative.me 请求和模板。
- `exchangeRate`：根据货币对生成 open.er-api.com 请求和模板。
- `httpStatus`：直接测量 HTTP 状态和响应耗时，不解析 JSON。
- `customApi`：原样使用填写的 URL、请求选项和模板。

### 监控项模板语法

| 语法 | 效果 |
| --- | --- |
| `$.field.path` | 读取嵌套属性 |
| `${$.field.path}` | 路径紧邻标点或其他文本时明确边界 |
| `${$.field.path:0.00}` | 格式化数值 |
| `$.list[0].price` | 读取数组元素 |
| `$.list[-1].price` | 读取最后一个数组元素 |
| `$..price` | 递归查找第一个同名字段 |
| `$$` | 输出字面量 `$` |

找不到值时显示 `--`。旧版监控项中的 `jsonPath` 字段仍可作为 `template` 的别名使用。

## 运行流程

```text
Program / App Delegate
        │
        ├── 读取 config.json，并按最短刷新间隔调度
        │
        └── 数据服务 ──> HTTP 请求 ──> 按监控项缓存
                                  │
                                  └── 单项模板 ──> 整体显示模板
                                                        │
                                                        └── 悬浮窗 + 托盘/菜单栏
```

Windows 从 `Program.Main` 启动并创建 `MonitorForm`。该窗体负责刷新计时器、托盘图标、设置流程、窗口绘制和 `MonitorService`。数据服务请求已启用的监控项、缓存渲染结果，并将 JSON 数据提取交给 `JsonTemplateRenderer`。

macOS 版本使用原生 Swift/AppKit 实现相同的数据流，由应用委托协调菜单栏、悬浮面板、配置存储、启动项管理和数据服务。

## 代码结构

```text
PinPulse/
├── README.md                    英文文档
├── README.zh-CN.md              简体中文文档
├── config.example.json          两个平台共用的配置示例
├── build.ps1                    Windows 构建脚本
├── src/
│   ├── Program.cs               Windows 入口、单实例控制、崩溃日志
│   ├── MonitorForm.cs           悬浮窗、托盘菜单、计时器、拖动和绘制
│   ├── SettingsForm.cs          Windows 设置界面和监控项测试
│   ├── AppConfig.cs             配置读取、校验、迁移和类型默认值
│   ├── MonitorService.cs        HTTP 请求、监控项调度/缓存、结果组合
│   ├── JsonTemplateRenderer.cs  JSON 路径求值和格式化
│   ├── Localization.cs          Windows 中英文界面文本
│   ├── StartupManager.cs        Windows 启动文件夹快捷方式管理
│   └── MonitorContext.cs        保留的旧应用上下文；Program.Main 当前未使用
├── macos/
│   ├── PinPulse.swift           AppKit UI、配置、网络、模板和本地化
│   └── build.sh                 构建 macOS `.app` 包
└── assets/                      应用图标源文件及生成结果
```

## 兼容性说明

- Windows 和 macOS 共用当前的 `items` 与 `window...` 配置结构。
- 为兼容旧版，仍可读取 `apiItems`、`jsonPath` 和 `taskbar...` 字段；保存时会写入当前字段。
- 旧版任务栏锚点和偏移设置在当前自由悬浮窗模式下不再生效。
- 默认公共 API 仅作为示例，可能受服务可用性、频率限制或响应结构变化影响。
