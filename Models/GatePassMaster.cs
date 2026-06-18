using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitorManagementSystem.Models
{
    [Table("GatePassMaster")]
    public class GatePassMaster
    {
        [Key]
        [Column("GatePassId")]
        public int GatePassId { get; set; }

        [Required(ErrorMessage = "Gate Pass Number is required.")]
        [StringLength(50, ErrorMessage = "Gate Pass Number cannot exceed 50 characters.")]
        [Column("GatePassNumber")]
        public string GatePassNumber { get; set; } = string.Empty;

        [Column("VisitEntryId")]
        public int? VisitEntryId { get; set; }

        [ForeignKey("VisitEntryId")]
        public virtual VisitEntryMaster? VisitEntry { get; set; }

        [Column("EntryRequestId")]
        public int? EntryRequestId { get; set; }

        [ForeignKey("EntryRequestId")]
        public virtual EntryRequestMaster? EntryRequest { get; set; }

        [Required(ErrorMessage = "Visitor ID is required.")]
        [Column("VisitorId")]
        public int VisitorId { get; set; }

        [ForeignKey("VisitorId")]
        public virtual VisitorMaster? Visitor { get; set; }

        [Required(ErrorMessage = "Employee ID is required.")]
        [Column("EmployeeId")]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual EmployeeMaster? Employee { get; set; }

        [Required(ErrorMessage = "Department ID is required.")]
        [Column("DepartmentId")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual DepartmentMaster? Department { get; set; }

        [Required]
        [Column("IssueDateTime")]
        public DateTime IssueDateTime { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("ExpiryDateTime")]
        public DateTime ExpiryDateTime { get; set; }

        [Required(ErrorMessage = "QR Code Path is required.")]
        [StringLength(250, ErrorMessage = "QR Code Path cannot exceed 250 characters.")]
        [Column("QRCodePath")]
        public string QRCodePath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(15, ErrorMessage = "Status cannot exceed 15 characters.")]
        [Column("Status")]
        public string Status { get; set; } = "Approved"; // Approved, Checked In, Checked Out

        [Required]
        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
