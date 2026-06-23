using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Filters;
using VisitorManagementSystem.Models;
using VisitorManagementSystem.ViewModels;

namespace VisitorManagementSystem.Controllers
{
    [SessionAuthorize]
    [HasPermission("User Master", "View")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await _context.UserMasters
                .Include(u => u.Role)
                .Include(u => u.Employee)
                .ToListAsync();
            
            // For custom dropdown filters in User Index
            var roles = await _context.RoleMasters.Where(r => r.Status == "Active").ToListAsync();
            ViewBag.RolesList = new SelectList(roles, "RoleName", "RoleName");

            return View(users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.UserMasters
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            
            if (user == null) return NotFound();

            return View(user);
        }

        // GET: User/Create
        public async Task<IActionResult> Create()
        {
            var activeRoles = await _context.RoleMasters
                .Where(r => r.Status == "Active")
                .ToListAsync();
            
            var employeeRole = activeRoles.FirstOrDefault(r => r.RoleName == "Employee");
            ViewBag.EmployeeRoleId = employeeRole?.RoleId;

            // Load active, unassigned employees
            var assignedEmployeeIds = await _context.UserMasters
                .Where(u => u.EmployeeId != null)
                .Select(u => u.EmployeeId)
                .ToListAsync();

            var activeUnassignedEmployees = await _context.EmployeeMasters
                .Where(e => e.Status == "Active" && !assignedEmployeeIds.Contains(e.EmployeeId))
                .Select(e => new {
                    e.EmployeeId,
                    FullName = e.FirstName + " " + (e.LastName ?? "")
                })
                .ToListAsync();

            ViewBag.RolesList = new SelectList(activeRoles, "RoleId", "RoleName");
            ViewBag.EmployeesList = new SelectList(activeUnassignedEmployees, "EmployeeId", "FullName");

            return View(new UserViewModel());
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required.");
            }

            var employeeRole = await _context.RoleMasters.FirstOrDefaultAsync(r => r.RoleName == "Employee");
            if (employeeRole != null)
            {
                if (model.RoleId == employeeRole.RoleId)
                {
                    if (model.EmployeeId == null || model.EmployeeId == 0)
                    {
                        ModelState.AddModelError("EmployeeId", "Employee selection is required for Employee role.");
                    }
                    else
                    {
                        var employeeExistsAndActive = await _context.EmployeeMasters
                            .AnyAsync(e => e.EmployeeId == model.EmployeeId && e.Status == "Active");
                        if (!employeeExistsAndActive)
                        {
                            ModelState.AddModelError("EmployeeId", "Selected employee is inactive or does not exist.");
                        }

                        var alreadyAssigned = await _context.UserMasters
                            .AnyAsync(u => u.EmployeeId == model.EmployeeId);
                        if (alreadyAssigned)
                        {
                            ModelState.AddModelError("EmployeeId", "This employee is already linked to another user account.");
                        }
                    }
                }
                else
                {
                    model.EmployeeId = null;
                }
            }

            if (ModelState.IsValid)
            {
                // Unique Username validation
                var usernameExists = await _context.UserMasters
                    .AnyAsync(u => u.Username.Trim().ToLower() == model.Username.Trim().ToLower());
                if (usernameExists)
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                }

                // Unique Email validation
                var emailExists = await _context.UserMasters
                    .AnyAsync(u => u.Email.Trim().ToLower() == model.Email.Trim().ToLower());
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email address is already registered.");
                }

                if (ModelState.IsValid)
                {
                    var user = new UserMaster
                    {
                        Username = model.Username,
                        FullName = model.FullName,
                        Email = model.Email,
                        MobileNumber = model.MobileNumber,
                        Password = model.Password ?? string.Empty,
                        RoleId = model.RoleId,
                        Status = model.Status,
                        EmployeeId = model.EmployeeId,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "User registered successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }

            var activeRoles = await _context.RoleMasters
                .Where(r => r.Status == "Active")
                .ToListAsync();
            ViewBag.EmployeeRoleId = employeeRole?.RoleId;

            var assignedEmployeeIds = await _context.UserMasters
                .Where(u => u.EmployeeId != null)
                .Select(u => u.EmployeeId)
                .ToListAsync();

            var activeUnassignedEmployees = await _context.EmployeeMasters
                .Where(e => e.Status == "Active" && !assignedEmployeeIds.Contains(e.EmployeeId))
                .Select(e => new {
                    e.EmployeeId,
                    FullName = e.FirstName + " " + (e.LastName ?? "")
                })
                .ToListAsync();

            ViewBag.RolesList = new SelectList(activeRoles, "RoleId", "RoleName", model.RoleId);
            ViewBag.EmployeesList = new SelectList(activeUnassignedEmployees, "EmployeeId", "FullName", model.EmployeeId);
            return View(model);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.UserMasters.FindAsync(id);
            if (user == null) return NotFound();

            var activeRoles = await _context.RoleMasters
                .Where(r => r.Status == "Active" || r.RoleId == user.RoleId)
                .ToListAsync();

            var employeeRole = activeRoles.FirstOrDefault(r => r.RoleName == "Employee");
            ViewBag.EmployeeRoleId = employeeRole?.RoleId;

            // Load active, unassigned employees (including current employee assigned to this user)
            var assignedEmployeeIds = await _context.UserMasters
                .Where(u => u.EmployeeId != null && u.UserId != id)
                .Select(u => u.EmployeeId)
                .ToListAsync();

            var activeUnassignedEmployees = await _context.EmployeeMasters
                .Where(e => e.Status == "Active" && (!assignedEmployeeIds.Contains(e.EmployeeId) || e.EmployeeId == user.EmployeeId))
                .Select(e => new {
                    e.EmployeeId,
                    FullName = e.FirstName + " " + (e.LastName ?? "")
                })
                .ToListAsync();

            ViewBag.RolesList = new SelectList(activeRoles, "RoleId", "RoleName", user.RoleId);
            ViewBag.EmployeesList = new SelectList(activeUnassignedEmployees, "EmployeeId", "FullName", user.EmployeeId);

            var model = new UserViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                Password = user.Password, // Preserve password
                RoleId = user.RoleId,
                Status = user.Status,
                EmployeeId = user.EmployeeId,
                CreatedDate = user.CreatedDate
            };

            return View(model);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserViewModel model)
        {
            if (id != model.UserId) return NotFound();

            var employeeRole = await _context.RoleMasters.FirstOrDefaultAsync(r => r.RoleName == "Employee");
            if (employeeRole != null)
            {
                if (model.RoleId == employeeRole.RoleId)
                {
                    if (model.EmployeeId == null || model.EmployeeId == 0)
                    {
                        ModelState.AddModelError("EmployeeId", "Employee selection is required for Employee role.");
                    }
                    else
                    {
                        var employeeExistsAndActive = await _context.EmployeeMasters
                            .AnyAsync(e => e.EmployeeId == model.EmployeeId && e.Status == "Active");
                        if (!employeeExistsAndActive)
                        {
                            ModelState.AddModelError("EmployeeId", "Selected employee is inactive or does not exist.");
                        }

                        var alreadyAssigned = await _context.UserMasters
                            .AnyAsync(u => u.EmployeeId == model.EmployeeId && u.UserId != id);
                        if (alreadyAssigned)
                        {
                            ModelState.AddModelError("EmployeeId", "This employee is already linked to another user account.");
                        }
                    }
                }
                else
                {
                    model.EmployeeId = null;
                }
            }

            if (ModelState.IsValid)
            {
                // Unique Username validation (excluding current user)
                var usernameExists = await _context.UserMasters
                    .AnyAsync(u => u.Username.Trim().ToLower() == model.Username.Trim().ToLower() && u.UserId != id);
                if (usernameExists)
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                }

                // Unique Email validation (excluding current user)
                var emailExists = await _context.UserMasters
                    .AnyAsync(u => u.Email.Trim().ToLower() == model.Email.Trim().ToLower() && u.UserId != id);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email address is already registered.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var dbUser = await _context.UserMasters.FindAsync(id);
                        if (dbUser == null) return NotFound();

                        dbUser.Username = model.Username;
                        dbUser.FullName = model.FullName;
                        dbUser.Email = model.Email;
                        dbUser.MobileNumber = model.MobileNumber;
                        dbUser.RoleId = model.RoleId;
                        dbUser.Status = model.Status;
                        dbUser.EmployeeId = model.EmployeeId;
                        dbUser.LastUpdatedDate = DateTime.UtcNow;

                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "User details updated successfully!";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!UserExists(model.UserId))
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
            }

            var activeRoles = await _context.RoleMasters
                .Where(r => r.Status == "Active" || r.RoleId == model.RoleId)
                .ToListAsync();
            ViewBag.EmployeeRoleId = employeeRole?.RoleId;

            var assignedEmployeeIds = await _context.UserMasters
                .Where(u => u.EmployeeId != null && u.UserId != id)
                .Select(u => u.EmployeeId)
                .ToListAsync();

            var activeUnassignedEmployees = await _context.EmployeeMasters
                .Where(e => e.Status == "Active" && (!assignedEmployeeIds.Contains(e.EmployeeId) || e.EmployeeId == model.EmployeeId))
                .Select(e => new {
                    e.EmployeeId,
                    FullName = e.FirstName + " " + (e.LastName ?? "")
                })
                .ToListAsync();

            ViewBag.RolesList = new SelectList(activeRoles, "RoleId", "RoleName", model.RoleId);
            ViewBag.EmployeesList = new SelectList(activeUnassignedEmployees, "EmployeeId", "FullName", model.EmployeeId);
            return View(model);
        }

        // POST: User/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.UserMasters.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.UserMasters.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                TempData["ErrorMessage"] = "Cannot delete user because related records exist.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: User/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int userId, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                TempData["ErrorMessage"] = "New Password is required.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.UserMasters.FindAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            user.Password = newPassword; // Store in plaintext as requested
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Password for '{user.Username}' reset successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: User/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.UserMasters.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            user.Status = (user.Status == "Active") ? "Inactive" : "Active";
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User '{user.Username}' status updated to '{user.Status}' successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.UserMasters.Any(e => e.UserId == id);
        }
    }
}
