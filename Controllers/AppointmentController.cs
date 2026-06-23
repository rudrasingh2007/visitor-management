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
    [HasPermission("Appointment Master", "View")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Appointment List
        public async Task<IActionResult> Index()
        {
            var roleName = HttpContext.Session.GetString("RoleName");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            var query = _context.AppointmentMasters
                .Include(a => a.Visitor)
                .Include(a => a.Department)
                .Include(a => a.Employee)
                .AsQueryable();

            if (roleName == "Employee" && employeeId.HasValue)
            {
                query = query.Where(a => a.EmployeeId == employeeId.Value);
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            // Populate filter dropdown data in ViewBag
            ViewBag.VisitorsList = new SelectList(await _context.VisitorMasters.OrderBy(v => v.FirstName).Select(v => new { v.VisitorId, Name = v.FirstName + " " + v.LastName }).ToListAsync(), "Name", "Name");
            ViewBag.EmployeesList = new SelectList(await _context.EmployeeMasters.OrderBy(e => e.FirstName).Select(e => new { e.EmployeeId, Name = e.FirstName + " " + e.LastName }).ToListAsync(), "Name", "Name");
            ViewBag.DepartmentsList = new SelectList(await _context.DepartmentMasters.OrderBy(d => d.DepartmentName).ToListAsync(), "DepartmentName", "DepartmentName");

            return View(appointments);
        }

        // 2. Create Appointment (GET)
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("RoleName") == "Employee")
            {
                TempData["ErrorMessage"] = "Employees are not authorized to create appointments.";
                return RedirectToAction("Index");
            }

            await PopulateDropdowns();
            return View(new AppointmentViewModel { AppointmentDate = DateTime.Today });
        }

        // 2. Create Appointment (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel model)
        {
            if (HttpContext.Session.GetString("RoleName") == "Employee")
            {
                TempData["ErrorMessage"] = "Employees are not authorized to create appointments.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                // Validation: Appointment Date cannot be in the past
                if (model.AppointmentDate.Date < DateTime.Today)
                {
                    ModelState.AddModelError("AppointmentDate", "Appointment Date cannot be in the past.");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                // Validation: Prevent duplicate appointments for the same visitor, employee, date, and time
                var isDuplicate = await _context.AppointmentMasters.AnyAsync(a =>
                    a.VisitorId == model.VisitorId &&
                    a.EmployeeId == model.EmployeeId &&
                    a.AppointmentDate == model.AppointmentDate.Date &&
                    a.AppointmentTime == model.AppointmentTime);

                if (isDuplicate)
                {
                    ModelState.AddModelError("", "A duplicate appointment for the same visitor, employee, date, and time already exists.");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                // Create and map AppointmentMaster entity
                var appointment = new AppointmentMaster
                {
                    VisitorId = model.VisitorId,
                    DepartmentId = model.DepartmentId,
                    EmployeeId = model.EmployeeId,
                    AppointmentDate = DateTime.SpecifyKind(model.AppointmentDate.Date, DateTimeKind.Utc),
                    AppointmentTime = model.AppointmentTime,
                    Purpose = model.Purpose.Trim(),
                    Remarks = model.Remarks?.Trim(),
                    Status = "Pending",
                    CreatedDate = DateTime.UtcNow
                };

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.AppointmentMasters.Add(appointment);
                        await _context.SaveChangesAsync(); // To get AppointmentId



                        await transaction.CommitAsync();
                        TempData["SuccessMessage"] = "Appointment scheduled successfully and sent for approval!";
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "Failed to create appointment: " + ex.Message;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        // 3. Edit Appointment (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.AppointmentMasters.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var model = new AppointmentViewModel
            {
                AppointmentId = appointment.AppointmentId,
                VisitorId = appointment.VisitorId,
                DepartmentId = appointment.DepartmentId,
                EmployeeId = appointment.EmployeeId,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                Purpose = appointment.Purpose,
                Remarks = appointment.Remarks,
                Status = appointment.Status
            };

            await PopulateDropdowns(model);
            return View(model);
        }

        // 3. Edit Appointment (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validation: Appointment Date cannot be in the past (only for new/upcoming dates, but allow if it remains same)
                var original = await _context.AppointmentMasters.AsNoTracking().FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId);
                if (original == null)
                {
                    return NotFound();
                }

                if (model.AppointmentDate.Date < DateTime.Today && model.AppointmentDate.Date != original.AppointmentDate.Date)
                {
                    ModelState.AddModelError("AppointmentDate", "Appointment Date cannot be in the past.");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                // Validation: Prevent duplicate appointments
                var isDuplicate = await _context.AppointmentMasters.AnyAsync(a =>
                    a.VisitorId == model.VisitorId &&
                    a.EmployeeId == model.EmployeeId &&
                    a.AppointmentDate == model.AppointmentDate.Date &&
                    a.AppointmentTime == model.AppointmentTime &&
                    a.AppointmentId != model.AppointmentId);

                if (isDuplicate)
                {
                    ModelState.AddModelError("", "A duplicate appointment for the same visitor, employee, date, and time already exists.");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                // Load database entity to modify
                var appointment = await _context.AppointmentMasters.FindAsync(model.AppointmentId);
                if (appointment == null)
                {
                    return NotFound();
                }

                // Map updated values
                appointment.VisitorId = model.VisitorId;
                appointment.DepartmentId = model.DepartmentId;
                appointment.EmployeeId = model.EmployeeId;
                appointment.AppointmentDate = DateTime.SpecifyKind(model.AppointmentDate.Date, DateTimeKind.Utc);
                appointment.AppointmentTime = model.AppointmentTime;
                appointment.Purpose = model.Purpose.Trim();
                appointment.Remarks = model.Remarks?.Trim();
                appointment.Status = model.Status;

                _context.AppointmentMasters.Update(appointment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Appointment updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        // 4. View Appointment Details
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.AppointmentMasters
                .Include(a => a.Visitor)
                .Include(a => a.Department)
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // 5. Approve Action (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? remarks)
        {
            var roleName = HttpContext.Session.GetString("RoleName");
            if (roleName != "Employee")
            {
                TempData["ErrorMessage"] = "Only Employees can approve appointments.";
                return RedirectToAction("Index", "Dashboard");
            }

            var appointment = await _context.AppointmentMasters.FindAsync(id);
            if (appointment == null) return NotFound();

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (appointment.EmployeeId != employeeId)
            {
                TempData["ErrorMessage"] = "You are not authorized to approve this appointment.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (appointment.Status != "Pending")
            {
                TempData["ErrorMessage"] = "This appointment has already been processed.";
                return RedirectToAction("Index", "Dashboard");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    appointment.Status = "Approved";
                    if (!string.IsNullOrEmpty(remarks))
                    {
                        appointment.Remarks = string.IsNullOrEmpty(appointment.Remarks) ? remarks : appointment.Remarks + " | " + remarks;
                    }
                    _context.AppointmentMasters.Update(appointment);


                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    TempData["SuccessMessage"] = "Appointment approved successfully!";
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Failed to approve appointment.";
                }
            }

            return RedirectToAction("Index", "Dashboard");
        }

        // 6. Reject Action (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? remarks)
        {
            var roleName = HttpContext.Session.GetString("RoleName");
            if (roleName != "Employee")
            {
                TempData["ErrorMessage"] = "Only Employees can reject appointments.";
                return RedirectToAction("Index", "Dashboard");
            }

            var appointment = await _context.AppointmentMasters.FindAsync(id);
            if (appointment == null) return NotFound();

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (appointment.EmployeeId != employeeId)
            {
                TempData["ErrorMessage"] = "You are not authorized to reject this appointment.";
                return RedirectToAction("Index", "Dashboard");
            }

            if (appointment.Status != "Pending")
            {
                TempData["ErrorMessage"] = "This appointment has already been processed.";
                return RedirectToAction("Index", "Dashboard");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    appointment.Status = "Rejected";
                    if (!string.IsNullOrEmpty(remarks))
                    {
                        appointment.Remarks = string.IsNullOrEmpty(appointment.Remarks) ? remarks : appointment.Remarks + " | " + remarks;
                    }
                    _context.AppointmentMasters.Update(appointment);


                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    TempData["SuccessMessage"] = "Appointment rejected successfully!";
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Failed to reject appointment.";
                }
            }

            return RedirectToAction("Index", "Dashboard");
        }

        // AJAX helper: load employees based on department selection
        [HttpGet]
        public async Task<JsonResult> GetEmployeesByDepartment(int departmentId)
        {
            var employees = await _context.EmployeeMasters
                .Where(e => e.DepartmentId == departmentId && e.Status == "Active")
                .OrderBy(e => e.FirstName)
                .Select(e => new { value = e.EmployeeId, text = e.FirstName + " " + e.LastName + " (" + e.Designation + ")" })
                .ToListAsync();

            return Json(employees);
        }

        // Helper to load selection items
        private async Task PopulateDropdowns(AppointmentViewModel? model = null)
        {
            // Load Active Visitors (include currently selected even if inactive)
            var currentVisitorId = model?.VisitorId ?? 0;
            var visitors = await _context.VisitorMasters
                .Where(v => v.Status == "Active" || v.VisitorId == currentVisitorId)
                .OrderBy(v => v.FirstName)
                .Select(v => new { v.VisitorId, Name = v.FirstName + " " + v.LastName + " (" + v.MobileNumber + ")" })
                .ToListAsync();
            ViewBag.VisitorsList = new SelectList(visitors, "VisitorId", "Name", model?.VisitorId);

            // Load Active Departments (include currently selected even if inactive)
            var currentDeptId = model?.DepartmentId ?? 0;
            var departments = await _context.DepartmentMasters
                .Where(d => d.Status == "Active" || d.DepartmentId == currentDeptId)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
            ViewBag.DepartmentsList = new SelectList(departments, "DepartmentId", "DepartmentName", model?.DepartmentId);

            // Load Employees: if department is selected, load employees of that department; else load empty list
            if (model != null && model.DepartmentId > 0)
            {
                var currentEmployeeId = model.EmployeeId;
                var employees = await _context.EmployeeMasters
                    .Where(e => e.DepartmentId == model.DepartmentId && (e.Status == "Active" || e.EmployeeId == currentEmployeeId))
                    .OrderBy(e => e.FirstName)
                    .Select(e => new { e.EmployeeId, Name = e.FirstName + " " + e.LastName + " (" + e.Designation + ")" })
                    .ToListAsync();
                ViewBag.EmployeesList = new SelectList(employees, "EmployeeId", "Name", model.EmployeeId);
            }
            else
            {
                ViewBag.EmployeesList = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }
    }
}
