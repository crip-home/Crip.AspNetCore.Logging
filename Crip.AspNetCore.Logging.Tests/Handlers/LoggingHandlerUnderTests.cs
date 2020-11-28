using System;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging.Tests.Handlers
{

    public class Foo
    {
    }

    public class LoggingHandlerUnderTests : LoggingHandler<Foo>
    {
        public LoggingHandlerUnderTests(ILoggerFactory loggerFactory, IHttpLoggerFactory httpLoggerFactory)
            : base(loggerFactory, httpLoggerFactory)
        {
        }

        protected override IStopwatch CreateStopwatch()
        {
            return new MockStopwatch(
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(100));
        }

    }
}