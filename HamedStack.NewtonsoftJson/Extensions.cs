// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable CommentTypo

using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HamedStack.NewtonsoftJson;

/// <summary>
/// Provides extension methods for Newtonsoft.Json library functionalities.
/// </summary>
public static class NewtonsoftJsonExtensions
{
    /// <summary>
    /// Converts a JS array text to a JObject representation with a specified root name.
    /// </summary>
    /// <param name="jsArrayText">The JS array text.</param>
    /// <param name="rootName">The root name for the resulting JObject.</param>
    /// <returns>The JObject representation of the provided JS array text.</returns>
    public static JObject ConvertJsArrayTextToJObject(this string jsArrayText, string rootName)
    {
        return JObject.Parse(jsArrayText.ConvertJsArrayTextToJsonObject(rootName));
    }

    /// <summary>
    /// Converts a JS array text to a JSON object string with a specified root name.
    /// </summary>
    /// <param name="jsArrayText">The JS array text to convert.</param>
    /// <param name="rootName">The root name for the resulting JSON object.</param>
    /// <returns>A JSON object string that represents the provided JS array text.</returns>
    public static string ConvertJsArrayTextToJsonObject(this string jsArrayText, string rootName)
    {
        if (string.IsNullOrWhiteSpace(jsArrayText))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(jsArrayText));
        if (string.IsNullOrWhiteSpace(rootName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(rootName));

        jsArrayText = jsArrayText.Trim().Trim(';').Trim(',');

        var isArrayText = jsArrayText.RemoveWhiteSpaces().Trim().StartsWith("[");
        if (!isArrayText)
        {
            throw new ArgumentException("Value is not a array.", nameof(jsArrayText));
        }

        var regex = new Regex(@"(\w+):");

        var matches = regex.Matches(jsArrayText);
        do
        {
            jsArrayText = jsArrayText.Replace(matches[0].Index, matches[0].Length, $"\"{matches[0].Groups[1].Value}\":");
            matches = regex.Matches(jsArrayText);
        } while (matches.Count > 0);

        var json = $"{{ \"{rootName}\" : {jsArrayText} }}".ToFormattedJson();
        return json;
    }

    /// <summary>
    /// Converts a JS array text to a JToken representation with a specified root name.
    /// </summary>
    /// <param name="jsArrayText">The JS array text to convert.</param>
    /// <param name="rootName">The root name for the resulting JToken.</param>
    /// <returns>The JToken representation of the provided JS array text.</returns>
    public static JToken ConvertJsArrayTextToJToken(this string jsArrayText, string rootName)
    {
        return JToken.Parse(jsArrayText.ConvertJsArrayTextToJsonObject(rootName));
    }

    /// <summary>
    /// Deserializes the byte array into an object of type T.
    /// </summary>
    /// <param name="data">Byte array containing serialized data.</param>
    /// <param name="settings">Optional JsonSerializerSettings to customize the deserialization process.</param>
    /// <returns>The deserialized object of type T.</returns>
    public static T? Deserialize<T>(this byte[] data, JsonSerializerSettings? settings = null)
    {
        return JsonConvert.DeserializeObject<T>(data.ToText(), settings);
    }

    /// <summary>
    /// Deserializes the content of a stream into an object of type T.
    /// </summary>
    /// <param name="stream">Stream containing serialized data.</param>
    /// <param name="settings">Optional JsonSerializerSettings to customize the deserialization process.</param>
    /// <returns>The deserialized object of type T.</returns>
    public static T? Deserialize<T>(this Stream stream, JsonSerializerSettings? settings = null)
    {
        return JsonConvert.DeserializeObject<T>(stream.ToText(), settings);
    }

    /// <summary>
    /// Flattens the provided JToken structure into a linear sequence of JTokens.
    /// </summary>
    /// <param name="jToken">The JToken to flatten.</param>
    /// <returns>A linear sequence of JTokens from the provided structure.</returns>
    public static IEnumerable<JToken> Flatten(this JToken jToken)
    {
        return FlattenByQueue(jToken, x => x.Children());
    }

    /// <summary>
    /// Deserializes the provided JSON text into an object of type T.
    /// </summary>
    /// <param name="jsonText">The JSON text to deserialize.</param>
    /// <param name="settings">Optional JsonSerializerSettings to customize the deserialization process.</param>
    /// <returns>The deserialized object of type T.</returns>
    public static T? FromJson<T>(this string jsonText, JsonSerializerSettings? settings = null)
    {
        return JsonConvert.DeserializeObject<T>(jsonText, settings);
    }

    /// <summary>
    /// Retrieves the last element in the JSON path.
    /// </summary>
    /// <param name="jsonDetail">The JSON detail containing the path.</param>
    /// <returns>The last element in the JSON path.</returns>
    public static string GetChild(this JsonDetail jsonDetail)
    {
        return jsonDetail.Path.Last();
    }

    /// <summary>
    /// Extracts a collection of JSON details from a JToken.
    /// </summary>
    /// <param name="token">The JToken to extract details from.</param>
    /// <returns>A collection of JSON details.</returns>
    public static IEnumerable<JsonDetail> GetJsonDetails(this JToken token)
    {
        var fields = new List<JsonDetail>();
        var queue = new Queue<JToken>();
        queue.Enqueue(token);

        while (queue.Count > 0)
        {
            var currentToken = queue.Dequeue();

            switch (currentToken.Type)
            {
                // If the token is a JObject, push its properties onto the stack
                case JTokenType.Object:
                    foreach (var prop in currentToken.Children<JProperty>())
                    {
                        queue.Enqueue(prop.Value);
                    }
                    break;
                // If the token is an array, push its elements onto the stack
                case JTokenType.Array:
                    foreach (var child in currentToken.Children())
                    {
                        queue.Enqueue(child);
                    }
                    break;
                // Otherwise, the token is a leaf node, so add its name and value to the list
                default:
                    var path = currentToken.Path.SplitPath();
                    var type = currentToken.Type;
                    fields
                        .Add(new JsonDetail()
                        {
                            Path = path,
                            Value = currentToken.ToObject<object>(),
                            JTokenType = type
                        });
                    break;
            }
        }

        return fields;
    }

    /// <summary>
    /// Retrieves the parent elements in the JSON path.
    /// </summary>
    /// <param name="jsonDetail">The JSON detail containing the path.</param>
    /// <returns>An array of parent elements or null if there's only one element.</returns>
    public static string[]? GetParents(this JsonDetail jsonDetail)
    {
        var p = jsonDetail.Path;
        return p.Length <= 1 ? null : p.Take(p.Length - 1).ToArray();
    }

    /// <summary>
    /// Extracts path details from a path section.
    /// </summary>
    /// <param name="pathSection">The section of the path to extract details from.</param>
    /// <returns>Path details extracted from the path section.</returns>
    public static PathDetail GetPathDetail(this string pathSection)
    {
        var regex = new Regex(@"\[(\d+)\]", RegexOptions.Compiled);
        var isArray = regex.IsMatch(pathSection);
        var match = regex.Match(pathSection);
        var name = isArray ? regex.Replace(pathSection, string.Empty) : pathSection;
        var index = isArray ? Convert.ToInt32(match.Groups[1].Value) : -1;
        return new PathDetail()
        {
            Index = index,
            IsArray = isArray,
            Name = name
        };
    }

    /// <summary>
    /// Extracts path details from a JSON detail.
    /// </summary>
    /// <param name="jsonDetail">The JSON detail to extract path details from.</param>
    /// <returns>A collection of path details.</returns>
    public static IEnumerable<PathDetail> GetPathDetail(this JsonDetail jsonDetail)
    {
        return jsonDetail.Path.Select(path => path.GetPathDetail());
    }

    /// <summary>
    /// Determines whether the provided JSON text represents an OpenAPI document.
    /// </summary>
    /// <param name="swaggerJsonText">The JSON text to evaluate.</param>
    /// <returns>true if the JSON text represents an OpenAPI document; otherwise, false. Returns null if parsing fails.</returns>
    public static bool? IsOpenApiDocument(string swaggerJsonText)
    {
        try
        {
            var parsedJson = JToken.Parse(swaggerJsonText);

            var openapi = parsedJson["openapi"];
            var swagger = parsedJson["swagger"];
            var info = parsedJson["info"];
            var title = parsedJson["info"]?["title"];
            var version = parsedJson["info"]?["version"];
            var paths = parsedJson["paths"];

            return (openapi != null || swagger != null)
                         && info != null
                         && version != null
                         && paths != null
                         && title != null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determines if the provided JTokenType is a primitive type.
    /// </summary>
    /// <param name="type">The JTokenType to evaluate.</param>
    /// <returns>true if the JTokenType is a primitive type; otherwise, false.</returns>
    public static bool IsPrimitive(this JTokenType type)
    {
        switch (type)
        {
            case JTokenType.Boolean:
            case JTokenType.Date:
            case JTokenType.Float:
            case JTokenType.Guid:
            case JTokenType.Integer:
            case JTokenType.String:
            case JTokenType.TimeSpan:
            case JTokenType.Undefined:
            case JTokenType.Null:
            case JTokenType.Uri:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Converts a JSON string to a dynamic object.
    /// </summary>
    /// <param name="jsonText">The JSON text to convert.</param>
    /// <param name="settings">Optional JsonSerializerSettings to customize the conversion process.</param>
    /// <returns>A dynamic object representing the provided JSON text.</returns>
    public static dynamic? ToDynamic(this string jsonText, JsonSerializerSettings? settings = null)
    {
        return JsonConvert.DeserializeObject<dynamic>(jsonText, settings);
    }

    /// <summary>
    /// Converts a JSON string to a formatted (indented) version of the JSON.
    /// </summary>
    /// <param name="jsonText">The JSON text to format.</param>
    /// <returns>The formatted JSON string.</returns>
    public static string ToFormattedJson(this string jsonText)
    {
        jsonText = jsonText.Trim().Trim(',');
        return JToken.Parse(jsonText).ToString(Formatting.Indented);
    }

    /// <summary>
    /// Converts the provided object into its JSON string representation with formatting.
    /// </summary>
    /// <param name="obj">The object to convert.</param>
    /// <returns>The formatted JSON string representation of the object.</returns>
    public static string ToIndentedJson<T>(this T obj)
    {
        return JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented
        });
    }

    /// <summary>
    /// Converts the provided object into its JSON string representation.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to convert.</param>
    /// <param name="settings">Optional JsonSerializerSettings to customize the serialization process.</param>
    /// <returns>The JSON string representation of the object.</returns>
    public static string ToJson<T>(this T obj, JsonSerializerSettings? settings = null)
    {
        return JsonConvert.SerializeObject(obj, settings);
    }

    /// <summary>
    /// Converts the provided JSON string into an object.
    /// </summary>
    /// <param name="jsonText">The JSON text to convert.</param>
    /// <param name="settings">Optional JsonSerializerSettings to customize the deserialization process.</param>
    /// <returns>The deserialized object from the provided JSON text.</returns>
    public static object? ToObject(this string jsonText, JsonSerializerSettings? settings = null)
    {
        return JsonConvert.DeserializeObject<object>(jsonText, settings);
    }

    /// <summary>
    /// Traverses a JToken and applies a specified action to each of its elements.
    /// </summary>
    /// <param name="jToken">The JToken to traverse.</param>
    /// <param name="action">The action to apply to each JToken during traversal.</param>
    public static void Traverse(this JToken jToken, Action<JToken> action)
    {
        foreach (var jt in jToken.Flatten())
        {
            action(jt);
        }
    }

    /// <summary>
    /// Attempts to parse a string as JSON and, if successful, deserializes it to an object of type T.
    /// </summary>
    /// <param name="jsonText">The JSON text to try to parse and deserialize.</param>
    /// <param name="result">The deserialized object if parsing is successful; otherwise, default(T).</param>
    /// <returns>true if the string was successfully parsed and deserialized; otherwise, false.</returns>
    public static bool TryParseAsJson<T>(this string jsonText, out T? result)
    {
        if (jsonText == null) throw new ArgumentNullException(nameof(jsonText));

        var isJson = jsonText.RemoveWhiteSpaces().StartsWith("{");
        if (!isJson)
        {
            result = default;
            return false;
        }
        try
        {
            result = JsonConvert.DeserializeObject<T>(jsonText, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Flattens a collection of items using a queue and a specified function to retrieve child items.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="items">The items to flatten.</param>
    /// <param name="getChildren">A function that returns child items for a given item.</param>
    /// <returns>A flattened collection of items.</returns>
    private static IEnumerable<T> FlattenByQueue<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>?>? getChildren)
    {
        var itemsToYield = new Queue<T>(items);
        while (itemsToYield.Count > 0)
        {
            var item = itemsToYield.Dequeue();
            yield return item;

            var children = getChildren?.Invoke(item);
            if (children == null) continue;
            foreach (var child in children)
            {
                if (child != null)
                    itemsToYield.Enqueue(child);
            }
        }
    }

    /// <summary>
    /// Removes whitespace characters from a string.
    /// </summary>
    /// <param name="text">The string from which whitespace characters will be removed.</param>
    /// <returns>The input string without whitespace characters.</returns>
    private static string RemoveWhiteSpaces(this string text)
    {
        return string.IsNullOrEmpty(text) ? text : Regex.Replace(text, @"\s+", string.Empty).Trim();
    }

    /// <summary>
    /// Replaces a portion of a string with a specified replacement string.
    /// </summary>
    /// <param name="text">The original string.</param>
    /// <param name="startIndex">The zero-based starting index of the portion to replace.</param>
    /// <param name="length">The length of the portion to replace.</param>
    /// <param name="replacement">The string that replaces the portion of the original string.</param>
    /// <returns>The modified string.</returns>
    private static string Replace(this string text, int startIndex, int length, string replacement)
    {
        return text.Remove(startIndex, length).Insert(startIndex, replacement);
    }

    /// <summary>
    /// Splits a JSON path into its individual sections.
    /// </summary>
    /// <param name="path">The JSON path to split.</param>
    /// <returns>An array of strings that contains the sections of the path.</returns>
    private static string[] SplitPath(this string path)
    {
        const string separator = "%|---|%";
        if (path.Contains("['") || path.Contains("[\""))
        {
            path = path.Replace("['", separator);
            path = path.Replace("']", string.Empty);
            path = path.Replace("[\"", separator);
            path = path.Replace("\"]", string.Empty);
            return path.Split(new[] { separator }, StringSplitOptions.None);
        }
        return path.Split('.');
    }

    /// <summary>
    /// Converts a byte array into its string representation using UTF32 encoding.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The string representation of the byte array.</returns>
    private static string ToText(this byte[] bytes)
    {
        return Encoding.UTF32.GetString(bytes);
    }

    /// <summary>
    /// Reads the entire content of a stream and converts it into a string using UTF8 encoding.
    /// </summary>
    /// <param name="this">The stream to read and convert.</param>
    /// <returns>The string representation of the stream's content.</returns>
    private static string ToText(this Stream @this)
    {
        using var sr = new StreamReader(@this, Encoding.UTF8);
        return sr.ReadToEnd();
    }
}