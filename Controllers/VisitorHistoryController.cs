using Microsoft.AspNetCore.Mvc;
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
    public class VisitorHistoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VisitorHistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Visitor History List
        public async Task<IActionResult> Index()
        {
            var visitors = await _context.VisitorMasters.ToListAsync();
            var visits = await _context.VisitEntryMasters.ToListAsync();

            var summaryList = visitors.Select(v => {
                var visitorVisits = visits.Where(vt => vt.VisitorId == v.VisitorId).ToList();
                return new VisitorHistorySummaryViewModel
                {
                    VisitorId = v.VisitorId,
                    PhotoPath = v.PhotoPath,
                    FullName = $"{v.FirstName} {v.LastName}",
                    MobileNumber = v.MobileNumber,
                    Email = v.Email,
                    CompanyName = v.CompanyName,
                    TotalVisits = visitorVisits.Count,
                    LastVisitDate = visitorVisits.Any() ? visitorVisits.Max(vt => vt.CheckInTime) : (DateTime?)null,
                    Status = v.Status
                };
            })
            .OrderByDescending(s => s.LastVisitDate)
            .ToList();

            return View(summaryList);
        }

        // 2. Visitor History Details Page
        public async Task<IActionResult> Details(int id)
        {
            var visitor = await _context.VisitorMasters.FirstOrDefaultAsync(v => v.VisitorId == id);
            if (visitor == null)
            {
                return NotFound();
            }

            var appointments = await _context.AppointmentMasters
                .Include(a => a.Employee)
                .Include(a => a.Department)
                .Where(a => a.VisitorId == id)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            var visits = await _context.VisitEntryMasters
                .Include(v => v.Employee)
                .Include(v => v.Department)
                .Include(v => v.Appointment)
                .Where(v => v.VisitorId == id)
                .OrderByDescending(v => v.CheckInTime)
                .ToListAsync();

            var gatePasses = await _context.GatePassMasters
                .Include(g => g.Employee)
                .Include(g => g.Department)
                .Include(g => g.VisitEntry)
                .Where(g => g.VisitorId == id)
                .OrderByDescending(g => g.IssueDateTime)
                .ToListAsync();

            // Calculate Frequently Visited Employee
            string freqEmployee = "N/A";
            if (visits.Any())
            {
                var employeeGroup = visits
                    .Where(v => v.Employee != null)
                    .GroupBy(v => v.EmployeeId)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                if (employeeGroup != null)
                {
                    var employee = employeeGroup.First().Employee;
                    if (employee != null)
                    {
                        freqEmployee = $"{employee.FirstName} {employee.LastName} ({employee.Designation})";
                    }
                }
            }

            var viewModel = new VisitorHistoryViewModel
            {
                Visitor = visitor,
                TotalVisits = visits.Count,
                LastVisitDate = visits.Any() ? visits.Max(vt => vt.CheckInTime) : (DateTime?)null,
                FrequentlyVisitedEmployee = freqEmployee,
                AppointmentHistory = appointments,
                VisitHistory = visits,
                GatePassHistory = gatePasses
            };

            return View(viewModel);
        }
    }
}
