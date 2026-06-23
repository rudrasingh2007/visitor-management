using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Filters;
using VisitorManagementSystem.Models;
using VisitorManagementSystem.ViewModels;

namespace VisitorManagementSystem.Controllers
{
    [SessionAuthorize]
    [HasPermission("Visitor Master", "View")]
    public class VisitorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public VisitorController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Visitor
        public async Task<IActionResult> Index()
        {
            var visitors = await _context.VisitorMasters.ToListAsync();
            return View(visitors);
        }

        // GET: Visitor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var visitor = await _context.VisitorMasters.FindAsync(id);
            if (visitor == null) return NotFound();

            // Mock counters for previous appointment and visit tracking integration
            ViewBag.VisitsCount = 0;
            ViewBag.AppointmentsCount = 0;

            return View(visitor);
        }

        // GET: Visitor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Visitor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VisitorViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Prevent duplicate visitors based on Mobile Number, Email, and ID Proof Number
                var mobileExists = await _context.VisitorMasters
                    .AnyAsync(v => v.MobileNumber.Trim() == model.MobileNumber.Trim());
                if (mobileExists)
                {
                    ModelState.AddModelError("MobileNumber", "Visitor Mobile Number already registered.");
                }

                var emailExists = await _context.VisitorMasters
                    .AnyAsync(v => v.Email.Trim().ToLower() == model.Email.Trim().ToLower());
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Visitor Email Address already registered.");
                }

                var idProofExists = await _context.VisitorMasters
                    .AnyAsync(v => v.IdProofNumber.Trim() == model.IdProofNumber.Trim());
                if (idProofExists)
                {
                    ModelState.AddModelError("IdProofNumber", "ID Proof Number already registered.");
                }

                if (ModelState.IsValid)
                {
                    string? photoPath = null;

                    var visitor = new VisitorMaster
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Gender = model.Gender,
                        MobileNumber = model.MobileNumber,
                        Email = model.Email,
                        Address = model.Address,
                        City = model.City,
                        State = model.State,
                        CompanyName = model.CompanyName,
                        PhotoPath = photoPath,
                        IdProofType = model.IdProofType,
                        IdProofNumber = model.IdProofNumber,
                        Status = model.Status,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Add(visitor);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Visitor registered successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        // GET: Visitor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var visitor = await _context.VisitorMasters.FindAsync(id);
            if (visitor == null) return NotFound();

            var model = new VisitorViewModel
            {
                VisitorId = visitor.VisitorId,
                FirstName = visitor.FirstName,
                LastName = visitor.LastName,
                Gender = visitor.Gender,
                MobileNumber = visitor.MobileNumber,
                Email = visitor.Email,
                Address = visitor.Address,
                City = visitor.City,
                State = visitor.State,
                CompanyName = visitor.CompanyName,
                PhotoPath = visitor.PhotoPath,
                IdProofType = visitor.IdProofType,
                IdProofNumber = visitor.IdProofNumber,
                Status = visitor.Status
            };

            return View(model);
        }

        // POST: Visitor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VisitorViewModel model)
        {
            if (id != model.VisitorId) return NotFound();

            if (ModelState.IsValid)
            {
                // Prevent duplicates excluding the current visitor being edited
                var mobileExists = await _context.VisitorMasters
                    .AnyAsync(v => v.MobileNumber.Trim() == model.MobileNumber.Trim() && v.VisitorId != id);
                if (mobileExists)
                {
                    ModelState.AddModelError("MobileNumber", "Visitor Mobile Number already registered.");
                }

                var emailExists = await _context.VisitorMasters
                    .AnyAsync(v => v.Email.Trim().ToLower() == model.Email.Trim().ToLower() && v.VisitorId != id);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Visitor Email Address already registered.");
                }

                var idProofExists = await _context.VisitorMasters
                    .AnyAsync(v => v.IdProofNumber.Trim() == model.IdProofNumber.Trim() && v.VisitorId != id);
                if (idProofExists)
                {
                    ModelState.AddModelError("IdProofNumber", "ID Proof Number already registered.");
                }

                if (ModelState.IsValid)
                {
                    var visitor = await _context.VisitorMasters.FindAsync(id);
                    if (visitor == null) return NotFound();

                    string? photoPath = visitor.PhotoPath;

                    visitor.FirstName = model.FirstName;
                    visitor.LastName = model.LastName;
                    visitor.Gender = model.Gender;
                    visitor.MobileNumber = model.MobileNumber;
                    visitor.Email = model.Email;
                    visitor.Address = model.Address;
                    visitor.City = model.City;
                    visitor.State = model.State;
                    visitor.CompanyName = model.CompanyName;
                    visitor.PhotoPath = photoPath;
                    visitor.IdProofType = model.IdProofType;
                    visitor.IdProofNumber = model.IdProofNumber;
                    visitor.Status = model.Status;
                    visitor.LastUpdatedDate = DateTime.UtcNow;

                    _context.Update(visitor);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Visitor details updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        // POST: Visitor/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var visitor = await _context.VisitorMasters.FindAsync(id);
            if (visitor == null)
            {
                TempData["ErrorMessage"] = "Visitor not found.";
                return RedirectToAction(nameof(Index));
            }

            visitor.Status = (visitor.Status == "Active") ? "Inactive" : "Active";
            visitor.LastUpdatedDate = DateTime.UtcNow;
            _context.Update(visitor);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Visitor '{visitor.FirstName}' status updated to '{visitor.Status}' successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Visitor/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var visitor = await _context.VisitorMasters.FindAsync(id);
            if (visitor == null)
            {
                TempData["ErrorMessage"] = "Visitor not found.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Remove associated photo file if it exists
                if (!string.IsNullOrEmpty(visitor.PhotoPath))
                {
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, visitor.PhotoPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.VisitorMasters.Remove(visitor);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Visitor record deleted successfully!";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                TempData["ErrorMessage"] = "Cannot delete visitor because related appointment or visit records exist.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
