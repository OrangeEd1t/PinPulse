import Cocoa
import Foundation

private let appName = "CryptoMonitor"
private let launchAgentLabel = "com.cryptomonitor.CryptoMonitor"

@main
final class CryptoMonitorApp: NSObject, NSApplicationDelegate {
    private let configStore = ConfigStore()
    private let startupManager = StartupManager()
    private let priceService = PriceService()
    private var config = AppConfig.defaults()
    private var refreshTimer: Timer?
    private var isRefreshing = false
    private var currentText = "BTC --   ETH --"
    private var statusItem: NSStatusItem?
    private var priceWindow: PriceWindowController?

    func applicationDidFinishLaunching(_ notification: Notification) {
        NSApp.setActivationPolicy(.accessory)
        config = configStore.load()

        let window = PriceWindowController(config: config)
        window.onMoved = { [weak self] origin in
            self?.saveWindowOrigin(origin)
        }
        priceWindow = window
        window.show()

        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
        statusItem?.button?.title = appName
        rebuildMenu()

        syncStartupSetting()
        refreshPrices()
        scheduleRefreshTimer()
    }

    private func scheduleRefreshTimer() {
        refreshTimer?.invalidate()
        let interval = max(AppConfig.minRefreshSeconds, config.effectivePollIntervalSeconds)
        refreshTimer = Timer.scheduledTimer(withTimeInterval: TimeInterval(interval), repeats: true) { [weak self] _ in
            self?.refreshPrices()
        }
    }

    private func refreshPrices() {
        if isRefreshing {
            return
        }

        isRefreshing = true
        priceService.fetch(config: config) { [weak self] result in
            DispatchQueue.main.async {
                guard let self = self else {
                    return
                }

                self.isRefreshing = false
                switch result {
                case .success(let text):
                    self.show(text: text, isError: false)
                case .failure:
                    self.show(text: self.localized("ApiError"), isError: true)
                }
            }
        }
    }

    private func show(text: String, isError: Bool) {
        currentText = text
        priceWindow?.update(text: text, isError: isError, config: config)
        statusItem?.button?.title = truncateStatusText(text)
    }

    private func rebuildMenu() {
        let menu = NSMenu()
        menu.addItem(makeMenuItem(localized("MenuShowHide"), action: #selector(toggleWindow)))
        menu.addItem(makeMenuItem(localized("MenuRefreshNow"), action: #selector(refreshNow), keyEquivalent: "r"))
        menu.addItem(NSMenuItem.separator())
        menu.addItem(makeMenuItem(localized("MenuOpenConfig"), action: #selector(openConfigFile)))
        menu.addItem(makeMenuItem(localized("MenuOpenConfigFolder"), action: #selector(openConfigFolder)))

        let startAtLogin = makeMenuItem(localized("StartWithLogin"), action: #selector(toggleStartAtLogin))
        startAtLogin.state = startupManager.isEnabled ? .on : .off
        menu.addItem(startAtLogin)

        menu.addItem(makeMenuItem(localized("MenuReloadConfig"), action: #selector(reloadConfig)))
        menu.addItem(NSMenuItem.separator())
        menu.addItem(makeMenuItem(localized("MenuExit"), action: #selector(quit), keyEquivalent: "q"))
        statusItem?.menu = menu
        priceWindow?.menu = menu
    }

    private func makeMenuItem(_ title: String, action: Selector, keyEquivalent: String = "") -> NSMenuItem {
        let item = NSMenuItem(title: title, action: action, keyEquivalent: keyEquivalent)
        item.target = self
        return item
    }

    @objc private func toggleWindow() {
        priceWindow?.toggle()
    }

    @objc private func refreshNow() {
        refreshPrices()
    }

    @objc private func reloadConfig() {
        config = configStore.load()
        priceWindow?.apply(config: config)
        rebuildMenu()
        syncStartupSetting()
        scheduleRefreshTimer()
        show(text: currentText, isError: false)
        refreshPrices()
    }

    @objc private func openConfigFile() {
        NSWorkspace.shared.open(configStore.configURL)
    }

    @objc private func openConfigFolder() {
        NSWorkspace.shared.open(configStore.directoryURL)
    }

    @objc private func toggleStartAtLogin() {
        let enabled = !startupManager.isEnabled
        do {
            try startupManager.setEnabled(enabled)
            config.startWithWindows = enabled
            configStore.save(config)
            rebuildMenu()
        } catch {
            show(text: localized("StartupUpdateFailed"), isError: true)
        }
    }

    @objc private func quit() {
        NSApp.terminate(nil)
    }

    private func saveWindowOrigin(_ origin: CGPoint) {
        config.windowLeft = Int(origin.x.rounded())
        config.windowTop = Int(origin.y.rounded())
        configStore.save(config)
    }

    private func syncStartupSetting() {
        do {
            try startupManager.setEnabled(config.startWithWindows)
        } catch {
            show(text: localized("StartupUpdateFailed"), isError: true)
        }
    }

    private func truncateStatusText(_ text: String) -> String {
        if text.count <= 28 {
            return text
        }

        return String(text.prefix(25)) + "..."
    }

    private func localized(_ key: String) -> String {
        return LocalizedText.text(key, language: config.language)
    }
}

private final class PriceWindowController: NSObject, NSWindowDelegate {
    private let panel: NSPanel
    private let label: NSTextField
    var onMoved: ((CGPoint) -> Void)?
    var menu: NSMenu? {
        didSet {
            panel.contentView?.menu = menu
            label.menu = menu
        }
    }

    init(config: AppConfig) {
        panel = NSPanel(
            contentRect: NSRect(x: 0, y: 0, width: 420, height: 34),
            styleMask: [.borderless, .nonactivatingPanel],
            backing: .buffered,
            defer: false)
        label = NSTextField(labelWithString: "BTC --   ETH --")
        super.init()

        panel.delegate = self
        panel.isReleasedWhenClosed = false
        panel.isFloatingPanel = true
        panel.level = .statusBar
        panel.collectionBehavior = [.canJoinAllSpaces, .fullScreenAuxiliary, .stationary]
        panel.backgroundColor = .clear
        panel.isOpaque = false
        panel.hasShadow = false
        panel.hidesOnDeactivate = false
        panel.isMovableByWindowBackground = true
        panel.ignoresMouseEvents = false

        label.alignment = .center
        label.lineBreakMode = .byTruncatingTail
        label.isSelectable = false
        label.backgroundColor = .clear
        label.drawsBackground = false
        label.translatesAutoresizingMaskIntoConstraints = false

        let content = NSView(frame: panel.contentView?.bounds ?? .zero)
        content.wantsLayer = true
        panel.contentView = content
        content.addSubview(label)
        NSLayoutConstraint.activate([
            label.leadingAnchor.constraint(equalTo: content.leadingAnchor, constant: 10),
            label.trailingAnchor.constraint(equalTo: content.trailingAnchor, constant: -10),
            label.topAnchor.constraint(equalTo: content.topAnchor, constant: 4),
            label.bottomAnchor.constraint(equalTo: content.bottomAnchor, constant: -4)
        ])

        apply(config: config)
        moveToConfiguredPosition(config)
    }

    func show() {
        panel.orderFrontRegardless()
    }

    func toggle() {
        if panel.isVisible {
            panel.orderOut(nil)
        } else {
            show()
        }
    }

    func apply(config: AppConfig) {
        label.font = Self.font(from: config)
        label.maximumNumberOfLines = config.windowTextWrap ? 0 : 1
        label.lineBreakMode = config.windowTextWrap ? .byWordWrapping : .byTruncatingTail

        if config.windowBackgroundTransparent {
            panel.contentView?.layer?.backgroundColor = NSColor.clear.cgColor
        } else {
            panel.contentView?.layer?.backgroundColor = Self.color(config.windowBackgroundColor, fallback: .white).cgColor
        }

        resize(config: config)
    }

    func update(text: String, isError: Bool, config: AppConfig) {
        label.stringValue = text
        label.textColor = isError ? NSColor(calibratedRed: 0.75, green: 0.10, blue: 0.10, alpha: 1) : .labelColor
        apply(config: config)
        show()
    }

    func windowDidMove(_ notification: Notification) {
        onMoved?(panel.frame.origin)
    }

    private func moveToConfiguredPosition(_ config: AppConfig) {
        if let left = config.windowLeft, let top = config.windowTop {
            panel.setFrameOrigin(NSPoint(x: CGFloat(left), y: CGFloat(top)))
            return
        }

        if let screen = NSScreen.main {
            let visible = screen.visibleFrame
            let frame = panel.frame
            let origin = NSPoint(
                x: visible.midX - frame.width / 2,
                y: visible.maxY - frame.height - 12)
            panel.setFrameOrigin(origin)
        } else {
            panel.center()
        }
    }

    private func resize(config: AppConfig) {
        let paddingX: CGFloat = 28
        let paddingY: CGFloat = 12
        let minWidth = CGFloat(max(80, config.windowMinWidth))
        let maxWidth = CGFloat(max(config.windowMaxWidth, config.windowMinWidth))
        let fixedWidth = config.windowFixedWidth > 0 ? CGFloat(config.windowFixedWidth) : 0
        let measuringWidth = fixedWidth > 0 ? max(1, fixedWidth - paddingX) : CGFloat.greatestFiniteMagnitude
        let attributes: [NSAttributedString.Key: Any] = [.font: label.font ?? NSFont.systemFont(ofSize: 12)]
        let bounds = (label.stringValue as NSString).boundingRect(
            with: NSSize(width: measuringWidth, height: CGFloat.greatestFiniteMagnitude),
            options: config.windowTextWrap ? [.usesLineFragmentOrigin, .usesFontLeading] : [],
            attributes: attributes)
        let width = fixedWidth > 0 ? fixedWidth : min(max(bounds.width + paddingX, minWidth), maxWidth)
        let wrapWidth = max(1, width - paddingX)
        let wrappedBounds = (label.stringValue as NSString).boundingRect(
            with: NSSize(width: wrapWidth, height: CGFloat.greatestFiniteMagnitude),
            options: [.usesLineFragmentOrigin, .usesFontLeading],
            attributes: attributes)
        let height = max(28, ceil((config.windowTextWrap ? wrappedBounds.height : bounds.height) + paddingY))
        var frame = panel.frame
        frame.size = NSSize(width: ceil(width), height: height)
        panel.setFrame(frame, display: true)
    }

    private static func font(from config: AppConfig) -> NSFont {
        let size = CGFloat(min(max(config.windowFontSize, 6), 36))
        let names = [
            config.windowFontFamily,
            "PingFang SC",
            "Helvetica Neue"
        ]

        for name in names {
            if let font = NSFont(name: name, size: size) {
                return config.windowFontBold ? NSFontManager.shared.convert(font, toHaveTrait: .boldFontMask) : font
            }
        }

        return config.windowFontBold ? NSFont.boldSystemFont(ofSize: size) : NSFont.systemFont(ofSize: size)
    }

    private static func color(_ value: String, fallback: NSColor) -> NSColor {
        var text = value.trimmingCharacters(in: .whitespacesAndNewlines)
        if text.count == 6 {
            text = "#" + text
        }

        guard text.count == 7, text.first == "#" else {
            return fallback
        }

        let hex = String(text.dropFirst())
        guard let intValue = UInt32(hex, radix: 16) else {
            return fallback
        }

        return NSColor(
            calibratedRed: CGFloat((intValue >> 16) & 0xff) / 255,
            green: CGFloat((intValue >> 8) & 0xff) / 255,
            blue: CGFloat(intValue & 0xff) / 255,
            alpha: 1)
    }
}

private struct AppConfig {
    static let minRefreshSeconds = 1
    static let maxRefreshSeconds = 86400

    var language: String
    var refreshSeconds: Int
    var requestTimeoutSeconds: Int
    var displayTemplate: String
    var itemSeparator: String
    var startWithWindows: Bool
    var windowFixedWidth: Int
    var windowMinWidth: Int
    var windowMaxWidth: Int
    var windowFontFamily: String
    var windowFontSize: Int
    var windowFontBold: Bool
    var windowTextWrap: Bool
    var windowBackgroundColor: String
    var windowBackgroundTransparent: Bool
    var windowLeft: Int?
    var windowTop: Int?
    var items: [ApiItemConfig]

    static func defaults() -> AppConfig {
        return AppConfig(
            language: Locale.preferredLanguages.first?.hasPrefix("zh") == true ? "zh-CN" : "en-US",
            refreshSeconds: 300,
            requestTimeoutSeconds: 10,
            displayTemplate: "{items}",
            itemSeparator: "   ",
            startWithWindows: false,
            windowFixedWidth: 0,
            windowMinWidth: 190,
            windowMaxWidth: 520,
            windowFontFamily: "PingFang SC",
            windowFontSize: 10,
            windowFontBold: false,
            windowTextWrap: false,
            windowBackgroundColor: "#FFFFFF",
            windowBackgroundTransparent: true,
            windowLeft: nil,
            windowTop: nil,
            items: [
                ApiItemConfig.knownCoin("BTC", dataId: "1"),
                ApiItemConfig.knownCoin("ETH", dataId: "1027")
            ])
    }

    var effectivePollIntervalSeconds: Int {
        var seconds = refreshSeconds
        for item in items where item.enabled && item.intervalSeconds > 0 && item.intervalSeconds < seconds {
            seconds = item.intervalSeconds
        }

        return min(max(seconds, Self.minRefreshSeconds), Self.maxRefreshSeconds)
    }

    var hasSavedWindowPosition: Bool {
        return windowLeft != nil && windowTop != nil
    }

    init(dictionary: [String: Any]) {
        let fallback = AppConfig.defaults()
        language = Self.string(dictionary, "language", fallback.language).hasPrefix("zh") ? "zh-CN" : "en-US"
        refreshSeconds = Self.clamp(Self.int(dictionary, "refreshSeconds", fallback.refreshSeconds), Self.minRefreshSeconds, Self.maxRefreshSeconds)
        requestTimeoutSeconds = Self.clamp(Self.int(dictionary, "requestTimeoutSeconds", fallback.requestTimeoutSeconds), 3, 120)
        displayTemplate = Self.string(dictionary, "displayTemplate", fallback.displayTemplate)
        itemSeparator = Self.string(dictionary, "itemSeparator", fallback.itemSeparator)
        startWithWindows = Self.bool(dictionary, "startWithWindows", fallback.startWithWindows)
        windowFixedWidth = Self.clamp(Self.int(dictionary, "windowFixedWidth", Self.int(dictionary, "taskbarFixedWidth", fallback.windowFixedWidth)), 0, 4000)
        windowMinWidth = Self.clamp(Self.int(dictionary, "windowMinWidth", Self.int(dictionary, "taskbarMinWidth", fallback.windowMinWidth)), 80, 4000)
        windowMaxWidth = Self.clamp(Self.int(dictionary, "windowMaxWidth", Self.int(dictionary, "taskbarMaxWidth", fallback.windowMaxWidth)), 80, 4000)
        windowFontFamily = Self.string(dictionary, "windowFontFamily", Self.string(dictionary, "taskbarFontFamily", fallback.windowFontFamily))
        windowFontSize = Self.clamp(Self.int(dictionary, "windowFontSize", Self.int(dictionary, "taskbarFontSize", fallback.windowFontSize)), 6, 36)
        windowFontBold = Self.bool(dictionary, "windowFontBold", Self.bool(dictionary, "taskbarFontBold", fallback.windowFontBold))
        windowTextWrap = Self.bool(dictionary, "windowTextWrap", Self.bool(dictionary, "taskbarTextWrap", fallback.windowTextWrap))
        windowBackgroundColor = Self.string(dictionary, "windowBackgroundColor", Self.string(dictionary, "taskbarBackgroundColor", fallback.windowBackgroundColor))
        windowBackgroundTransparent = Self.bool(dictionary, "windowBackgroundTransparent", Self.bool(dictionary, "taskbarBackgroundTransparent", fallback.windowBackgroundTransparent))
        windowLeft = Self.optionalInt(dictionary, "windowLeft")
        windowTop = Self.optionalInt(dictionary, "windowTop")

        let rawItems = dictionary["items"] as? [[String: Any]] ?? dictionary["apiItems"] as? [[String: Any]] ?? []
        items = rawItems.map { ApiItemConfig(dictionary: $0) }.filter { !$0.url.isEmpty || $0.type == ApiItemConfig.typeHttpStatus }
        if items.isEmpty {
            items = fallback.items
        }

        if windowMaxWidth < windowMinWidth {
            windowMaxWidth = windowMinWidth
        }
    }

    private init(
        language: String,
        refreshSeconds: Int,
        requestTimeoutSeconds: Int,
        displayTemplate: String,
        itemSeparator: String,
        startWithWindows: Bool,
        windowFixedWidth: Int,
        windowMinWidth: Int,
        windowMaxWidth: Int,
        windowFontFamily: String,
        windowFontSize: Int,
        windowFontBold: Bool,
        windowTextWrap: Bool,
        windowBackgroundColor: String,
        windowBackgroundTransparent: Bool,
        windowLeft: Int?,
        windowTop: Int?,
        items: [ApiItemConfig]) {
        self.language = language
        self.refreshSeconds = refreshSeconds
        self.requestTimeoutSeconds = requestTimeoutSeconds
        self.displayTemplate = displayTemplate
        self.itemSeparator = itemSeparator
        self.startWithWindows = startWithWindows
        self.windowFixedWidth = windowFixedWidth
        self.windowMinWidth = windowMinWidth
        self.windowMaxWidth = windowMaxWidth
        self.windowFontFamily = windowFontFamily
        self.windowFontSize = windowFontSize
        self.windowFontBold = windowFontBold
        self.windowTextWrap = windowTextWrap
        self.windowBackgroundColor = windowBackgroundColor
        self.windowBackgroundTransparent = windowBackgroundTransparent
        self.windowLeft = windowLeft
        self.windowTop = windowTop
        self.items = items
    }

    func dictionary() -> [String: Any] {
        var result: [String: Any] = [
            "language": language,
            "refreshSeconds": refreshSeconds,
            "requestTimeoutSeconds": requestTimeoutSeconds,
            "displayTemplate": displayTemplate.isEmpty ? "{items}" : displayTemplate,
            "itemSeparator": itemSeparator,
            "startWithWindows": startWithWindows,
            "windowFixedWidth": windowFixedWidth,
            "windowMinWidth": windowMinWidth,
            "windowMaxWidth": max(windowMaxWidth, windowMinWidth),
            "windowFontFamily": windowFontFamily,
            "windowFontSize": windowFontSize,
            "windowFontBold": windowFontBold,
            "windowTextWrap": windowTextWrap,
            "windowBackgroundColor": windowBackgroundColor,
            "windowBackgroundTransparent": windowBackgroundTransparent,
            "items": items.map { $0.dictionary() }
        ]

        if let left = windowLeft, let top = windowTop {
            result["windowLeft"] = left
            result["windowTop"] = top
        }

        return result
    }

    private static func string(_ dictionary: [String: Any], _ key: String, _ fallback: String) -> String {
        guard let value = dictionary[key] else {
            return fallback
        }

        let text = String(describing: value)
        return text.isEmpty ? fallback : text
    }

    private static func int(_ dictionary: [String: Any], _ key: String, _ fallback: Int) -> Int {
        guard let value = dictionary[key] else {
            return fallback
        }

        if let intValue = value as? Int {
            return intValue
        }

        if let numberValue = value as? NSNumber {
            return numberValue.intValue
        }

        return Int(String(describing: value)) ?? fallback
    }

    private static func optionalInt(_ dictionary: [String: Any], _ key: String) -> Int? {
        guard let value = dictionary[key] else {
            return nil
        }

        if let intValue = value as? Int {
            return intValue
        }

        if let numberValue = value as? NSNumber {
            return numberValue.intValue
        }

        return Int(String(describing: value))
    }

    private static func bool(_ dictionary: [String: Any], _ key: String, _ fallback: Bool) -> Bool {
        guard let value = dictionary[key] else {
            return fallback
        }

        if let boolValue = value as? Bool {
            return boolValue
        }

        if let numberValue = value as? NSNumber {
            return numberValue.boolValue
        }

        let text = String(describing: value).lowercased()
        if text == "true" || text == "1" || text == "yes" {
            return true
        }

        if text == "false" || text == "0" || text == "no" {
            return false
        }

        return fallback
    }

    private static func clamp(_ value: Int, _ minValue: Int, _ maxValue: Int) -> Int {
        return min(max(value, minValue), maxValue)
    }
}

private struct ApiItemConfig {
    static let typeCustomApi = "customApi"
    static let typeCoin = "coin"
    static let typeExchangeRate = "exchangeRate"
    static let typeHttpStatus = "httpStatus"

    var type: String
    var symbol: String
    var baseCurrency: String
    var quoteCurrency: String
    var id: String
    var name: String
    var enabled: Bool
    var url: String
    var method: String
    var headers: [String: String]
    var body: String
    var template: String
    var intervalSeconds: Int
    var timeoutSeconds: Int

    static func knownCoin(_ symbol: String, dataId: String) -> ApiItemConfig {
        return ApiItemConfig(
            type: typeCoin,
            symbol: symbol,
            baseCurrency: "",
            quoteCurrency: "USD",
            id: symbol.lowercased(),
            name: symbol,
            enabled: true,
            url: "https://api.alternative.me/v2/ticker/?convert=USD&limit=10",
            method: "GET",
            headers: [:],
            body: "",
            template: "\(symbol) ${$.data.\(dataId).quotes.USD.price:0.00}",
            intervalSeconds: 0,
            timeoutSeconds: 0)
    }

    init(dictionary: [String: Any]) {
        type = Self.normalizeType(AppConfig.stringValue(dictionary["type"], fallback: Self.typeCustomApi))
        symbol = AppConfig.stringValue(dictionary["symbol"], fallback: "")
        baseCurrency = AppConfig.stringValue(dictionary["baseCurrency"], fallback: "")
        quoteCurrency = AppConfig.stringValue(dictionary["quoteCurrency"], fallback: "USD")
        id = AppConfig.stringValue(dictionary["id"], fallback: "")
        name = AppConfig.stringValue(dictionary["name"], fallback: "")
        enabled = AppConfig.boolValue(dictionary["enabled"], fallback: true)
        url = AppConfig.stringValue(dictionary["url"], fallback: "")
        method = AppConfig.stringValue(dictionary["method"], fallback: "GET").uppercased()
        headers = dictionary["headers"] as? [String: String] ?? [:]
        body = AppConfig.stringValue(dictionary["body"], fallback: "")
        template = AppConfig.stringValue(dictionary["template"] ?? dictionary["jsonPath"], fallback: "")
        intervalSeconds = AppConfig.intValue(dictionary["intervalSeconds"], fallback: 0)
        timeoutSeconds = AppConfig.intValue(dictionary["timeoutSeconds"], fallback: 0)
        applyTypeDefaults()
    }

    private init(
        type: String,
        symbol: String,
        baseCurrency: String,
        quoteCurrency: String,
        id: String,
        name: String,
        enabled: Bool,
        url: String,
        method: String,
        headers: [String: String],
        body: String,
        template: String,
        intervalSeconds: Int,
        timeoutSeconds: Int) {
        self.type = type
        self.symbol = symbol
        self.baseCurrency = baseCurrency
        self.quoteCurrency = quoteCurrency
        self.id = id
        self.name = name
        self.enabled = enabled
        self.url = url
        self.method = method
        self.headers = headers
        self.body = body
        self.template = template
        self.intervalSeconds = intervalSeconds
        self.timeoutSeconds = timeoutSeconds
    }

    mutating func applyTypeDefaults() {
        type = Self.normalizeType(type)
        if type == Self.typeCoin {
            let normalizedSymbol = (symbol.isEmpty ? (name.isEmpty ? "BTC" : name) : symbol).uppercased()
            let quote = quoteCurrency.isEmpty ? "USD" : quoteCurrency.uppercased()
            symbol = normalizedSymbol
            quoteCurrency = quote
            if id.isEmpty {
                id = normalizedSymbol.lowercased()
            }
            if name.isEmpty {
                name = normalizedSymbol
            }
            if let dataId = Self.alternativeMeDataId(normalizedSymbol) {
                url = "https://api.alternative.me/v2/ticker/?convert=\(quote)&limit=10"
                method = "GET"
                template = "\(normalizedSymbol) ${$.data.\(dataId).quotes.\(quote).price:0.00} (${$.data.\(dataId).quotes.\(quote).percentage_change_24h:0.00}%)"
            }
        } else if type == Self.typeExchangeRate {
            let base = baseCurrency.isEmpty ? "USD" : baseCurrency.uppercased()
            let quote = quoteCurrency.isEmpty ? "CNY" : quoteCurrency.uppercased()
            baseCurrency = base
            quoteCurrency = quote
            if id.isEmpty {
                id = "\(base)-\(quote)".lowercased()
            }
            if name.isEmpty {
                name = "\(base)/\(quote)"
            }
            url = "https://open.er-api.com/v6/latest/\(base)"
            method = "GET"
            template = "\(base)/\(quote) ${$.rates.\(quote):0.0000}"
        } else if type == Self.typeHttpStatus {
            if name.isEmpty {
                name = "Website"
            }
            method = "GET"
            template = ""
        }
    }

    func effectiveId(_ index: Int) -> String {
        if !id.isEmpty {
            return id
        }
        if !name.isEmpty {
            return name
        }
        return "item-\(index)"
    }

    func displayName(_ index: Int) -> String {
        if !name.isEmpty {
            return name
        }
        if !id.isEmpty {
            return id
        }
        return "Item \(index + 1)"
    }

    func dictionary() -> [String: Any] {
        var result: [String: Any] = [
            "type": type,
            "id": id,
            "name": name,
            "enabled": enabled,
            "url": url,
            "method": method,
            "body": body,
            "template": template
        ]
        if !symbol.isEmpty {
            result["symbol"] = symbol
        }
        if !baseCurrency.isEmpty {
            result["baseCurrency"] = baseCurrency
        }
        if !quoteCurrency.isEmpty {
            result["quoteCurrency"] = quoteCurrency
        }
        if !headers.isEmpty {
            result["headers"] = headers
        }
        if intervalSeconds > 0 {
            result["intervalSeconds"] = intervalSeconds
        }
        if timeoutSeconds > 0 {
            result["timeoutSeconds"] = timeoutSeconds
        }
        return result
    }

    private static func normalizeType(_ type: String) -> String {
        switch type.lowercased() {
        case typeCoin.lowercased():
            return typeCoin
        case typeExchangeRate.lowercased():
            return typeExchangeRate
        case typeHttpStatus.lowercased():
            return typeHttpStatus
        default:
            return typeCustomApi
        }
    }

    private static func alternativeMeDataId(_ symbol: String) -> String? {
        return [
            "BTC": "1",
            "ETH": "1027",
            "SOL": "11733",
            "XRP": "52",
            "DOGE": "74",
            "ADA": "2010",
            "BNB": "1839",
            "TRX": "1958",
            "DOT": "11517"
        ][symbol.uppercased()]
    }
}

fileprivate extension AppConfig {
    static func stringValue(_ value: Any?, fallback: String) -> String {
        guard let value = value else {
            return fallback
        }
        let text = String(describing: value)
        return text.isEmpty ? fallback : text
    }

    static func intValue(_ value: Any?, fallback: Int) -> Int {
        guard let value = value else {
            return fallback
        }
        if let intValue = value as? Int {
            return intValue
        }
        if let numberValue = value as? NSNumber {
            return numberValue.intValue
        }
        return Int(String(describing: value)) ?? fallback
    }

    static func boolValue(_ value: Any?, fallback: Bool) -> Bool {
        guard let value = value else {
            return fallback
        }
        if let boolValue = value as? Bool {
            return boolValue
        }
        if let numberValue = value as? NSNumber {
            return numberValue.boolValue
        }
        let text = String(describing: value).lowercased()
        if text == "true" || text == "1" || text == "yes" {
            return true
        }
        if text == "false" || text == "0" || text == "no" {
            return false
        }
        return fallback
    }
}

private final class ConfigStore {
    let directoryURL: URL
    let configURL: URL

    init() {
        let support = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask).first!
        directoryURL = support.appendingPathComponent(appName, isDirectory: true)
        configURL = directoryURL.appendingPathComponent("config.json")
    }

    func load() -> AppConfig {
        do {
            try FileManager.default.createDirectory(at: directoryURL, withIntermediateDirectories: true)
            if !FileManager.default.fileExists(atPath: configURL.path) {
                let config = AppConfig.defaults()
                save(config)
                return config
            }

            let data = try Data(contentsOf: configURL)
            let json = try JSONSerialization.jsonObject(with: data)
            guard let dictionary = json as? [String: Any] else {
                return AppConfig.defaults()
            }

            return AppConfig(dictionary: dictionary)
        } catch {
            return AppConfig.defaults()
        }
    }

    func save(_ config: AppConfig) {
        do {
            try FileManager.default.createDirectory(at: directoryURL, withIntermediateDirectories: true)
            let data = try JSONSerialization.data(withJSONObject: config.dictionary(), options: [.prettyPrinted, .sortedKeys])
            try data.write(to: configURL)
        } catch {
        }
    }
}

private final class StartupManager {
    private var launchAgentURL: URL {
        let library = FileManager.default.urls(for: .libraryDirectory, in: .userDomainMask).first!
        return library
            .appendingPathComponent("LaunchAgents", isDirectory: true)
            .appendingPathComponent("\(launchAgentLabel).plist")
    }

    var isEnabled: Bool {
        return FileManager.default.fileExists(atPath: launchAgentURL.path)
    }

    func setEnabled(_ enabled: Bool) throws {
        let fileManager = FileManager.default
        if !enabled {
            if fileManager.fileExists(atPath: launchAgentURL.path) {
                try fileManager.removeItem(at: launchAgentURL)
            }
            return
        }

        guard let executable = Bundle.main.executablePath else {
            throw NSError(domain: appName, code: 1)
        }

        try fileManager.createDirectory(at: launchAgentURL.deletingLastPathComponent(), withIntermediateDirectories: true)
        let workingDirectory = URL(fileURLWithPath: executable).deletingLastPathComponent().path
        let plist = """
        <?xml version="1.0" encoding="UTF-8"?>
        <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
        <plist version="1.0">
        <dict>
          <key>Label</key>
          <string>\(launchAgentLabel)</string>
          <key>ProgramArguments</key>
          <array>
            <string>\(Self.escapeXml(executable))</string>
          </array>
          <key>RunAtLoad</key>
          <true/>
          <key>WorkingDirectory</key>
          <string>\(Self.escapeXml(workingDirectory))</string>
        </dict>
        </plist>
        """
        try plist.data(using: .utf8)!.write(to: launchAgentURL)
    }

    private static func escapeXml(_ text: String) -> String {
        return text
            .replacingOccurrences(of: "&", with: "&amp;")
            .replacingOccurrences(of: "<", with: "&lt;")
            .replacingOccurrences(of: ">", with: "&gt;")
            .replacingOccurrences(of: "\"", with: "&quot;")
    }
}

private final class PriceService {
    private struct CachedItem {
        let text: String
        let fetchedAt: Date
    }

    private var cache: [String: CachedItem] = [:]

    func fetch(config: AppConfig, completion: @escaping (Result<String, Error>) -> Void) {
        DispatchQueue.global(qos: .utility).async {
            do {
                let text = try self.fetchConfiguredItems(config: config)
                completion(.success(text))
            } catch {
                completion(.failure(error))
            }
        }
    }

    private func fetchConfiguredItems(config: AppConfig) throws -> String {
        var parts: [String] = []
        let now = Date()
        for (index, var item) in config.items.enumerated() where item.enabled {
            item.applyTypeDefaults()
            let id = item.effectiveId(index)
            let interval = item.intervalSeconds > 0 ? item.intervalSeconds : config.refreshSeconds
            let cached = cache[id]
            let shouldFetch = cached == nil || cached!.fetchedAt.addingTimeInterval(TimeInterval(interval)) <= now

            if shouldFetch {
                do {
                    let text = try render(item: item, fallbackTimeoutSeconds: config.requestTimeoutSeconds, index: index)
                    cache[id] = CachedItem(text: text, fetchedAt: now)
                } catch {
                    if cached == nil {
                        cache[id] = CachedItem(text: "\(item.displayName(index)): --", fetchedAt: now)
                    }
                }
            }

            if let text = cache[id]?.text, !text.isEmpty {
                parts.append(text)
            }
        }

        if parts.isEmpty {
            throw NSError(domain: appName, code: 2)
        }

        let joined = parts.joined(separator: TemplateRenderer.decodeEscapes(config.itemSeparator))
        return TemplateRenderer.renderDisplayTemplate(config.displayTemplate, items: joined, count: parts.count, date: now)
    }

    private func render(item: ApiItemConfig, fallbackTimeoutSeconds: Int, index: Int) throws -> String {
        if item.type == ApiItemConfig.typeHttpStatus {
            return renderHttpStatus(item: item, fallbackTimeoutSeconds: fallbackTimeoutSeconds, index: index)
        }

        let raw = try download(item: item, fallbackTimeoutSeconds: fallbackTimeoutSeconds)
        return try TemplateRenderer.render(json: raw, template: TemplateRenderer.decodeEscapes(item.template))
    }

    private func renderHttpStatus(item: ApiItemConfig, fallbackTimeoutSeconds: Int, index: Int) -> String {
        let started = Date()
        do {
            _ = try download(item: item, fallbackTimeoutSeconds: fallbackTimeoutSeconds)
            let milliseconds = Int(Date().timeIntervalSince(started) * 1000)
            return "\(item.displayName(index)) OK \(milliseconds)ms"
        } catch {
            return "\(item.displayName(index)) ERR"
        }
    }

    private func download(item: ApiItemConfig, fallbackTimeoutSeconds: Int) throws -> String {
        guard let url = URL(string: item.url) else {
            throw NSError(domain: appName, code: 3)
        }

        let timeout = TimeInterval(item.timeoutSeconds > 0 ? item.timeoutSeconds : fallbackTimeoutSeconds)
        var request = URLRequest(url: url, timeoutInterval: timeout)
        request.httpMethod = item.method.isEmpty ? "GET" : item.method
        request.setValue("CryptoMonitor/0.1", forHTTPHeaderField: "User-Agent")
        for (key, value) in item.headers {
            request.setValue(value, forHTTPHeaderField: key)
        }

        if !item.body.isEmpty && request.httpMethod != "GET" && request.httpMethod != "HEAD" {
            request.httpBody = item.body.data(using: .utf8)
            if request.value(forHTTPHeaderField: "Content-Type") == nil {
                request.setValue("application/json", forHTTPHeaderField: "Content-Type")
            }
        }

        let semaphore = DispatchSemaphore(value: 0)
        var result: Result<String, Error>?
        URLSession(configuration: .ephemeral).dataTask(with: request) { data, response, error in
            defer {
                semaphore.signal()
            }

            if let error = error {
                result = .failure(error)
                return
            }

            if let http = response as? HTTPURLResponse, http.statusCode >= 400 {
                result = .failure(NSError(domain: appName, code: http.statusCode))
                return
            }

            guard let data = data, let text = String(data: data, encoding: .utf8) else {
                result = .failure(NSError(domain: appName, code: 4))
                return
            }

            result = .success(text)
        }.resume()

        if semaphore.wait(timeout: .now() + timeout + 2) == .timedOut {
            throw NSError(domain: appName, code: 5)
        }

        return try result!.get()
    }
}

private enum TemplateRenderer {
    static func render(json: String, template: String) throws -> String {
        if template.isEmpty {
            return json
        }

        let data = Data(json.utf8)
        let root = try JSONSerialization.jsonObject(with: data)
        var output = ""
        var index = template.startIndex
        while index < template.endIndex {
            if template[index] != "$" {
                output.append(template[index])
                index = template.index(after: index)
                continue
            }

            let next = template.index(after: index)
            if next < template.endIndex && template[next] == "$" {
                output.append("$")
                index = template.index(after: next)
                continue
            }

            if next < template.endIndex && template[next] == "{" {
                if let close = template[next...].firstIndex(of: "}") {
                    let token = String(template[template.index(after: next)..<close]).trimmingCharacters(in: .whitespacesAndNewlines)
                    let split = splitToken(token)
                    output += format(evaluate(root, path: split.path), format: split.format)
                    index = template.index(after: close)
                    continue
                }
            }

            let pathEnd = scanPath(template, from: index)
            if pathEnd > index {
                let path = String(template[index..<pathEnd])
                output += format(evaluate(root, path: path), format: nil)
                index = pathEnd
                continue
            }

            output.append(template[index])
            index = template.index(after: index)
        }

        return output
    }

    static func renderDisplayTemplate(_ template: String, items: String, count: Int, date: Date) -> String {
        let formatter = DateFormatter()
        formatter.dateFormat = "yyyy-MM-dd"
        let day = formatter.string(from: date)
        formatter.dateFormat = "HH:mm:ss"
        let time = formatter.string(from: date)
        return decodeEscapes(template.isEmpty ? "{items}" : template)
            .replacingOccurrences(of: "{items}", with: items)
            .replacingOccurrences(of: "{count}", with: String(count))
            .replacingOccurrences(of: "{date}", with: day)
            .replacingOccurrences(of: "{time}", with: time)
    }

    static func decodeEscapes(_ text: String) -> String {
        return text
            .replacingOccurrences(of: "\\r\\n", with: "\r\n")
            .replacingOccurrences(of: "\\n", with: "\n")
            .replacingOccurrences(of: "\\t", with: "\t")
    }

    private static func scanPath(_ text: String, from start: String.Index) -> String.Index {
        var i = start
        var inBracket = false
        var quote: Character?
        while i < text.endIndex {
            let ch = text[i]
            if let activeQuote = quote {
                if ch == activeQuote {
                    quote = nil
                }
                i = text.index(after: i)
                continue
            }

            if inBracket {
                if ch == "\"" || ch == "'" {
                    quote = ch
                } else if ch == "]" {
                    inBracket = false
                }
                i = text.index(after: i)
                continue
            }

            if ch == "[" {
                inBracket = true
                i = text.index(after: i)
                continue
            }

            if ch == "$" || ch == "." || ch == "_" || ch == "-" || ch.isLetter || ch.isNumber {
                i = text.index(after: i)
                continue
            }

            break
        }

        return text.distance(from: start, to: i) > 1 ? i : start
    }

    private static func evaluate(_ root: Any, path: String) -> Any? {
        if path.isEmpty || path.first != "$" {
            return nil
        }
        if path == "$" {
            return root
        }

        var current: Any? = root
        var i = path.index(after: path.startIndex)
        while i < path.endIndex {
            if path[i] == "." {
                let next = path.index(after: i)
                if next < path.endIndex && path[next] == "." {
                    i = path.index(after: next)
                    let key = readPropertyName(path, index: &i)
                    current = findRecursive(current, key: key)
                } else {
                    i = next
                    let key = readPropertyName(path, index: &i)
                    current = property(current, key: key)
                }
            } else if path[i] == "[" {
                guard let close = path[i...].firstIndex(of: "]") else {
                    return nil
                }
                let token = String(path[path.index(after: i)..<close]).trimmingCharacters(in: .whitespacesAndNewlines)
                current = bracket(current, token: token)
                i = path.index(after: close)
            } else {
                return nil
            }

            if current == nil {
                return nil
            }
        }

        return current
    }

    private static func readPropertyName(_ path: String, index: inout String.Index) -> String {
        let start = index
        while index < path.endIndex && path[index] != "." && path[index] != "[" {
            index = path.index(after: index)
        }
        return String(path[start..<index])
    }

    private static func property(_ current: Any?, key: String) -> Any? {
        return (current as? [String: Any])?[key]
    }

    private static func bracket(_ current: Any?, token: String) -> Any? {
        if token.isEmpty {
            return nil
        }

        if (token.first == "\"" && token.last == "\"") || (token.first == "'" && token.last == "'") {
            return property(current, key: String(token.dropFirst().dropLast()))
        }

        if let index = Int(token), let array = current as? [Any] {
            let normalized = index < 0 ? array.count + index : index
            return normalized >= 0 && normalized < array.count ? array[normalized] : nil
        }

        return property(current, key: token)
    }

    private static func findRecursive(_ current: Any?, key: String) -> Any? {
        if let dictionary = current as? [String: Any] {
            if let value = dictionary[key] {
                return value
            }
            for value in dictionary.values {
                if let found = findRecursive(value, key: key) {
                    return found
                }
            }
        }

        if let array = current as? [Any] {
            for value in array {
                if let found = findRecursive(value, key: key) {
                    return found
                }
            }
        }

        return nil
    }

    private static func splitToken(_ token: String) -> (path: String, format: String?) {
        var depth = 0
        var quote: Character?
        var i = token.startIndex
        while i < token.endIndex {
            let ch = token[i]
            if let activeQuote = quote {
                if ch == activeQuote {
                    quote = nil
                }
                i = token.index(after: i)
                continue
            }

            if ch == "\"" || ch == "'" {
                quote = ch
            } else if ch == "[" {
                depth += 1
            } else if ch == "]" {
                depth -= 1
            } else if ch == ":" && depth == 0 {
                return (
                    String(token[..<i]).trimmingCharacters(in: .whitespacesAndNewlines),
                    String(token[token.index(after: i)...]).trimmingCharacters(in: .whitespacesAndNewlines))
            }

            i = token.index(after: i)
        }

        return (token, nil)
    }

    private static func format(_ value: Any?, format: String?) -> String {
        guard let value = value else {
            return "--"
        }

        if let text = value as? String {
            return text
        }

        if let number = value as? NSNumber {
            guard let format = format, let dot = format.firstIndex(of: ".") else {
                return String(describing: number)
            }

            let decimals = format.distance(from: format.index(after: dot), to: format.endIndex)
            let formatter = NumberFormatter()
            formatter.locale = Locale(identifier: "en_US_POSIX")
            formatter.usesGroupingSeparator = false
            formatter.minimumFractionDigits = decimals
            formatter.maximumFractionDigits = decimals
            return formatter.string(from: number) ?? String(describing: number)
        }

        if let data = try? JSONSerialization.data(withJSONObject: value),
           let json = String(data: data, encoding: .utf8) {
            return json
        }

        return String(describing: value)
    }
}

private enum LocalizedText {
    private static let english = [
        "ApiError": "API error",
        "MenuShowHide": "Show / Hide",
        "MenuRefreshNow": "Refresh now",
        "MenuOpenConfig": "Open config",
        "MenuOpenConfigFolder": "Open config folder",
        "MenuReloadConfig": "Reload config",
        "MenuExit": "Exit",
        "StartWithLogin": "Start at login",
        "StartupUpdateFailed": "Could not update login startup setting."
    ]

    private static let chinese = [
        "ApiError": "API 错误",
        "MenuShowHide": "显示 / 隐藏",
        "MenuRefreshNow": "立即刷新",
        "MenuOpenConfig": "打开配置文件",
        "MenuOpenConfigFolder": "打开配置文件夹",
        "MenuReloadConfig": "重新加载配置",
        "MenuExit": "退出",
        "StartWithLogin": "登录时启动",
        "StartupUpdateFailed": "无法更新登录启动设置。"
    ]

    static func text(_ key: String, language: String) -> String {
        let table = language.hasPrefix("zh") ? chinese : english
        return table[key] ?? english[key] ?? key
    }
}
