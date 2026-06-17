using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitorManagementSystem.Models
{
    [Table("DepartmentMaster")]
    public class DepartmentMaster
    {
        [Key]
        [Column("DepartmentId")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Department Name is required.")]
        [StringLength(100, ErrorMessage = "Department Name cannot exceed 100 characters.")]
        [Column("DepartmentName")]
        public string DepartmentName { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        [Column("Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(15, ErrorMessage = "Status cannot exceed 15 characters.")]
        [Column("Status")]
        public string Status { get; set; } = "Active"; // Active, Inactive

        [Required]
        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation property for One-to-Many relationship
        public virtual ICollection<EmployeeMaster> Employees { get; set; } = new List<EmployeeMaster>();
    }
}
