using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitorManagementSystem.Models
{
    [Table("RoleMaster")]
    public class RoleMaster
    {
        [Key]
        [Column("RoleId")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Role Name is required.")]
        [StringLength(50, ErrorMessage = "Role Name cannot exceed 50 characters.")]
        [Column("RoleName")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        [Column("Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(15, ErrorMessage = "Status cannot exceed 15 characters.")]
        [Column("Status")]
        public string Status { get; set; } = "Active"; // Active or Inactive

        [Column("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<UserMaster> Users { get; set; } = new List<UserMaster>();
    }
}
