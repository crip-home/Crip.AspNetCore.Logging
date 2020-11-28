using System;
using System.Threading.Tasks;

namespace Crip.AspNetCore.Logging
{
    public interface IHttpLogger
    {
        Task LogRequest(RequestDetails request);

        Task LogResponse(RequestDetails request, ResponseDetails response);

        Task LogInfo(RequestDetails request, ResponseDetails response);

        void LogError(Exception exception, RequestDetails? request, ResponseDetails? response);
    }
}
