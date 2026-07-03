using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace medicalapp.Models.ViewModels
{
    public class ReceptionistBookAppointmentViewModel
    {
        [Required]
        public int PatientId { get; set; }
        public List<SelectListItem> Patients { get; set; } = new();

        [Required]
        public int DoctorId { get; set; }
        public List<SelectListItem> Doctors { get; set; } = new();

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string ReasonForVisit { get; set; }

        public string Symptoms { get; set; }
    }
}