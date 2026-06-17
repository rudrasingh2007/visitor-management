using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Helpers;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to Dashboard
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username and Password are required.";
                return View();
            }

            // Auto-seed rudra123 if it is missing
            if (username == "rudra123")
            {
                var existing = await _context.UserMasters.FirstOrDefaultAsync(u => u.Username == "rudra123");
                if (existing == null)
                {
                    var employeeRole = await _context.RoleMasters.FirstOrDefaultAsync(r => r.RoleName == "Employee");
                    var employee = await _context.EmployeeMasters.FirstOrDefaultAsync(e => e.Email == "rudra123@gmail.com");
                    if (employeeRole != null && employee != null)
                    {
                        var newUser = new UserMaster
                        {
                            Username = "rudra123",
                            FullName = "Rudra Singh",
                            Email = "rudra123@gmail.com",
                            MobileNumber = "9876543211",
                            Password = "rudra123",
                            RoleId = employeeRole.RoleId,
                            Status = "Active",
                            EmployeeId = employee.EmployeeId,
                            CreatedDate = DateTime.UtcNow
                        };
                        _context.UserMasters.Add(newUser);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // Fetch user by username
            var user = await _context.UserMasters
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            // Plaintext password comparison as requested
            if (user == null || user.Password != password)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            // Check if user account is Active
            if (user.Status != "Active")
            {
                ViewBag.Error = "Your account is inactive. Please contact the administrator.";
                return View();
            }

            // Create Session variables
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("RoleName", user.Role?.RoleName ?? "No Role");
            HttpContext.Session.SetString("Email", user.Email);

            // Resolve and cache EmployeeId in session if user is linked to an Employee
            if (user.EmployeeId.HasValue)
            {
                HttpContext.Session.SetInt32("EmployeeId", user.EmployeeId.Value);
            }

            // Cache Role Rights in Session
            var rights = await _context.RoleRights
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
            HttpContext.Session.SetString("UserRights", rightsJson);

            // Dynamic redirect based on permissions
            if (string.Equals(user.Role?.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // Find first accessible module view in priority order
            var priorityList = new List<(string Module, string Controller, string Action)>
            {
                ("Dashboard", "Dashboard", "Index"),
                ("Visitor Master", "Visitor", "Index"),
                ("Appointment Master", "Appointment", "Index"),
                ("Visit Entry", "Visit", "Index"),
                ("Gate Pass", "GatePass", "Index"),
                ("Reports", "VisitorHistory", "Index"),
                ("Employee Master", "Employee", "Index"),
                ("Department Master", "Department", "Index"),
                ("User Master", "User", "Index"),
                ("Role Master", "Role", "Index")
            };

            foreach (var item in priorityList)
            {
                var hasAccess = rights.Any(r => string.Equals(r.ModuleName, item.Module, StringComparison.OrdinalIgnoreCase) && r.CanView);
                if (hasAccess)
                {
                    return RedirectToAction(item.Action, item.Controller);
                }
            }

            // Fallback if no permissions are configured
            ViewBag.Error = "Your account role has no viewing permissions. Please contact an administrator.";
            HttpContext.Session.Clear();
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Destroy Session
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }



        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
