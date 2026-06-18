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
            var visits = await _context.VisitEntryMasters
                .Include(v => v.Appointment)
                .Include(v => v.Visitor)
                .Include(v => v.Employee)
                .Include(v => v.Department)
                .OrderByDescending(v => v.CheckInTime)
                .ToListAsync();

            // Populate filter lists in ViewBag
            ViewBag.VisitorsList = new SelectList(await _context.VisitorMasters.OrderBy(v => v.FirstName).Select(v => new { Name = v.FirstName + " " + v.LastName }).ToListAsync(), "Name", "Name");
            ViewBag.EmployeesList = new SelectList(await _context.EmployeeMasters.OrderBy(e => e.FirstName).Select(e => new { Name = e.FirstName + " " + e.LastName }).ToListAsync(), "Name", "Name");
            ViewBag.DepartmentsList = new SelectList(await _context.DepartmentMasters.OrderBy(d => d.DepartmentName).ToListAsync(), "DepartmentName", "DepartmentName");

            return View(visits);
        }

        // 2. Visitor Check-In (GET)
        public async Task<IActionResult> CheckIn(int? appointmentId)
        {
            await PopulateAppointmentsList(appointmentId);
            
            var model = new VisitEntryViewModel();
            if (appointmentId.HasValue)
            {
                model.AppointmentId = appointmentId.Value;
            }

            return View(model);
        }

        // 2. Visitor Check-In (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(VisitEntryViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate appointment
                var appointment = await _context.AppointmentMasters
                    .Include(a => a.Visitor)
                    .Include(a => a.Employee)
                    .Include(a => a.Department)
                    .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId);

                if (appointment == null)
                {
                    ModelState.AddModelError("AppointmentId", "Selected appointment does not exist.");
                    await PopulateAppointmentsList(model.AppointmentId);
                    return View(model);
                }

                if (appointment.Status != "Approved")
                {
                    ModelState.AddModelError("AppointmentId", "Only Approved appointments can be checked in.");
                    await PopulateAppointmentsList(model.AppointmentId);
                    return View(model);
                }

                // Prevent duplicate check-in (only check in once per appointment)
                var isAlreadyCheckedIn = await _context.VisitEntryMasters.AnyAsync(v => v.AppointmentId == model.AppointmentId);
                if (isAlreadyCheckedIn)
                {
                    ModelState.AddModelError("AppointmentId", "Visitor has already checked in for this appointment.");
                    await PopulateAppointmentsList(model.AppointmentId);
                    return View(model);
                }

                // Create VisitEntryMaster record
                var visitEntry = new VisitEntryMaster
                {
                    AppointmentId = appointment.AppointmentId,
                    VisitorId = appointment.VisitorId,
                    EmployeeId = appointment.EmployeeId,
                    DepartmentId = appointment.DepartmentId,
                    CheckInTime = DateTime.UtcNow,
                    VisitStatus = "Checked In",
                    Remarks = model.Remarks?.Trim(),
                    CreatedDate = DateTime.UtcNow
                };

                _context.VisitEntryMasters.Add(visitEntry);
                await _context.SaveChangesAsync();

                // Link active gate pass record to this VisitEntryId
                var gatePass = await _context.GatePassMasters
                    .FirstOrDefaultAsync(g => g.VisitorId == appointment.VisitorId &&
                                              g.EmployeeId == appointment.EmployeeId &&
                                              g.Status == "Approved" &&
                                              g.VisitEntryId == null);
                if (gatePass != null)
                {
                    gatePass.VisitEntryId = visitEntry.VisitEntryId;
                    gatePass.Status = "Checked In";

                    // Re-generate QR Code payload to reflect Check-In status and timestamp
                    var visitorName = appointment.Visitor != null ? $"{appointment.Visitor.FirstName} {appointment.Visitor.LastName}" : "Unknown";
                    var employeeName = appointment.Employee != null ? $"{appointment.Employee.FirstName} {appointment.Employee.LastName}" : "Unknown";
                    var checkInStr = visitEntry.CheckInTime.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt");

                    var qrPayload = $"Pass No: {gatePass.GatePassNumber}\n" +
                                    $"Visitor: {visitorName}\n" +
                                    $"Visitor ID: {gatePass.VisitorId}\n" +
                                    $"Host: {employeeName}\n" +
                                    $"Check-In: {checkInStr}\n" +
                                    $"Status: Checked In\n" +
                                    $"Link: http://localhost:5000/GatePass/Verify?number={gatePass.GatePassNumber}";

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
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Visitor checked in successfully!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateAppointmentsList(model.AppointmentId);
            return View(model);
        }

        // AJAX Helper: Fetch details for check-in auto-populate
        [HttpGet]
        public async Task<JsonResult> GetAppointmentDetails(int appointmentId)
        {
            var appointment = await _context.AppointmentMasters
                .Include(a => a.Visitor)
                .Include(a => a.Employee)
                .Include(a => a.Department)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                return Json(null);
            }

            return Json(new
            {
                visitorName = appointment.Visitor?.FirstName + " " + appointment.Visitor?.LastName,
                visitorMobile = appointment.Visitor?.MobileNumber,
                visitorEmail = appointment.Visitor?.Email,
                visitorCompany = appointment.Visitor?.CompanyName ?? "N/A",
                employeeName = appointment.Employee?.FirstName + " " + appointment.Employee?.LastName,
                employeeDesignation = appointment.Employee?.Designation,
                departmentName = appointment.Department?.DepartmentName ?? "N/A",
                purpose = appointment.Purpose,
                appointmentDate = appointment.AppointmentDate.ToString("dd MMM yyyy"),
                appointmentTime = DateTime.Today.Add(appointment.AppointmentTime).ToString("hh:mm tt")
            });
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
                var checkInStr = visit.CheckInTime.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt");
                var checkOutStr = visit.CheckOutTime?.ToLocalTime().ToString("dd-MM-yyyy hh:mm tt") ?? "N/A";
                
                var qrPayload = $"Pass No: {gatePass.GatePassNumber}\n" +
                                $"Visitor: {visitorName}\n" +
                                $"Visitor ID: {gatePass.VisitorId}\n" +
                                $"Host: {employeeName}\n" +
                                $"Check-In: {checkInStr}\n" +
                                $"Check-Out: {checkOutStr}\n" +
                                $"Status: Checked Out\n" +
                                $"Link: http://localhost:5000/GatePass/Verify?number={gatePass.GatePassNumber}";
                                
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

        // Helper to load approved appointments for check-in
        private async Task PopulateAppointmentsList(int? selectedApptId = null)
        {
            var checkedInApptIds = await _context.VisitEntryMasters.Select(v => v.AppointmentId).ToListAsync();
            
            var query = _context.AppointmentMasters
                .Include(a => a.Visitor)
                .Where(a => a.Status == "Approved");

            // If we are editing or reloading, allow the pre-selected appointment in the list even if it is checked in
            if (selectedApptId.HasValue)
            {
                query = query.Where(a => !checkedInApptIds.Contains(a.AppointmentId) || a.AppointmentId == selectedApptId.Value);
            }
            else
            {
                query = query.Where(a => !checkedInApptIds.Contains(a.AppointmentId));
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new
                {
                    AppointmentId = a.AppointmentId,
                    Text = $"Appt #{a.AppointmentId} - {a.Visitor!.FirstName} {a.Visitor!.LastName} ({a.AppointmentDate:dd MMM yyyy})"
                })
                .ToListAsync();

            ViewBag.ApprovedAppointments = new SelectList(appointments, "AppointmentId", "Text", selectedApptId);
        }
    }
}
