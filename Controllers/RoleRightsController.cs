using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Filters;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Controllers
{
    [SessionAuthorize]
    [HasPermission("Role Master", "View")] // Restrict to users who can manage roles
    public class RoleRightsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoleRightsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // View Rights Management Grid
        [HttpGet]
        public async Task<IActionResult> Index(int? roleId)
        {
            var roles = await _context.RoleMasters
                .OrderBy(r => r.RoleName)
                .ToListAsync();

            ViewBag.Roles = roles;
            ViewBag.SelectedRoleId = roleId;
            return View();
        }

        // AJAX Get permissions for a role
        [HttpGet]
        public async Task<IActionResult> GetRights(int roleId)
        {
            var modules = await _context.ModuleMasters.OrderBy(m => m.ModuleId).ToListAsync();
            var existingRights = await _context.RoleRights
                .Where(r => r.RoleId == roleId)
                .ToListAsync();

            var result = modules.Select(m => {
                var right = existingRights.FirstOrDefault(r => r.ModuleId == m.ModuleId);
                return new
                {
                    ModuleId = m.ModuleId,
                    ModuleName = m.ModuleName,
                    CanView = right?.CanView ?? false,
                    CanAdd = right?.CanAdd ?? false,
                    CanEdit = right?.CanEdit ?? false,
                    CanDelete = right?.CanDelete ?? false
                };
            }).ToList();

            return Json(result);
        }

        // POST Save/Update permissions for a role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRights(int roleId, List<RoleRightSaveModel> rights)
        {
            if (roleId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid Role selected.";
                return RedirectToAction(nameof(Index));
            }

            var role = await _context.RoleMasters.FindAsync(roleId);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            // Admin rights cannot be modified since Admin bypasses checks
            if (string.Equals(role.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Administrator permissions are fixed and cannot be modified.";
                return RedirectToAction(nameof(Index));
            }

            var dbRights = await _context.RoleRights.Where(r => r.RoleId == roleId).ToListAsync();

            foreach (var item in rights)
            {
                var dbRight = dbRights.FirstOrDefault(r => r.ModuleId == item.ModuleId);
                if (dbRight != null)
                {
                    dbRight.CanView = item.CanView;
                    dbRight.CanAdd = item.CanAdd;
                    dbRight.CanEdit = item.CanEdit;
                    dbRight.CanDelete = item.CanDelete;
                }
                else
                {
                    var newRight = new RoleRight
                    {
                        RoleId = roleId,
                        ModuleId = item.ModuleId,
                        CanView = item.CanView,
                        CanAdd = item.CanAdd,
                        CanEdit = item.CanEdit,
                        CanDelete = item.CanDelete
                    };
                    _context.RoleRights.Add(newRight);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Role rights for '{role.RoleName}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }

    public class RoleRightSaveModel
    {
        public int ModuleId { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
