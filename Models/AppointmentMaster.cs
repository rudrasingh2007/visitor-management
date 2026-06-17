using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitorManagementSystem.Models
{
    [Table("AppointmentMaster")]
    public class AppointmentMaster
    {
        [Key]
        [Column("AppointmentId")]
        public int AppointmentId { get; set; }

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

        [Required(ErrorMessage = "Appointment Date is mandatory.")]
        [Column("AppointmentDate", TypeName = "date")]
        [DataType(DataType.Date)]
        [FutureOrTodayDate]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Appointment Time is mandatory.")]
        [Column("AppointmentTime")]
        [DataType(DataType.Time)]
        public TimeSpan AppointmentTime { get; set; }

        [Required(ErrorMessage = "Purpose of visit is mandatory.")]
        [StringLength(250, ErrorMessage = "Purpose cannot exceed 250 characters.")]
        [Column("Purpose")]
        public string Purpose { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        [Column("Remarks")]
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(15, ErrorMessage = "Status cannot exceed 15 characters.")]
        [Column("Status")]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled, Completed

        [Required]
        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Custom validation attribute to ensure the date is not in the past (based on UTC/Local system date).
    /// </summary>
    public class FutureOrTodayDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateValue)
            {
                // Compare only the date component in local timezone (since input comes from user local calendar)
                if (dateValue.Date < DateTime.Today)
                {
                    return new ValidationResult("Appointment Date cannot be in the past.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
