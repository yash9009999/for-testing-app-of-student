using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace api.Infrastructure.Http;

/// <summary>
/// SSD: prevents raw exception/stack traces from reaching API clients in non-development environments.
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    public static void UseApiGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.ContentType = "application/json";
                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var ex = feature?.Error;

                if (app.Environment.IsDevelopment())
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        message = "An error occurred.",
                        detail = ex?.Message,
                        stack = ex?.StackTrace
                    }));
                    return;
                }

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    message = "An error occurred."
                }));
            });
        });
    }
}
