using System.ComponentModel.DataAnnotations;

namespace medicalapp.Models.ViewModels
{
    public class RescheduleAppointmentViewModel
    {
        public int AppointmentId { get; set; }

        [Display(Name = "Patient")]
        public string PatientName { get; set; }

        [Display(Name = "Doctor")]
        public string DoctorName { get; set; }

        [Display(Name = "Current Date")]
        public DateTime CurrentDate { get; set; }

        [Display(Name = "Current Start Time")]
        public TimeSpan CurrentStartTime { get; set; }

        [Display(Name = "Current End Time")]
        public TimeSpan CurrentEndTime { get; set; }

        [Required(ErrorMessage = "Please select a new date")]
        [Display(Name = "New Date")]
        public DateTime NewAppointmentDate { get; set; }

        [Required(ErrorMessage = "Please select a new start time")]
        [Display(Name = "New Start Time")]
        public TimeSpan NewStartTime { get; set; }

        [Required(ErrorMessage = "Please select a new end time")]
        [Display(Name = "New End Time")]
        public TimeSpan NewEndTime { get; set; }
    }
}