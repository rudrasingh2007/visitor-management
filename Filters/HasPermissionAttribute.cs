using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;
using VisitorManagementSystem.Helpers;

namespace VisitorManagementSystem.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HasPermissionAttribute : TypeFilterAttribute
    {
        public HasPermissionAttribute(string moduleName, string permissionType) : base(typeof(PermissionFilter))
        {
            Arguments = new object[] { moduleName, permissionType };
        }
    }

    public class PermissionFilter : IAsyncActionFilter
    {
        private readonly string _moduleName;
        private readonly string _permissionType;

        public PermissionFilter(string moduleName, string permissionType)
        {
            _moduleName = moduleName;
            _permissionType = permissionType;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var session = httpContext.Session;
            
            var userId = session.GetInt32("UserId");
            var roleName = session.GetString("RoleName");

            // 1. Check if user is logged in
            if (userId == null || string.IsNullOrEmpty(roleName))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // 2. Bypass check for Admin
            if (string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            // 3. Verify permission from session cache
            if (httpContext.HasPermission(_moduleName, _permissionType))
            {
                await next();
                return;
            }

            // 4. Access Denied redirect
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}
