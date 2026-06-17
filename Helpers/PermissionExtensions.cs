using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace VisitorManagementSystem.Helpers
{
    public class UserPermissionDto
    {
        public string ModuleName { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public static class PermissionExtensions
    {
        public static bool HasPermission(this HttpContext context, string moduleName, string permissionType)
        {
            if (context == null) return false;

            var roleName = context.Session.GetString("RoleName");
            if (string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return true; // Admin has full access to everything
            }

            var rightsJson = context.Session.GetString("UserRights");
            if (string.IsNullOrEmpty(rightsJson))
            {
                return false;
            }

            try
            {
                var rights = JsonSerializer.Deserialize<List<UserPermissionDto>>(rightsJson);
                if (rights == null) return false;

                var moduleRight = rights.FirstOrDefault(r => string.Equals(r.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase));
                if (moduleRight == null) return false;

                return permissionType.ToLower() switch
                {
                    "view" => moduleRight.CanView,
                    "add" => moduleRight.CanAdd,
                    "edit" => moduleRight.CanEdit,
                    "delete" => moduleRight.CanDelete,
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }
    }
}
