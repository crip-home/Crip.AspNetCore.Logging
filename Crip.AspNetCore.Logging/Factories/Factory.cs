using System;
using Microsoft.Extensions.DependencyInjection;

namespace Crip.AspNetCore.Logging
{
    public abstract class Factory
    {
        private readonly IServiceProvider _services;

        protected Factory(IServiceProvider services)
        {
            _services = services;
        }

        protected T GetService<T>()
        {
            return _services.GetService<T>();
        }
    }
}
