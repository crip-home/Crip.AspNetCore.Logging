using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Logging content factory to resolve body content.
    /// </summary>
    public class LogContentFactory
    {
        private const StringComparison Comparison = StringComparison.InvariantCultureIgnoreCase;
        private readonly IEnumerable<IRequestContentLogMiddleware> _middlewares;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogContentFactory"/> class.
        /// </summary>
        /// <param name="middlewares">The middlewares.</param>
        public LogContentFactory(IEnumerable<IRequestContentLogMiddleware>? middlewares)
        {
            _middlewares = middlewares ?? Enumerable.Empty<IRequestContentLogMiddleware>();
        }

        /// <summary>
        /// Prepares the body for logging.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="content">The content.</param>
        /// <returns>Prepared body.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="content"/> not provided.
        /// </exception>
        public async Task<string> PrepareBody(string? contentType, Stream content)
        {
            var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);

            var middlewares = _middlewares
                .Where(x => (contentType?.IndexOf(x.ContentType, Comparison) ?? -1) >= 0)
                .ToList();

            foreach (var middleware in middlewares)
            {
                var outputStream = new MemoryStream();
                middleware.Modify(ms, outputStream);
                ms = outputStream;
                ms.Seek(0, SeekOrigin.Begin);
            }

            string text = string.Empty;
            if (ms.CanRead)
            {
                using var sr = new StreamReader(ms);
                text = await sr.ReadToEndAsync();
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                return new StringBuilder()
                    .AppendLine()
                    .Append(text)
                    .ToString();
            }

            return string.Empty;
        }
    }
}