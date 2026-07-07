using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace medicalapp.Models.ViewModels
{
    public class BookAppointmentViewModel
    {
        [Required(ErrorMessage = "Please select a doctor")]
        public int DoctorId { get; set; }
        public List<SelectListItem> Doctors { get; set; } = new();

        [Required(ErrorMessage = "Please select a date and time")]
        [Display(Name = "Appointment Date & Time")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Please tell us why you're visiting")]
        [Display(Name = "Reason for Visit")]
        public string ReasonForVisit { get; set; }

        [Display(Name = "Symptoms (Optional)")]
        public string? Symptoms { get; set; }

        // Display properties - make them nullable or give default values
        public string? DoctorName { get; set; }
        public decimal ConsultationFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Health Screening Package properties
        public string? PackageName { get; set; }
        public decimal? PackagePrice { get; set; }
        public List<int> AvailableDaysOfWeek { get; set; } = new();
        public List<string> AvailableDayNames { get; set; } = new();
    }
}