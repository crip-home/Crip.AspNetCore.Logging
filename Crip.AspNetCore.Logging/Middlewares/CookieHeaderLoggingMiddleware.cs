using System;
using System.Collections.Generic;
using System.Linq;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Set-Cookie/Cookie header value middleware.
    /// </summary>
    /// <remarks>
    /// Replaces Set-Cookie values with asterisk if value is longer than 50 characters.
    /// </remarks>
    public class CookieHeaderLoggingMiddleware : IHeaderLogMiddleware
    {
        private const StringComparison Comparison = StringComparison.InvariantCultureIgnoreCase;
        private const int CookieValueMaxLength = 50;
        private const int CookieValueTrimLength = 10;

        /// <inheritdoc/>
        public string Modify(string key, string value)
        {
            if (IsNotCookieHeader(key) ||
                string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var parsedValues = value
                .Split(';')
                .Aggregate(new List<string>(), ParseValue);

            return string.Join(';', parsedValues);
        }

        private static bool IsNotCookieHeader(string key)
        {
            return !key.Equals("Set-Cookie", Comparison) &&
                   !key.Equals("Cookie", Comparison);
        }

        private static List<string> ParseValue(List<string> target, string property)
        {
            var parsed = ParseProp(property);
            target.Add(parsed);

            return target;
        }

        private static string ParseProp(string cookiePart)
        {
            if (!cookiePart.Contains('='))
            {
                return cookiePart;
            }

            var (key, value) = SplitKeyAndValue(cookiePart);
            return $"{key}={Trim(value)}";
        }

        private static (string Key, string Value) SplitKeyAndValue(string prop)
        {
            var parts = prop.Split('=', 2);

            return (parts[0], parts[1]);
        }

        private static string Trim(string value)
        {
            if (value.Length <= CookieValueMaxLength)
            {
                return value;
            }

            var length = CookieValueTrimLength;
            var shortValue = value.Substring(0, length);

            return $"{shortValue}***";
        }
    }
}
