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
        public int AdminPendingAppointments { get; set; }

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
        public int EmpTodaysAppointments { get; set; }
        public int EmpCompletedAppointments { get; set; }
        public int EmpVisitorsToday { get; set; }

        // Lists for Employee Dashboard
        public List<AppointmentMaster> PendingAppointmentRequests { get; set; } = new();
        public List<EntryRequestMaster> PendingEntryRequests { get; set; } = new();
        public List<VisitEntryMaster> ApprovedRequests { get; set; } = new();
        public List<VisitEntryMaster> RejectedRequests { get; set; } = new();
        public List<VisitEntryMaster> VisitorHistory { get; set; } = new();

        // Security Guard Dashboard lists
        public List<ReadyForPassItem> SGReadyForPassRequests { get; set; } = new();
    }

    public class ReadyForPassItem
    {
        public int? AppointmentId { get; set; }
        public int? EntryRequestId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
    }
}
