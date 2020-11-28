using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// HTTP context logger factory.
    /// </summary>
    public class ContextLoggerFactory : Factory, IContextLoggerFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContextLoggerFactory"/> class.
        /// </summary>
        /// <param name="services">The service provider instance.</param>
        public ContextLoggerFactory(IServiceProvider services)
            : base(services)
        {
        }

        /// <inheritdoc cref="IContextLoggerFactory"/>
        public IContextLogger Create<T>(HttpContext context)
        {
            return new ContextLogger<T>(
                GetService<ILoggerFactory>(),
                GetService<IHttpLoggerFactory>(),
                context);
        }
    }
}
