using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitorManagementSystem.Models
{
    [Table("RoleRights")]
    public class RoleRight
    {
        [Key]
        [Column("RoleRightId")]
        public int RoleRightId { get; set; }

        [Required]
        [Column("RoleId")]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual RoleMaster? Role { get; set; }

        [Required]
        [Column("ModuleId")]
        public int ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public virtual ModuleMaster? Module { get; set; }

        [Required]
        [Column("CanView")]
        public bool CanView { get; set; } = false;

        [Required]
        [Column("CanAdd")]
        public bool CanAdd { get; set; } = false;

        [Required]
        [Column("CanEdit")]
        public bool CanEdit { get; set; } = false;

        [Required]
        [Column("CanDelete")]
        public bool CanDelete { get; set; } = false;
    }
}
