# CryptoMonitor

A small floating API data monitor for Windows and macOS.

## Features

- Fetches and displays JSON API data with configurable items.
- Shows a transparent always-on-top price window that can be dragged anywhere on screen.
- Provides refresh, settings, config folder, and exit commands from the price text right-click menu.
- Supports Chinese and English UI text.
- Provides a friendly Windows settings window for common display, window, and monitor item changes.
- Keeps `config.json` as an advanced editable configuration file.

## Build on Windows

The Windows version targets .NET Framework and can be built with the Windows-bundled C# compiler:

```powershell
.\build.ps1
```

The output is:

```text
CryptoMonitor.exe
```

## Run on Windows

```powershell
.\CryptoMonitor.exe
```

On first run, the app creates `config.json` next to the executable.

## Build on macOS

The macOS version is an AppKit menu bar app with an always-on-top floating window. Build it on macOS with:

```bash
bash macos/build.sh
```

The output is:

```text
dist/macos/CryptoMonitor.app
```

Run it with Finder or:

```bash
open dist/macos/CryptoMonitor.app
```

On first run, the macOS app creates its config at:

```text
~/Library/Application Support/CryptoMonitor/config.json
```

The macOS status bar menu can refresh, show or hide the floating window, open/reload the config, toggle login startup, and exit. The config schema is shared with the Windows version, including `items`, `displayTemplate`, `itemSeparator`, `windowFixedWidth`, `windowMinWidth`, `windowMaxWidth`, font, wrapping, background, and `startWithWindows`.

Right-click the price text to refresh, open Settings, open the config folder, or exit.

In Settings:

- `General`: language, startup option, default refresh/timeout values, and overall display text.
- `Window`: automatic/fixed width and text appearance.
- `Monitor Items`: add a blank item or a request template, enable/disable items, edit URL/method/headers/body, set display text, copy/delete/reorder items, and test a single item before saving.

Most users only need the `Monitor Items` tab: choose a data card type such as coin price, exchange rate, or website status, fill in the simple field such as `BTC`, `USD/CNY`, or `https://example.com`, click `Test`, then Save. Advanced users can still choose `Custom API` and edit URL, method, headers, body, and JSONPath display text directly.

## Configuration

Example:

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
      "id": "btc",
      "name": "BTC",
      "enabled": true,
      "url": "https://api.alternative.me/v2/ticker/?convert=USD&limit=10",
      "method": "GET",
      "template": "BTC ${$.data.1.quotes.USD.price:0.00}"
    },
    {
      "id": "eth",
      "name": "ETH",
      "enabled": true,
      "url": "https://api.alternative.me/v2/ticker/?convert=USD&limit=10",
      "method": "GET",
      "template": "ETH ${$.data.1027.quotes.USD.price:0.00}"
    }
  ],
  "windowFixedWidth": 0,
  "windowMinWidth": 190,
  "windowMaxWidth": 520
}
```

The default BTC/ETH display is only an example. Every monitor entry uses the same generic `items` structure, so the Settings window can be used for crypto prices, market indicators, service health, build status, weather, or any JSON API that can be rendered with a template. This follows the same idea as TrafficMonitor plugins such as TMFetchPlugin: each item owns its URL, request options, refresh interval, and display template.

The Settings window groups item templates by use case:

- `Market: BTC price`, `Market: USD/CNY rate`, and `Market: Fear & Greed index`: ready-to-edit market examples.
- `Service: Website status`: checks a website and renders status/latency.
- `API: GET JSON` and `API: POST JSON`: generic JSON request starters.
- `Blank item`: creates a minimal custom item.

Another item example:

```json
{
  "id": "fgi",
  "name": "FGI",
  "enabled": true,
  "url": "https://api.alternative.me/fng/",
  "method": "GET",
  "headers": {
    "Accept": "application/json"
  },
  "template": "FGI: $.data[0].value $.data[0].value_classification",
  "intervalSeconds": 3600,
  "timeoutSeconds": 10
}
```

Item fields:

- `type`: item type. Supported values are `coin`, `exchangeRate`, `httpStatus`, and `customApi`. Older items without `type` are treated as `customApi`.
- `symbol`: coin symbol for `coin` items, such as `BTC`.
- `baseCurrency` / `quoteCurrency`: currency pair fields for `exchangeRate` items.
- `id`: stable item id, used for caching.
- `name`: fallback display name when the item cannot be fetched.
- `enabled`: set `false` to keep an item in config without showing it.
- `url`: request URL.
- `method`: `GET` by default. `POST`, `PUT`, `PATCH`, `DELETE`, and `HEAD` are also accepted.
- `headers`: request headers as key/value pairs.
- `body`: optional UTF-8 request body for non-GET requests.
- `template`: free text with JSONPath placeholders. `jsonPath` is also accepted as an alias for TMFetchPlugin-style configs.
- `intervalSeconds`: optional item refresh interval. Omit it, or set it to `0`, to use the General refresh interval. The app timer uses the shortest enabled effective interval, with a minimum of 1 second.
- `timeoutSeconds`: optional item request timeout. Omit it, or set it to `0`, to use the General timeout.

Template syntax:

- `$.field.path` reads a value.
- `${$.field.path}` is useful when the path is next to punctuation or other text.
- `${$.field.path:0.00}` formats numeric values.
- `$$` renders a literal `$`, so `$$$.bitcoin.usd` becomes `$` plus the value at `$.bitcoin.usd`.
- `$.list[0].price` reads an array item.
- `$.list[-1].price` reads the last array item.
- `$..price` finds the first matching field recursively.
- Missing values render as `--`.

Overall display format:

- `displayTemplate` controls the final window text after all enabled items are rendered.
- `{items}` inserts the joined item text.
- `{count}` inserts the rendered item count.
- `{date}` inserts the current date as `yyyy-MM-dd`.
- `{time}` inserts the current time as `HH:mm:ss`.
- `itemSeparator` controls the separator used inside `{items}`.
- `startWithWindows` is `false` by default. On Windows, set it from Settings to launch CryptoMonitor when Windows starts. On macOS, toggle `Start at login` from the status bar menu or edit the same config key.

Examples:

```json
"displayTemplate": "{items}"
```

```json
"displayTemplate": "[{time}] {items}"
```

Window width and appearance:

- `windowFixedWidth`: fixed floating window width in pixels. Use `0` for automatic width.
- `windowMinWidth`: minimum automatic width.
- `windowMaxWidth`: maximum automatic width.
- `windowFontFamily`, `windowFontSize`, `windowFontBold`: window text font, size, and bold style.

`language` supports `zh-CN` and `en-US`. If omitted, the app uses the system UI language when it is Chinese, otherwise English.

Older `taskbar...` width and font keys are still accepted for compatibility, but the app now saves the newer `window...` keys. The old taskbar anchor, offset, and `showTaskbarWindow` options are ignored by the floating-window mode.

## Platform notes

- Windows uses the existing C# WinForms implementation in `src/`.
- macOS uses the native Swift/AppKit implementation in `macos/`.
- The Windows settings dialog is not ported to macOS yet; use the status bar menu to open and reload the JSON config.
