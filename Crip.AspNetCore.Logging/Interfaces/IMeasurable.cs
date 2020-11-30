namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Time measurement service contract.
    /// </summary>
    public interface IMeasurable
    {
        /// <summary>
        /// Start measure time.
        /// </summary>
        /// <returns>Started time measurement service.</returns>
        IMeasurable StartMeasure();

        /// <summary>
        /// Stop time measure and get measurement result.
        /// </summary>
        /// <returns>Stopped time measurement state.</returns>
        IStopwatch StopMeasure();
    }
}