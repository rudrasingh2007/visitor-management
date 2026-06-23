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
using Microsoft.AspNetCore.Authorization;

namespace VisitorManagementSystem.Controllers
{
    [SessionAuthorize]
    [HasPermission("Visit Entry", "View")]
    public class VisitController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VisitController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. Visit Entry List
        public async Task<IActionResult> Index()
        {
            var roleName = HttpContext.Session.GetString("RoleName");
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            var query = _context.VisitEntryMasters
                .Include(v => v.Appointment)
                .Include(v => v.Visitor)
                .Include(v => v.Employee)
                .Include(v => v.Department)
                .AsQueryable();

            if (roleName == "Employee" && employeeId.HasValue)
            {
                query = query.Where(v => v.EmployeeId == employeeId.Value);
            }

            var visits = await query.ToListAsync();

            // Define priority mapping for statuses
            var statusPriority = new Dictionary<string, int>
            {
                {"Pending", 0},
                {"Approved", 1}, // Pass not printed will be handled later if needed
                {"Checked In", 2},
                {"Rejected", 3},
                {"Checked Out", 4}
            };

            // Sort: Active (not Checked Out) first, then by status priority, then latest check‑in/create date
            visits = visits
                .OrderBy(v => v.VisitStatus == "Checked Out") // false (active) first
                .ThenBy(v => statusPriority.ContainsKey(v.VisitStatus) ? statusPriority[v.VisitStatus] : 5)
                .ThenByDescending(v => v.CheckInTime ?? v.CreatedDate)
                .ToList();

            // Populate filter lists in ViewBag
            ViewBag.VisitorsList = new SelectList(await _context.VisitorMasters.OrderBy(v => v.FirstName).Select(v => new { Name = v.FirstName + " " + v.LastName }).ToListAsync(), "Name", "Name");
            ViewBag.EmployeesList = new SelectList(await _context.EmployeeMasters.OrderBy(e => e.FirstName).Select(e => new { Name = e.FirstName + " " + e.LastName }).ToListAsync(), "Name", "Name");
            ViewBag.DepartmentsList = new SelectList(await _context.DepartmentMasters.OrderBy(d => d.DepartmentName).ToListAsync(), "DepartmentName", "DepartmentName");

            return View(visits);
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DebugInfo()
        {
            var appts = await _context.AppointmentMasters.OrderByDescending(a => a.AppointmentId).Take(5).Select(a => new { a.AppointmentId, a.Status, a.AppointmentDate }).ToListAsync();
            var visits = await _context.VisitEntryMasters.OrderByDescending(v => v.VisitEntryId).Take(5).Select(v => new { v.VisitEntryId, v.AppointmentId, v.VisitStatus, v.EntryRequestId }).ToListAsync();
            return Json(new { appts, visits });
        }

        // 3. Visit Entry Details (GET)
        public async Task<IActionResult> Details(int id)
        {
            var visit = await _context.VisitEntryMasters
                .Include(v => v.Appointment)
                .Include(v => v.Visitor)
                .Include(v => v.Employee)
                .Include(v => v.Department)
                .FirstOrDefaultAsync(v => v.VisitEntryId == id);

            if (visit == null)
            {
                return NotFound();
            }

            return View(visit);
        }

        // 4. Visitor Check-Out (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id, string? checkoutRemarks)
        {
            var visit = await _context.VisitEntryMasters.FindAsync(id);
            if (visit == null)
            {
                return NotFound();
            }

            if (visit.VisitStatus == "Checked Out")
            {
                TempData["ErrorMessage"] = "Visitor has already checked out.";
                return RedirectToAction(nameof(Index));
            }

            // Record check-out time (UTC) and complete visit status
            visit.CheckOutTime = DateTime.UtcNow;
            visit.VisitStatus = "Checked Out";
            if (!string.IsNullOrWhiteSpace(checkoutRemarks))
            {
                visit.Remarks = string.IsNullOrWhiteSpace(visit.Remarks) 
                    ? checkoutRemarks.Trim() 
                    : $"{visit.Remarks} | Check-Out: {checkoutRemarks.Trim()}";
            }

            // Also complete the corresponding appointment record to close the loop
            var appointment = await _context.AppointmentMasters.FindAsync(visit.AppointmentId);
            if (appointment != null)
            {
                appointment.Status = "Checked Out";
            }

            // Close the linked active gate pass record
            var gatePass = await _context.GatePassMasters
                .FirstOrDefaultAsync(g => g.VisitEntryId == visit.VisitEntryId && g.Status == "Checked In");
            if (gatePass != null)
            {
                gatePass.Status = "Checked Out";
                
                // Re-generate QR Code payload to reflect Checked Out status (Invalid)
                if (visit.Visitor == null)
                {
                    await _context.Entry(visit).Reference(v => v.Visitor).LoadAsync();
                }
                if (visit.Employee == null)
                {
                    await _context.Entry(visit).Reference(v => v.Employee).LoadAsync();
                }
                var visitorName = visit.Visitor != null ? $"{visit.Visitor.FirstName} {visit.Visitor.LastName}" : "Unknown";
                var employeeName = visit.Employee != null ? $"{visit.Employee.FirstName} {visit.Employee.LastName}" : "Unknown";
                var checkInStr = visit.CheckInTime.HasValue ? visit.CheckInTime.Value.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt") : "Pending";
                var checkOutStr = visit.CheckOutTime?.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt") ?? "N/A";
                
                var qrPayload = $"GATEPASS:{gatePass.GatePassNumber}|VISITOR:{visit.VisitorId}|REF:{visit.AppointmentId ?? visit.EntryRequestId ?? 0}|STATUS:CHECKED_OUT";

                using (var qrGenerator = new QRCodeGenerator())
                using (var qrCodeData = qrGenerator.CreateQrCode(qrPayload, QRCodeGenerator.ECCLevel.Q))
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrBytes = qrCode.GetGraphic(20);
                    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "qrcodes");
                    var filePath = Path.Combine(uploadFolder, $"{gatePass.GatePassNumber}.png");
                    await System.IO.File.WriteAllBytesAsync(filePath, qrBytes);
                }

                _context.GatePassMasters.Update(gatePass);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Visitor checked out successfully!";
            return RedirectToAction(nameof(Index));
        }


        // 5. Print Pass (GET)
        [HasPermission("Visit Entry", "View")]
        public async Task<IActionResult> PrintPass(int id)
        {
            var visit = await _context.VisitEntryMasters
                .Include(v => v.Visitor)
                .Include(v => v.Employee)
                .Include(v => v.Department)
                .Include(v => v.Appointment)
                .FirstOrDefaultAsync(v => v.VisitEntryId == id);

            if (visit == null)
            {
                return NotFound();
            }

            // A pass is only available for Approved, Checked In, or Checked Out statuses
            if (visit.VisitStatus != "Approved" && visit.VisitStatus != "Checked In" && visit.VisitStatus != "Checked Out")
            {
                TempData["ErrorMessage"] = "Pass is not available for this status.";
                return RedirectToAction(nameof(Index));
            }

            // Retrieve associated gate pass record
            var gatePass = await _context.GatePassMasters
                .FirstOrDefaultAsync(g => g.VisitEntryId == visit.VisitEntryId);

            if (gatePass == null)
            {
                // Auto generate if not exists
                var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
                var prefix = $"GP-{todayStr}-";
                var todayCount = await _context.GatePassMasters.CountAsync(g => g.GatePassNumber.StartsWith(prefix));
                var newSequence = todayCount + 1;
                var gatePassNumber = $"{prefix}{newSequence:D4}";

                var visitorName = visit.Visitor != null ? $"{visit.Visitor.FirstName} {visit.Visitor.LastName}" : "Unknown";
                var employeeName = visit.Employee != null ? $"{visit.Employee.FirstName} {visit.Employee.LastName}" : "Unknown";

                var qrPayload = $"Pass No: {gatePassNumber}\n" +
                                $"Visitor: {visitorName}\n" +
                                $"Visitor ID: {visit.VisitorId}\n" +
                                $"Host: {employeeName}\n" +
                                $"Check-In: {(visit.CheckInTime.HasValue ? visit.CheckInTime.Value.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt") : "Pending")}\n" +
                                $"Status: {visit.VisitStatus}\n" +
                                $"Link: http://localhost:5000/GatePass/Verify?number={gatePassNumber}";

                string qrCodePath = string.Empty;
                using (var qrGenerator = new QRCodeGenerator())
                using (var qrCodeData = qrGenerator.CreateQrCode(qrPayload, QRCodeGenerator.ECCLevel.Q))
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrBytes = qrCode.GetGraphic(20);
                    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "qrcodes");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }
                    var fileName = $"{gatePassNumber}.png";
                    var filePath = Path.Combine(uploadFolder, fileName);
                    await System.IO.File.WriteAllBytesAsync(filePath, qrBytes);
                    qrCodePath = $"/uploads/qrcodes/{fileName}";
                }

                gatePass = new GatePassMaster
                {
                    GatePassNumber = gatePassNumber,
                    EntryRequestId = visit.EntryRequestId,
                    VisitorId = visit.VisitorId,
                    EmployeeId = visit.EmployeeId,
                    DepartmentId = visit.DepartmentId,
                    VisitEntryId = visit.VisitEntryId,
                    IssueDateTime = DateTime.UtcNow,
                    ExpiryDateTime = DateTime.UtcNow.AddHours(24),
                    QRCodePath = qrCodePath,
                    Status = visit.VisitStatus,
                    CreatedDate = DateTime.UtcNow
                };
                _context.GatePassMasters.Add(gatePass);
                await _context.SaveChangesAsync();
            }

            ViewBag.GatePass = gatePass;
            return View(visit);
        }
    }
}
