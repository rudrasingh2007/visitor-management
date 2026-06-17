using System;
using System.ComponentModel.DataAnnotations;
using VisitorManagementSystem.Models;

namespace VisitorManagementSystem.ViewModels
{
    public class AppointmentViewModel
    {
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "Visitor is mandatory.")]
        [Display(Name = "Visitor")]
        public int VisitorId { get; set; }

        [Required(ErrorMessage = "Department is mandatory.")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Employee is mandatory.")]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Appointment Date is mandatory.")]
        [DataType(DataType.Date)]
        [Display(Name = "Appointment Date")]
        [FutureOrTodayDate]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Appointment Time is mandatory.")]
        [DataType(DataType.Time)]
        [Display(Name = "Appointment Time")]
        public TimeSpan AppointmentTime { get; set; }

        [Required(ErrorMessage = "Purpose of Visit is mandatory.")]
        [StringLength(250, ErrorMessage = "Purpose cannot exceed 250 characters.")]
        [Display(Name = "Purpose of Visit")]
        public string Purpose { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(15, ErrorMessage = "Status cannot exceed 15 characters.")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";
    }
}
