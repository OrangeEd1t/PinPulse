# PinPulse

[English](README.md) | [简体中文](README.zh-CN.md)

PinPulse is a lightweight, always-on-top desktop monitor for Windows and macOS. It periodically requests JSON APIs and renders selected values in a draggable floating window. Although the default configuration shows BTC and ETH prices, the same item model can display exchange rates, website availability, market indicators, build status, weather, or other JSON data.

## Features

- Configurable `GET`, `POST`, `PUT`, `PATCH`, `DELETE`, and `HEAD` requests
- JSONPath-like value extraction and numeric formatting
- Independent refresh interval and timeout for each monitor item
- Cached last-known values when a refresh fails
- Draggable, always-on-top window with automatic or fixed width
- Configurable font, wrapping, background color, and transparency
- Chinese and English interface text
- Start-at-login support on Windows and macOS
- Native Windows settings window for editing and testing items

## Quick start

### Windows

Requirements:

- 64-bit Windows
- .NET Framework 4.x, including the compiler at `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe`
- PowerShell

Build and run from the repository root:

```powershell
.\build.ps1
.\PinPulse.exe
```

The build has no NuGet dependencies. It compiles every `.cs` file under `src/` and writes `PinPulse.exe` to the repository root.

On first launch, `config.json` is created beside the executable. Only one instance can run at a time.

Windows controls:

- Drag the floating text with the left mouse button.
- Right-click the tray icon to show/hide the window, refresh, open Settings, open the config folder, or exit.
- Double-click the tray icon to open Settings.
- Use the **General** tab for language, default timing, startup, and combined display text.
- Use the **Window** tab for width, font, wrapping, color, and transparency.
- Use the **Monitor Items** tab to add, duplicate, reorder, enable, test, and edit data sources.

### macOS

Requirements:

- macOS 11 or later
- Xcode Command Line Tools with `swiftc`

Build and run:

```bash
bash macos/build.sh
open dist/macos/PinPulse.app
```

The build script creates `dist/macos/PinPulse.app`. On first launch, the app creates:

```text
~/Library/Application Support/PinPulse/config.json
```

Use the menu bar item or right-click the floating window to refresh, show/hide the window, open or reload the configuration, toggle start at login, or exit. macOS currently uses JSON configuration rather than the Windows settings dialog.

## Configuration

Start with [`config.example.json`](config.example.json). Windows writes its active configuration beside `PinPulse.exe`; macOS stores it under the user Application Support directory shown above.

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

### Global fields

| Field | Purpose |
| --- | --- |
| `language` | `zh-CN` or `en-US`; defaults to the system language when omitted |
| `refreshSeconds` | Default refresh interval, from 1 to 86400 seconds |
| `requestTimeoutSeconds` | Default request timeout, from 3 to 120 seconds |
| `displayTemplate` | Final display format; must include `{items}` when saved through Windows Settings |
| `itemSeparator` | Text placed between rendered items; `\n` and `\t` escapes are supported |
| `startWithWindows` | Controls startup on both platforms; the historical key name is retained for schema compatibility |
| `windowFixedWidth` | Fixed width in pixels; `0` enables automatic width |
| `windowMinWidth` / `windowMaxWidth` | Bounds used by automatic sizing |
| `windowFontFamily` / `windowFontSize` / `windowFontBold` | Floating-window font settings |
| `windowTextWrap` | Enables multi-line wrapping |
| `windowBackgroundColor` | Background color in `#RRGGBB` format |
| `windowBackgroundTransparent` | Makes the window background transparent when `true` |
| `windowLeft` / `windowTop` | Saved window position; maintained automatically when the window is moved |

The combined display template supports `{items}`, `{count}`, `{date}`, and `{time}`. Dates use `yyyy-MM-dd`; times use `HH:mm:ss`.

Examples:

```json
"displayTemplate": "[{time}] {items}"
```

```json
"displayTemplate": "{count} feeds\n{items}"
```

### Monitor item fields

| Field | Purpose |
| --- | --- |
| `type` | `coin`, `exchangeRate`, `httpStatus`, or `customApi` |
| `symbol` | Coin symbol used by `coin` items |
| `baseCurrency` / `quoteCurrency` | Currency pair used by `exchangeRate` items |
| `id` | Stable, unique cache key |
| `name` | Display name and fallback label |
| `enabled` | Keeps an item in the file without displaying it when `false` |
| `url` | Request URL |
| `method` | HTTP method; defaults to `GET` |
| `headers` | Request headers as a JSON object |
| `body` | Optional UTF-8 request body for non-`GET`/`HEAD` methods |
| `template` | Text plus data paths used to render the response |
| `intervalSeconds` | Per-item interval; `0` or omission uses the global interval |
| `timeoutSeconds` | Per-item timeout; `0` or omission uses the global timeout |

The application wakes at the shortest effective interval among enabled items. Each item is fetched only when its own interval is due. If a request fails, the cached value is kept; before the first successful request, the fallback is `<name>: --`.

The built-in item types provide defaults:

- `coin` builds an Alternative.me request/template for known symbols.
- `exchangeRate` builds an open.er-api.com request/template from the currency pair.
- `httpStatus` measures HTTP status and response time without parsing JSON.
- `customApi` uses the supplied URL, request options, and template unchanged.

### Item template syntax

| Syntax | Result |
| --- | --- |
| `$.field.path` | Reads a nested property |
| `${$.field.path}` | Delimits a path next to punctuation or other text |
| `${$.field.path:0.00}` | Formats a numeric value |
| `$.list[0].price` | Reads an array item |
| `$.list[-1].price` | Reads the last array item |
| `$..price` | Finds the first matching field recursively |
| `$$` | Renders a literal `$` |

Missing values render as `--`. The legacy `jsonPath` item field is accepted as an alias for `template`.

## How it works

```text
Program / App delegate
        │
        ├── loads config.json and schedules the shortest refresh interval
        │
        └── Monitor service ──> HTTP requests ──> per-item cache
                                      │
                                      └── item templates ──> combined display template
                                                                  │
                                                                  └── floating window + tray/menu bar
```

Windows starts in `Program.Main`, which creates `MonitorForm`. The form owns the refresh timers, tray icon, settings workflow, window rendering, and `MonitorService`. The service requests enabled items, caches their rendered text, and delegates JSON extraction to `JsonTemplateRenderer`.

The macOS implementation follows the same data flow in native Swift/AppKit. Its app delegate coordinates the menu bar, floating panel, configuration store, startup manager, and monitor service.

## Repository structure

```text
PinPulse/
├── README.md                    English documentation
├── README.zh-CN.md              Simplified Chinese documentation
├── config.example.json          Shared configuration example
├── build.ps1                    Windows build script
├── src/
│   ├── Program.cs               Windows entry point, single-instance guard, crash logging
│   ├── MonitorForm.cs           Floating window, tray menu, timers, dragging, rendering
│   ├── SettingsForm.cs          Windows settings and item test UI
│   ├── AppConfig.cs             Config loading, validation, migration, and item defaults
│   ├── MonitorService.cs        HTTP requests, item scheduling/cache, final composition
│   ├── JsonTemplateRenderer.cs  JSON path evaluation and value formatting
│   ├── Localization.cs          Chinese and English Windows strings
│   ├── StartupManager.cs        Windows Startup-folder shortcut management
│   └── MonitorContext.cs        Legacy application context; not used by Program.Main
├── macos/
│   ├── PinPulse.swift           AppKit UI, config, networking, templates, localization
│   └── build.sh                 Builds the macOS `.app` bundle
└── assets/                      Application icon sources and generated icons
```

## Compatibility notes

- The Windows and macOS implementations share the current `items` and `window...` schema.
- Older `apiItems`, `jsonPath`, and `taskbar...` keys are read for backward compatibility. Saved configuration uses the current keys.
- Historical taskbar anchor/offset settings are ignored because the current UI is a free-floating window.
- The default public endpoints are examples and may be subject to provider availability, rate limits, or schema changes.
