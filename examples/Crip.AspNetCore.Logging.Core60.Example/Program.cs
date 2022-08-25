using Crip.AspNetCore.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) => configuration
    .WriteTo.Console(outputTemplate: "{Timestamp:o} [{Level:u3}] {Message}{NewLine}{Properties}{NewLine}{Exception}")
    .ReadFrom.Configuration(context.Configuration));

builder.Services.AddRequestLogging();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestLoggingMiddleware();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app
    .MapGet("/", () => Results.Json(new { status = "OK" }))
    .WithName("HomeRouteName");

app.Run();