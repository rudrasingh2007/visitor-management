using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitorManagementSystem.Models
{
    [Table("VisitorMaster")]
    public class VisitorMaster
    {
        [Key]
        [Column("VisitorId")]
        public int VisitorId { get; set; }

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

        [Required(ErrorMessage = "Mobile Number is required.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Mobile number must contain exactly 10 digits.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile number must contain exactly 10 digits.")]
        [Column("MobileNumber")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        [Column("Address")]
        public string? Address { get; set; }

        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
        [Column("City")]
        public string? City { get; set; }

        [StringLength(50, ErrorMessage = "State cannot exceed 50 characters.")]
        [Column("State")]
        public string? State { get; set; }

        [StringLength(100, ErrorMessage = "Company Name cannot exceed 100 characters.")]
        [Column("CompanyName")]
        public string? CompanyName { get; set; }

        [StringLength(250, ErrorMessage = "Photo file path cannot exceed 250 characters.")]
        [Column("PhotoPath")]
        public string? PhotoPath { get; set; }

        [Required(ErrorMessage = "ID Proof Type is required.")]
        [StringLength(50, ErrorMessage = "ID Proof Type cannot exceed 50 characters.")]
        [Column("IdProofType")]
        public string IdProofType { get; set; } = "Aadhaar Card"; // Aadhaar Card, PAN Card, etc.

        [Required(ErrorMessage = "ID Proof Number is required.")]
        [StringLength(50, ErrorMessage = "ID Proof Number cannot exceed 50 characters.")]
        [Column("IdProofNumber")]
        public string IdProofNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(15, ErrorMessage = "Status cannot exceed 15 characters.")]
        [Column("Status")]
        public string Status { get; set; } = "Active"; // Active, Inactive

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Column("LastUpdatedDate")]
        public DateTime? LastUpdatedDate { get; set; }
    }
}
