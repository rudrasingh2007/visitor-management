using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.ViewModels
{
    public class VisitorViewModel
    {
        public int VisitorId { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Gender selection is required.")]
        [StringLength(15)]
        public string Gender { get; set; } = "Male"; // Male, Female, Other

        [Required(ErrorMessage = "Mobile Number is required.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile Number must contain exactly 10 digits.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile Number must contain exactly 10 digits.")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Address is required.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "State is required.")]
        public string State { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Company Name cannot exceed 100 characters.")]
        public string? CompanyName { get; set; }

        public string? PhotoPath { get; set; }

        [Required(ErrorMessage = "ID Proof Type is required.")]
        [StringLength(50)]
        public string IdProofType { get; set; } = "Aadhaar Card"; // Aadhaar Card, PAN Card, etc.

        [Required(ErrorMessage = "ID Proof Number is required.")]
        [StringLength(50, ErrorMessage = "ID Proof Number cannot exceed 50 characters.")]
        public string IdProofNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(15)]
        public string Status { get; set; } = "Active"; // Active, Inactive
    }
}
