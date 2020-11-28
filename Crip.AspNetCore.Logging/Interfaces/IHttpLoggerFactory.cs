using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging
{
    public interface IHttpLoggerFactory
    {
        IHttpLogger Create(ILogger logger);
    }
}
