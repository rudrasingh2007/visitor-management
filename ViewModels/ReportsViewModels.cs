using System;
using System.Collections.Generic;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.ViewModels
{
    // 1. Summary details for Visitor History List
    public class VisitorHistorySummaryViewModel
    {
        public int VisitorId { get; set; }
        public string? PhotoPath { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public int TotalVisits { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public string Status { get; set; } = "Active";
    }

    // 2. Full details for Visitor History Details Page
    public class VisitorHistoryViewModel
    {
        public VisitorMaster Visitor { get; set; } = new VisitorMaster();
        public int TotalVisits { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public string FrequentlyVisitedEmployee { get; set; } = "N/A";
        
        public IEnumerable<AppointmentMaster> AppointmentHistory { get; set; } = new List<AppointmentMaster>();
        public IEnumerable<VisitEntryMaster> VisitHistory { get; set; } = new List<VisitEntryMaster>();
        public IEnumerable<GatePassMaster> GatePassHistory { get; set; } = new List<GatePassMaster>();
    }

    // 3. Visitor Report Dashboard state
    public class VisitorReportViewModel
    {
        public IEnumerable<VisitorMaster> Visitors { get; set; } = new List<VisitorMaster>();
        public int TotalVisitors { get; set; }
        public int ActiveVisitors { get; set; }
        public int InactiveVisitors { get; set; }
        public int NewVisitors { get; set; } // Registered in the last 30 days
    }

    // 4. Appointment Report Dashboard state
    public class AppointmentReportViewModel
    {
        public IEnumerable<AppointmentMaster> Appointments { get; set; } = new List<AppointmentMaster>();
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int ApprovedAppointments { get; set; }
        public int RejectedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int CompletedAppointments { get; set; }
    }

    // 5. Visit Report Dashboard state
    public class VisitReportViewModel
    {
        public IEnumerable<VisitEntryMaster> Visits { get; set; } = new List<VisitEntryMaster>();
        public int TotalCheckIns { get; set; }
        public int TotalCheckOuts { get; set; }
        public int ActiveVisitorsInside { get; set; }
    }

    // 6. Gate Pass Report Dashboard state
    public class GatePassReportViewModel
    {
        public IEnumerable<GatePassMaster> GatePasses { get; set; } = new List<GatePassMaster>();
        public int TotalGatePasses { get; set; }
        public int ActiveGatePasses { get; set; }
        public int ClosedGatePasses { get; set; }
        public int ExpiredGatePasses { get; set; }
    }
}
