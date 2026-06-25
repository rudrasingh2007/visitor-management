using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitorManagementSystem.Models
{
    [Table("UserMaster")]
    public class UserMaster
    {
        [Key]
        [Column("UserId")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        [Column("Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        [Column("FullName")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is required.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must contain exactly 10 digits.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile number must contain exactly 10 digits.")]
        [Column("MobileNumber")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        [Column("Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role selection is mandatory.")]
        [Column("RoleId")]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual RoleMaster? Role { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(15, ErrorMessage = "Status cannot exceed 15 characters.")]
        [Column("Status")]
        public string Status { get; set; } = "Active"; // Active or Inactive

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("LastUpdatedDate")]
        public DateTime? LastUpdatedDate { get; set; }

        [Column("EmployeeId")]
        public int? EmployeeId { get; set; }

        [Column("PhotoPath")]
        [StringLength(255)]
        public string? PhotoPath { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual EmployeeMaster? Employee { get; set; }

        // Lockout fields
        [Column("FailedLoginAttempts")]
        public int FailedLoginAttempts { get; set; } = 0;

        [Column("LockoutEnd")]
        public DateTime? LockoutEnd { get; set; }

        [Column("LockoutLevel")]
        public int LockoutLevel { get; set; } = 0;
    }
}
