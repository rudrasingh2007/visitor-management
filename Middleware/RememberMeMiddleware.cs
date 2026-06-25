using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Helpers;

namespace VisitorManagementSystem.Middleware
{
    public class RememberMeMiddleware
    {
        private readonly RequestDelegate _next;

        public RememberMeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, IDataProtectionProvider dataProtectionProvider)
        {
            var session = context.Session;
            
            // If user is not authenticated in the current session
            if (session.GetInt32("UserId") == null)
            {
                // Check if the Remember Me cookie exists
                if (context.Request.Cookies.TryGetValue("RememberMeToken", out string encryptedToken))
                {
                    try
                    {
                        var protector = dataProtectionProvider.CreateProtector("RememberMe");
                        var username = protector.Unprotect(encryptedToken);

                        var user = await dbContext.UserMasters
                            .Include(u => u.Role)
                            .FirstOrDefaultAsync(u => u.Username == username && u.Status == "Active");

                        if (user != null)
                        {
                            // Re-hydrate session
                            session.SetInt32("UserId", user.UserId);
                            session.SetInt32("RoleId", user.RoleId);
                            session.SetString("Username", user.Username);
                            session.SetString("FullName", user.FullName);
                            session.SetString("RoleName", user.Role?.RoleName ?? "No Role");
                            session.SetString("Email", user.Email);
                            if (!string.IsNullOrEmpty(user.PhotoPath))
                            {
                                session.SetString("UserPhoto", user.PhotoPath);
                            }

                            if (user.EmployeeId.HasValue)
                            {
                                session.SetInt32("EmployeeId", user.EmployeeId.Value);
                            }

                            var rights = await dbContext.RoleRights
                                .Include(r => r.Module)
                                .Where(r => r.RoleId == user.RoleId)
                                .Select(r => new UserPermissionDto
                                {
                                    ModuleName = r.Module != null ? r.Module.ModuleName : string.Empty,
                                    CanView = r.CanView,
                                    CanAdd = r.CanAdd,
                                    CanEdit = r.CanEdit,
                                    CanDelete = r.CanDelete
                                }).ToListAsync();

                            var rightsJson = JsonSerializer.Serialize(rights);
                            session.SetString("UserRights", rightsJson);
                        }
                    }
                    catch
                    {
                        // Invalid or tampered token, clear it
                        context.Response.Cookies.Delete("RememberMeToken");
                    }
                }
            }

            await _next(context);
        }
    }
}
