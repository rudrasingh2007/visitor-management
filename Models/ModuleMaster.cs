using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitorManagementSystem.Models
{
    [Table("ModuleMaster")]
    public class ModuleMaster
    {
        [Key]
        [Column("ModuleId")]
        public int ModuleId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("ModuleName")]
        public string ModuleName { get; set; } = string.Empty;
    }
}
