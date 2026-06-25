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

using Microsoft.AspNetCore.DataProtection;

namespace VisitorManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public AccountController(ApplicationDbContext context, IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _dataProtectionProvider = dataProtectionProvider;
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
        public async Task<IActionResult> Login(string username, string password, string captchaCode, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username and Password are required.";
                return View();
            }

            var sessionCaptcha = HttpContext.Session.GetString("CaptchaCode");
            if (string.IsNullOrEmpty(captchaCode) || string.IsNullOrEmpty(sessionCaptcha) || !captchaCode.Equals(sessionCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Invalid CAPTCHA code. Please try again.";
                return View();
            }

            // Auto-seed rudra123 if it is missing or needs linking
            if (username == "rudra123")
            {
                var existing = await _context.UserMasters.FirstOrDefaultAsync(u => u.Username == "rudra123");
                var employeeRole = await _context.RoleMasters.FirstOrDefaultAsync(r => r.RoleName == "Employee");
                var employee = await _context.EmployeeMasters.FirstOrDefaultAsync(e => e.Email == "rudra123@gmail.com");

                if (existing == null)
                {
                    if (employeeRole != null && employee != null)
                    {
                        var newUser = new UserMaster
                        {
                            Username = "rudra123",
                            FullName = "Rudra Singh",
                            Email = "rudra123@gmail.com",
                            MobileNumber = "9876543211",
                            Password = PasswordHelper.HashPassword("rudra123"),
                            RoleId = employeeRole.RoleId,
                            Status = "Active",
                            EmployeeId = employee.EmployeeId,
                            CreatedDate = DateTime.UtcNow
                        };
                        _context.UserMasters.Add(newUser);
                        await _context.SaveChangesAsync();
                    }
                }
                else if (existing.EmployeeId == null && employee != null)
                {
                    existing.EmployeeId = employee.EmployeeId;
                    _context.UserMasters.Update(existing);
                    await _context.SaveChangesAsync();
                }
            }

            // Fetch user by username
            var user = await _context.UserMasters
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            // Check if user is currently locked out
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                if (user.LockoutLevel >= 2)
                {
                    ViewBag.Error = "Contact your administrator.";
                }
                else
                {
                    var timeRemaining = user.LockoutEnd.Value - DateTime.UtcNow;
                    var mins = Math.Ceiling(timeRemaining.TotalMinutes);
                    ViewBag.Error = $"Account locked. Please try again after {mins} minute(s).";
                }
                return View();
            }

            // Hashed password comparison
            var hashedPassword = PasswordHelper.HashPassword(password);
            if (user.Password != hashedPassword)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutLevel++;
                    if (user.LockoutLevel == 1)
                        user.LockoutEnd = DateTime.UtcNow.AddMinutes(5);
                    else if (user.LockoutLevel == 2)
                        user.LockoutEnd = DateTime.UtcNow.AddHours(1);
                    else
                        user.LockoutEnd = DateTime.UtcNow.AddHours(24);
                    
                    user.FailedLoginAttempts = 0;
                    _context.UserMasters.Update(user);
                    await _context.SaveChangesAsync();

                    if (user.LockoutLevel >= 2)
                    {
                        ViewBag.Error = "Contact your administrator.";
                    }
                    else
                    {
                        ViewBag.Error = "Account locked for 5 minutes due to too many failed attempts.";
                    }
                }
                else
                {
                    _context.UserMasters.Update(user);
                    await _context.SaveChangesAsync();
                    ViewBag.Error = $"Invalid username or password. Attempts remaining: {5 - user.FailedLoginAttempts}";
                }
                return View();
            }

            // Reset lockout counters on successful login
            if (user.FailedLoginAttempts > 0 || user.LockoutLevel > 0 || user.LockoutEnd.HasValue)
            {
                user.FailedLoginAttempts = 0;
                user.LockoutLevel = 0;
                user.LockoutEnd = null;
                _context.UserMasters.Update(user);
                await _context.SaveChangesAsync();
            }

            // Check if user account is Active
            if (user.Status != "Active")
            {
                ViewBag.Error = "Your account is inactive. Please contact the administrator.";
                return View();
            }

            // Dynamic check: if user has Role == "Employee" but EmployeeId is NULL, resolve it by matching Email
            if (!user.EmployeeId.HasValue && string.Equals(user.Role?.RoleName, "Employee", StringComparison.OrdinalIgnoreCase))
            {
                var matchedEmployee = await _context.EmployeeMasters.FirstOrDefaultAsync(e => e.Email == user.Email);
                if (matchedEmployee != null)
                {
                    user.EmployeeId = matchedEmployee.EmployeeId;
                    _context.UserMasters.Update(user);
                    await _context.SaveChangesAsync();
                }
            }

            // Create Session variables
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("RoleName", user.Role?.RoleName ?? "No Role");
            HttpContext.Session.SetString("Email", user.Email);
            if (!string.IsNullOrEmpty(user.PhotoPath))
            {
                HttpContext.Session.SetString("UserPhoto", user.PhotoPath);
            }

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

            // Handle Remember Me
            if (rememberMe)
            {
                var protector = _dataProtectionProvider.CreateProtector("RememberMe");
                var encryptedUsername = protector.Protect(user.Username);
                
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true
                };
                Response.Cookies.Append("RememberMeToken", encryptedUsername, cookieOptions);
            }
            // Check for default password
            if (hashedPassword == PasswordHelper.HashPassword("Welcome@123"))
            {
                HttpContext.Session.Clear();
                TempData["ForceChangeUsername"] = user.Username;
                TempData["ForceChangeMessage"] = "For security reasons, please change your default password to continue.";
                return RedirectToAction("ForceChangePassword", "Account");
            }

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
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string username, string emailOrMobile)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(emailOrMobile))
            {
                ViewBag.Error = "Please enter both Username and Email/Mobile.";
                return View();
            }

            var user = await _context.UserMasters.FirstOrDefaultAsync(u => 
                u.Username == username && 
                (u.Email == emailOrMobile || u.MobileNumber == emailOrMobile));

            if (user == null)
            {
                ViewBag.Error = "No matching account found with the provided details.";
                return View();
            }

            // Create a temporary token based on UserId and current time to allow reset
            var protector = _dataProtectionProvider.CreateProtector("PasswordReset");
            var tokenPayload = $"{user.UserId}:{DateTime.UtcNow.Ticks}";
            var resetToken = protector.Protect(tokenPayload);

            return RedirectToAction("ResetPassword", new { token = resetToken });
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }
            ViewBag.ResetToken = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");
            
            ViewBag.ResetToken = token;

            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match or are empty.";
                return View();
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(newPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"))
            {
                ViewBag.Error = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.";
                return View();
            }

            try
            {
                var protector = _dataProtectionProvider.CreateProtector("PasswordReset");
                var tokenPayload = protector.Unprotect(token);
                var parts = tokenPayload.Split(':');
                if (parts.Length != 2 || !int.TryParse(parts[0], out int userId))
                {
                    ViewBag.Error = "Invalid reset token.";
                    return View();
                }

                // Check expiry (e.g. 15 minutes)
                long ticks = long.Parse(parts[1]);
                var issued = new DateTime(ticks, DateTimeKind.Utc);
                if (DateTime.UtcNow - issued > TimeSpan.FromMinutes(15))
                {
                    ViewBag.Error = "Reset token has expired. Please try again.";
                    return View();
                }

                var user = await _context.UserMasters.FindAsync(userId);
                if (user == null)
                {
                    ViewBag.Error = "User not found.";
                    return View();
                }

                if (user.Password == PasswordHelper.HashPassword(newPassword))
                {
                    ViewBag.Error = "New password cannot be the same as the current password.";
                    return View();
                }

                user.Password = PasswordHelper.HashPassword(newPassword);
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                user.LockoutLevel = 0;
                
                _context.UserMasters.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Password has been reset successfully. You can now login.";
                return RedirectToAction("Login");
            }
            catch
            {
                ViewBag.Error = "Invalid reset token.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ViewBag.Error = "Please ensure all fields are filled and new passwords match.";
                return View();
            }

            if (oldPassword == newPassword)
            {
                ViewBag.Error = "New password cannot be the same as the old password.";
                return View();
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(newPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"))
            {
                ViewBag.Error = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.";
                return View();
            }

            var user = await _context.UserMasters.FindAsync(userId.Value);
            if (user == null) return RedirectToAction("Login", "Account");

            var hashedOld = PasswordHelper.HashPassword(oldPassword);
            if (user.Password != hashedOld)
            {
                ViewBag.Error = "Incorrect old password.";
                return View();
            }

            user.Password = PasswordHelper.HashPassword(newPassword);
            _context.UserMasters.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult ForceChangePassword()
        {
            var username = TempData["ForceChangeUsername"] as string;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");
            
            ViewBag.Username = username;
            ViewBag.Message = TempData["ForceChangeMessage"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceChangePassword(string username, string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");
            ViewBag.Username = username;

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ViewBag.Error = "Please ensure all fields are filled and new passwords match.";
                return View();
            }

            if (oldPassword == newPassword)
            {
                ViewBag.Error = "New password cannot be the same as the old password.";
                return View();
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(newPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"))
            {
                ViewBag.Error = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.";
                return View();
            }

            var user = await _context.UserMasters.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Account");

            var hashedOld = PasswordHelper.HashPassword(oldPassword);
            if (user.Password != hashedOld)
            {
                ViewBag.Error = "Incorrect old password.";
                return View();
            }

            user.Password = PasswordHelper.HashPassword(newPassword);
            _context.UserMasters.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully! Please login with your new password.";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _context.UserMasters
                .Include(u => u.Role)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null) return RedirectToAction("Login", "Account");

            return View(user);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("RememberMeToken");
            return RedirectToAction("Login", "Account");
        }



        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CaptchaImage()
        {
            var random = new Random();
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude I, O, 0, 1 for clarity
            var captcha = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
            
            HttpContext.Session.SetString("CaptchaCode", captcha);
            
            // Create SVG
            var width = 120;
            var height = 40;
            var svg = $"<svg width='{width}' height='{height}' xmlns='http://www.w3.org/2000/svg'>";
            svg += $"<rect width='100%' height='100%' fill='#f0f2f5' />";
            
            // Add noise lines
            for(int i = 0; i < 7; i++)
            {
                var x1 = random.Next(width);
                var y1 = random.Next(height);
                var x2 = random.Next(width);
                var y2 = random.Next(height);
                svg += $"<line x1='{x1}' y1='{y1}' x2='{x2}' y2='{y2}' stroke='#888' stroke-width='2' />";
            }

            // Add text with slight random rotation
            int x = 10;
            foreach (var ch in captcha)
            {
                var rotate = random.Next(-15, 15);
                var y = random.Next(25, 32);
                svg += $"<text x='{x}' y='{y}' font-family='Arial' font-size='24' font-weight='bold' fill='#1e3c72' transform='rotate({rotate} {x},{y})'>{ch}</text>";
                x += 20;
            }
            svg += "</svg>";

            return Content(svg, "image/svg+xml");
        }
    }
}
