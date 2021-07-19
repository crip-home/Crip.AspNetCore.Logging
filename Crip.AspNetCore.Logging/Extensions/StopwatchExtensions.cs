namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Stopwatch extension methods.
    /// </summary>
    internal static class StopwatchExtensions
    {
        /// <summary>
        /// Get user friendly time representation from stopwatch.
        /// </summary>
        /// <param name="stopwatch">The stopwatch state.</param>
        /// <returns>User friendly time representation.</returns>
        public static string Time(this IStopwatch stopwatch) =>
            stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff");
    }
}
