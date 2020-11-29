using System;
using Microsoft.Extensions.DependencyInjection;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Time measurement service.
    /// </summary>
    public class TimeMeasurable : IMeasurable
    {
        private readonly IServiceProvider? _services;
        private IStopwatch? _stopwatch = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeMeasurable"/> class.
        /// </summary>
        /// <param name="services">The DI service provider.</param>
        public TimeMeasurable(IServiceProvider services)
        {
            _services = services;
        }

        /// <inheritdoc />
        public IMeasurable StartMeasure()
        {
            _stopwatch = _services.GetService<IStopwatch>();
            _stopwatch.Start();

            return this;
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