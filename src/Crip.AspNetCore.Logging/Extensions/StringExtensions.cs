namespace Crip.AspNetCore.Logging;

/// <summary>
/// System string extensions.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Indicates whether the specified string is <c>null</c> or an empty string ("").
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns>The <c>true</c> if value is <c>null</c> or an empty string.</returns>
    public static bool IsNullOrEmpty(this string value)
        => string.IsNullOrEmpty(value);

    /// <summary>
    /// Indicates whether the specified string is not <c>null</c> and not empty string ("").
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns>The <c>true</c> if value is null or an empty string.</returns>
    public static bool NotNullAndNotEmpty(this string value)
        => !value.IsNullOrEmpty();

    /// <summary>
    /// Indicates whether the specified string is <c>null</c>, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns>The <c>true</c> if value is <c>null</c>, empty, or consists only of white-space characters.</returns>
    public static bool IsNullOrWhiteSpace(this string value)
        => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Indicates whether the specified string is not <c>null</c>, not empty, and not consists only of white-space characters.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns>The <c>true</c> if value is not <c>null</c>, not empty, and not consists only of white-space characters.</returns>
    public static bool NotNullAndNotWhiteSpace(this string value)
        => !value.IsNullOrWhiteSpace();
}