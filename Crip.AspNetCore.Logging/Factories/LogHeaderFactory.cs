using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Header logging factory.
    /// </summary>
    public class LogHeaderFactory
    {
        private readonly IEnumerable<IHeaderLogMiddleware> _middlewares;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogHeaderFactory"/> class.
        /// </summary>
        /// <param name="middlewares">Header logging middlewares.</param>
        public LogHeaderFactory(IEnumerable<IHeaderLogMiddleware>? middlewares)
        {
            _middlewares = middlewares ?? Enumerable.Empty<IHeaderLogMiddleware>();
        }

        /// <summary>
        /// Prepare header value for logging.
        /// </summary>
        /// <param name="header">Request header.</param>
        /// <returns>Logging ready value.</returns>
        public string PrepareHeader(KeyValuePair<string, StringValues> header)
        {
            var (key, values) = header;

            return _middlewares.Aggregate(
                values.ToString(),
                (current, middleware) => middleware.Modify(key, current));
        }
    }
}
