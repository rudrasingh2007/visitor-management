using System.Collections.Generic;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        // 6 KPI Cards
        public int VisitorsToday { get; set; }
        public int PendingAppointments { get; set; }
        public int ApprovedAppointments { get; set; }
        public int VisitorsCurrentlyInside { get; set; }
        public int TotalVisitsToday { get; set; }
        public int GatePassesToday { get; set; }

        // Chart Data - Daily Trend (Last 7 Days)
        public List<string> DailyTrendLabels { get; set; } = new();
        public List<int> DailyTrendData { get; set; } = new();

        // Chart Data - Appointment Statuses
        public List<string> AppointmentStatusLabels { get; set; } = new();
        public List<int> AppointmentStatusData { get; set; } = new();

        // Chart Data - Department-wise Visitors
        public List<string> DepartmentLabels { get; set; } = new();
        public List<int> DepartmentData { get; set; } = new();

        // Chart Data - Monthly Stats (Last 6 Months)
        public List<string> MonthlyLabels { get; set; } = new();
        public List<int> MonthlyData { get; set; } = new();

        // Recent lists
        public List<AppointmentMaster> RecentAppointments { get; set; } = new();
        public List<VisitEntryMaster> RecentCheckIns { get; set; } = new();

        // Role-based custom KPI counts
        public string RoleName { get; set; } = string.Empty;

        // Security Guard KPIs
        public int SGPendingRequests { get; set; }
        public int SGApprovedRequests { get; set; }
        public int SGRejectedRequests { get; set; }
        public int SGVisitorsInside { get; set; }
        public int SGPassesGeneratedToday { get; set; }

        // Employee KPIs
        public int EmpPendingApprovals { get; set; }
        public int EmpApprovedToday { get; set; }
        public int EmpRejectedToday { get; set; }
        public int EmpVisitorsWaiting { get; set; }

        // Pending Requests for Employee Dashboard list
        public List<EntryRequestMaster> PendingRequests { get; set; } = new();
    }
}
