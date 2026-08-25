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
            EnglishText["DisplayFormat"] = "Display format";
            EnglishText["DisplayFormatSimple"] = "Items only";
            EnglishText["DisplayFormatWithTime"] = "Time + items";
            EnglishText["DisplayFormatWithDateTime"] = "Date/time + items";
            EnglishText["DisplayFormatWithCount"] = "Items + count";
            EnglishText["DisplayFormatCustom"] = "Custom";
            EnglishText["DisplayPreview"] = "Preview:";
            EnglishText["DisplayTemplate"] = "Display template";
            EnglishText["DisplayTemplateHint"] = "Advanced: click a field button to insert it, or edit the template directly.";
            EnglishText["TokenItems"] = "Items";
            EnglishText["TokenTime"] = "Time";
            EnglishText["TokenDate"] = "Date";
            EnglishText["TokenCount"] = "Count";
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
            EnglishText["TaskbarWidthAutoMode"] = "Auto width";
            EnglishText["TaskbarWidthFixedMode"] = "Fixed width";
            EnglishText["TaskbarFixedWidth"] = "Width";
            EnglishText["TaskbarMinWidth"] = "Min width";
            EnglishText["TaskbarMaxWidth"] = "Max width";
            EnglishText["TaskbarWidthHint"] = "Choose how the floating window width is calculated.";
            EnglishText["TaskbarAutoWidthHint"] = "The window follows the text length, constrained by the min and max width.";
            EnglishText["TaskbarFixedWidthHint"] = "The window always uses this width, regardless of text length.";
            EnglishText["TaskbarAppearance"] = "Window appearance";
            EnglishText["TaskbarFontFamily"] = "Font";
            EnglishText["TaskbarFontSize"] = "Size";
            EnglishText["TaskbarFontBold"] = "Bold";
            EnglishText["WindowTextWrap"] = "Allow line wrap";
            EnglishText["WindowBackground"] = "Background";
            EnglishText["WindowBackgroundColor"] = "Background color";
            EnglishText["WindowBackgroundTransparent"] = "Transparent background";
            EnglishText["ChooseColor"] = "Choose";
            EnglishText["ShowTaskbarWindow"] = "Show floating window";
            EnglishText["CurrentItems"] = "Added monitor items";
            EnglishText["AddCustom"] = "New blank item";
            EnglishText["Duplicate"] = "Copy";
            EnglishText["Delete"] = "Delete";
            EnglishText["MoveUp"] = "Up";
            EnglishText["MoveDown"] = "Down";
            EnglishText["ItemEnabled"] = "Enable this item";
            EnglishText["ItemBasics"] = "Monitor item info";
            EnglishText["ItemDataSource"] = "Data source settings";
            EnglishText["ItemRefreshSettings"] = "Refresh";
            EnglishText["ItemRequestSettings"] = "Request";
            EnglishText["ItemTestResult"] = "Test result";
            EnglishText["ItemName"] = "Name";
            EnglishText["ItemSymbol"] = "Symbol";
            EnglishText["ItemQuoteCurrency"] = "Quote";
            EnglishText["ItemCurrencyPair"] = "Pair";
            EnglishText["ItemWebsiteUrl"] = "Website URL";
            EnglishText["ItemId"] = "ID";
            EnglishText["ItemUrl"] = "API URL";
            EnglishText["ItemMethod"] = "Method";
            EnglishText["ItemTemplate"] = "Display text";
            EnglishText["ItemTemplateHint"] = "Use JSONPath placeholders such as ${$.price:0.00}, $.data[0].value, or $..value.";
            EnglishText["ItemUseGlobalTiming"] = "Use General refresh and timeout";
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
            EnglishText["NewItemName"] = "New item";
            EnglishText["Save"] = "Save";
            EnglishText["Cancel"] = "Cancel";
            EnglishText["ValidationSymbolRequired"] = "Enter at least one symbol.";
            EnglishText["ValidationApiUrlRequired"] = "API URL is required.";
            EnglishText["ValidationApiItemsInvalid"] = "API Items JSON is invalid.";
            EnglishText["ValidationApiItemsRequired"] = "Enable at least one API item.";
            EnglishText["ValidationHeadersInvalid"] = "Headers are invalid. Use the format Name: Value.";
            EnglishText["ValidationDisplayTemplateItemsRequired"] = "Display template must include the Items field.";
            EnglishText["ValidationCoinSymbolUnsupported"] = "This coin symbol is not supported yet. Try BTC, ETH, SOL, XRP, DOGE, ADA, BNB, TRX, or DOT.";
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
            ChineseText["TaskbarWidthAutoMode"] = "自动宽度";
            ChineseText["TaskbarWidthFixedMode"] = "固定宽度";
            ChineseText["TaskbarFixedWidth"] = "宽度";
            ChineseText["TaskbarMinWidth"] = "最小宽度";
            ChineseText["TaskbarMaxWidth"] = "最大宽度";
            ChineseText["TaskbarWidthHint"] = "选择悬浮窗口宽度的计算方式。";
            ChineseText["TaskbarAutoWidthHint"] = "窗口会跟随文字长度自动伸缩，并限制在最小和最大宽度之间。";
            ChineseText["TaskbarFixedWidthHint"] = "窗口始终使用这个宽度，不随文字长度变化。";
            ChineseText["TaskbarAppearance"] = "窗口外观";
            ChineseText["TaskbarFontFamily"] = "\u5b57\u4f53";
            ChineseText["TaskbarFontSize"] = "\u5b57\u53f7";
            ChineseText["TaskbarFontBold"] = "\u52a0\u7c97";
            ChineseText["WindowTextWrap"] = "\u5141\u8bb8\u6362\u884c";
            ChineseText["WindowBackground"] = "\u80cc\u666f";
            ChineseText["WindowBackgroundColor"] = "\u80cc\u666f\u989c\u8272";
            ChineseText["WindowBackgroundTransparent"] = "\u80cc\u666f\u900f\u660e";
            ChineseText["ChooseColor"] = "\u9009\u62e9";
            ChineseText["ShowTaskbarWindow"] = "显示悬浮窗口";
            ChineseText["CurrentItems"] = "已添加的监控项";
            ChineseText["AddCustom"] = "新建空白项";
            ChineseText["Duplicate"] = "复制";
            ChineseText["Delete"] = "删除";
            ChineseText["MoveUp"] = "上移";
            ChineseText["MoveDown"] = "下移";
            ChineseText["ItemEnabled"] = "启用此项";
            ChineseText["ItemBasics"] = "监控项信息";
            ChineseText["ItemDataSource"] = "数据来源设置";
            ChineseText["ItemRefreshSettings"] = "刷新设置";
            ChineseText["ItemRequestSettings"] = "请求设置";
            ChineseText["ItemTestResult"] = "测试结果";
            ChineseText["ItemName"] = "名称";
            ChineseText["ItemSymbol"] = "\u5e01\u79cd";
            ChineseText["ItemQuoteCurrency"] = "\u8ba1\u4ef7";
            ChineseText["ItemCurrencyPair"] = "\u8d27\u5e01\u5bf9";
            ChineseText["ItemWebsiteUrl"] = "\u7f51\u7ad9\u5730\u5740";
            ChineseText["ItemId"] = "ID";
            ChineseText["ItemUrl"] = "接口地址";
            ChineseText["ItemMethod"] = "方法";
            ChineseText["ItemTemplate"] = "显示内容";
            ChineseText["ItemTemplateHint"] = "使用 JSONPath 占位符，如 ${$.price:0.00}、$.data[0].value 或 $..value。";
            ChineseText["ItemUseGlobalTiming"] = "使用通用刷新和超时设置";
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
            ChineseText["NewItemName"] = "新监控项";
            ChineseText["Save"] = "\u4fdd\u5b58";
            ChineseText["Cancel"] = "\u53d6\u6d88";
            ChineseText["ValidationSymbolRequired"] = "\u8bf7\u81f3\u5c11\u8f93\u5165\u4e00\u4e2a\u5e01\u79cd\u3002";
            ChineseText["ValidationApiUrlRequired"] = "API \u5730\u5740\u4e0d\u80fd\u4e3a\u7a7a\u3002";
            ChineseText["ValidationApiItemsInvalid"] = "API \u9879 JSON \u683c\u5f0f\u4e0d\u6b63\u786e\u3002";
            ChineseText["ValidationApiItemsRequired"] = "\u8bf7\u81f3\u5c11\u542f\u7528\u4e00\u4e2a API \u9879\u3002";
            ChineseText["ValidationHeadersInvalid"] = "请求头格式不正确，请使用“名称: 值”格式。";
            ChineseText["ValidationCoinSymbolUnsupported"] = "\u6682\u65f6\u4e0d\u652f\u6301\u8fd9\u4e2a\u5e01\u79cd\uff0c\u53ef\u5148\u4f7f\u7528 BTC\u3001ETH\u3001SOL\u3001XRP\u3001DOGE\u3001ADA\u3001BNB\u3001TRX \u6216 DOT\u3002";
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

            ChineseText["DisplayFormat"] = "\u663e\u793a\u683c\u5f0f";
            ChineseText["DisplayFormatSimple"] = "\u4ec5\u663e\u793a\u76d1\u63a7\u9879";
            ChineseText["DisplayFormatWithTime"] = "\u65f6\u95f4 + \u76d1\u63a7\u9879";
            ChineseText["DisplayFormatWithDateTime"] = "\u65e5\u671f\u65f6\u95f4 + \u76d1\u63a7\u9879";
            ChineseText["DisplayFormatWithCount"] = "\u76d1\u63a7\u9879 + \u6570\u91cf";
            ChineseText["DisplayFormatCustom"] = "\u81ea\u5b9a\u4e49";
            ChineseText["DisplayPreview"] = "\u9884\u89c8\uff1a";
            ChineseText["DisplayTemplateHint"] = "\u9ad8\u7ea7\uff1a\u70b9\u51fb\u5b57\u6bb5\u6309\u94ae\u63d2\u5165\uff0c\u4e5f\u53ef\u76f4\u63a5\u7f16\u8f91\u6a21\u677f\u3002";
            ChineseText["TokenItems"] = "\u76d1\u63a7\u9879";
            ChineseText["TokenTime"] = "\u65f6\u95f4";
            ChineseText["TokenDate"] = "\u65e5\u671f";
            ChineseText["TokenCount"] = "\u6570\u91cf";
            ChineseText["ValidationDisplayTemplateItemsRequired"] = "\u663e\u793a\u6a21\u677f\u9700\u8981\u5305\u542b\u201c\u76d1\u63a7\u9879\u201d\u5b57\u6bb5\u3002";
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
