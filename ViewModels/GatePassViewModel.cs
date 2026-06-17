using System;
using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.ViewModels
{
    public class GatePassViewModel
    {
        [Required(ErrorMessage = "Visit Entry selection is mandatory.")]
        [Display(Name = "Visit Entry")]
        public int VisitEntryId { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
    }
}
