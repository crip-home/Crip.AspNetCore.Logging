using System;

namespace Crip.AspNetCore.Logging.Example.Web
{
    public class HttpResponseException : Exception
    {
        public int Status { get; set; } = 500;

        public object Value { get; set; }
    }
}