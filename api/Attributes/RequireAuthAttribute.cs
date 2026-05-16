using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAuthAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var externalUserId = context.HttpContext.Items["UserId"] as string;

        // SSD fix (information disclosure): reject unauthenticated access with a generic message only
        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required" });
        }
    }
}
