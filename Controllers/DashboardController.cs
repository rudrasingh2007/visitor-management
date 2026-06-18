using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VisitorManagementSystem.Data;
using VisitorManagementSystem.Filters;
using VisitorManagementSystem.ViewModels;

namespace VisitorManagementSystem.Controllers
{
    [SessionAuthorize]
    [HasPermission("Dashboard", "View")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var roleName = HttpContext.Session.GetString("RoleName") ?? "Guest";
            var model = new DashboardViewModel { RoleName = roleName };

            var todayStart = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var todayEnd = todayStart.AddDays(1);

            if (string.Equals(roleName, "Security Guard", StringComparison.OrdinalIgnoreCase))
            {
                model.SGPendingRequests = await _context.EntryRequestMasters.CountAsync(r => r.ApprovalStatus == "Pending Approval");
                model.SGApprovedRequests = await _context.EntryRequestMasters.CountAsync(r => r.ApprovalStatus == "Approved" && r.CreatedDate >= todayStart && r.CreatedDate < todayEnd);
                model.SGRejectedRequests = await _context.EntryRequestMasters.CountAsync(r => r.ApprovalStatus == "Rejected" && r.CreatedDate >= todayStart && r.CreatedDate < todayEnd);
                model.SGVisitorsInside = await _context.VisitEntryMasters.CountAsync(v => v.VisitStatus == "Checked In");
                model.SGPassesGeneratedToday = await _context.GatePassMasters.CountAsync(g => g.IssueDateTime >= todayStart && g.IssueDateTime < todayEnd);

                // Security Guard Dashboard list of recent check-ins
                model.RecentCheckIns = await _context.VisitEntryMasters
                    .Include(v => v.Visitor)
                    .OrderByDescending(v => v.CheckInTime)
                    .Take(5)
                    .ToListAsync();
            }
            else if (string.Equals(roleName, "Employee", StringComparison.OrdinalIgnoreCase))
            {
                var employeeId = HttpContext.Session.GetInt32("EmployeeId") ?? 0;

                model.EmpPendingApprovals = await _context.EntryRequestMasters.CountAsync(r => r.EmployeeId == employeeId && r.ApprovalStatus == "Pending Approval");
                model.EmpApprovedToday = await _context.EntryRequestMasters.CountAsync(r => r.EmployeeId == employeeId && r.ApprovalStatus == "Approved" && r.ApprovalDateTime >= todayStart && r.ApprovalDateTime < todayEnd);
                model.EmpRejectedToday = await _context.EntryRequestMasters.CountAsync(r => r.EmployeeId == employeeId && r.ApprovalStatus == "Rejected" && r.ApprovalDateTime >= todayStart && r.ApprovalDateTime < todayEnd);
                model.EmpVisitorsWaiting = await _context.VisitEntryMasters.CountAsync(v => v.EmployeeId == employeeId && v.VisitStatus == "Checked In");

                // Fetch pending requests for inline dashboard approval
                model.PendingRequests = await _context.EntryRequestMasters
                    .Include(r => r.Visitor)
                    .Include(r => r.Department)
                    .Include(r => r.Employee)
                    .Where(r => r.EmployeeId == employeeId && r.ApprovalStatus == "Pending Approval")
                    .OrderByDescending(r => r.RequestDateTime)
                    .ToListAsync();
            }
            else
            {
                // 1. KPI Cards
                // Visitors Today (distinct visitors checked in today)
                model.VisitorsToday = await _context.VisitEntryMasters
                    .Where(v => v.CheckInTime >= todayStart && v.CheckInTime < todayEnd)
                    .Select(v => v.VisitorId)
                    .Distinct()
                    .CountAsync();

                // Pending Requests
                model.PendingAppointments = await _context.EntryRequestMasters.CountAsync(r => r.ApprovalStatus == "Pending Approval");

                // Approved Requests
                model.ApprovedAppointments = await _context.EntryRequestMasters.CountAsync(r => r.ApprovalStatus == "Approved");

                // Visitors Currently Inside
                model.VisitorsCurrentlyInside = await _context.VisitEntryMasters
                    .CountAsync(v => v.VisitStatus == "Checked In");

                // Checked-Out Visitors Today
                model.TotalVisitsToday = await _context.VisitEntryMasters
                    .CountAsync(v => v.VisitStatus == "Checked Out" && v.CheckOutTime >= todayStart && v.CheckOutTime < todayEnd);

                // Gate Passes Generated Today
                model.GatePassesToday = await _context.GatePassMasters
                    .CountAsync(g => g.IssueDateTime >= todayStart && g.IssueDateTime < todayEnd);

                // 2. Daily Visitor Trend (Last 7 Days)
                var sevenDaysAgo = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-6), DateTimeKind.Utc);
                var dailyTrend = await _context.VisitEntryMasters
                    .Where(v => v.CheckInTime >= sevenDaysAgo)
                    .GroupBy(v => v.CheckInTime.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .ToListAsync();

                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-i), DateTimeKind.Utc);
                    model.DailyTrendLabels.Add(date.ToString("dd MMM"));
                    var match = dailyTrend.FirstOrDefault(d => d.Date == date);
                    model.DailyTrendData.Add(match?.Count ?? 0);
                }

                // 3. Appointment Status Summary (Pending, Approved, Rejected, Cancelled, Completed)
                var appointmentStats = await _context.AppointmentMasters
                    .GroupBy(a => a.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var statuses = new[] { "Pending Approval", "Approved", "Rejected", "Cancelled", "Checked Out" };
                foreach (var status in statuses)
                {
                    model.AppointmentStatusLabels.Add(status);
                    var match = appointmentStats.FirstOrDefault(a => string.Equals(a.Status, status, StringComparison.OrdinalIgnoreCase));
                    model.AppointmentStatusData.Add(match?.Count ?? 0);
                }

                // 4. Department-wise Visitors
                var deptStats = await _context.VisitEntryMasters
                    .Include(v => v.Department)
                    .Where(v => v.Department != null)
                    .GroupBy(v => v.Department!.DepartmentName)
                    .Select(g => new { DepartmentName = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .ToListAsync();

                foreach (var dept in deptStats)
                {
                    model.DepartmentLabels.Add(dept.DepartmentName);
                    model.DepartmentData.Add(dept.Count);
                }

                // 5. Monthly Visitor Statistics (Last 6 Months)
                var sixMonthsAgo = DateTime.SpecifyKind(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5), DateTimeKind.Utc);
                var monthlyStats = await _context.VisitEntryMasters
                    .Where(v => v.CheckInTime >= sixMonthsAgo)
                    .GroupBy(v => new { Year = v.CheckInTime.Year, Month = v.CheckInTime.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                    .ToListAsync();

                for (int i = 5; i >= 0; i--)
                {
                    var targetMonth = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddMonths(-i), DateTimeKind.Utc);
                    model.MonthlyLabels.Add(targetMonth.ToString("MMM yyyy"));
                    var match = monthlyStats.FirstOrDefault(m => m.Year == targetMonth.Year && m.Month == targetMonth.Month);
                    model.MonthlyData.Add(match?.Count ?? 0);
                }

                // 6. Recent Appointments List (top 5, sorted by date/time descending)
                model.RecentAppointments = await _context.AppointmentMasters
                    .Include(a => a.Visitor)
                    .Include(a => a.Employee)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime)
                    .Take(5)
                    .ToListAsync();

                // 7. Recent Visitor Activity List (top 5, sorted by check-in time descending)
                model.RecentCheckIns = await _context.VisitEntryMasters
                    .Include(v => v.Visitor)
                    .OrderByDescending(v => v.CheckInTime)
                    .Take(5)
                    .ToListAsync();
            }

            return View(model);
        }
    }
}
