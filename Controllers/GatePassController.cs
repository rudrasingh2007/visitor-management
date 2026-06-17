using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QRCoder;
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
    [HasPermission("Gate Pass", "View")]
    public class GatePassController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GatePassController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. Gate Pass List
        public async Task<IActionResult> Index()
        {
            var gatePasses = await _context.GatePassMasters
                .Include(g => g.VisitEntry)
                .Include(g => g.Visitor)
                .Include(g => g.Employee)
                .Include(g => g.Department)
                .OrderByDescending(g => g.IssueDateTime)
                .ToListAsync();

            // Populate filters in ViewBag
            ViewBag.VisitorsList = new SelectList(await _context.VisitorMasters.OrderBy(v => v.FirstName).Select(v => new { Name = v.FirstName + " " + v.LastName }).ToListAsync(), "Name", "Name");
            ViewBag.EmployeesList = new SelectList(await _context.EmployeeMasters.OrderBy(e => e.FirstName).Select(e => new { Name = e.FirstName + " " + e.LastName }).ToListAsync(), "Name", "Name");

            return View(gatePasses);
        }

        // 2. Generate Gate Pass (Disabled - Automated in background)
        public IActionResult Generate()
        {
            return RedirectToAction(nameof(Index));
        }

        // AJAX helper: Fetch visit details on Select
        [HttpGet]
        public async Task<JsonResult> GetVisitDetails(int visitEntryId)
        {
            var visit = await _context.VisitEntryMasters
                .Include(v => v.Visitor)
                .Include(v => v.Employee)
                .Include(v => v.Department)
                .Include(v => v.Appointment)
                .FirstOrDefaultAsync(v => v.VisitEntryId == visitEntryId);

            if (visit == null)
            {
                return Json(null);
            }

            return Json(new
            {
                visitorName = visit.Visitor?.FirstName + " " + visit.Visitor?.LastName,
                visitorMobile = visit.Visitor?.MobileNumber,
                visitorCompany = visit.Visitor?.CompanyName ?? "N/A",
                visitorPhoto = visit.Visitor?.PhotoPath ?? "",
                employeeName = visit.Employee?.FirstName + " " + visit.Employee?.LastName,
                employeeDesignation = visit.Employee?.Designation,
                departmentName = visit.Department?.DepartmentName ?? "N/A",
                purpose = visit.Appointment?.Purpose ?? "N/A",
                checkInTime = visit.CheckInTime.ToLocalTime().ToString("dd MMM yyyy hh:mm tt")
            });
        }

        // 3. View Gate Pass Details
        public async Task<IActionResult> Details(int id)
        {
            var gatePass = await _context.GatePassMasters
                .Include(g => g.VisitEntry)
                .Include(g => g.Visitor)
                .Include(g => g.Employee)
                .Include(g => g.Department)
                .FirstOrDefaultAsync(g => g.GatePassId == id);

            if (gatePass == null)
            {
                return NotFound();
            }

            // Verify Expiry State dynamically
            if (gatePass.Status == "Active" && DateTime.UtcNow > gatePass.ExpiryDateTime)
            {
                gatePass.Status = "Expired";
                _context.GatePassMasters.Update(gatePass);
                await _context.SaveChangesAsync();
            }

            return View(gatePass);
        }

        // 4. Print Gate Pass (GET - Printer friendly layout)
        public async Task<IActionResult> Print(int id)
        {
            var gatePass = await _context.GatePassMasters
                .Include(g => g.VisitEntry)
                .Include(g => g.Visitor)
                .Include(g => g.Employee)
                .Include(g => g.Department)
                .FirstOrDefaultAsync(g => g.GatePassId == id);

            if (gatePass == null)
            {
                return NotFound();
            }

            return View(gatePass);
        }

        // Helper: Populate SelectList of visits that need a Gate Pass
        private async Task PopulateVisitsList(int? selectedVisitId = null)
        {
            var generatedVisits = await _context.GatePassMasters.Select(g => g.VisitEntryId).ToListAsync();

            var query = _context.VisitEntryMasters
                .Include(v => v.Visitor)
                .Where(v => v.VisitStatus == "Checked In" || v.VisitStatus == "In Meeting");

            if (selectedVisitId.HasValue)
            {
                query = query.Where(v => !generatedVisits.Contains(v.VisitEntryId) || v.VisitEntryId == selectedVisitId.Value);
            }
            else
            {
                query = query.Where(v => !generatedVisits.Contains(v.VisitEntryId));
            }

            var visits = await query
                .OrderByDescending(v => v.CheckInTime)
                .Select(v => new
                {
                    VisitEntryId = v.VisitEntryId,
                    Text = $"Entry #{v.VisitEntryId} - {v.Visitor!.FirstName} {v.Visitor!.LastName} (In: {v.CheckInTime.ToLocalTime():dd MMM hh:mm tt})"
                })
                .ToListAsync();

            ViewBag.VisitsList = new SelectList(visits, "VisitEntryId", "Text", selectedVisitId);
        }
    }
}
