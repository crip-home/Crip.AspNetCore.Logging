using System;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// Common action extensions.
/// </summary>
internal static class CommonExtensions
{
    private const StringComparison OrdinalIgnoreCase = StringComparison.OrdinalIgnoreCase;
    private const StringComparison Ordinal = StringComparison.Ordinal;

    /// <summary>
    /// Determines is the string value is ordinal equal.
    /// </summary>
    /// <param name="s">The source value to compare.</param>
    /// <param name="value">The target value to compare with.</param>
    /// <param name="ignoreCase">Ignore case while comparing values.</param>
    /// <returns>The <c>true</c> if values are ordinal equal.</returns>
    internal static bool OrdinalEquals(this string? s, string value, bool ignoreCase = false) =>
        s != null && s.Equals(value, ignoreCase ? OrdinalIgnoreCase : Ordinal);

    /// <summary>
    /// Determines is the string value is inside source.
    /// </summary>
    /// <param name="s">The source value to compare.</param>
    /// <param name="value">The target value to compare with.</param>
    /// <param name="ignoreCase">Ignore case while comparing values.</param>
    /// <returns>The <c>true</c> if is inside source.</returns>
    internal static bool OrdinalContains(this string? s, string value, bool ignoreCase = false) =>
        s != null && s.IndexOf(value, ignoreCase ? OrdinalIgnoreCase : Ordinal) >= 0;

    /// <summary>
    /// Determines is the string value starts from value.
    /// </summary>
    /// <param name="s">The source value to compare.</param>
    /// <param name="value">The target value to compare with.</param>
    /// <param name="ignoreCase">Ignore case while comparing values.</param>
    /// <returns>The <c>true</c> if starts from value.</returns>
    internal static bool OrdinalStartsWith(this string? s, string value, bool ignoreCase = false) =>
        s != null && s.StartsWith(value, ignoreCase ? OrdinalIgnoreCase : Ordinal);

    /// <summary>
    /// Determines is the string value ends with value.
    /// </summary>
    /// <param name="s">The source value to compare.</param>
    /// <param name="value">The target value to compare with.</param>
    /// <param name="ignoreCase">Ignore case while comparing values.</param>
    /// <returns>The <c>true</c> if ends with value.</returns>
    internal static bool OrdinalEndsWith(this string? s, string value, bool ignoreCase = false) =>
        s != null && s.EndsWith(value, ignoreCase ? OrdinalIgnoreCase : Ordinal);
}