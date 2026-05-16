using Microsoft.AspNetCore.Http;

namespace api.Infrastructure.Http;

/// <summary>
/// SSD fix: enforce allowed HTTP methods on the read-only product catalog before MVC authorization runs,
/// so POST/DELETE receive 405 Method Not Allowed instead of 401 from [Authorize].
/// </summary>
public sealed class ProductCatalogHttpMethodMiddleware
{
    private readonly RequestDelegate _next;

    public ProductCatalogHttpMethodMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (!path.StartsWithSegments("/api/Product", StringComparison.OrdinalIgnoreCase))
            return _next(context);

        var method = context.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
            return _next(context);

        context.Response.Headers.Allow = "GET, HEAD, OPTIONS";
        context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
        return Task.CompletedTask;
    }
}
