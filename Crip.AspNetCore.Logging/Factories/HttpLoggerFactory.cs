using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging
{
    public class HttpLoggerFactory : Factory, IHttpLoggerFactory
    {
        public HttpLoggerFactory(IServiceProvider services)
            : base(services)
        {
        }

        public IHttpLogger Create(ILogger logger)
        {
            return new HttpLogger(
                logger,
                GetService<IRequestLogger>(),
                GetService<IResponseLogger>(),
                GetService<IBasicInfoLogger>(),
                GetService<IEnumerable<IHttpRequestPredicate>>());
        }
    }
}
