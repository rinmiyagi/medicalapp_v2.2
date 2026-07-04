using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace medicalapp.Models.ViewModels
{
    public class CreateReferralViewModel
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int FromDoctorId { get; set; }

        [Required(ErrorMessage = "Please select a doctor")]
        public int ToDoctorId { get; set; }

        public List<SelectListItem> Doctors { get; set; } = new();

        [Required(ErrorMessage = "Please provide a reason for the referral")]
        public string Reason { get; set; }

        public string Notes { get; set; }
    }
}