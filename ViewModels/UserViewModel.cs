using System;
using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.ViewModels
{
    public class UserViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is required.")]
        [StringLength(15, ErrorMessage = "Mobile number cannot exceed 15 characters.")]
        [RegularExpression(@"^[0-9]{10,15}$", ErrorMessage = "Mobile Number must be between 10 and 15 digits.")]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Role selection is mandatory.")]
        [Display(Name = "Assign Role")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(15, ErrorMessage = "Status cannot exceed 15 characters.")]
        public string Status { get; set; } = "Active";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Select Employee")]
        public int? EmployeeId { get; set; }
    }
}
