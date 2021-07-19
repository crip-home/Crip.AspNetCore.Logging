using System;
using Microsoft.Extensions.DependencyInjection;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Time measurement service.
    /// </summary>
    public class TimeMeasurable : IMeasurable
    {
        private readonly IServiceProvider _services;
        private readonly IStopwatch? _stopwatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeMeasurable"/> class.
        /// </summary>
        /// <param name="services">The DI service provider.</param>
        public TimeMeasurable(IServiceProvider services)
        {
            _services = services;
        }

        private TimeMeasurable(IServiceProvider services, IStopwatch stopwatch)
        {
            _services = services;
            _stopwatch = stopwatch;
            _stopwatch.Start();
        }

        /// <inheritdoc />
        public IMeasurable StartMeasure()
        {
            var stopwatch = _services.GetRequiredService<IStopwatch>();
            return new TimeMeasurable(_services, stopwatch);
        }

        /// <inheritdoc />
        public IStopwatch StopMeasure()
        {
            if (_stopwatch is null)
            {
                throw new ArgumentNullException(
                    nameof(_stopwatch),
                    "Could not stop not started time measurement.");
            }

            _stopwatch.Stop();

            return _stopwatch;
        }
    }
}