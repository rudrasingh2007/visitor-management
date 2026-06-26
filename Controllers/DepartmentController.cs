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
    [HasPermission("Department Master", "View")]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Department
        public async Task<IActionResult> Index()
        {
            var departments = await _context.DepartmentMasters
                .Include(d => d.Employees)
                .ToListAsync();
            return View(departments);
        }

        // GET: Department/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.DepartmentMasters
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(m => m.DepartmentId == id);
            
            if (department == null) return NotFound();

            return View(department);
        }

        // GET: Department/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentMaster department)
        {
            if (ModelState.IsValid)
            {
                // Prevent duplicate department names
                var exists = await _context.DepartmentMasters
                    .AnyAsync(d => d.DepartmentName.Trim().ToLower() == department.DepartmentName.Trim().ToLower());
                
                if (exists)
                {
                    ModelState.AddModelError("DepartmentName", "Department Name already exists. Please choose a unique name.");
                    return View(department);
                }

                department.DepartmentName = department.DepartmentName.Trim();
                department.Description = department.Description?.Trim();
                department.CreatedDate = DateTime.UtcNow;

                _context.Add(department);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Department created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: Department/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.DepartmentMasters.FindAsync(id);
            if (department == null) return NotFound();

            return View(department);
        }

        // POST: Department/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentMaster department)
        {
            if (id != department.DepartmentId) return NotFound();

            if (ModelState.IsValid)
            {
                // Prevent duplicate names excluding self
                var exists = await _context.DepartmentMasters
                    .AnyAsync(d => d.DepartmentName.Trim().ToLower() == department.DepartmentName.Trim().ToLower() && d.DepartmentId != id);
                
                if (exists)
                {
                    ModelState.AddModelError("DepartmentName", "Department Name already exists. Please choose a unique name.");
                    return View(department);
                }

                try
                {
                    // Load original record from DB to preserve CreatedDate
                    var dbDept = await _context.DepartmentMasters.FindAsync(id);
                    if (dbDept == null) return NotFound();

                    dbDept.DepartmentName = department.DepartmentName.Trim();
                    dbDept.Description = department.Description?.Trim();
                    dbDept.Status = department.Status;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Department updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DepartmentId))
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
            return View(department);
        }

        // POST: Department/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var department = await _context.DepartmentMasters.FindAsync(id);
            if (department == null)
            {
                TempData["ErrorMessage"] = "Department not found.";
                return RedirectToAction(nameof(Index));
            }

            department.Status = (department.Status == "Active") ? "Inactive" : "Active";
            _context.Update(department);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Department '{department.DepartmentName}' status updated to '{department.Status}' successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Department/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.DepartmentMasters.FindAsync(id);
            if (department == null)
            {
                TempData["ErrorMessage"] = "Department not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.DepartmentMasters.Remove(department);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Department deleted successfully!";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                TempData["ErrorMessage"] = "Cannot delete department because employees or appointments are linked to it.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred during deletion: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            return _context.DepartmentMasters.Any(e => e.DepartmentId == id);
        }
    }
}
