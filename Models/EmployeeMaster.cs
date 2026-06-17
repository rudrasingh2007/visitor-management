using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitorManagementSystem.Models
{
    [Table("EmployeeMaster")]
    public class EmployeeMaster
    {
        [Key]
        [Column("EmployeeId")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Employee Code is required.")]
        [StringLength(50, ErrorMessage = "Employee Code cannot exceed 50 characters.")]
        [Column("EmployeeCode")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        [Column("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        [Column("LastName")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Gender selection is required.")]
        [StringLength(15, ErrorMessage = "Gender cannot exceed 15 characters.")]
        [Column("Gender")]
        public string Gender { get; set; } = "Male"; // Male, Female, Other

        [Required(ErrorMessage = "Department is required.")]
        [Column("DepartmentId")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual DepartmentMaster? Department { get; set; }

        [Required(ErrorMessage = "Designation is required.")]
        [StringLength(100, ErrorMessage = "Designation cannot exceed 100 characters.")]
        [Column("Designation")]
        public string Designation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is required.")]
        [StringLength(15, ErrorMessage = "Mobile number cannot exceed 15 characters.")]
        [RegularExpression(@"^[0-9]{10,15}$", ErrorMessage = "Mobile number must be between 10 and 15 digits.")]
        [Column("MobileNumber")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(15, ErrorMessage = "Status cannot exceed 15 characters.")]
        [Column("Status")]
        public string Status { get; set; } = "Active"; // Active, Inactive

        [Required]
        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
