# Crip.AspNetCore.Logging

![issues](https://img.shields.io/github/issues/crip-home/Crip.AspNetCore.Logging?style=for-the-badge&logo=appveyor)
![forks](https://img.shields.io/github/forks/crip-home/Crip.AspNetCore.Logging?style=for-the-badge&logo=appveyor)
![stars](https://img.shields.io/github/stars/crip-home/Crip.AspNetCore.Logging?style=for-the-badge&logo=appveyor)
![license](https://img.shields.io/github/license/crip-home/Crip.AspNetCore.Logging?style=for-the-badge&logo=appveyor)

Make HTTP request logging ease.

## Setup request/response logging in application

Configure log level for all requests:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Crip.AspNetCore.Logging.RequestLoggingMiddleware": "Trace"
    }
  }
}
```

Configure dependency injection in `Startup.ConfigureServices`:

```cs
services.AddRequestLogging();
```

Add logging middleware in `Startup.Configure`:

```cs
app.UseRouting();

// After routing
app.UseRequestLoggingMiddleware();
// And before endpoints

app.UseEndpoints(endpoints => ... );
```

> In case of `net6.0` it is not important where place `app.UseRequestLoggingMiddleware();`. Check
> out [example net6.0 project](./examples/Crip.AspNetCore.Logging.Core60.Example/Program.cs)

And now you are ready to see all request/response in logging output:

```yaml
[ 12:52:31 VRB ] POST http://localhost:5000/api/test/verbose HTTP/1.1
Connection: keep-alive
Content-Type: application/json
Accept: */*
Accept-Encoding: gzip, deflate, br
Host: localhost:5000
Content-Length: 27
{ "body": "content" }
  { Crip.AspNetCore.Logging.RequestLoggingMiddleware.Test.Verbose } { EventName="HttpRequest", Endpoint="http://localhost:5000/api/test/verbose", HttpMethod="POST", RequestId="xxx", RequestPath="/api/test/verbose", SpanId="|xxx.", TraceId="xxx", ParentId="", ConnectionId="xxx" }

  [ 12:52:31 VRB ] HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
{ "level": "Verbose" }
  { Crip.AspNetCore.Logging.RequestLoggingMiddleware.Test.Verbose } { EventName="HttpResponse", StatusCode=200, Elapsed=6, Endpoint="http://localhost:5000/api/test/verbose", HttpMethod="POST", RequestId="xxx", RequestPath="/api/test/verbose", SpanId="|xxx.", TraceId="xxx", ParentId="", ConnectionId="xxx" }

  [ 12:52:31 INF ] POST http://localhost:5000/api/test/verbose at 00:00:00.006 with 200 OK {Crip.AspNetCore.Logging.RequestLoggingMiddleware.Test.Verbose} {EventName="HttpResponse", StatusCode=200, Elapsed=6, Endpoint="http://localhost:5000/api/test/verbose", HttpMethod="POST", RequestId="xxx", RequestPath="/api/test/verbose", SpanId="|xxx.", TraceId="xxx", ParentId="", ConnectionId="xxx"}
```

## Setup HTTP client request/response logging

Configure log level for a service:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Crip.AspNetCore.Logging.LoggingHandler": "Trace"
    }
  }
}
```

Configure dependency injection in `Startup.ConfigureServices`:

```cs
services.AddRequestLogging();
```

Register HTTP client with message handler:

```cs
services
    .AddRequestLoggingHandler()
    .AddHttpClient<MyHttpClient>()
    .AddHttpMessageHandler<LoggingHandler<MyHttpClient>>();

// Or use predefined extension method:
services.AddLoggableHttpClient<MyHttpClient>();
```

## Configuration options

### Change verbosity level to reduce logs

With different verbosity level, different output will be written to logs

- `Trace`
    - Writes log message with incoming request headers and body
    - Writes log message with returned response headers and body
    - Writes basic response timing/status message

   ```yaml
   [12:52:31 VRB] POST http://localhost:5000/api/test/verbose HTTP/1.1
   Connection: keep-alive
   Content-Type: application/json
   Accept: */*
   Accept-Encoding: gzip, deflate, br
   Host: localhost:5000
   Content-Length: 27
   {"body":"content"}
    {Crip.AspNetCore.Logging.RequestLoggingMiddleware.Test.Verbose} {EventName="HttpRequest", Endpoint="http://localhost:5000/api/test/verbose", HttpMethod="POST", RequestId="xxx", RequestPath="/api/test/verbose", SpanId="|xxx.", TraceId="xxx", ParentId="", ConnectionId="xxx"}
   
   [12:52:31 VRB] HTTP/1.1 200 OK
   Content-Type: application/json; charset=utf-8
   {"level":"Verbose"}
    {Crip.AspNetCore.Logging.RequestLoggingMiddleware.Test.Verbose} {EventName="HttpResponse", StatusCode=200, Elapsed=6, Endpoint="http://localhost:5000/api/test/verbose", HttpMethod="POST", RequestId="xxx", RequestPath="/api/test/verbose", SpanId="|xxx.", TraceId="xxx", ParentId="", ConnectionId="xxx"}
   
   [12:52:31 INF] POST http://localhost:5000/api/test/verbose at 00:00:00.006 with 200 OK {Crip.AspNetCore.Logging.RequestLoggingMiddleware.Test.Verbose} {EventName="HttpResponse", StatusCode=200, Elapsed=6, Endpoint="http://localhost:5000/api/test/verbose", HttpMethod="POST", RequestId="xxx", RequestPath="/api/test/verbose", SpanId="|xxx.", TraceId="xxx", ParentId="", ConnectionId="xxx"}
   ```

- `Debug`
    - Writes log message with incoming request headers
    - Writes log message with returned response headers
    - Writes basic response timing/status message

   ```yaml
   [12:55:58 DBG] POST http://localhost:5000/api/test/debug HTTP/1.1
   Connection: keep-alive
   Content-Type: application/json
   Accept: */*
   Accept-Encoding: gzip, deflate, br
   Host: localhost:5000
   Content-Length: 27
   {Crip.AspNetCore.Logging.RequestLoggingMiddleware.Test.Debug} {EventName="HttpRequest", Endpoint="http://localhost:5000/api/test/debug", HttpMethod="POST", RequestId="yyy", RequestPath="/api/test/debug", SpanId="|yyy.", TraceId="yyy", ParentId="", ConnectionId="yyy"}
   
   [12:55:58 DBG] HTTP/1.1 200 OK
   Date: Fri, 26 Aug 2022 09:55:57 GMT
   Transfer-Encoding: chunked
   Content-Type: application/json; charset=utf-8
   {Crip.AspNetCore.Logging.RequestLoggingMiddleware.Test.Debug} {EventName="HttpResponse", StatusCode=200, Elapsed=5, Endpoint="http://localhost:5000/api/test/debug", HttpMethod="POST", RequestId="yyy", RequestPath="/api/test/debug", SpanId="|yyy.", TraceId="yyy", ParentId="", ConnectionId="yyy"}
   
   [12:55:58 INF] POST http://localhost:5000/api/test/debug at 00:00:00.004 with 200 OK {Crip.AspNetCore.Logging.RequestLoggingMiddleware.Test.Debug} {EventName="HttpResponse", StatusCode=200, Elapsed=5, Endpoint="http://localhost:5000/api/test/debug", HttpMethod="POST", RequestId="yyy", RequestPath="/api/test/debug", SpanId="|yyy.", TraceId="yyy", ParentId="", ConnectionId="yyy"}
   ```

- `Information`
    - Writes basic response timing/status message

   ```yaml
   [12:57:24 INF] POST http://localhost:5000/api/test/info at 00:00:00.001 with 200 OK {Crip.AspNetCore.Logging.RequestLoggingMiddleware.Test.Info} {EventName="HttpResponse", StatusCode=200, Elapsed=2, Endpoint="http://localhost:5000/api/test/info", HttpMethod="POST", RequestId="zzz", RequestPath="/api/test/info", SpanId="|zzz.", TraceId="zzz", ParentId="", ConnectionId="zzz"}
   ```

- Any other level will not write logs at all.

### Configure verbosity for controller

Each controller has its own source context. This allows configure specific verbosity for a controller. If controller is
named `OrdersController`, you can set verbosity for it in configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Crip.AspNetCore.Logging.RequestLoggingMiddleware": "None",
      "Crip.AspNetCore.Logging.RequestLoggingMiddleware.Orders": "Trace"
    }
  }
}
```

In this case only `OrdersController` requests/responses will be written to logs.

### Configure verbosity for controller method

Each controller method has its own source context. This allows configure specific verbosity for a controller method. If
controller is named `OrdersController` and you want see requests going to `Index` method, you can set verbosity for it
in configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Crip.AspNetCore.Logging.RequestLoggingMiddleware": "None",
      "Crip.AspNetCore.Logging.RequestLoggingMiddleware.Orders.Index": "Trace"
    }
  }
}
```

In this case only `OrdersController` `Index` method requests/responses will be written to logs.

### Configure verbosity for mapped method

When registering methods like in `net6.0` minimal API, you can provide name, and that name will be used for logging
context:

```cs
app
    .MapGet("/", () => Results.Json(new { status = "OK" }))
    .WithName("SomeRouteName");
```

You can set verbosity for it in configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Crip.AspNetCore.Logging.RequestLoggingMiddleware": "None",
      "Crip.AspNetCore.Logging.RequestLoggingMiddleware.SomeRouteName": "Trace"
    }
  }
}
```

In this case only this method requests/responses will be written to logs.

> If there is no name for request method (for example, unknown controller),
> `Crip.AspNetCore.Logging.RequestLoggingMiddleware` context will be used.

### Configure verbosity for HTTP client

When you register client message handler with type `.AddHttpMessageHandler<LoggingHandler<MyHttpClient>>()`, this value
is used as source context postfix.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Crip.AspNetCore.Logging.LoggingHandler": "None",
      "Crip.AspNetCore.Logging.LoggingHandler.MyHttpClient": "Trace"
    }
  }
}
```

In this case only `MyHttpClient` requests/responses will be written to logs.

### Filter endpoints

If there is some endpoints you would like to exclude from logs, you can configure predicate:

```cs
services.AddRequestLoggingExclude("/images*", "/swagger*")
```

Or if you like to include only API requests in logging:

```cs
services.AddSingleton<IHttpRequestPredicate>(provider =>
    new EndpointPredicate(false, "/api*"));
```

Or create your own `IHttpRequestPredicate` implementation and add it to service collection.

### Filter logged content

By default `AuthorizationHeaderLoggingMiddleware` and `LongJsonContentMiddleware` are added in to logger. You can create
own implementations of the `IHeaderLogMiddleware` or `IRequestContentLogMiddleware` to modify logged content for your
own needs.

#### IHeaderLogMiddleware

`AuthorizationHeaderLoggingMiddleware` implements `IHeaderLogMiddleware` interface and will hide Authorization header
values replacing `Basic` auth header value with `Basic *****` and `Bearer` auth header value with `Bearer *****`.

```yaml
[15:26:52 VRB] GET http://localhost:5000/api/test HTTP/1.1
Connection: keep-alive
Authorization: Bearer *****
Host: localhost:5000
```

You can add `CookieHeaderLoggingMiddleware` to avoid long cookie value write to logs:

```cs
services
    .AddRequestLogging()
    .AddRequestLoggingCookieValueMiddleware();
```

```yaml
[15:34:27 VRB] GET http://localhost:5000/api/test HTTP/1.1
Connection: keep-alive
Accept-Encoding: gzip, deflate, br
Cookie: a=Rtgyhjk456***
Host: localhost:5000
```

#### IRequestContentLogMiddleware

`LongJsonContentMiddleware` implements `IRequestContentLogMiddleware` interface and will hide properties value if its
length exceeds 500 characters and will output only first 10 symbols:

```json
{
  "fileBase64": "SGVsbG8gV2***"
}
```

This middleware runs only if request content type is `application/json`. You can change this middleware values within
configuration file:

```json
{
  "Logging": {
    "Request": {
      "MaxCharCountInField": 1000,
      "LeaveOnTrimCharCountInField": 3
    }
  }
}
```

---

For more technical details take a look
in [example project Startup file](./examples/Crip.AspNetCore.Logging.Core31.Example/Startup.cs).
