using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Script.Serialization;

namespace PinPulse
{
    internal static class JsonTemplateRenderer
    {
        public static string Render(string json, string template)
        {
            if (String.IsNullOrEmpty(template))
            {
                return json;
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            object root = serializer.DeserializeObject(json);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < template.Length;)
            {
                if (template[i] != '$')
                {
                    builder.Append(template[i]);
                    i++;
                    continue;
                }

                if (i + 1 < template.Length && template[i + 1] == '$')
                {
                    builder.Append('$');
                    i += 2;
                    continue;
                }

                if (i + 1 < template.Length && template[i + 1] == '{')
                {
                    int end = template.IndexOf('}', i + 2);
                    if (end >= 0)
                    {
                        string token = template.Substring(i + 2, end - i - 2).Trim();
                        string path;
                        string format;
                        SplitToken(token, out path, out format);
                        builder.Append(FormatValue(Evaluate(root, path), format));
                        i = end + 1;
                        continue;
                    }
                }

                int pathEnd = ScanPath(template, i);
                if (pathEnd > i)
                {
                    string path = template.Substring(i, pathEnd - i);
                    builder.Append(FormatValue(Evaluate(root, path), null));
                    i = pathEnd;
                    continue;
                }

                builder.Append(template[i]);
                i++;
            }

            return builder.ToString();
        }

        private static int ScanPath(string text, int start)
        {
            int i = start;
            bool inBracket = false;
            char quote = '\0';
            while (i < text.Length)
            {
                char ch = text[i];
                if (quote != '\0')
                {
                    if (ch == quote)
                    {
                        quote = '\0';
                    }
                    i++;
                    continue;
                }

                if (inBracket)
                {
                    if (ch == '"' || ch == '\'')
                    {
                        quote = ch;
                    }
                    else if (ch == ']')
                    {
                        inBracket = false;
                    }
                    i++;
                    continue;
                }

                if (ch == '[')
                {
                    inBracket = true;
                    i++;
                    continue;
                }

                if (ch == '$' || ch == '.' || ch == '_' || ch == '-' || Char.IsLetterOrDigit(ch))
                {
                    i++;
                    continue;
                }

                break;
            }

            return i > start + 1 ? i : start;
        }

        private static object Evaluate(object root, string path)
        {
            if (String.IsNullOrEmpty(path) || path[0] != '$')
            {
                return null;
            }

            if (path.Length == 1)
            {
                return root;
            }

            object current = root;
            int i = 1;
            while (i < path.Length)
            {
                if (path[i] == '.')
                {
                    if (i + 1 < path.Length && path[i + 1] == '.')
                    {
                        i += 2;
                        string recursiveKey = ReadPropertyName(path, ref i);
                        current = FindRecursive(current, recursiveKey);
                    }
                    else
                    {
                        i++;
                        string key = ReadPropertyName(path, ref i);
                        current = GetProperty(current, key);
                    }
                }
                else if (path[i] == '[')
                {
                    int close = path.IndexOf(']', i + 1);
                    if (close < 0)
                    {
                        return null;
                    }

                    string token = path.Substring(i + 1, close - i - 1).Trim();
                    current = EvaluateBracket(current, token);
                    i = close + 1;
                }
                else
                {
                    return null;
                }

                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }

        private static string ReadPropertyName(string path, ref int index)
        {
            int start = index;
            while (index < path.Length && path[index] != '.' && path[index] != '[')
            {
                index++;
            }

            return path.Substring(start, index - start);
        }

        private static object EvaluateBracket(object current, string token)
        {
            if (String.IsNullOrEmpty(token))
            {
                return null;
            }

            if ((token[0] == '"' && token[token.Length - 1] == '"') || (token[0] == '\'' && token[token.Length - 1] == '\''))
            {
                return GetProperty(current, token.Substring(1, token.Length - 2));
            }

            int index;
            if (Int32.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
            {
                return GetArrayItem(current, index);
            }

            return GetProperty(current, token);
        }

        private static object GetProperty(object current, string key)
        {
            Dictionary<string, object> dictionary = current as Dictionary<string, object>;
            if (dictionary == null || key == null)
            {
                return null;
            }

            object value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }

            return null;
        }

        private static object GetArrayItem(object current, int index)
        {
            ArrayList array = current as ArrayList;
            if (array != null)
            {
                if (index < 0)
                {
                    index = array.Count + index;
                }

                return index >= 0 && index < array.Count ? array[index] : null;
            }

            object[] objectArray = current as object[];
            if (objectArray != null)
            {
                if (index < 0)
                {
                    index = objectArray.Length + index;
                }

                return index >= 0 && index < objectArray.Length ? objectArray[index] : null;
            }

            return null;
        }

        private static object FindRecursive(object current, string key)
        {
            Dictionary<string, object> dictionary = current as Dictionary<string, object>;
            if (dictionary != null)
            {
                object value;
                if (dictionary.TryGetValue(key, out value))
                {
                    return value;
                }

                foreach (object child in dictionary.Values)
                {
                    object found = FindRecursive(child, key);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            ArrayList array = current as ArrayList;
            if (array != null)
            {
                foreach (object child in array)
                {
                    object found = FindRecursive(child, key);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            object[] objectArray = current as object[];
            if (objectArray != null)
            {
                foreach (object child in objectArray)
                {
                    object found = FindRecursive(child, key);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        private static void SplitToken(string token, out string path, out string format)
        {
            path = token;
            format = null;
            int depth = 0;
            char quote = '\0';
            for (int i = 0; i < token.Length; i++)
            {
                char ch = token[i];
                if (quote != '\0')
                {
                    if (ch == quote)
                    {
                        quote = '\0';
                    }
                    continue;
                }

                if (ch == '"' || ch == '\'')
                {
                    quote = ch;
                    continue;
                }

                if (ch == '[')
                {
                    depth++;
                    continue;
                }

                if (ch == ']')
                {
                    depth--;
                    continue;
                }

                if (ch == ':' && depth == 0)
                {
                    path = token.Substring(0, i).Trim();
                    format = token.Substring(i + 1).Trim();
                    return;
                }
            }
        }

        private static string FormatValue(object value, string format)
        {
            if (value == null)
            {
                return "--";
            }

            if (value is string)
            {
                return Convert.ToString(value);
            }

            if (value is IFormattable)
            {
                return ((IFormattable)value).ToString(String.IsNullOrEmpty(format) ? null : format, CultureInfo.InvariantCulture);
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(value);
        }
    }
}
