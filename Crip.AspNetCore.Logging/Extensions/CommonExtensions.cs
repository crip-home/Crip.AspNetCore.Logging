using System;

namespace Crip.AspNetCore.Logging
{
    internal static class CommonExtensions
    {
        private const StringComparison OrdinalIgnoreCase = StringComparison.OrdinalIgnoreCase;
        private const StringComparison Ordinal = StringComparison.Ordinal;

        internal static bool OrdinalEquals(this string? s, string value, bool ignoreCase = false) =>
            s != null && s.Equals(value, ignoreCase ? OrdinalIgnoreCase : Ordinal);

        internal static bool OrdinalContains(this string? s, string value, bool ignoreCase = false) =>
            s != null && s.IndexOf(value, ignoreCase ? OrdinalIgnoreCase : Ordinal) >= 0;

        internal static bool OrdinalStartsWith(this string? s, string value, bool ignoreCase = false) =>
            s != null && s.StartsWith(value, ignoreCase ? OrdinalIgnoreCase : Ordinal);

        internal static bool OrdinalEndsWith(this string? s, string value, bool ignoreCase = false) =>
            s != null && s.EndsWith(value, ignoreCase ? OrdinalIgnoreCase : Ordinal);
    }
}
