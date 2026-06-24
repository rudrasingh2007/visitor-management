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

        // 6. Generate Pass UI (Webcam Capture)
        [HttpGet]
        public async Task<IActionResult> Generate(int? appointmentId, int? entryRequestId)
        {
            var model = new GeneratePassViewModel();

            if (appointmentId.HasValue)
            {
                var app = await _context.AppointmentMasters
                    .Include(a => a.Visitor)
                    .Include(a => a.Employee)
                    .Include(a => a.Department)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
                
                if (app == null) return NotFound();

                if (app.Status != "Approved")
                {
                    TempData["ErrorMessage"] = "Cannot generate a pass. The appointment is not in an 'Approved' state.";
                    return RedirectToAction("Index", "Dashboard");
                }

                var existingPass = await _context.GatePassMasters.AnyAsync(g => g.AppointmentId == appointmentId);
                if (existingPass)
                {
                    TempData["ErrorMessage"] = "A Gate Pass has already been generated for this appointment.";
                    return RedirectToAction("Index", "Dashboard");
                }

                model.AppointmentId = app.AppointmentId;
                model.Visitor = app.Visitor;
                model.Employee = app.Employee;
                model.Department = app.Department;
                model.Purpose = app.Purpose ?? "";
                model.RequestType = "Appointment";
                if (model.Visitor != null)
                {
                    model.Address = $"{model.Visitor.Address}, {model.Visitor.City}, {model.Visitor.State}";
                }
            }
            else if (entryRequestId.HasValue)
            {
                var req = await _context.EntryRequestMasters
                    .Include(r => r.Visitor)
                    .Include(r => r.Employee)
                    .Include(r => r.Department)
                    .FirstOrDefaultAsync(r => r.EntryRequestId == entryRequestId);
                
                if (req == null) return NotFound();

                if (req.ApprovalStatus != "Approved")
                {
                    TempData["ErrorMessage"] = "Cannot generate a pass. The entry request is not in an 'Approved' state.";
                    return RedirectToAction("Index", "Dashboard");
                }

                var existingPass = await _context.GatePassMasters.AnyAsync(g => g.EntryRequestId == entryRequestId);
                if (existingPass)
                {
                    TempData["ErrorMessage"] = "A Gate Pass has already been generated for this entry request.";
                    return RedirectToAction("Index", "Dashboard");
                }

                model.EntryRequestId = req.EntryRequestId;
                model.Visitor = req.Visitor;
                model.Employee = req.Employee;
                model.Department = req.Department;
                model.Purpose = req.Purpose ?? "";
                model.RequestType = "Walk-In";
                if (model.Visitor != null)
                {
                    model.Address = $"{model.Visitor.Address}, {model.Visitor.City}, {model.Visitor.State}";
                }
            }
            else
            {
                return BadRequest();
            }

            var refId = model.AppointmentId ?? model.EntryRequestId ?? 0;
            model.GatePassNumber = $"GP-{DateTime.UtcNow.ToString("yyyyMMdd")}-{refId}";

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                var qrData = $"GATEPASS:{model.GatePassNumber}|VISITOR:{model.Visitor?.VisitorId}|REF:{refId}";
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                using PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrBytes = qrCode.GetGraphic(20);
                model.QRCodeBase64 = Convert.ToBase64String(qrBytes);
            }

            return View(model);
        }

        // 7. Save Photo, Generate Pass, and Check-In
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int? appointmentId, int? entryRequestId, string photoBase64)
        {
            if (!appointmentId.HasValue && !entryRequestId.HasValue) return BadRequest();

            VisitorMaster? visitor = null;
            int employeeId = 0;
            int departmentId = 0;
            int refId = 0;

            if (appointmentId.HasValue)
            {
                var app = await _context.AppointmentMasters.Include(a => a.Visitor).FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
                if (app == null) return NotFound();
                
                if (app.Status != "Approved")
                {
                    TempData["ErrorMessage"] = "Cannot generate a pass. The appointment is not in an 'Approved' state.";
                    return RedirectToAction("Index", "Dashboard");
                }

                var existingPass = await _context.GatePassMasters.AnyAsync(g => g.AppointmentId == appointmentId);
                if (existingPass)
                {
                    TempData["ErrorMessage"] = "A Gate Pass has already been generated for this appointment.";
                    return RedirectToAction("Index", "Dashboard");
                }

                visitor = app.Visitor;
                employeeId = app.EmployeeId;
                departmentId = app.DepartmentId;
                refId = app.AppointmentId;
            }
            else
            {
                var req = await _context.EntryRequestMasters.Include(r => r.Visitor).FirstOrDefaultAsync(r => r.EntryRequestId == entryRequestId);
                if (req == null) return NotFound();
                
                if (req.ApprovalStatus != "Approved")
                {
                    TempData["ErrorMessage"] = "Cannot generate a pass. The entry request is not in an 'Approved' state.";
                    return RedirectToAction("Index", "Dashboard");
                }

                var existingPass = await _context.GatePassMasters.AnyAsync(g => g.EntryRequestId == entryRequestId);
                if (existingPass)
                {
                    TempData["ErrorMessage"] = "A Gate Pass has already been generated for this entry request.";
                    return RedirectToAction("Index", "Dashboard");
                }

                visitor = req.Visitor;
                employeeId = req.EmployeeId;
                departmentId = req.DepartmentId;
                refId = req.EntryRequestId;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Save Photo
                    if (!string.IsNullOrEmpty(photoBase64) && visitor != null)
                    {
                        var base64Data = photoBase64.Substring(photoBase64.IndexOf(",") + 1);
                        var imageBytes = Convert.FromBase64String(base64Data);
                        var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "visitors");
                        
                        if (!Directory.Exists(uploadFolder))
                        {
                            Directory.CreateDirectory(uploadFolder);
                        }

                        var fileName = $"visitor_{visitor.VisitorId}_{DateTime.UtcNow.Ticks}.jpg";
                        var filePath = Path.Combine(uploadFolder, fileName);
                        await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                        
                        visitor.PhotoPath = $"/uploads/visitors/{fileName}";
                        _context.VisitorMasters.Update(visitor);
                        await _context.SaveChangesAsync();
                    }

                    // 2. Create VisitEntryMaster
                    var visitEntry = new VisitEntryMaster
                    {
                        AppointmentId = appointmentId,
                        EntryRequestId = entryRequestId,
                        VisitorId = visitor!.VisitorId,
                        EmployeeId = employeeId,
                        DepartmentId = departmentId,
                        CheckInTime = DateTime.UtcNow,
                        VisitStatus = "Checked In",
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.VisitEntryMasters.Add(visitEntry);
                    await _context.SaveChangesAsync();

                    // 3. Generate Gate Pass
                    var gatePassNumber = $"GP-{DateTime.UtcNow.ToString("yyyyMMdd")}-{refId}";
                    
                    string? qrCodePath = null;
                    using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                    {
                        var qrData = $"GATEPASS:{gatePassNumber}|VISITOR:{visitor.VisitorId}|REF:{refId}";
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                        using PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
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

                    var gatePass = new GatePassMaster
                    {
                        GatePassNumber = gatePassNumber,
                        AppointmentId = appointmentId,
                        EntryRequestId = entryRequestId,
                        VisitorId = visitor.VisitorId,
                        EmployeeId = employeeId,
                        DepartmentId = departmentId,
                        VisitEntryId = visitEntry.VisitEntryId,
                        IssueDateTime = DateTime.UtcNow,
                        ExpiryDateTime = DateTime.UtcNow.AddHours(24),
                        QRCodePath = qrCodePath,
                        Status = "Checked In",
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.GatePassMasters.Add(gatePass);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Gate Pass generated and Visitor checked in successfully.";
                    
                    return RedirectToAction("Index", "Dashboard");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Failed to generate pass: " + ex.Message;
                    return RedirectToAction("Index", "Dashboard");
                }
            }
        }

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
            if (gatePass.Status == "Checked Out" || (gatePass.VisitEntry != null && gatePass.VisitEntry.VisitStatus == "Checked Out"))
            {
                ViewBag.Status = "Invalid";
                ViewBag.Message = "Visitor Already Checked Out";
            }
            else if (gatePass.Status == "Approved" || gatePass.Status == "Checked In")
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
