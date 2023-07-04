using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Crip.AspNetCore.Logging.Core60.Example;

public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
{
    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is HttpResponseException exception)
        {
            context.Result = new ObjectResult(exception.Value)
            {
                StatusCode = exception.Status,
            };
            context.ExceptionHandled = true;
        }
    }
}