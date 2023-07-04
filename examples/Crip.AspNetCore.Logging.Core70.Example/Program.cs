using Crip.AspNetCore.Logging;
using Crip.AspNetCore.Logging.Core60.Example;
using Crip.AspNetCore.Logging.LongJsonContent;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => configuration
    .WriteTo.Console(outputTemplate: "{Timestamp:o} [{Level:u3}] {Message}{NewLine}{Properties}{NewLine}{Exception}")
    .ReadFrom.Configuration(context.Configuration));

builder.Services
    // Adds all required services for RequestLoggingMiddleware
    .AddRequestLogging(options =>
    {
        // Allows to disable response content logging for all requests.
        options.LogResponse = true;

        // By default only "authorization" header is masked, but this configuration allows
        // to set any header name to mask secure values of authorization.
        options.AuthorizationHeaders.AuthorizationHeaderNames.Add("x-auth");
    })
    // Adds middleware to reduce log size by removing values from "application/json" request
    // where single property takes more than allowed character count.
    // Very useful when API receives/sends blobs in base64 format.
    .AddRequestLoggingLongJson(options =>
    {
        // Maximum allowed characters in a json string property.
        options.MaxCharCountInField = 50;

        // Character count to leave in a field after it is trimmed.
        options.LeaveOnTrimCharCountInField = 10;
    })
    // Hide cookie header values in log messages
    .AddRequestLoggingCookieValueMiddleware()
    // Do not log if request path matches provided pattern.
    .AddRequestLoggingExclude(
        "/favicon.ico",
        "/weatherforecast/pattern-exclude*",
        "/weatherforecast/exact-exclude",
        "/swagger*");

// Register HTTP client and write all request logs
// As an alternative, could be used handler `.AddHttpMessageHandler<LoggingHandler<NamedHttpClient>>()`
// but in such case make sure you register `.AddRequestLoggingHandler()` in DI.
builder.Services.AddLoggableHttpClient<MyTypedClient>(client =>
{
    client.BaseAddress = new Uri("http://postman-echo.com/");
});

builder.Services.AddControllers(options => options.Filters.Add(new HttpResponseExceptionFilter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Register middleware to log requests
app.UseRequestLoggingMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app
    .MapGet("/", () => Results.Json(new { status = "OK" }))
    // Use route name to extend logger scope.
    // In this example, source context will be "Crip.AspNetCore.Logging.RequestLoggingMiddleware.HomeRouteName"
    .WithName("HomeRouteName");

app.Run();