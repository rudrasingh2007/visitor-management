using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
using System.Collections.Generic;
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
    [HasPermission("Entry Requests", "View")]
    public class EntryRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EntryRequestController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. Index Page - Security Guard Requests List
        public async Task<IActionResult> Index()
        {
            var requests = await _context.EntryRequestMasters
                .Include(r => r.Visitor)
                .Include(r => r.Department)
                .Include(r => r.Employee)
                .Include(r => r.GatePasses)
                .OrderByDescending(r => r.RequestDateTime)
                .ToListAsync();

            return View(requests);
        }

        // 2. Create Page - Security Guard Request Submission (GET)
        [HasPermission("Entry Requests", "Add")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new EntryRequestViewModel());
        }

        // 2. Create Page - Security Guard Request Submission (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("Entry Requests", "Add")]
        public async Task<IActionResult> Create(EntryRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validation: Prevent duplicate pending requests for the same visitor and employee
                var isDuplicate = await _context.EntryRequestMasters.AnyAsync(r =>
                    r.VisitorId == model.VisitorId &&
                    r.EmployeeId == model.EmployeeId &&
                    r.ApprovalStatus == "Pending Approval");

                if (isDuplicate)
                {
                    ModelState.AddModelError("", "A duplicate pending entry request for this visitor and employee already exists.");
                    await PopulateDropdowns(model);
                    return View(model);
                }

                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                var entryRequest = new EntryRequestMaster
                {
                    VisitorId = model.VisitorId,
                    DepartmentId = model.DepartmentId,
                    EmployeeId = model.EmployeeId,
                    Purpose = model.Purpose.Trim(),
                    ApprovalStatus = "Pending Approval",
                    CreatedByUserId = userId,
                    RequestDateTime = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };

                _context.EntryRequestMasters.Add(entryRequest);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Entry Request submitted successfully for approval!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        // 3. Pending Approvals Page - Employee Screen
        [HasPermission("Entry Requests", "Edit")]
        public async Task<IActionResult> Pending()
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
            {
                ViewBag.Error = "You are not matched to any employee profile in EmployeeMaster. Please contact your administrator.";
                return View(new List<EntryRequestMaster>());
            }

            var pendingRequests = await _context.EntryRequestMasters
                .Include(r => r.Visitor)
                .Include(r => r.Department)
                .Include(r => r.Employee)
                .Where(r => r.EmployeeId == employeeId && r.ApprovalStatus == "Pending Approval")
                .OrderByDescending(r => r.RequestDateTime)
                .ToListAsync();

            return View(pendingRequests);
        }

        // 4. Approval History Page - Employee Screen
        [HasPermission("Entry Requests", "View")]
        public async Task<IActionResult> History()
        {
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
            {
                ViewBag.Error = "You are not matched to any employee profile in EmployeeMaster. Please contact your administrator.";
                return View(new List<EntryRequestMaster>());
            }

            var historyRequests = await _context.EntryRequestMasters
                .Include(r => r.Visitor)
                .Include(r => r.Department)
                .Include(r => r.Employee)
                .Where(r => r.EmployeeId == employeeId && r.ApprovalStatus != "Pending Approval")
                .OrderByDescending(r => r.ApprovalDateTime)
                .ToListAsync();

            return View(historyRequests);
        }

        // 5. Approve Action (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("Entry Requests", "Edit")]
        public async Task<IActionResult> Approve(int id, string? approvalRemarks)
        {
            var request = await _context.EntryRequestMasters.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null || request.EmployeeId != employeeId)
            {
                TempData["ErrorMessage"] = "You are not authorized to approve this request.";
                return RedirectToAction(nameof(Pending));
            }

            if (request.ApprovalStatus != "Pending Approval")
            {
                TempData["ErrorMessage"] = "This request has already been processed.";
                return RedirectToAction(nameof(Pending));
            }

            // Update EntryRequestMaster details
            request.ApprovalStatus = "Approved";
            request.ApprovalRemarks = approvalRemarks?.Trim() ?? "Approved";
            request.ApprovedByEmployeeId = employeeId.Value;
            request.ApprovalDateTime = DateTime.UtcNow;

            // Auto-create approved appointment record
            var appointment = new AppointmentMaster
            {
                VisitorId = request.VisitorId,
                DepartmentId = request.DepartmentId,
                EmployeeId = request.EmployeeId,
                AppointmentDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc),
                AppointmentTime = DateTime.UtcNow.TimeOfDay,
                Purpose = request.Purpose,
                Remarks = $"Entry Request Approved: {request.ApprovalRemarks}",
                Status = "Approved",
                CreatedDate = DateTime.UtcNow
            };

            // 1. Auto-generate Gate Pass Number: GP-YYYYMMDD-0001
            var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = $"GP-{todayStr}-";
            var todayCount = await _context.GatePassMasters
                .CountAsync(g => g.GatePassNumber.StartsWith(prefix));
            var newSequence = todayCount + 1;
            var gatePassNumber = $"{prefix}{newSequence:D4}";

            // Fetch visitor & employee names for the QR Code
            var visitor = await _context.VisitorMasters.FindAsync(request.VisitorId);
            var employee = await _context.EmployeeMasters.FindAsync(request.EmployeeId);
            var visitorName = visitor != null ? $"{visitor.FirstName} {visitor.LastName}" : "Unknown";
            var employeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown";

            // 2. Create QR Code payload
            var qrPayload = $"Pass No: {gatePassNumber}\n" +
                            $"Visitor: {visitorName}\n" +
                            $"Visitor ID: {request.VisitorId}\n" +
                            $"Host: {employeeName}\n" +
                            $"Check-In: Pending Check-In\n" +
                            $"Status: Approved\n" +
                            $"Link: http://localhost:5000/GatePass/Verify?number={gatePassNumber}";

            // 3. Generate QR Code image file automatically using QRCoder (PNG Byte helper)
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

            // 4. Create and map GatePassMaster record
            var gatePass = new GatePassMaster
            {
                GatePassNumber = gatePassNumber,
                EntryRequestId = request.EntryRequestId,
                VisitorId = request.VisitorId,
                EmployeeId = request.EmployeeId,
                DepartmentId = request.DepartmentId,
                IssueDateTime = DateTime.UtcNow,
                ExpiryDateTime = DateTime.UtcNow.AddHours(24), // 24 Hour expiry
                QRCodePath = qrCodePath,
                Status = "Approved",
                CreatedDate = DateTime.UtcNow
            };

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.EntryRequestMasters.Update(request);
                    _context.AppointmentMasters.Add(appointment);
                    _context.GatePassMasters.Add(gatePass);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Visitor entry request approved, appointment created, and gate pass generated successfully!";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = $"An error occurred during approval: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(Pending));
        }

        // 6. Reject Action (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission("Entry Requests", "Edit")]
        public async Task<IActionResult> Reject(int id, string? approvalRemarks)
        {
            var request = await _context.EntryRequestMasters.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null || request.EmployeeId != employeeId)
            {
                TempData["ErrorMessage"] = "You are not authorized to reject this request.";
                return RedirectToAction(nameof(Pending));
            }

            if (request.ApprovalStatus != "Pending Approval")
            {
                TempData["ErrorMessage"] = "This request has already been processed.";
                return RedirectToAction(nameof(Pending));
            }

            // Update details
            request.ApprovalStatus = "Rejected";
            request.ApprovalRemarks = approvalRemarks?.Trim() ?? "Rejected";
            request.ApprovedByEmployeeId = employeeId.Value;
            request.ApprovalDateTime = DateTime.UtcNow;

            _context.EntryRequestMasters.Update(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Visitor entry request rejected successfully.";
            return RedirectToAction(nameof(Pending));
        }

        // AJAX Helper: Load employees based on department selection
        [HttpGet]
        public async Task<JsonResult> GetEmployeesByDepartment(int departmentId)
        {
            var employees = await _context.EmployeeMasters
                .Where(e => e.DepartmentId == departmentId && e.Status == "Active")
                .OrderBy(e => e.FirstName)
                .Select(e => new { value = e.EmployeeId, text = $"{e.FirstName} {e.LastName} ({e.Designation})" })
                .ToListAsync();

            return Json(employees);
        }

        // Helper Dropdowns
        private async Task PopulateDropdowns(EntryRequestViewModel? model = null)
        {
            var visitors = await _context.VisitorMasters
                .Where(v => v.Status == "Active")
                .OrderBy(v => v.FirstName)
                .Select(v => new { VisitorId = v.VisitorId, Name = $"{v.FirstName} {v.LastName} ({v.MobileNumber})" })
                .ToListAsync();

            var departments = await _context.DepartmentMasters
                .Where(d => d.Status == "Active")
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            ViewBag.VisitorsList = new SelectList(visitors, "VisitorId", "Name", model?.VisitorId);
            ViewBag.DepartmentsList = new SelectList(departments, "DepartmentId", "DepartmentName", model?.DepartmentId);

            if (model != null && model.DepartmentId > 0)
            {
                var employees = await _context.EmployeeMasters
                    .Where(e => e.DepartmentId == model.DepartmentId && e.Status == "Active")
                    .OrderBy(e => e.FirstName)
                    .Select(e => new { EmployeeId = e.EmployeeId, Name = $"{e.FirstName} {e.LastName} ({e.Designation})" })
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
