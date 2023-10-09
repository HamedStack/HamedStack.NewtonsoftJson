// ReSharper disable IdentifierTypo

namespace HamedStack.NewtonsoftJson;

/// <summary>
/// Represents details about a specific path within a JSON structure.
/// </summary>
public class PathDetail
{
    /// <summary>
    /// Gets or sets the index of the element if it's part of an array. 
    /// A default value of -1 indicates that the detail is not referencing an array element.
    /// </summary>
    public int Index { get; set; } = -1;

    /// <summary>
    /// Gets or sets a value indicating whether the detail refers to an array.
    /// </summary>
    public bool IsArray { get; set; }

    /// <summary>
    /// Gets or sets the name of the property or element.
    /// </summary>
    public string Name { get; set; } = null!;
}