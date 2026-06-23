using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitorManagementSystem.Models
{
    [Table("VisitEntryMaster")]
    public class VisitEntryMaster
    {
        [Key]
        [Column("VisitEntryId")]
        public int VisitEntryId { get; set; }

        [Column("AppointmentId")]
        public int? AppointmentId { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual AppointmentMaster? Appointment { get; set; }

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

        [Column("CheckInTime")]
        public DateTime? CheckInTime { get; set; }

        [Column("CheckOutTime")]
        public DateTime? CheckOutTime { get; set; }

        [Required(ErrorMessage = "Visit Status is required.")]
        [StringLength(20, ErrorMessage = "Visit Status cannot exceed 20 characters.")]
        [Column("VisitStatus")]
        public string VisitStatus { get; set; } = "Pending"; // Pending, Approved, Rejected, Checked In, Checked Out

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        [Column("Remarks")]
        public string? Remarks { get; set; }

        [Required]
        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
