# CryptoMonitor

A small Windows taskbar-embedded API data monitor.

## Features

- Fetches and displays JSON API data with configurable items.
- Embeds the price window inside the Windows taskbar window.
- Provides refresh, settings, config folder, and exit commands from the taskbar text right-click menu.
- Supports Chinese and English UI text.
- Provides a friendly settings window for common display, taskbar, and monitor item changes.
- Keeps `config.json` as an advanced editable configuration file.

## Build

This first version targets .NET Framework and can be built with the Windows-bundled C# compiler:

```powershell
.\build.ps1
```

The output is:

```text
CryptoMonitor.exe
```

## Run

```powershell
.\CryptoMonitor.exe
```

On first run, the app creates `config.json` next to the executable.

Right-click the price text in the taskbar to refresh, open Settings, open the config folder, or exit.

In Settings:

- `General`: language, refresh/timeout defaults, and overall display text.
- `Taskbar`: taskbar alignment, offset, and automatic/fixed width.
- `Monitor Items`: add common coins from the drop-down, enable/disable items, edit display text, copy/delete/reorder items, or edit advanced API fields.

Most users only need the `Monitor Items` tab: choose a coin such as BTC or ETH, click Add, then Save.

## Configuration

Example:

```json
{
  "language": "zh-CN",
  "refreshSeconds": 300,
  "requestTimeoutSeconds": 10,
  "displayTemplate": "{items}",
  "itemSeparator": "   ",
  "items": [
    {
      "id": "btc",
      "name": "BTC",
      "enabled": true,
      "url": "https://api.alternative.me/v2/ticker/?convert=USD&limit=10",
      "method": "GET",
      "template": "BTC ${$.data.1.quotes.USD.price:0.00}",
      "intervalSeconds": 300,
      "timeoutSeconds": 10
    },
    {
      "id": "eth",
      "name": "ETH",
      "enabled": true,
      "url": "https://api.alternative.me/v2/ticker/?convert=USD&limit=10",
      "method": "GET",
      "template": "ETH ${$.data.1027.quotes.USD.price:0.00}",
      "intervalSeconds": 300,
      "timeoutSeconds": 10
    }
  ],
  "showTaskbarWindow": true,
  "taskbarAnchor": "left",
  "taskbarOffsetX": 280,
  "taskbarOffsetY": 0,
  "taskbarFixedWidth": 0,
  "taskbarMinWidth": 190,
  "taskbarMaxWidth": 520
}
```

The default BTC/ETH display uses the same `items` structure as every other API. You can manage these entries from Settings, so direct JSON editing is only needed for advanced or bulk changes. This follows the same idea as TrafficMonitor plugins such as TMFetchPlugin: each item owns its URL, request options, refresh interval, and display template.

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

- `id`: stable item id, used for caching.
- `name`: fallback display name when the item cannot be fetched.
- `enabled`: set `false` to keep an item in config without showing it.
- `url`: request URL.
- `method`: `GET` by default. `POST`, `PUT`, `PATCH`, `DELETE`, and `HEAD` are also accepted.
- `headers`: request headers as key/value pairs.
- `body`: optional UTF-8 request body for non-GET requests.
- `template`: free text with JSONPath placeholders. `jsonPath` is also accepted as an alias for TMFetchPlugin-style configs.
- `intervalSeconds`: item refresh interval. The app timer uses the shortest enabled interval, with a minimum of 30 seconds.
- `timeoutSeconds`: item request timeout.

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

- `displayTemplate` controls the final taskbar text after all enabled items are rendered.
- `{items}` inserts the joined item text.
- `{count}` inserts the rendered item count.
- `{date}` inserts the current date as `yyyy-MM-dd`.
- `{time}` inserts the current time as `HH:mm:ss`.
- `itemSeparator` controls the separator used inside `{items}`.

Examples:

```json
"displayTemplate": "{items}"
```

```json
"displayTemplate": "[{time}] {items}"
```

Taskbar width:

- `taskbarFixedWidth`: fixed taskbar window width in pixels. Use `0` for automatic width.
- `taskbarMinWidth`: minimum automatic width.
- `taskbarMaxWidth`: maximum automatic width.

`language` supports `zh-CN` and `en-US`. If omitted, the app uses the system UI language when it is Chinese, otherwise English.

`showTaskbarWindow` is kept for compatibility with earlier configs. The current app always embeds the window in the taskbar so it remains reachable without a tray icon.

`taskbarAnchor` can be `left` or `right`. If it overlaps TrafficMonitor or taskbar icons, adjust `taskbarOffsetX` and restart the app.
