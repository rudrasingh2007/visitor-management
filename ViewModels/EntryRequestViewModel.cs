using System.ComponentModel.DataAnnotations;

namespace VisitorManagementSystem.ViewModels
{
    public class EntryRequestViewModel
    {
        [Required(ErrorMessage = "Visitor selection is mandatory.")]
        public int VisitorId { get; set; }

        [Required(ErrorMessage = "Department selection is mandatory.")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Employee selection is mandatory.")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Purpose of visit is mandatory.")]
        [StringLength(250, ErrorMessage = "Purpose cannot exceed 250 characters.")]
        public string Purpose { get; set; } = string.Empty;
    }
}
