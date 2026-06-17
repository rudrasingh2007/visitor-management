using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Filters;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Controllers
{
    [SessionAuthorize]
    [HasPermission("Role Master", "View")]
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Role
        public async Task<IActionResult> Index()
        {
            var roles = await _context.RoleMasters.Include(r => r.Users).ToListAsync();
            return View(roles);
        }

        // GET: Role/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleMaster role)
        {
            if (ModelState.IsValid)
            {
                // Prevent Duplicate Role Names
                var existingRole = await _context.RoleMasters
                    .AnyAsync(r => r.RoleName.Trim().ToLower() == role.RoleName.Trim().ToLower());
                
                if (existingRole)
                {
                    ModelState.AddModelError("RoleName", "Role Name already exists. Please choose a unique name.");
                    return View(role);
                }

                role.CreatedDate = DateTime.UtcNow;
                _context.Add(role);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Role '{role.RoleName}' created successfully! Please configure its permissions below.";
                return RedirectToAction("Index", "RoleRights", new { roleId = role.RoleId });
            }
            return View(role);
        }

        // GET: Role/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var role = await _context.RoleMasters.FindAsync(id);
            if (role == null) return NotFound();

            return View(role);
        }

        // POST: Role/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleMaster role)
        {
            if (id != role.RoleId) return NotFound();

            if (ModelState.IsValid)
            {
                // Prevent Duplicate Role Names (excluding self)
                var existingRole = await _context.RoleMasters
                    .AnyAsync(r => r.RoleName.Trim().ToLower() == role.RoleName.Trim().ToLower() && r.RoleId != id);

                if (existingRole)
                {
                    ModelState.AddModelError("RoleName", "Role Name already exists. Please choose a unique name.");
                    return View(role);
                }

                try
                {
                    var dbRole = await _context.RoleMasters.FindAsync(id);
                    if (dbRole == null) return NotFound();

                    dbRole.RoleName = role.RoleName;
                    dbRole.Description = role.Description;
                    dbRole.Status = role.Status;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Role updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoleExists(role.RoleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(role);
        }

        // POST: Role/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _context.RoleMasters.FindAsync(id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            // Do not allow deletion of roles that are assigned to users
            var hasUsers = await _context.UserMasters.AnyAsync(u => u.RoleId == id);
            if (hasUsers)
            {
                TempData["ErrorMessage"] = $"Cannot delete role '{role.RoleName}' because it is assigned to existing users.";
                return RedirectToAction(nameof(Index));
            }

            _context.RoleMasters.Remove(role);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Role deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Role/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var role = await _context.RoleMasters.FindAsync(id);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            role.Status = (role.Status == "Active") ? "Inactive" : "Active";
            _context.Update(role);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Role '{role.RoleName}' status updated to '{role.Status}' successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool RoleExists(int id)
        {
            return _context.RoleMasters.Any(e => e.RoleId == id);
        }
    }
}
