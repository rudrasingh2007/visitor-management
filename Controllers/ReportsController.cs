using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Filters;
using VisitorManagementSystem.ViewModels;

namespace VisitorManagementSystem.Controllers
{
    [SessionAuthorize]
    [HasPermission("Reports", "View")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Visitor Report
        public async Task<IActionResult> Visitor()
        {
            var total = await _context.VisitorMasters.CountAsync();
            var active = await _context.VisitorMasters.CountAsync(v => v.Status == "Active");
            var inactive = await _context.VisitorMasters.CountAsync(v => v.Status == "Inactive");
            
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var newVisitors = await _context.VisitorMasters.CountAsync(v => v.CreatedDate >= thirtyDaysAgo);

            var visitors = await _context.VisitorMasters
                .OrderByDescending(v => v.CreatedDate)
                .ToListAsync();

            var viewModel = new VisitorReportViewModel
            {
                Visitors = visitors,
                TotalVisitors = total,
                ActiveVisitors = active,
                InactiveVisitors = inactive,
                NewVisitors = newVisitors
            };

            return View(viewModel);
        }

        // 2. Appointment Report
        public async Task<IActionResult> Appointment()
        {
            var total = await _context.AppointmentMasters.CountAsync();
            var pending = await _context.AppointmentMasters.CountAsync(a => a.Status == "Pending");
            var approved = await _context.AppointmentMasters.CountAsync(a => a.Status == "Approved");
            var rejected = await _context.AppointmentMasters.CountAsync(a => a.Status == "Rejected");
            var cancelled = await _context.AppointmentMasters.CountAsync(a => a.Status == "Cancelled");
            var completed = await _context.AppointmentMasters.CountAsync(a => a.Status == "Completed");

            var appointments = await _context.AppointmentMasters
                .Include(a => a.Visitor)
                .Include(a => a.Employee)
                .Include(a => a.Department)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            await PopulateFilterLists();

            var viewModel = new AppointmentReportViewModel
            {
                Appointments = appointments,
                TotalAppointments = total,
                PendingAppointments = pending,
                ApprovedAppointments = approved,
                RejectedAppointments = rejected,
                CancelledAppointments = cancelled,
                CompletedAppointments = completed
            };

            return View(viewModel);
        }

        // 3. Visit Entry Report
        public async Task<IActionResult> Visit()
        {
            var total = await _context.VisitEntryMasters.CountAsync();
            var checkouts = await _context.VisitEntryMasters.CountAsync(v => v.CheckOutTime != null);
            var activeInside = await _context.VisitEntryMasters.CountAsync(v => v.CheckOutTime == null && (v.VisitStatus == "Checked In" || v.VisitStatus == "In Meeting"));

            var visits = await _context.VisitEntryMasters
                .Include(v => v.Visitor)
                .Include(v => v.Employee)
                .Include(v => v.Department)
                .OrderByDescending(v => v.CheckInTime)
                .ToListAsync();

            await PopulateFilterLists();

            var viewModel = new VisitReportViewModel
            {
                Visits = visits,
                TotalCheckIns = total,
                TotalCheckOuts = checkouts,
                ActiveVisitorsInside = activeInside
            };

            return View(viewModel);
        }

        // 4. Gate Pass Report
        public async Task<IActionResult> GatePass()
        {
            var total = await _context.GatePassMasters.CountAsync();
            var closed = await _context.GatePassMasters.CountAsync(g => g.Status == "Closed");
            
            // Check dynamic expiry as well
            var now = DateTime.UtcNow;
            var active = await _context.GatePassMasters.CountAsync(g => g.Status == "Active" && now <= g.ExpiryDateTime);
            var expired = await _context.GatePassMasters.CountAsync(g => g.Status == "Expired" || (g.Status == "Active" && now > g.ExpiryDateTime));

            var gatePasses = await _context.GatePassMasters
                .Include(g => g.Visitor)
                .Include(g => g.Employee)
                .Include(g => g.Department)
                .OrderByDescending(g => g.IssueDateTime)
                .ToListAsync();

            await PopulateFilterLists();

            var viewModel = new GatePassReportViewModel
            {
                GatePasses = gatePasses,
                TotalGatePasses = total,
                ActiveGatePasses = active,
                ClosedGatePasses = closed,
                ExpiredGatePasses = expired
            };

            return View(viewModel);
        }

        // Helper: Populate filter selections in ViewBag
        private async Task PopulateFilterLists()
        {
            ViewBag.VisitorsList = new SelectList(await _context.VisitorMasters
                .OrderBy(v => v.FirstName)
                .Select(v => new { Name = $"{v.FirstName} {v.LastName}" })
                .ToListAsync(), "Name", "Name");

            ViewBag.EmployeesList = new SelectList(await _context.EmployeeMasters
                .OrderBy(e => e.FirstName)
                .Select(e => new { Name = $"{e.FirstName} {e.LastName}" })
                .ToListAsync(), "Name", "Name");

            ViewBag.DepartmentsList = new SelectList(await _context.DepartmentMasters
                .OrderBy(d => d.DepartmentName)
                .ToListAsync(), "DepartmentName", "DepartmentName");
        }
    }
}
