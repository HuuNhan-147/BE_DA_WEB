using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace BE_DA_WEB.Filters
{
    public class AdminOnlyAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Kiểm tra claim "isAdmin" (giả sử bạn lưu thông tin này trong token)
            var isAdmin = user.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value;
            if (isAdmin != "True" && isAdmin != "true" && isAdmin != "1")
            {
                context.Result = new ForbidResult();
            }
        }
    }
}