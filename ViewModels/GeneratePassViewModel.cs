using System;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.ViewModels
{
    public class GeneratePassViewModel
    {
        public int? AppointmentId { get; set; }
        public int? EntryRequestId { get; set; }
        
        public VisitorMaster? Visitor { get; set; }
        public EmployeeMaster? Employee { get; set; }
        public DepartmentMaster? Department { get; set; }
        
        public string Purpose { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        
        public string GatePassNumber { get; set; } = string.Empty;
        public string QRCodeBase64 { get; set; } = string.Empty;
    }
}
