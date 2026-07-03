using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace medicalapp.Models.ViewModels
{
    public class MedicalReportRequestViewModel
    {
        [Required(ErrorMessage = "Please select a doctor")]
        public int DoctorId { get; set; }

        public List<SelectListItem> Doctors { get; set; } = new();

        [Required(ErrorMessage = "Please provide a reason for your request")]
        public string Reason { get; set; }

        [Display(Name = "Date From")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Date To")]
        public DateTime? DateTo { get; set; }
    }
}