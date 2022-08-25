using System;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// System stopwatch wrapper contract. Is created to implement ease library unit-testing.
/// </summary>
public interface IStopwatch
{
    /// <summary>
    /// Gets a value indicating whether stopwatch instance is running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets stopwatch elapsed time.
    /// </summary>
    TimeSpan Elapsed { get; }

    /// <summary>
    /// Start timer.
    /// </summary>
    void Start();

    /// <summary>
    /// Stop timer.
    /// </summary>
    void Stop();

    /// <summary>
    /// Restart timer.
    /// </summary>
    void Restart();

    /// <summary>
    /// Reset timer to zero.
    /// </summary>
    void Reset();
}