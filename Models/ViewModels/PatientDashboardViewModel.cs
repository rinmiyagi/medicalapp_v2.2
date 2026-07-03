using medicalapp.Models;

namespace medicalapp.Models.ViewModels
{
    public class PatientDashboardViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;

        public List<Appointment> UpcomingAppointments { get; set; } = new();
        public List<Appointment> PastAppointments { get; set; } = new();
        public List<Prescription> ActivePrescriptions { get; set; } = new();

        public int UpcomingCount { get; set; }
        public int PastCount { get; set; }
    }
}