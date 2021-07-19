using System;
using System.Diagnostics;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Logging stopwatch implementation.
    /// </summary>
    internal class LoggingStopwatch : IStopwatch
    {
        private readonly Stopwatch _stopwatch = new();

        /// <inheritdoc/>
        public bool IsRunning => _stopwatch.IsRunning;

        /// <inheritdoc/>
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        /// <inheritdoc/>
        public void Start() => _stopwatch.Start();

        /// <inheritdoc/>
        public void Stop() => _stopwatch.Stop();

        /// <inheritdoc/>
        public void Reset() => _stopwatch.Reset();

        /// <inheritdoc/>
        public void Restart() => _stopwatch.Restart();
    }
}