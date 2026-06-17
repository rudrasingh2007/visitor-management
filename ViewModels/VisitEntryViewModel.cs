using System;
using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.ViewModels
{
    public class VisitEntryViewModel
    {
        [Required(ErrorMessage = "Appointment selection is mandatory.")]
        [Display(Name = "Appointment")]
        public int AppointmentId { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
    }
}
