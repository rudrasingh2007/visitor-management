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
    [HasPermission("Employee Master", "View")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employee
        public async Task<IActionResult> Index()
        {
            var employees = await _context.EmployeeMasters
                .Include(e => e.Department)
                .ToListAsync();
            
            // Get active departments for list view filter
            var departments = await _context.DepartmentMasters
                .Where(d => d.Status == "Active")
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
            
            ViewBag.DepartmentsList = new SelectList(departments, "DepartmentName", "DepartmentName");

            return View(employees);
        }

        // GET: Employee/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.EmployeeMasters
                .Include(e => e.Department)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            
            if (employee == null) return NotFound();

            // Mock counters for future integration modules
            ViewBag.VisitsCount = 0;
            ViewBag.AppointmentsCount = 0;

            return View(employee);
        }

        // GET: Employee/Create
        public async Task<IActionResult> Create()
        {
            var activeDepts = await _context.DepartmentMasters
                .Where(d => d.Status == "Active")
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
            
            ViewBag.DepartmentsList = new SelectList(activeDepts, "DepartmentId", "DepartmentName");
            return View();
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Unique Employee Code check
                var codeExists = await _context.EmployeeMasters
                    .AnyAsync(e => e.EmployeeCode.Trim().ToLower() == model.EmployeeCode.Trim().ToLower());
                if (codeExists)
                {
                    ModelState.AddModelError("EmployeeCode", "Employee Code is already registered.");
                }

                // Unique Email check
                var emailExists = await _context.EmployeeMasters
                    .AnyAsync(e => e.Email.Trim().ToLower() == model.Email.Trim().ToLower());
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email address is already registered.");
                }

                // Unique Mobile Number check
                var mobileExists = await _context.EmployeeMasters
                    .AnyAsync(e => e.MobileNumber.Trim() == model.MobileNumber.Trim());
                if (mobileExists)
                {
                    ModelState.AddModelError("MobileNumber", "Mobile Number is already registered.");
                }

                if (ModelState.IsValid)
                {
                    var employee = new EmployeeMaster
                    {
                        EmployeeCode = model.EmployeeCode.Trim(),
                        FirstName = model.FirstName.Trim(),
                        LastName = model.LastName?.Trim(),
                        Gender = model.Gender,
                        DepartmentId = model.DepartmentId,
                        Designation = model.Designation.Trim(),
                        Email = model.Email.Trim().ToLower(),
                        MobileNumber = model.MobileNumber.Trim(),
                        Status = model.Status,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Add(employee);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Employee registered successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }

            var activeDepts = await _context.DepartmentMasters
                .Where(d => d.Status == "Active")
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
            ViewBag.DepartmentsList = new SelectList(activeDepts, "DepartmentId", "DepartmentName", model.DepartmentId);
            return View(model);
        }

        // GET: Employee/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.EmployeeMasters.FindAsync(id);
            if (employee == null) return NotFound();

            var model = new EmployeeViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmployeeCode = employee.EmployeeCode,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Gender = employee.Gender,
                DepartmentId = employee.DepartmentId,
                Designation = employee.Designation,
                Email = employee.Email,
                MobileNumber = employee.MobileNumber,
                Status = employee.Status
            };

            var activeDepts = await _context.DepartmentMasters
                .Where(d => d.Status == "Active" || d.DepartmentId == employee.DepartmentId)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
            ViewBag.DepartmentsList = new SelectList(activeDepts, "DepartmentId", "DepartmentName", employee.DepartmentId);

            return View(model);
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel model)
        {
            if (id != model.EmployeeId) return NotFound();

            if (ModelState.IsValid)
            {
                // Unique Employee Code check (excluding current employee)
                var codeExists = await _context.EmployeeMasters
                    .AnyAsync(e => e.EmployeeCode.Trim().ToLower() == model.EmployeeCode.Trim().ToLower() && e.EmployeeId != id);
                if (codeExists)
                {
                    ModelState.AddModelError("EmployeeCode", "Employee Code is already registered.");
                }

                // Unique Email check (excluding current employee)
                var emailExists = await _context.EmployeeMasters
                    .AnyAsync(e => e.Email.Trim().ToLower() == model.Email.Trim().ToLower() && e.EmployeeId != id);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email address is already registered.");
                }

                // Unique Mobile Number check (excluding current employee)
                var mobileExists = await _context.EmployeeMasters
                    .AnyAsync(e => e.MobileNumber.Trim() == model.MobileNumber.Trim() && e.EmployeeId != id);
                if (mobileExists)
                {
                    ModelState.AddModelError("MobileNumber", "Mobile Number is already registered.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        // Load database record to preserve CreatedDate
                        var employee = await _context.EmployeeMasters.FindAsync(id);
                        if (employee == null) return NotFound();

                        employee.EmployeeCode = model.EmployeeCode.Trim();
                        employee.FirstName = model.FirstName.Trim();
                        employee.LastName = model.LastName?.Trim();
                        employee.Gender = model.Gender;
                        employee.DepartmentId = model.DepartmentId;
                        employee.Designation = model.Designation.Trim();
                        employee.Email = model.Email.Trim().ToLower();
                        employee.MobileNumber = model.MobileNumber.Trim();
                        employee.Status = model.Status;

                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Employee details updated successfully!";
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!EmployeeExists(model.EmployeeId))
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

            var activeDepts = await _context.DepartmentMasters
                .Where(d => d.Status == "Active" || d.DepartmentId == model.DepartmentId)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
            ViewBag.DepartmentsList = new SelectList(activeDepts, "DepartmentId", "DepartmentName", model.DepartmentId);
            return View(model);
        }

        // POST: Employee/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var employee = await _context.EmployeeMasters.FindAsync(id);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            employee.Status = (employee.Status == "Active") ? "Inactive" : "Active";
            _context.Update(employee);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Employee '{employee.FirstName}' status updated to '{employee.Status}' successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Employee/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.EmployeeMasters.FindAsync(id);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.EmployeeMasters.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employee deleted successfully!";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                TempData["ErrorMessage"] = "Cannot delete employee because appointments, users, or visit records are linked to this employee.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred during deletion: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.EmployeeMasters.Any(e => e.EmployeeId == id);
        }
    }
}
