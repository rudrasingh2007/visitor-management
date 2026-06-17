using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitorManagementSystem.Models
{
    [Table("EntryRequestMaster")]
    public class EntryRequestMaster
    {
        [Key]
        [Column("EntryRequestId")]
        public int EntryRequestId { get; set; }

        [Required(ErrorMessage = "Visitor is mandatory.")]
        [Column("VisitorId")]
        public int VisitorId { get; set; }

        [ForeignKey("VisitorId")]
        public virtual VisitorMaster? Visitor { get; set; }

        [Required(ErrorMessage = "Department is mandatory.")]
        [Column("DepartmentId")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual DepartmentMaster? Department { get; set; }

        [Required(ErrorMessage = "Employee is mandatory.")]
        [Column("EmployeeId")]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual EmployeeMaster? Employee { get; set; }

        [Required(ErrorMessage = "Purpose is required.")]
        [StringLength(250, ErrorMessage = "Purpose cannot exceed 250 characters.")]
        [Column("Purpose")]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        [Column("RequestDateTime")]
        public DateTime RequestDateTime { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(15)]
        [Column("ApprovalStatus")]
        public string ApprovalStatus { get; set; } = "Pending"; // Pending, Approved, Rejected

        [StringLength(500)]
        [Column("ApprovalRemarks")]
        public string? ApprovalRemarks { get; set; }

        [Column("ApprovedByEmployeeId")]
        public int? ApprovedByEmployeeId { get; set; }

        [ForeignKey("ApprovedByEmployeeId")]
        public virtual EmployeeMaster? ApprovedByEmployee { get; set; }

        [Column("ApprovalDateTime")]
        public DateTime? ApprovalDateTime { get; set; }

        [Required]
        [Column("CreatedByUserId")]
        public int CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual UserMaster? CreatedByUser { get; set; }

        [Required]
        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual System.Collections.Generic.ICollection<GatePassMaster> GatePasses { get; set; } = new System.Collections.Generic.List<GatePassMaster>();
    }
}
