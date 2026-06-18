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
        [SessionAuthorize]
        [HasPermission("Gate Pass", "View")]
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
        [SessionAuthorize]
        [HasPermission("Gate Pass", "Add")]
        public IActionResult Generate()
        {
            return RedirectToAction(nameof(Index));
        }

        // AJAX helper: Fetch visit details on Select
        [SessionAuthorize]
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
        [SessionAuthorize]
        [HasPermission("Gate Pass", "View")]
        public async Task<IActionResult> Details(int id)
        {
            var gatePass = await _context.GatePassMasters
                .Include(g => g.VisitEntry)
                .Include(g => g.Visitor)
                .Include(g => g.Employee)
                .Include(g => g.Department)
                .Include(g => g.EntryRequest)
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
        [SessionAuthorize]
        [HasPermission("Gate Pass", "View")]
        public async Task<IActionResult> Print(int id)
        {
            var gatePass = await _context.GatePassMasters
                .Include(g => g.VisitEntry)
                .Include(g => g.Visitor)
                .Include(g => g.Employee)
                .Include(g => g.Department)
                .Include(g => g.EntryRequest)
                .FirstOrDefaultAsync(g => g.GatePassId == id);

            if (gatePass == null)
            {
                return NotFound();
            }

            return View(gatePass);
        }

        // 5. Verify Gate Pass (Scan check, public access)
        [HttpGet]
        public async Task<IActionResult> Verify(string number)
        {
            if (string.IsNullOrEmpty(number))
            {
                ViewBag.Status = "Invalid";
                ViewBag.Message = "PASS CLOSED / INVALID";
                return View();
            }

            var gatePass = await _context.GatePassMasters
                .Include(g => g.Visitor)
                .Include(g => g.Employee)
                .Include(g => g.Department)
                .Include(g => g.VisitEntry)
                .Include(g => g.EntryRequest)
                .FirstOrDefaultAsync(g => g.GatePassNumber == number);

            if (gatePass == null)
            {
                ViewBag.Status = "Invalid";
                ViewBag.Message = "PASS CLOSED / INVALID";
                return View();
            }

            // QR verification logic: VALID PASS if status is Approved or Checked In
            if (gatePass.Status == "Approved" || gatePass.Status == "Checked In")
            {
                ViewBag.Status = "Valid";
                ViewBag.Message = "VALID PASS";
            }
            else
            {
                ViewBag.Status = "Invalid";
                ViewBag.Message = "PASS CLOSED / INVALID";
            }

            return View(gatePass);
        }
    }
}
