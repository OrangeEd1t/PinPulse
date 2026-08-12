using System;
using System.Collections.Generic;
using System.Globalization;

namespace CryptoMonitor
{
    internal static class Localization
    {
        public const string English = "en-US";
        public const string Chinese = "zh-CN";

        private static readonly Dictionary<string, string> EnglishText = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> ChineseText = new Dictionary<string, string>();

        static Localization()
        {
            EnglishText["SettingsTitle"] = "CryptoMonitor Settings";
            EnglishText["Language"] = "Language";
            EnglishText["ApiUrl"] = "API URL";
            EnglishText["BasicSettings"] = "Basic";
            EnglishText["GeneralSettings"] = "General";
            EnglishText["TaskbarSettings"] = "Window";
            EnglishText["DisplaySettings"] = "Display";
            EnglishText["MonitorItems"] = "Monitor Items";
            EnglishText["ApiItems"] = "API Items";
            EnglishText["Symbols"] = "Symbols";
            EnglishText["Currency"] = "Currency";
            EnglishText["RefreshSeconds"] = "Refresh interval";
            EnglishText["TimeoutSeconds"] = "Timeout";
            EnglishText["DisplayTemplate"] = "Display template";
            EnglishText["DisplayTemplateHint"] = "Use {items} for all enabled items. You can also add {time}, {date}, or {count}.";
            EnglishText["ItemSeparator"] = "Item separator";
            EnglishText["ItemSeparatorHint"] = "For example: spaces, a vertical bar, or a line break.";
            EnglishText["StartWithWindows"] = "Start with Windows";
            EnglishText["TaskbarPosition"] = "Position";
            EnglishText["TaskbarAnchor"] = "Anchor";
            EnglishText["TaskbarAnchorLeft"] = "Left";
            EnglishText["TaskbarAnchorRight"] = "Right";
            EnglishText["TaskbarOffsetX"] = "Horizontal offset";
            EnglishText["TaskbarOffsetY"] = "Vertical offset";
            EnglishText["TaskbarOffsetHint"] = "Positive values move right or down. Negative values move the other way.";
            EnglishText["TaskbarWidth"] = "Window width";
            EnglishText["TaskbarFixedWidth"] = "Fixed width (0 = auto)";
            EnglishText["TaskbarMinWidth"] = "Min auto width";
            EnglishText["TaskbarMaxWidth"] = "Max auto width";
            EnglishText["TaskbarWidthHint"] = "Set fixed width to 0 to let the floating window resize to the text.";
            EnglishText["TaskbarAppearance"] = "Window appearance";
            EnglishText["TaskbarFontFamily"] = "Font";
            EnglishText["TaskbarFontSize"] = "Size";
            EnglishText["TaskbarFontBold"] = "Bold";
            EnglishText["WindowBackground"] = "Background";
            EnglishText["WindowBackgroundColor"] = "Background color";
            EnglishText["WindowBackgroundTransparent"] = "Transparent background";
            EnglishText["ChooseColor"] = "Choose";
            EnglishText["ShowTaskbarWindow"] = "Show floating window";
            EnglishText["AddPreset"] = "Add template";
            EnglishText["AddCustom"] = "Blank";
            EnglishText["Duplicate"] = "Copy";
            EnglishText["Delete"] = "Delete";
            EnglishText["MoveUp"] = "Up";
            EnglishText["MoveDown"] = "Down";
            EnglishText["ItemEnabled"] = "Enable this item";
            EnglishText["ItemName"] = "Name";
            EnglishText["ItemId"] = "ID";
            EnglishText["ItemUrl"] = "API URL";
            EnglishText["ItemMethod"] = "Method";
            EnglishText["ItemTemplate"] = "Display text";
            EnglishText["ItemTemplateHint"] = "Use JSONPath placeholders such as ${$.price:0.00}, $.data[0].value, or $..value.";
            EnglishText["ItemInterval"] = "Refresh interval";
            EnglishText["ItemTimeout"] = "Timeout";
            EnglishText["UnitSeconds"] = "sec";
            EnglishText["UnitPixels"] = "px";
            EnglishText["UnitPoints"] = "pt";
            EnglishText["ItemHeaders"] = "Headers";
            EnglishText["ItemBody"] = "Body";
            EnglishText["AdvancedSettings"] = "Advanced";
            EnglishText["TestItem"] = "Test";
            EnglishText["TestingItem"] = "Testing...";
            EnglishText["TestItemFailed"] = "Test failed";
            EnglishText["PreviewRendered"] = "Rendered text:";
            EnglishText["PreviewResponse"] = "Raw response:";
            EnglishText["PresetGetJson"] = "GET JSON";
            EnglishText["PresetPostJson"] = "POST JSON";
            EnglishText["PresetFearGreed"] = "Fear & Greed";
            EnglishText["PresetCryptoPrice"] = "BTC price";
            EnglishText["NewItemName"] = "New item";
            EnglishText["Save"] = "Save";
            EnglishText["Cancel"] = "Cancel";
            EnglishText["ValidationSymbolRequired"] = "Enter at least one symbol.";
            EnglishText["ValidationApiUrlRequired"] = "API URL is required.";
            EnglishText["ValidationApiItemsInvalid"] = "API Items JSON is invalid.";
            EnglishText["ValidationApiItemsRequired"] = "Enable at least one API item.";
            EnglishText["ValidationHeadersInvalid"] = "Headers are invalid. Use the format Name: Value.";
            EnglishText["StartupUpdateFailed"] = "Could not update Windows startup setting.";
            EnglishText["MenuShowHide"] = "Show / Hide";
            EnglishText["MenuRefreshNow"] = "Refresh now";
            EnglishText["MenuSettings"] = "Settings";
            EnglishText["MenuOpenConfigFolder"] = "Open config folder";
            EnglishText["MenuExit"] = "Exit";
            EnglishText["Updating"] = "Updating...";
            EnglishText["UpdateFailed"] = "Update failed";
            EnglishText["ApiError"] = "API error";
            EnglishText["ApiResponseNotJson"] = "API response is not a JSON object.";
            EnglishText["ApiResponseNoData"] = "API response does not contain data.";
            EnglishText["ApiResponseNoSymbols"] = "No configured symbols were found in API response.";
            EnglishText["ApiResponseNoItems"] = "No enabled API items were found.";

            ChineseText["SettingsTitle"] = "CryptoMonitor \u8bbe\u7f6e";
            ChineseText["Language"] = "\u8bed\u8a00";
            ChineseText["ApiUrl"] = "API \u5730\u5740";
            ChineseText["BasicSettings"] = "\u57fa\u7840";
            ChineseText["GeneralSettings"] = "\u901a\u7528";
            ChineseText["TaskbarSettings"] = "悬浮窗口";
            ChineseText["DisplaySettings"] = "显示";
            ChineseText["MonitorItems"] = "监控项";
            ChineseText["ApiItems"] = "API \u9879";
            ChineseText["Symbols"] = "\u5e01\u79cd";
            ChineseText["Currency"] = "\u8ba1\u4ef7\u8d27\u5e01";
            ChineseText["RefreshSeconds"] = "\u5237\u65b0\u95f4\u9694";
            ChineseText["TimeoutSeconds"] = "\u8d85\u65f6\u65f6\u95f4";
            ChineseText["DisplayTemplate"] = "\u663e\u793a\u6a21\u677f";
            ChineseText["DisplayTemplateHint"] = "使用 {items} 显示所有监控项，也可加入 {time}、{date} 或 {count}。";
            ChineseText["ItemSeparator"] = "\u9879\u95f4\u5206\u9694";
            ChineseText["ItemSeparatorHint"] = "例如空格、竖线或换行。";
            ChineseText["StartWithWindows"] = "\u5f00\u673a\u81ea\u542f";
            ChineseText["TaskbarPosition"] = "位置";
            ChineseText["TaskbarAnchor"] = "对齐位置";
            ChineseText["TaskbarAnchorLeft"] = "靠左";
            ChineseText["TaskbarAnchorRight"] = "靠右";
            ChineseText["TaskbarOffsetX"] = "水平偏移";
            ChineseText["TaskbarOffsetY"] = "垂直偏移";
            ChineseText["TaskbarOffsetHint"] = "数值越大越向右或向下，负数反向移动。";
            ChineseText["TaskbarWidth"] = "窗口宽度";
            ChineseText["TaskbarFixedWidth"] = "\u56fa\u5b9a\u5bbd\u5ea6(0=\u81ea\u52a8)";
            ChineseText["TaskbarMinWidth"] = "\u81ea\u52a8\u6700\u5c0f\u5bbd\u5ea6";
            ChineseText["TaskbarMaxWidth"] = "\u81ea\u52a8\u6700\u5927\u5bbd\u5ea6";
            ChineseText["TaskbarWidthHint"] = "固定宽度为 0 时，悬浮窗口会根据文字自动伸缩。";
            ChineseText["TaskbarAppearance"] = "窗口外观";
            ChineseText["TaskbarFontFamily"] = "\u5b57\u4f53";
            ChineseText["TaskbarFontSize"] = "\u5b57\u53f7";
            ChineseText["TaskbarFontBold"] = "\u52a0\u7c97";
            ChineseText["WindowBackground"] = "\u80cc\u666f";
            ChineseText["WindowBackgroundColor"] = "\u80cc\u666f\u989c\u8272";
            ChineseText["WindowBackgroundTransparent"] = "\u80cc\u666f\u900f\u660e";
            ChineseText["ChooseColor"] = "\u9009\u62e9";
            ChineseText["ShowTaskbarWindow"] = "显示悬浮窗口";
            ChineseText["AddPreset"] = "添加模板";
            ChineseText["AddCustom"] = "空白项";
            ChineseText["Duplicate"] = "复制";
            ChineseText["Delete"] = "删除";
            ChineseText["MoveUp"] = "上移";
            ChineseText["MoveDown"] = "下移";
            ChineseText["ItemEnabled"] = "启用此项";
            ChineseText["ItemName"] = "名称";
            ChineseText["ItemId"] = "ID";
            ChineseText["ItemUrl"] = "接口地址";
            ChineseText["ItemMethod"] = "方法";
            ChineseText["ItemTemplate"] = "显示内容";
            ChineseText["ItemTemplateHint"] = "使用 JSONPath 占位符，如 ${$.price:0.00}、$.data[0].value 或 $..value。";
            ChineseText["ItemInterval"] = "刷新间隔";
            ChineseText["ItemTimeout"] = "超时";
            ChineseText["UnitSeconds"] = "\u79d2";
            ChineseText["UnitPixels"] = "\u50cf\u7d20";
            ChineseText["UnitPoints"] = "\u78c5";
            ChineseText["ItemHeaders"] = "请求头";
            ChineseText["ItemBody"] = "请求体";
            ChineseText["AdvancedSettings"] = "高级";
            ChineseText["TestItem"] = "测试";
            ChineseText["TestingItem"] = "正在测试...";
            ChineseText["TestItemFailed"] = "测试失败";
            ChineseText["PreviewRendered"] = "渲染结果：";
            ChineseText["PreviewResponse"] = "原始响应：";
            ChineseText["PresetGetJson"] = "GET JSON";
            ChineseText["PresetPostJson"] = "POST JSON";
            ChineseText["PresetFearGreed"] = "恐惧贪婪";
            ChineseText["PresetCryptoPrice"] = "BTC 价格";
            ChineseText["NewItemName"] = "新监控项";
            ChineseText["Save"] = "\u4fdd\u5b58";
            ChineseText["Cancel"] = "\u53d6\u6d88";
            ChineseText["ValidationSymbolRequired"] = "\u8bf7\u81f3\u5c11\u8f93\u5165\u4e00\u4e2a\u5e01\u79cd\u3002";
            ChineseText["ValidationApiUrlRequired"] = "API \u5730\u5740\u4e0d\u80fd\u4e3a\u7a7a\u3002";
            ChineseText["ValidationApiItemsInvalid"] = "API \u9879 JSON \u683c\u5f0f\u4e0d\u6b63\u786e\u3002";
            ChineseText["ValidationApiItemsRequired"] = "\u8bf7\u81f3\u5c11\u542f\u7528\u4e00\u4e2a API \u9879\u3002";
            ChineseText["ValidationHeadersInvalid"] = "请求头格式不正确，请使用“名称: 值”格式。";
            ChineseText["StartupUpdateFailed"] = "\u65e0\u6cd5\u66f4\u65b0 Windows \u5f00\u673a\u81ea\u542f\u8bbe\u7f6e\u3002";
            ChineseText["MenuShowHide"] = "\u663e\u793a / \u9690\u85cf";
            ChineseText["MenuRefreshNow"] = "\u7acb\u5373\u5237\u65b0";
            ChineseText["MenuSettings"] = "\u8bbe\u7f6e";
            ChineseText["MenuOpenConfigFolder"] = "\u6253\u5f00\u914d\u7f6e\u6587\u4ef6\u5939";
            ChineseText["MenuExit"] = "\u9000\u51fa";
            ChineseText["Updating"] = "\u6b63\u5728\u66f4\u65b0...";
            ChineseText["UpdateFailed"] = "\u66f4\u65b0\u5931\u8d25";
            ChineseText["ApiError"] = "API \u9519\u8bef";
            ChineseText["ApiResponseNotJson"] = "API \u54cd\u5e94\u4e0d\u662f JSON \u5bf9\u8c61\u3002";
            ChineseText["ApiResponseNoData"] = "API \u54cd\u5e94\u4e0d\u5305\u542b data \u5b57\u6bb5\u3002";
            ChineseText["ApiResponseNoSymbols"] = "API \u54cd\u5e94\u4e2d\u6ca1\u6709\u627e\u5230\u5df2\u914d\u7f6e\u7684\u5e01\u79cd\u3002";
            ChineseText["ApiResponseNoItems"] = "\u6ca1\u6709\u627e\u5230\u5df2\u542f\u7528\u7684 API \u9879\u3002";
        }

        public static string DefaultLanguage()
        {
            return NormalizeLanguage(CultureInfo.CurrentUICulture.Name);
        }

        public static string NormalizeLanguage(string language)
        {
            if (String.IsNullOrEmpty(language))
            {
                return English;
            }

            string value = language.Trim();
            if (value.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("cn", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("chinese", StringComparison.OrdinalIgnoreCase))
            {
                return Chinese;
            }

            if (value.StartsWith("en", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("english", StringComparison.OrdinalIgnoreCase))
            {
                return English;
            }

            return English;
        }

        public static string Text(AppConfig config, string key)
        {
            string language = config == null ? English : config.Language;
            return Text(language, key);
        }

        public static string Text(string language, string key)
        {
            Dictionary<string, string> table = NormalizeLanguage(language) == Chinese ? ChineseText : EnglishText;
            string text;
            if (table.TryGetValue(key, out text))
            {
                return text;
            }

            if (EnglishText.TryGetValue(key, out text))
            {
                return text;
            }

            return key;
        }

        public static string LanguageDisplayName(string language)
        {
            return NormalizeLanguage(language) == Chinese ? "\u4e2d\u6587" : "English";
        }
    }
}
