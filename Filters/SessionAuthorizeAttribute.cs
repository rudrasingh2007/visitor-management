using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace VisitorManagementSystem.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
                                 .Any(em => em.GetType() == typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute));

            if (hasAllowAnonymous) return;

            var session = context.HttpContext.Session;
            var userId = session.GetInt32("UserId");
            var username = session.GetString("Username");

            if (userId == null || string.IsNullOrEmpty(username))
            {
                // User is not logged in, redirect to Login action of AccountController
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }
}
