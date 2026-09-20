using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace PinPulse
{
    internal sealed class AppConfig
    {
        public const int MinRefreshSeconds = 1;
        public const int MaxRefreshSeconds = 86400;

        public string ApiUrl;
        public string Language;
        public string Currency;
        public int RefreshSeconds;
        public int RequestTimeoutSeconds;
        public string DisplayTemplate;
        public string ItemSeparator;
        public bool StartWithWindows;
        public bool ShowTaskbarWindow;
        public string TaskbarAnchor;
        public int TaskbarOffsetX;
        public int TaskbarOffsetY;
        public int TaskbarFixedWidth;
        public int TaskbarMinWidth;
        public int TaskbarMaxWidth;
        public string TaskbarFontFamily;
        public int TaskbarFontSize;
        public bool TaskbarFontBold;
        public bool WindowTextWrap;
        public string WindowBackgroundColor;
        public bool WindowBackgroundTransparent;
        public int WindowLeft;
        public int WindowTop;
        public List<string> Symbols;
        public List<ApiItemConfig> Items;

        public static string ConfigFileName = "config.json";

        public AppConfig()
        {
            ApiUrl = "https://api.alternative.me/v2/ticker/?convert=USD&limit=10";
            Language = Localization.DefaultLanguage();
            Currency = "USD";
            RefreshSeconds = 300;
            RequestTimeoutSeconds = 10;
            DisplayTemplate = "{items}";
            ItemSeparator = "   ";
            StartWithWindows = false;
            ShowTaskbarWindow = true;
            TaskbarAnchor = "left";
            TaskbarOffsetX = 280;
            TaskbarOffsetY = 0;
            TaskbarFixedWidth = 0;
            TaskbarMinWidth = 190;
            TaskbarMaxWidth = 520;
            TaskbarFontFamily = "Microsoft YaHei UI";
            TaskbarFontSize = 10;
            TaskbarFontBold = false;
            WindowTextWrap = false;
            WindowBackgroundColor = "#FFFFFF";
            WindowBackgroundTransparent = true;
            WindowLeft = Int32.MinValue;
            WindowTop = Int32.MinValue;
            Symbols = new List<string>();
            Symbols.Add("BTC");
            Symbols.Add("ETH");
            Items = CreateDefaultItems();
        }

        public static AppConfig Load(string appDir)
        {
            string path = Path.Combine(appDir, ConfigFileName);
            AppConfig config = new AppConfig();
            if (!File.Exists(path))
            {
                config.Save(appDir);
                return config;
            }

            try
            {
                string json = File.ReadAllText(path);
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> root = serializer.DeserializeObject(json) as Dictionary<string, object>;
                if (root == null)
                {
                    return config;
                }

                config.ApiUrl = GetString(root, "apiUrl", config.ApiUrl);
                config.Language = Localization.NormalizeLanguage(GetString(root, "language", config.Language));
                config.Currency = GetString(root, "currency", config.Currency).ToUpperInvariant();
                config.RefreshSeconds = Clamp(GetInt(root, "refreshSeconds", config.RefreshSeconds), MinRefreshSeconds, MaxRefreshSeconds);
                config.RequestTimeoutSeconds = Clamp(GetInt(root, "requestTimeoutSeconds", config.RequestTimeoutSeconds), 3, 120);
                config.DisplayTemplate = GetString(root, "displayTemplate", config.DisplayTemplate);
                config.ItemSeparator = GetString(root, "itemSeparator", config.ItemSeparator);
                config.StartWithWindows = GetBool(root, "startWithWindows", config.StartWithWindows);
                config.ShowTaskbarWindow = GetBool(root, "showTaskbarWindow", config.ShowTaskbarWindow);
                config.TaskbarAnchor = NormalizeAnchor(GetString(root, "taskbarAnchor", config.TaskbarAnchor));
                config.TaskbarOffsetX = Clamp(GetInt(root, "taskbarOffsetX", config.TaskbarOffsetX), -4000, 4000);
                config.TaskbarOffsetY = Clamp(GetInt(root, "taskbarOffsetY", config.TaskbarOffsetY), -4000, 4000);
                config.TaskbarFixedWidth = Clamp(GetInt(root, "windowFixedWidth", GetInt(root, "taskbarFixedWidth", config.TaskbarFixedWidth)), 0, 4000);
                config.TaskbarMinWidth = Clamp(GetInt(root, "windowMinWidth", GetInt(root, "taskbarMinWidth", config.TaskbarMinWidth)), 80, 4000);
                config.TaskbarMaxWidth = Clamp(GetInt(root, "windowMaxWidth", GetInt(root, "taskbarMaxWidth", config.TaskbarMaxWidth)), 80, 4000);
                config.TaskbarFontFamily = GetString(root, "windowFontFamily", GetString(root, "taskbarFontFamily", config.TaskbarFontFamily));
                config.TaskbarFontSize = Clamp(GetInt(root, "windowFontSize", GetInt(root, "taskbarFontSize", config.TaskbarFontSize)), 6, 36);
                config.TaskbarFontBold = GetBool(root, "windowFontBold", GetBool(root, "taskbarFontBold", config.TaskbarFontBold));
                config.WindowTextWrap = GetBool(root, "windowTextWrap", GetBool(root, "taskbarTextWrap", config.WindowTextWrap));
                config.WindowBackgroundColor = NormalizeColorHex(GetString(root, "windowBackgroundColor", GetString(root, "taskbarBackgroundColor", config.WindowBackgroundColor)), config.WindowBackgroundColor);
                config.WindowBackgroundTransparent = GetBool(root, "windowBackgroundTransparent", GetBool(root, "taskbarBackgroundTransparent", config.WindowBackgroundTransparent));
                config.WindowLeft = GetOptionalInt(root, "windowLeft", config.WindowLeft, -32000, 32000);
                config.WindowTop = GetOptionalInt(root, "windowTop", config.WindowTop, -32000, 32000);
                if (config.TaskbarMaxWidth < config.TaskbarMinWidth)
                {
                    config.TaskbarMaxWidth = config.TaskbarMinWidth;
                }

                object symbolsValue;
                if (root.TryGetValue("symbols", out symbolsValue))
                {
                    ArrayList array = symbolsValue as ArrayList;
                    object[] objectArray = symbolsValue as object[];
                    List<string> symbols = new List<string>();
                    if (array != null)
                    {
                        foreach (object item in array)
                        {
                            AddSymbol(symbols, item);
                        }
                    }
                    else if (objectArray != null)
                    {
                        foreach (object item in objectArray)
                        {
                            AddSymbol(symbols, item);
                        }
                    }

                    if (symbols.Count > 0)
                    {
                        config.Symbols = symbols;
                    }
                }

                List<ApiItemConfig> items = ReadItems(root, "items");
                if (items.Count == 0)
                {
                    items = ReadItems(root, "apiItems");
                }

                if (items.Count > 0)
                {
                    config.Items = items;
                }
                else
                {
                    config.Items = CreateLegacyItems(config);
                }
            }
            catch
            {
                // Keep defaults when config is malformed; saving from Settings will rewrite it.
            }

            return config;
        }

        public void Save(string appDir)
        {
            string path = Path.Combine(appDir, ConfigFileName);
            Dictionary<string, object> root = new Dictionary<string, object>();
            root["language"] = Localization.NormalizeLanguage(Language);
            root["refreshSeconds"] = RefreshSeconds;
            root["requestTimeoutSeconds"] = RequestTimeoutSeconds;
            root["displayTemplate"] = String.IsNullOrEmpty(DisplayTemplate) ? "{items}" : DisplayTemplate;
            root["itemSeparator"] = ItemSeparator;
            root["startWithWindows"] = StartWithWindows;
            root["windowFixedWidth"] = Clamp(TaskbarFixedWidth, 0, 4000);
            root["windowMinWidth"] = Clamp(TaskbarMinWidth, 80, 4000);
            root["windowMaxWidth"] = Clamp(Math.Max(TaskbarMaxWidth, TaskbarMinWidth), 80, 4000);
            root["windowFontFamily"] = String.IsNullOrWhiteSpace(TaskbarFontFamily) ? "Microsoft YaHei UI" : TaskbarFontFamily;
            root["windowFontSize"] = Clamp(TaskbarFontSize, 6, 36);
            root["windowFontBold"] = TaskbarFontBold;
            root["windowTextWrap"] = WindowTextWrap;
            root["windowBackgroundColor"] = NormalizeColorHex(WindowBackgroundColor, "#FFFFFF");
            root["windowBackgroundTransparent"] = WindowBackgroundTransparent;
            if (HasSavedWindowPosition())
            {
                root["windowLeft"] = Clamp(WindowLeft, -32000, 32000);
                root["windowTop"] = Clamp(WindowTop, -32000, 32000);
            }

            if (Items != null && Items.Count > 0)
            {
                ArrayList items = new ArrayList();
                foreach (ApiItemConfig item in Items)
                {
                    items.Add(item.ToDictionary());
                }

                root["items"] = items;
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(root);
            File.WriteAllText(path, PrettyJson(json));
        }

        public string ItemsToJson()
        {
            ArrayList items = new ArrayList();
            if (Items != null)
            {
                foreach (ApiItemConfig item in Items)
                {
                    if (item != null)
                    {
                        items.Add(item.ToDictionary());
                    }
                }
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return PrettyJson(serializer.Serialize(items));
        }

        public static List<ApiItemConfig> ParseItemsJson(string json)
        {
            List<ApiItemConfig> items = new List<ApiItemConfig>();
            if (String.IsNullOrWhiteSpace(json))
            {
                return items;
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            object value = serializer.DeserializeObject(json);
            ArrayList array = value as ArrayList;
            object[] objectArray = value as object[];
            Dictionary<string, object> root = value as Dictionary<string, object>;
            if (array != null)
            {
                foreach (object item in array)
                {
                    AddItem(items, item);
                }
            }
            else if (objectArray != null)
            {
                foreach (object item in objectArray)
                {
                    AddItem(items, item);
                }
            }
            else if (root != null)
            {
                List<ApiItemConfig> nestedItems = ReadItems(root, "items");
                if (nestedItems.Count == 0)
                {
                    nestedItems = ReadItems(root, "apiItems");
                }

                if (nestedItems.Count > 0)
                {
                    return nestedItems;
                }

                AddItem(items, root);
            }
            else
            {
                throw new InvalidOperationException("API items must be a JSON array or object.");
            }

            return items;
        }

        public static string DecodeTextEscapes(string text)
        {
            if (text == null)
            {
                return "";
            }

            return text.Replace("\\r\\n", "\r\n").Replace("\\n", "\n").Replace("\\t", "\t");
        }

        private static List<ApiItemConfig> CreateDefaultItems()
        {
            List<ApiItemConfig> items = new List<ApiItemConfig>();
            items.Add(CreateAlternativeMeItem("BTC", "1", "USD", "https://api.alternative.me/v2/ticker/?convert=USD&limit=10", 0, 0));
            items.Add(CreateAlternativeMeItem("ETH", "1027", "USD", "https://api.alternative.me/v2/ticker/?convert=USD&limit=10", 0, 0));
            return items;
        }

        private static List<ApiItemConfig> CreateLegacyItems(AppConfig config)
        {
            List<ApiItemConfig> items = new List<ApiItemConfig>();
            string[] symbols = config.GetSymbols();
            for (int i = 0; i < symbols.Length; i++)
            {
                string dataId = AlternativeMeDataId(symbols[i]);
                if (dataId.Length > 0)
                {
                    items.Add(CreateAlternativeMeItem(symbols[i], dataId, config.Currency, config.ApiUrl, 0, 0));
                }
            }

            if (items.Count == 0)
            {
                items = CreateDefaultItems();
            }

            return items;
        }

        private static ApiItemConfig CreateAlternativeMeItem(string symbol, string dataId, string currency, string url, int intervalSeconds, int timeoutSeconds)
        {
            ApiItemConfig item = new ApiItemConfig();
            item.Id = symbol.ToLowerInvariant();
            item.Name = symbol.ToUpperInvariant();
            item.Type = ApiItemConfig.TypeCoin;
            item.Symbol = symbol.ToUpperInvariant();
            item.QuoteCurrency = currency.ToUpperInvariant();
            item.Enabled = true;
            item.Url = url;
            item.Method = "GET";
            item.Template = symbol.ToUpperInvariant() + " ${$.data." + dataId + ".quotes." + currency.ToUpperInvariant() + ".price:0.00}";
            item.IntervalSeconds = intervalSeconds;
            item.TimeoutSeconds = timeoutSeconds;
            return item;
        }

        public static string AlternativeMeDataId(string symbol)
        {
            if (String.IsNullOrEmpty(symbol))
            {
                return "";
            }

            string value = symbol.Trim().ToUpperInvariant();
            if (value == "BTC")
            {
                return "1";
            }

            if (value == "ETH")
            {
                return "1027";
            }

            if (value == "SOL")
            {
                return "11733";
            }

            if (value == "XRP")
            {
                return "52";
            }

            if (value == "DOGE")
            {
                return "74";
            }

            if (value == "ADA")
            {
                return "2010";
            }

            if (value == "BNB")
            {
                return "1839";
            }

            if (value == "TRX")
            {
                return "1958";
            }

            if (value == "DOT")
            {
                return "11517";
            }

            return "";
        }

        public static void ApplyItemTypeDefaults(ApiItemConfig item)
        {
            if (item == null)
            {
                return;
            }

            item.Type = ApiItemConfig.NormalizeType(item.Type);
            if (item.Type == ApiItemConfig.TypeCoin)
            {
                string symbol = String.IsNullOrWhiteSpace(item.Symbol) ? item.Name : item.Symbol;
                symbol = String.IsNullOrWhiteSpace(symbol) ? "BTC" : symbol.Trim().ToUpperInvariant();
                string quote = String.IsNullOrWhiteSpace(item.QuoteCurrency) ? "USD" : item.QuoteCurrency.Trim().ToUpperInvariant();
                string dataId = AlternativeMeDataId(symbol);
                item.Symbol = symbol;
                item.QuoteCurrency = quote;
                if (String.IsNullOrWhiteSpace(item.Id))
                {
                    item.Id = symbol.ToLowerInvariant();
                }

                if (String.IsNullOrWhiteSpace(item.Name))
                {
                    item.Name = symbol;
                }

                if (dataId.Length > 0)
                {
                    item.Url = "https://api.alternative.me/v2/ticker/?convert=" + quote + "&limit=10";
                    item.Method = "GET";
                    item.Template = symbol + " ${$.data." + dataId + ".quotes." + quote + ".price:0.00} (${$.data." + dataId + ".quotes." + quote + ".percentage_change_24h:0.00}%)";
                }

                return;
            }

            if (item.Type == ApiItemConfig.TypeExchangeRate)
            {
                string baseCurrency = String.IsNullOrWhiteSpace(item.BaseCurrency) ? "USD" : item.BaseCurrency.Trim().ToUpperInvariant();
                string quoteCurrency = String.IsNullOrWhiteSpace(item.QuoteCurrency) ? "CNY" : item.QuoteCurrency.Trim().ToUpperInvariant();
                item.BaseCurrency = baseCurrency;
                item.QuoteCurrency = quoteCurrency;
                if (String.IsNullOrWhiteSpace(item.Id))
                {
                    item.Id = (baseCurrency + "-" + quoteCurrency).ToLowerInvariant();
                }

                if (String.IsNullOrWhiteSpace(item.Name))
                {
                    item.Name = baseCurrency + "/" + quoteCurrency;
                }

                item.Url = "https://open.er-api.com/v6/latest/" + baseCurrency;
                item.Method = "GET";
                item.Template = baseCurrency + "/" + quoteCurrency + " ${$.rates." + quoteCurrency + ":0.0000}";
                return;
            }

            if (item.Type == ApiItemConfig.TypeHttpStatus)
            {
                if (String.IsNullOrWhiteSpace(item.Name))
                {
                    item.Name = "Website";
                }

                item.Method = "GET";
                item.Template = "";
            }
        }

        public string[] GetSymbols()
        {
            return Symbols.ToArray();
        }

        public int GetPollIntervalSeconds()
        {
            int seconds = RefreshSeconds;
            if (Items != null)
            {
                foreach (ApiItemConfig item in Items)
                {
                    if (item != null && item.Enabled && item.IntervalSeconds > 0 && item.IntervalSeconds < seconds)
                    {
                        seconds = item.IntervalSeconds;
                    }
                }
            }

            return Clamp(seconds, MinRefreshSeconds, MaxRefreshSeconds);
        }

        public bool HasSavedWindowPosition()
        {
            return WindowLeft != Int32.MinValue && WindowTop != Int32.MinValue;
        }

        public bool HasEnabledItems()
        {
            if (Items == null)
            {
                return false;
            }

            foreach (ApiItemConfig item in Items)
            {
                if (item != null && item.Enabled)
                {
                    return true;
                }
            }

            return false;
        }

        public static ApiItemConfig CreateKnownCoinItem(string symbol, int intervalSeconds, int timeoutSeconds)
        {
            string normalized = String.IsNullOrEmpty(symbol) ? "BTC" : symbol.Trim().ToUpperInvariant();
            string dataId = AlternativeMeDataId(normalized);
            if (dataId.Length == 0)
            {
                dataId = "1";
                normalized = "BTC";
            }

            return CreateAlternativeMeItem(normalized, dataId, "USD", "https://api.alternative.me/v2/ticker/?convert=USD&limit=10", intervalSeconds, timeoutSeconds);
        }

        private static List<ApiItemConfig> ReadItems(Dictionary<string, object> root, string key)
        {
            List<ApiItemConfig> items = new List<ApiItemConfig>();
            object value;
            if (!root.TryGetValue(key, out value) || value == null)
            {
                return items;
            }

            ArrayList array = value as ArrayList;
            object[] objectArray = value as object[];
            if (array != null)
            {
                foreach (object item in array)
                {
                    AddItem(items, item);
                }
            }
            else if (objectArray != null)
            {
                foreach (object item in objectArray)
                {
                    AddItem(items, item);
                }
            }

            return items;
        }

        private static void AddItem(List<ApiItemConfig> items, object value)
        {
            Dictionary<string, object> root = value as Dictionary<string, object>;
            if (root == null)
            {
                return;
            }

            ApiItemConfig item = new ApiItemConfig();
            bool hasType = root.ContainsKey("type");
            item.Type = ApiItemConfig.NormalizeType(GetString(root, "type", item.Type));
            item.Symbol = GetString(root, "symbol", item.Symbol);
            item.BaseCurrency = GetString(root, "baseCurrency", item.BaseCurrency);
            item.QuoteCurrency = GetString(root, "quoteCurrency", item.QuoteCurrency);
            item.Id = GetString(root, "id", item.Id);
            item.Name = GetString(root, "name", item.Name);
            item.Enabled = GetBool(root, "enabled", item.Enabled);
            item.Url = GetString(root, "url", item.Url);
            item.Method = NormalizeMethod(GetString(root, "method", item.Method));
            item.Body = GetString(root, "body", item.Body);
            item.Template = GetString(root, "template", item.Template);
            item.Template = GetString(root, "jsonPath", item.Template);
            item.IntervalSeconds = ClampOptionalTiming(GetInt(root, "intervalSeconds", item.IntervalSeconds), MinRefreshSeconds, MaxRefreshSeconds);
            item.TimeoutSeconds = ClampOptionalTiming(GetInt(root, "timeoutSeconds", item.TimeoutSeconds), 3, 120);
            item.Headers = ReadHeaders(root);

            if (!hasType)
            {
                UpgradeLegacyKnownCoinItem(item);
            }

            ApplyItemTypeDefaults(item);

            if (item.Url.Length > 0 || item.Type == ApiItemConfig.TypeHttpStatus)
            {
                items.Add(item);
            }
        }

        private static void UpgradeLegacyKnownCoinItem(ApiItemConfig item)
        {
            if (item == null || String.IsNullOrWhiteSpace(item.Url))
            {
                return;
            }

            if (item.Url.IndexOf("api.alternative.me/v2/ticker", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return;
            }

            string symbol = String.IsNullOrWhiteSpace(item.Name) ? item.Id : item.Name;
            symbol = String.IsNullOrWhiteSpace(symbol) ? "" : symbol.Trim().ToUpperInvariant();
            if (AlternativeMeDataId(symbol).Length == 0)
            {
                return;
            }

            item.Type = ApiItemConfig.TypeCoin;
            item.Symbol = symbol;
            item.QuoteCurrency = ExtractQuoteCurrency(item.Template, "USD");
        }

        private static string ExtractQuoteCurrency(string template, string fallback)
        {
            if (String.IsNullOrWhiteSpace(template))
            {
                return fallback;
            }

            string marker = ".quotes.";
            int start = template.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (start < 0)
            {
                return fallback;
            }

            start += marker.Length;
            int end = start;
            while (end < template.Length && Char.IsLetter(template[end]))
            {
                end++;
            }

            return end > start ? template.Substring(start, end - start).ToUpperInvariant() : fallback;
        }

        private static Dictionary<string, string> ReadHeaders(Dictionary<string, object> root)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            object value;
            if (!root.TryGetValue("headers", out value) || value == null)
            {
                return headers;
            }

            Dictionary<string, object> headerRoot = value as Dictionary<string, object>;
            if (headerRoot == null)
            {
                return headers;
            }

            foreach (KeyValuePair<string, object> pair in headerRoot)
            {
                if (!String.IsNullOrEmpty(pair.Key) && pair.Value != null)
                {
                    headers[pair.Key] = Convert.ToString(pair.Value);
                }
            }

            return headers;
        }

        private static void AddSymbol(List<string> symbols, object item)
        {
            if (item == null)
            {
                return;
            }

            string symbol = Convert.ToString(item).Trim().ToUpperInvariant();
            if (symbol.Length > 0 && !symbols.Contains(symbol))
            {
                symbols.Add(symbol);
            }
        }

        private static string GetString(Dictionary<string, object> root, string key, string fallback)
        {
            object value;
            if (!root.TryGetValue(key, out value) || value == null)
            {
                return fallback;
            }

            string text = Convert.ToString(value);
            return text.Length == 0 ? fallback : text;
        }

        private static int GetInt(Dictionary<string, object> root, string key, int fallback)
        {
            object value;
            if (!root.TryGetValue(key, out value) || value == null)
            {
                return fallback;
            }

            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return fallback;
            }
        }

        private static int GetOptionalInt(Dictionary<string, object> root, string key, int fallback, int min, int max)
        {
            object value;
            if (!root.TryGetValue(key, out value) || value == null)
            {
                return fallback;
            }

            try
            {
                return Clamp(Convert.ToInt32(value), min, max);
            }
            catch
            {
                return fallback;
            }
        }

        private static bool GetBool(Dictionary<string, object> root, string key, bool fallback)
        {
            object value;
            if (!root.TryGetValue(key, out value) || value == null)
            {
                return fallback;
            }

            try
            {
                return Convert.ToBoolean(value);
            }
            catch
            {
                return fallback;
            }
        }

        public static string NormalizeColorHex(string value, string fallback)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            string text = value.Trim();
            if (text.Length == 6)
            {
                text = "#" + text;
            }

            if (text.Length != 7 || text[0] != '#')
            {
                return fallback;
            }

            for (int i = 1; i < text.Length; i++)
            {
                char ch = text[i];
                bool isHex = (ch >= '0' && ch <= '9') ||
                    (ch >= 'a' && ch <= 'f') ||
                    (ch >= 'A' && ch <= 'F');
                if (!isHex)
                {
                    return fallback;
                }
            }

            return text.ToUpperInvariant();
        }

        private static string NormalizeMethod(string method)
        {
            if (String.IsNullOrEmpty(method))
            {
                return "GET";
            }

            method = method.Trim().ToUpperInvariant();
            if (method.Length == 0)
            {
                return "GET";
            }

            return method;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private static int ClampOptionalTiming(int value, int min, int max)
        {
            if (value <= 0)
            {
                return 0;
            }

            return Clamp(value, min, max);
        }

        private static string NormalizeAnchor(string anchor)
        {
            if (String.IsNullOrEmpty(anchor))
            {
                return "left";
            }

            string value = anchor.Trim().ToLowerInvariant();
            if (value == "right" || value == "left")
            {
                return value;
            }

            return "left";
        }

        private static string PrettyJson(string compact)
        {
            int indent = 0;
            bool quoted = false;
            bool escaped = false;
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int i = 0; i < compact.Length; i++)
            {
                char ch = compact[i];
                if (escaped)
                {
                    builder.Append(ch);
                    escaped = false;
                    continue;
                }

                if (ch == '\\')
                {
                    builder.Append(ch);
                    escaped = true;
                    continue;
                }

                if (ch == '"')
                {
                    quoted = !quoted;
                    builder.Append(ch);
                    continue;
                }

                if (!quoted && (ch == '{' || ch == '['))
                {
                    builder.Append(ch);
                    builder.AppendLine();
                    indent++;
                    AppendIndent(builder, indent);
                    continue;
                }

                if (!quoted && (ch == '}' || ch == ']'))
                {
                    builder.AppendLine();
                    indent--;
                    AppendIndent(builder, indent);
                    builder.Append(ch);
                    continue;
                }

                if (!quoted && ch == ',')
                {
                    builder.Append(ch);
                    builder.AppendLine();
                    AppendIndent(builder, indent);
                    continue;
                }

                if (!quoted && ch == ':')
                {
                    builder.Append(": ");
                    continue;
                }

                builder.Append(ch);
            }

            return builder.ToString();
        }

        private static void AppendIndent(System.Text.StringBuilder builder, int indent)
        {
            for (int i = 0; i < indent; i++)
            {
                builder.Append("  ");
            }
        }
    }

    internal sealed class ApiItemConfig
    {
        public const string TypeCustomApi = "customApi";
        public const string TypeCoin = "coin";
        public const string TypeExchangeRate = "exchangeRate";
        public const string TypeHttpStatus = "httpStatus";

        public string Type;
        public string Symbol;
        public string BaseCurrency;
        public string QuoteCurrency;
        public string Id;
        public string Name;
        public bool Enabled;
        public string Url;
        public string Method;
        public Dictionary<string, string> Headers;
        public string Body;
        public string Template;
        public int IntervalSeconds;
        public int TimeoutSeconds;

        public ApiItemConfig()
        {
            Type = TypeCustomApi;
            Symbol = "";
            BaseCurrency = "";
            QuoteCurrency = "USD";
            Id = "";
            Name = "";
            Enabled = true;
            Url = "";
            Method = "GET";
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Body = "";
            Template = "";
            IntervalSeconds = 0;
            TimeoutSeconds = 0;
        }

        public string EffectiveId(int index)
        {
            if (!String.IsNullOrEmpty(Id))
            {
                return Id;
            }

            if (!String.IsNullOrEmpty(Name))
            {
                return Name;
            }

            return "item-" + index.ToString();
        }

        public string DisplayName(int index)
        {
            if (!String.IsNullOrEmpty(Name))
            {
                return Name;
            }

            if (!String.IsNullOrEmpty(Id))
            {
                return Id;
            }

            return "Item " + (index + 1).ToString();
        }

        public static string NormalizeType(string type)
        {
            if (String.IsNullOrWhiteSpace(type))
            {
                return TypeCustomApi;
            }

            string value = type.Trim();
            if (String.Equals(value, TypeCoin, StringComparison.OrdinalIgnoreCase))
            {
                return TypeCoin;
            }

            if (String.Equals(value, TypeExchangeRate, StringComparison.OrdinalIgnoreCase))
            {
                return TypeExchangeRate;
            }

            if (String.Equals(value, TypeHttpStatus, StringComparison.OrdinalIgnoreCase))
            {
                return TypeHttpStatus;
            }

            return TypeCustomApi;
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> root = new Dictionary<string, object>();
            root["type"] = NormalizeType(Type);
            if (!String.IsNullOrWhiteSpace(Symbol))
            {
                root["symbol"] = Symbol;
            }

            if (!String.IsNullOrWhiteSpace(BaseCurrency))
            {
                root["baseCurrency"] = BaseCurrency;
            }

            if (!String.IsNullOrWhiteSpace(QuoteCurrency))
            {
                root["quoteCurrency"] = QuoteCurrency;
            }

            root["id"] = Id;
            root["name"] = Name;
            root["enabled"] = Enabled;
            root["url"] = Url;
            root["method"] = Method;
            if (Headers != null && Headers.Count > 0)
            {
                Dictionary<string, object> headers = new Dictionary<string, object>();
                foreach (KeyValuePair<string, string> pair in Headers)
                {
                    headers[pair.Key] = pair.Value;
                }

                root["headers"] = headers;
            }
            root["body"] = Body;
            root["template"] = Template;
            if (IntervalSeconds > 0)
            {
                root["intervalSeconds"] = IntervalSeconds;
            }

            if (TimeoutSeconds > 0)
            {
                root["timeoutSeconds"] = TimeoutSeconds;
            }
            return root;
        }
    }
}
