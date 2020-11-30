using System;
using System.Collections.Generic;
using System.Linq;

namespace Crip.AspNetCore.Logging.Tests
{
    /// <summary>
    /// IStopwatch interface implementation with fixed value
    /// <see href="https://codereview.stackexchange.com/questions/180217/stopwatch-dependency-for-unit-testing#answer-180265" />
    /// </summary>
    public class MockStopwatch : IStopwatch
    {
        public MockStopwatch(params TimeSpan[] intervals)
            : this(intervals.AsEnumerable())
        {
        }

        public MockStopwatch(IEnumerable<TimeSpan> intervals)
        {
            Interval = intervals
                .ToList()
                .GetEnumerator();

            Reset();
        }

        public IEnumerator<TimeSpan> Interval { get; }

        public bool IsRunning { get; private set; }

        public TimeSpan Elapsed => IsRunning ? NextElapsed : SameElapsed;

        public TimeSpan SameElapsed => Interval.Current;

        public TimeSpan NextElapsed => Interval.MoveNext()
            ? Interval.Current
            : throw new InvalidOperationException("You did not define enough timestamps.");

        public void Start() => IsRunning = true;

        public void Stop() => IsRunning = false;

        public void Restart()
        {
            Stop();
            Reset();
            Start();
        }

        public void Reset()
        {
            Interval.Reset();
            Interval.MoveNext();
        }
    }
}
