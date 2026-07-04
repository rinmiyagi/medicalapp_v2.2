using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace medicalapp.Models.ViewModels
{
    public class ReceptionistBookAppointmentViewModel
    {
        [Required]
        public int PatientId { get; set; }
        public string PatientName { get; set; } // NEW

        [Required]
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } // NEW

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string ReasonForVisit { get; set; }

        public string Symptoms { get; set; }

        // For dropdowns (if needed elsewhere)
        public List<SelectListItem> Patients { get; set; } = new();
        public List<SelectListItem> Doctors { get; set; } = new();
    }
}