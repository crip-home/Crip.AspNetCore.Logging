using System;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Logging extension methods methods.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Return enabled loglevel for logger.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <returns>Loglevel value.</returns>
        public static LogLevel GetLogLevel(this ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            var level =
                logger.IsEnabled(LogLevel.Trace) ? LogLevel.Trace :
                logger.IsEnabled(LogLevel.Debug) ? LogLevel.Debug :
                logger.IsEnabled(LogLevel.Information) ? LogLevel.Information :
                logger.IsEnabled(LogLevel.Warning) ? LogLevel.Warning :
                logger.IsEnabled(LogLevel.Error) ? LogLevel.Error :
                logger.IsEnabled(LogLevel.Critical) ? LogLevel.Critical :
                LogLevel.None;

            return level;
        }
    }
}
