using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CryptoMonitor
{
    internal sealed class CryptoPriceService
    {
        private readonly Dictionary<string, CachedApiItem> cachedItems = new Dictionary<string, CachedApiItem>(StringComparer.OrdinalIgnoreCase);

        public Task<string> FetchAsync(AppConfig config)
        {
            return Task.Factory.StartNew(delegate
            {
                return FetchConfiguredItems(config);
            });
        }

        internal static string Download(ApiItemConfig item, int fallbackTimeoutSeconds)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            int timeoutSeconds = item.TimeoutSeconds > 0 ? item.TimeoutSeconds : fallbackTimeoutSeconds;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(item.Url);
            request.Method = String.IsNullOrEmpty(item.Method) ? "GET" : item.Method.ToUpperInvariant();
            request.Timeout = timeoutSeconds * 1000;
            request.ReadWriteTimeout = timeoutSeconds * 1000;
            request.UserAgent = "CryptoMonitor/0.1";

            if (item.Headers != null)
            {
                foreach (KeyValuePair<string, string> pair in item.Headers)
                {
                    ApplyHeader(request, pair.Key, pair.Value);
                }
            }

            if (!String.IsNullOrEmpty(item.Body) && request.Method != "GET" && request.Method != "HEAD")
            {
                byte[] bytes = Encoding.UTF8.GetBytes(item.Body);
                if (String.IsNullOrEmpty(request.ContentType))
                {
                    request.ContentType = "application/json";
                }

                request.ContentLength = bytes.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            using (WebResponse response = request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private string FetchConfiguredItems(AppConfig config)
        {
            List<string> parts = new List<string>();
            DateTime now = DateTime.Now;
            for (int i = 0; i < config.Items.Count; i++)
            {
                ApiItemConfig item = config.Items[i];
                if (item == null || !item.Enabled)
                {
                    continue;
                }

                string id = item.EffectiveId(i);
                CachedApiItem cached;
                bool hasCache = cachedItems.TryGetValue(id, out cached);
                int intervalSeconds = item.IntervalSeconds > 0 ? item.IntervalSeconds : config.RefreshSeconds;
                bool shouldFetch = !hasCache || cached.LastFetched.AddSeconds(intervalSeconds) <= now;
                if (shouldFetch)
                {
                    try
                    {
                        string response = Download(item, config.RequestTimeoutSeconds);
                        string text = JsonTemplateRenderer.Render(response, item.Template);
                        cached = new CachedApiItem();
                        cached.Text = text;
                        cached.LastFetched = now;
                        cachedItems[id] = cached;
                        hasCache = true;
                    }
                    catch
                    {
                        if (!hasCache)
                        {
                            cached = new CachedApiItem();
                            cached.Text = item.DisplayName(i) + ": --";
                            cached.LastFetched = now;
                            cachedItems[id] = cached;
                            hasCache = true;
                        }
                    }
                }

                if (hasCache && cached != null && !String.IsNullOrEmpty(cached.Text))
                {
                    parts.Add(cached.Text);
                }
            }

            if (parts.Count == 0)
            {
                throw new InvalidOperationException(Localization.Text(config, "ApiResponseNoItems"));
            }

            string joinedItems = String.Join(AppConfig.DecodeTextEscapes(config.ItemSeparator), parts.ToArray());
            return RenderDisplayTemplate(config.DisplayTemplate, joinedItems, parts.Count, now);
        }

        private static string RenderDisplayTemplate(string template, string items, int count, DateTime now)
        {
            if (String.IsNullOrEmpty(template))
            {
                return items;
            }

            return template
                .Replace("{items}", items)
                .Replace("{count}", count.ToString())
                .Replace("{date}", now.ToString("yyyy-MM-dd"))
                .Replace("{time}", now.ToString("HH:mm:ss"));
        }

        private static void ApplyHeader(HttpWebRequest request, string key, string value)
        {
            if (String.IsNullOrEmpty(key) || value == null)
            {
                return;
            }

            if (String.Equals(key, "Accept", StringComparison.OrdinalIgnoreCase))
            {
                request.Accept = value;
            }
            else if (String.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
            {
                request.ContentType = value;
            }
            else if (String.Equals(key, "Referer", StringComparison.OrdinalIgnoreCase))
            {
                request.Referer = value;
            }
            else if (String.Equals(key, "User-Agent", StringComparison.OrdinalIgnoreCase))
            {
                request.UserAgent = value;
            }
            else
            {
                request.Headers[key] = value;
            }
        }


        private sealed class CachedApiItem
        {
            public string Text;
            public DateTime LastFetched;
        }
    }
}
