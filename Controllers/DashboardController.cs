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
                model.SGPendingRequests = await _context.EntryRequestMasters.CountAsync(r => r.ApprovalStatus == "Pending");
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

                // Approved Requests Ready For Pass Printing (Approved but no gate pass issued yet)
                var approvedAppointments = await _context.AppointmentMasters
                    .Include(a => a.Visitor)
                    .Include(a => a.Employee)
                    .Where(a => a.Status == "Approved" && !_context.GatePassMasters.Any(g => g.AppointmentId == a.AppointmentId))
                    .Select(a => new ReadyForPassItem
                    {
                        AppointmentId = a.AppointmentId,
                        EntryRequestId = null,
                        CreatedDate = a.CreatedDate,
                        VisitorName = a.Visitor != null ? a.Visitor.FirstName + " " + a.Visitor.LastName : "-",
                        EmployeeName = a.Employee != null ? a.Employee.FirstName + " " + a.Employee.LastName : "-"
                    })
                    .ToListAsync();

                var approvedRequests = await _context.EntryRequestMasters
                    .Include(r => r.Visitor)
                    .Include(r => r.Employee)
                    .Where(r => r.ApprovalStatus == "Approved" && !_context.GatePassMasters.Any(g => g.EntryRequestId == r.EntryRequestId))
                    .Select(r => new ReadyForPassItem
                    {
                        AppointmentId = null,
                        EntryRequestId = r.EntryRequestId,
                        CreatedDate = r.CreatedDate,
                        VisitorName = r.Visitor != null ? r.Visitor.FirstName + " " + r.Visitor.LastName : "-",
                        EmployeeName = r.Employee != null ? r.Employee.FirstName + " " + r.Employee.LastName : "-"
                    })
                    .ToListAsync();

                model.SGReadyForPassRequests = approvedAppointments.Concat(approvedRequests)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToList();
            }
            else if (string.Equals(roleName, "Employee", StringComparison.OrdinalIgnoreCase))
            {
                var employeeId = HttpContext.Session.GetInt32("EmployeeId") ?? 0;
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                model.EmpTodaysAppointments = await _context.AppointmentMasters.CountAsync(a => a.EmployeeId == employeeId && a.AppointmentDate.Date == todayStart.Date);
                model.EmpCompletedAppointments = await _context.AppointmentMasters.CountAsync(a => a.EmployeeId == employeeId && a.Status == "Completed");
                
                // My Visitors Today
                model.EmpVisitorsToday = await _context.VisitEntryMasters
                    .Where(v => v.EmployeeId == employeeId && v.CheckInTime.HasValue && v.CheckInTime >= todayStart && v.CheckInTime < todayEnd)
                    .Select(v => v.VisitorId)
                    .Distinct()
                    .CountAsync();

                // Employee Dashboard Lists
                model.PendingAppointmentRequests = await _context.AppointmentMasters
                    .Include(a => a.Visitor)
                    .Include(a => a.Department)
                    .Where(a => a.EmployeeId == employeeId && a.Status == "Pending")
                    .OrderByDescending(a => a.CreatedDate)
                    .ToListAsync();

                model.PendingEntryRequests = await _context.EntryRequestMasters
                    .Include(r => r.Visitor)
                    .Include(r => r.Department)
                    .Where(r => r.EmployeeId == employeeId && r.ApprovalStatus == "Pending")
                    .OrderByDescending(r => r.RequestDateTime)
                    .ToListAsync();

                model.ApprovedRequests = await _context.VisitEntryMasters
                    .Include(v => v.Visitor)
                    .Include(v => v.Department)
                    .Where(v => v.EmployeeId == employeeId && v.VisitStatus == "Approved")
                    .OrderByDescending(v => v.CreatedDate)
                    .ToListAsync();

                model.RejectedRequests = await _context.VisitEntryMasters
                    .Include(v => v.Visitor)
                    .Include(v => v.Department)
                    .Where(v => v.EmployeeId == employeeId && v.VisitStatus == "Rejected")
                    .OrderByDescending(v => v.CreatedDate)
                    .ToListAsync();

                model.VisitorHistory = await _context.VisitEntryMasters
                    .Include(v => v.Visitor)
                    .Include(v => v.Department)
                    .Where(v => v.EmployeeId == employeeId && v.VisitStatus == "Checked Out")
                    .OrderByDescending(v => v.CheckOutTime)
                    .ToListAsync();
            }
            else
            {
                // 1. KPI Cards
                // Visitors Today (distinct visitors checked in today)
                model.VisitorsToday = await _context.VisitEntryMasters
                    .Where(v => v.CheckInTime.HasValue && v.CheckInTime >= todayStart && v.CheckInTime < todayEnd)
                    .Select(v => v.VisitorId)
                    .Distinct()
                    .CountAsync();

                // Pending Requests
                model.PendingAppointments = await _context.EntryRequestMasters.CountAsync(r => r.ApprovalStatus == "Pending");

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
                    .Where(v => v.CheckInTime.HasValue && v.CheckInTime >= sevenDaysAgo)
                    .GroupBy(v => v.CheckInTime.Value.Date)
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

                var statuses = new[] { "Pending", "Approved", "Rejected", "Cancelled", "Checked Out" };
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
                    .Where(v => v.CheckInTime.HasValue && v.CheckInTime.Value >= sixMonthsAgo)
                    .GroupBy(v => new { Year = v.CheckInTime.Value.Year, Month = v.CheckInTime.Value.Month })
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
