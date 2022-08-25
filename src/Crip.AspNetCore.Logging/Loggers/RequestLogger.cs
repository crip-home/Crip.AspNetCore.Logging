using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// HTTP request logger implementation.
/// </summary>
public class RequestLogger : ContentLogger, IRequestLogger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequestLogger"/> class.
    /// </summary>
    /// <param name="contentFactory">Request content value factory.</param>
    /// <param name="headerFactory">Request header value factory.</param>
    public RequestLogger(
        LogContentFactory contentFactory,
        LogHeaderFactory headerFactory)
        : base(headerFactory, contentFactory)
    {
    }

    /// <inheritdoc cref="IRequestLogger"/>
    public async Task LogRequest(ILogger logger, RequestDetails request)
    {
        var level = logger.GetLogLevel();
        if (level > LogLevel.Debug)
        {
            return;
        }

        StringBuilder text = RequestHead(request);

        if (level <= LogLevel.Trace)
        {
            // Write whole request body only when Trace log level is enabled.
            text.AppendLine(await ReadBody(request));
        }

        logger.Log(level, text.ToString());
    }

    private StringBuilder RequestHead(RequestDetails request)
    {
        var text = $"{request.Method} {request.Url} {request.Protocol}{NewLine}";

        StringBuilder builder = new(text);
        AppendHeaders(builder, request.Headers);

        return builder;
    }

    private Task<string> ReadBody(RequestDetails request)
    {
        return ReadContent(request.ContentType, request.Content);
    }
}