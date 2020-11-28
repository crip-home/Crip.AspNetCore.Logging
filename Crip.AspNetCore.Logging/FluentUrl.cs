using System.Linq;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Fluent URL builder.
    /// </summary>
    internal class FluentUrl
    {
        private readonly string _url;

        private FluentUrl(string url)
        {
            _url = url;
        }

        /// <summary>
        /// Implicit conversion from FluentUrl to String.
        /// </summary>
        /// <param name="url">The FluentUrl object.</param>
        /// <returns>The String url version.</returns>
        public static implicit operator string(FluentUrl url) => url.ToString();

        /// <summary>
        /// Implicit conversion from String to FluentUrl.
        /// </summary>
        /// <param name="url">The string representation of the URL.</param>
        /// <returns>The FluentUrl object.</returns>
        public static implicit operator FluentUrl(string url) => new FluentUrl(url);

        /// <summary>
        /// Combines url parts adding/removing "/" character.
        /// </summary>
        /// <param name="parts">The parts to be combined.</param>
        /// <returns>Parts combined in to single url.</returns>
        public static string Combine(params string[] parts) =>
            parts.Aggregate(string.Empty, Combine);

        /// <inheritdoc />
        public override string ToString()
        {
            return _url;
        }

        private static string Combine(string path, string part)
        {
            if (part.IsNullOrWhiteSpace())
            {
                return path;
            }

            if (StartQueryString(path, part))
            {
                return Combine(path, part, '?');
            }

            if (StartFragment(path, part))
            {
                return Combine(path, part, '#');
            }

            if (path.OrdinalContains("#"))
            {
                return path + part;
            }

            if (path.OrdinalContains("?"))
            {
                return Combine(path, part, '&');
            }

            return Combine(path, part, '/');
        }

        private static string Combine(string a, string b, char separator)
        {
            if (a.IsNullOrWhiteSpace())
            {
                return b;
            }

            if (b.IsNullOrWhiteSpace())
            {
                return a;
            }

            return a.TrimEnd(separator) + separator + b.TrimStart(separator);
        }

        private static bool StartFragment(string path, string part)
        {
            return path.OrdinalEndsWith("#") || part.OrdinalStartsWith("#");
        }

        private static bool StartQueryString(string path, string part)
        {
            return path.OrdinalEndsWith("?") || part.OrdinalStartsWith("?");
        }
    }
}
