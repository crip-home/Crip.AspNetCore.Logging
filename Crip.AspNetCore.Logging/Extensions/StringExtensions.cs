namespace Crip.AspNetCore.Logging
{
    internal static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string value)
            => string.IsNullOrEmpty(value);

        public static bool NotNullAndNotEmpty(this string value)
            => !value.IsNullOrEmpty();

        public static bool IsNullOrWhiteSpace(this string value)
            => string.IsNullOrWhiteSpace(value);

        public static bool NotNullAndNotWhiteSpace(this string value)
            => !value.IsNullOrWhiteSpace();
    }
}
