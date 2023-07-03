namespace Crip.AspNetCore.Logging.Core60.Example;

public class HttpResponseException : Exception
{
    public int Status { get; set; } = 500;

    public object? Value { get; set; }
}