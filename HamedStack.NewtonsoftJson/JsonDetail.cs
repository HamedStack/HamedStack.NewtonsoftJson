// ReSharper disable IdentifierTypo

using Newtonsoft.Json.Linq;

namespace HamedStack.NewtonsoftJson;

/// <summary>
/// Represents specific details about a JSON structure.
/// </summary>
public class JsonDetail
{
    /// <summary>
    /// Gets or sets the type of the JSON token.
    /// </summary>
    public JTokenType JTokenType { get; set; }

    /// <summary>
    /// Gets or sets the path to the specific detail within the JSON structure.
    /// Each string in the array represents a step in the path.
    /// </summary>
    public string[] Path { get; set; } = null!;

    /// <summary>
    /// Gets or sets the value of the specific detail within the JSON structure.
    /// </summary>
    public object? Value { get; set; }
}