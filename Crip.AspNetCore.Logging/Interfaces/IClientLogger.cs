using System;
using System.Threading.Tasks;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Basic client logger contract.
    /// </summary>
    public interface IClientLogger
    {
        /// <summary>
        /// Write request log.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task LogRequest();

        /// <summary>
        /// Write response log.
        /// </summary>
        /// <param name="stopwatch">Request execution performance watch state.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task LogResponse(IStopwatch stopwatch);

        /// <summary>
        /// Write request basic information log.
        /// </summary>
        /// <param name="stopwatch">Request execution performance watch state.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task LogInfo(IStopwatch stopwatch);

        /// <summary>
        /// Write request execution error log.
        /// </summary>
        /// <param name="exception">Execution error instance.</param>
        /// <param name="stopwatch">Request execution performance watch state.</param>
        void LogError(Exception exception, IStopwatch? stopwatch);
    }
}
