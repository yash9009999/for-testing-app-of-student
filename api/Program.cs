using System.Threading.RateLimiting;
using api.Infrastructure.EntityFramework;
using api.Infrastructure.Http;
using api.Infrastructure.SuccessfulPaymentService;
using api.Infrastructure.SystemDateTimeProvider;
using api.Infrastructure.JwtAuth;
using api.Services;
using api.Services.Provided;
using api.Services.Required;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// SSD: SQLite path from configuration — avoids hardcoding secrets/paths in source (swap via environment in production).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=scoops2go.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IPaymentGateway, SuccessfulPaymentService>();
builder.Services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddHttpClient("IdentityIssuerJwks", client =>
{
    var authority = builder.Configuration["Auth:Authority"] ?? "http://localhost:5001";
    if (!authority.EndsWith("/")) authority += "/";
    client.BaseAddress = new Uri(authority);
});
builder.Services.AddScoped<IAuthenticationService, JwtAuthenticationService>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();

// SSD: coarse per-IP throttling — complements edge/WAF rate limits; tune PermitLimit for real traffic studies.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 300,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});

// SSD: dev SPA origin — production should narrow headers/methods and list real front-end origins only.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowViteApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .WithHeaders("Content-Type", "Authorization", "X-Guest-Token")
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// SSD: global handler — non-development responses omit exception detail to reduce stack trace / path disclosure.
app.UseApiGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // SSD: EnsureCreated is dev-friendly only — production deployments should apply EF migrations (schema versioning).
    context.Database.EnsureCreated();
    SqliteSchemaPatcher.EnsureOrderGuestAccessTokenColumn(context);
    SeedData.Initialize(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowViteApp");

app.UseRateLimiter();

app.UseMiddleware<JwtAuthMiddleware>();

app.UseHttpsRedirection();

app.UseMiddleware<ProductCatalogHttpMethodMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
